﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AstraTech
{
    public class AstraBrain : ThingWithComps, IPawnContainer
    {
        public override string Label => "Astra Brain (" + GetPawn().NameFullColored + ")";
        public bool IsUnstable => Def.unstableWorktimeInDays > 0;
        public AstraBrainDef Def
        {
            get
            {
                if (_def == null) _def = (AstraBrainDef)def;
                return _def;
            }
        }


        public int unstableWorktimeInTicksLeft = -1;
        public Pawn innerPawn;
        public HashSet<SkillDef> availableSkills;

        private AstraBrainDef _def;


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            // If just crafter or spawned via DevTools
            if (innerPawn == null)
            {
                innerPawn = Building_AstraPawnMachine.CreateBlank();
                innerPawn.Name = new NameSingle("Unnamed");
            }

            if (innerPawn.story.Childhood == null)
            {
                innerPawn.story.Childhood = AstraDefOf.astra_blank;
                innerPawn.story.Adulthood = AstraDefOf.astra_blank_adult;
            }

            if (IsUnstable && unstableWorktimeInTicksLeft == -1)
            {
                unstableWorktimeInTicksLeft = (int)(Def.unstableWorktimeInDays * GenDate.TicksPerDay);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerPawn, nameof(innerPawn));
            Scribe_Values.Look(ref unstableWorktimeInTicksLeft, nameof(unstableWorktimeInTicksLeft));
            Scribe_Collections.Look(ref availableSkills, nameof(availableSkills));
        }

        public override string GetInspectString()
        {
            if (IsUnstable)
            {
                return "Wear: " + (int)(100 * (1 - unstableWorktimeInTicksLeft / (Def.unstableWorktimeInDays * GenDate.TicksPerDay))) + "%";
            }
            else
            {
                return base.GetInspectString();
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var o in base.GetFloatMenuOptions(selPawn))
            {
                yield return o;
            }

            yield return new FloatMenuOption("Insert into blank or machine", () =>
            {
                Find.Targeter.BeginTargeting(new TargetingParameters()
                {
                    canTargetPawns = true,
                    onlyTargetControlledPawns = true,
                    mapObjectTargetsMustBeAutoAttackable = false,
                    canTargetItems = false,
                    validator = (i) =>
                    {
                        bool reserved = Map.reservationManager.IsReserved(i.Thing);
                        if (reserved) return false;

                        if (i.Thing is Pawn pawn && pawn.health.hediffSet.TryGetHediff(out Hediff_AstraBrainSocket socket) && socket.brain == null) return true;
                        if (i.Thing is Building_AstraPawnMachine machine && machine.ContainsBrainByStable(this) == false) return true;

                        return false;
                    }
                }, (i) =>
                {
                    if (i.Thing is Pawn blank)
                    {
                        GenJob.TryGiveJob<JobDriver_InsertBrainIntoBlank>(selPawn, this, blank);
                    }
                    else if (i.Thing is Building_AstraPawnMachine machine)
                    {
                        GenJob.TryGiveJob<JobDriver_InsertBrainIntoMachine>(selPawn, this, machine);
                    }                   
                });
            });
        }

        

        public static HashSet<NeedDef> brainRelatedNeeds = new HashSet<NeedDef>()
        {
            AstraDefOf.Mood,
            //AstraDefOf.Rest,
            //AstraDefOf.Food,
            AstraDefOf.Joy,
            AstraDefOf.Beauty,
            AstraDefOf.Comfort,
            AstraDefOf.Outdoors,
            AstraDefOf.Indoors,
            //AstraDefOf.DrugDesire,
            AstraDefOf.RoomSize,
        };

        public static HashSet<TraitDef> bodyRelatedTraits = new HashSet<TraitDef>()
        {
            TraitDefOf.AnnoyingVoice,
            TraitDefOf.CreepyBreathing,
            DefDatabase<TraitDef>.GetNamed("Beauty"),
            DefDatabase<TraitDef>.GetNamed("Immunity"),
            DefDatabase<TraitDef>.GetNamed("Tough"),
        };

        //private static FieldInfo cachedThoughtsField = typeof(SituationalThoughtHandler).GetField("cachedThoughts", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void ClearPawn(Pawn p)
        {
            p.Name = new NameSingle("Blank");

            foreach (SkillRecord pawnSkill in p.skills.skills)
            {
                pawnSkill.Level = 0;
                pawnSkill.passion = Passion.None;
            }


            // Remove only brain related traits
            p.story.traits.allTraits.RemoveAll(t => bodyRelatedTraits.Contains(t.def) == false);


            // Clear story
            p.story.AllBackstories.Clear();
            p.story.Childhood = AstraDefOf.astra_blank;
            p.story.Adulthood = AstraDefOf.astra_blank_adult;

            // Clear relationships
            p.relations.ClearAllRelations();


            
            if (p.Dead == false)
            {
                // Remove thoughts
                p.needs.mood.thoughts.memories.Memories.Clear();

                // Remove needs
                p.needs.AllNeeds.RemoveAll(n => brainRelatedNeeds.Contains(n.def));

                // Remove disabled work types
                p.Notify_DisabledWorkTypesChanged();
            }
        }

        public void CopyReplicantToInnerPawn(Pawn replicant)
        {
            //List<Thought_Situational> replicantSituationalThoughts = (List<Thought_Situational>)cachedThoughtsField.GetValue(replicantThoughts.situational);
            //Log.Message(replicantSituationalThoughts.Count);
            //cachedThoughtsField.SetValue(brainThoughts.situational, replicantSituationalThoughts.ListFullCopy()); // Be aware to not copy Ref to list, but elements of list

            if (replicant.health.Dead == false)
            {
                // Create brain needs
                innerPawn.needs.AllNeeds.Clear();
                foreach (NeedDef needDef in brainRelatedNeeds)
                {
                    Need replicantNeed = replicant.needs.TryGetNeed(needDef);
                    if (replicantNeed != null)
                    {
                        Need brainNeed = innerPawn.needs.TryGetNeed(needDef);
                        if (brainNeed == null)
                        {
                            brainNeed = (Need)Activator.CreateInstance(needDef.needClass, innerPawn);
                            brainNeed.def = needDef;
                            innerPawn.needs.AllNeeds.Add(brainNeed);
                        }

                        brainNeed.CurLevel = replicantNeed.CurLevel;
                    }
                }
                innerPawn.needs.BindDirectNeedFields();

                // Copy brain thoughts
                ThoughtHandler brainThoughts = innerPawn.needs.mood.thoughts;
                ThoughtHandler replicantThoughts = replicant.needs.mood.thoughts;
                brainThoughts.memories.Memories.Clear();
                brainThoughts.memories.Memories.AddRange(replicantThoughts.memories.Memories);
            }            
        }

        public void CopyInnerPawnToBlank(Pawn p)
        {
            p.Name = innerPawn.Name;
            p.ageTracker.AgeChronologicalTicks = innerPawn.ageTracker.AgeChronologicalTicks;

            // Copy story
            Pawn_StoryTracker newStory = new Pawn_StoryTracker(p);
            Pawn_StoryTracker oldStory = p.story;
            Pawn_StoryTracker brainStory = innerPawn.story;

            newStory.bodyType = oldStory.bodyType;
            newStory.headType = oldStory.headType;
            newStory.hairDef = oldStory.hairDef;
            newStory.SkinColorBase = oldStory.SkinColorBase;

            // Add only brain related traits
            newStory.traits.allTraits.Clear();
            newStory.traits.allTraits.AddRange(brainStory.traits.allTraits);

            newStory.AllBackstories.Clear();
            newStory.Childhood = brainStory.Childhood;
            newStory.Adulthood = brainStory.Adulthood;

            p.story = newStory;

            // Copy skills
            foreach (SkillRecord s in p.skills.skills)
            {
                SkillRecord brainSkill = innerPawn.skills.GetSkill(s.def);
                s.Level = brainSkill.levelInt;
                s.passion = brainSkill.passion;
            }

            p.workSettings.Notify_DisabledWorkTypesChanged();
            p.workSettings.Notify_UseWorkPrioritiesChanged();

            p.Notify_DisabledWorkTypesChanged();


            // Copy relations
            p.relations.ClearAllRelations();
            p.relations.DirectRelations.AddRange(innerPawn.relations.DirectRelations);
            p.relations.VirtualRelations.AddRange(innerPawn.relations.VirtualRelations);


            // Copy style
            Pawn_StyleTracker newStyle = new Pawn_StyleTracker(p);
            Pawn_StyleTracker oldStyle = p.style;
            newStyle.beardDef = oldStyle.beardDef;
            newStyle.BodyTattoo = oldStyle.BodyTattoo;
            newStyle.FaceTattoo = oldStyle.FaceTattoo;

            p.style = newStyle;


            // Create brain needs
            // Give a basic pawn Need set
            p.needs.AddOrRemoveNeedsAsAppropriate();
            innerPawn.needs.AddOrRemoveNeedsAsAppropriate();
            foreach (NeedDef needDef in brainRelatedNeeds)
            {
                Need brainNeed = innerPawn.needs.TryGetNeed(needDef);
                if (brainNeed != null)
                {
                    Need blankNeed = p.needs.TryGetNeed(needDef);
                    if (blankNeed == null)
                    {
                        blankNeed = (Need)Activator.CreateInstance(needDef.needClass, p);
                        blankNeed.def = needDef;
                        p.needs.AllNeeds.Add(blankNeed);
                    }

                    blankNeed.CurLevel = brainNeed.CurLevel;
                }
            }
            p.needs.BindDirectNeedFields();
            innerPawn.needs.BindDirectNeedFields();

            // Copy brain thoughts
            ThoughtHandler brainThoughts = innerPawn.needs.mood.thoughts;
            ThoughtHandler blankThoughts = p.needs.mood.thoughts;
            blankThoughts.memories.Memories.Clear();
            blankThoughts.memories.Memories.AddRange(brainThoughts.memories.Memories);
        }

        public void CopyInnerPawnToAutomaton(AstraBrain automatonBrain, HashSet<SkillDef> skillsToCopy)
        {
            Pawn p = automatonBrain.GetPawn();

            p.Name = new NameSingle(innerPawn.Name + "'s Automaton");
            p.ageTracker.AgeChronologicalTicks = innerPawn.ageTracker.AgeChronologicalTicks;

            // Copy story
            Pawn_StoryTracker newStory = new Pawn_StoryTracker(p);
            Pawn_StoryTracker oldStory = p.story;
            Pawn_StoryTracker brainStory = innerPawn.story;

            newStory.bodyType = oldStory.bodyType;
            newStory.headType = oldStory.headType;
            newStory.hairDef = oldStory.hairDef;
            newStory.SkinColorBase = oldStory.SkinColorBase;

            // Remove any traits
            newStory.traits.allTraits.Clear();

            newStory.AllBackstories.Clear();
            newStory.Childhood = brainStory.Childhood;
            newStory.Adulthood = brainStory.Adulthood;

            p.story = newStory;


            // Copy skills and disable works
            if (automatonBrain.availableSkills == null) automatonBrain.availableSkills = new HashSet<SkillDef>();
            automatonBrain.availableSkills.AddRange(skillsToCopy);

            foreach (SkillRecord automatonSkill in p.skills.skills)
            {
                if (skillsToCopy.Contains(automatonSkill.def))
                {
                    SkillRecord brainSkill = innerPawn.skills.GetSkill(automatonSkill.def);
                    automatonSkill.Level = brainSkill.levelInt;
                    automatonSkill.passion = brainSkill.passion;
                }
                else
                {
                    automatonSkill.Level = 0;
                    automatonSkill.passion = Passion.None;
                }
            }

            p.workSettings.Notify_DisabledWorkTypesChanged();
            p.workSettings.Notify_UseWorkPrioritiesChanged();

            p.Notify_DisabledWorkTypesChanged();


            // Clear relationships
            p.relations.ClearAllRelations();


            // Copy style
            Pawn_StyleTracker newStyle = new Pawn_StyleTracker(p);
            Pawn_StyleTracker oldStyle = p.style;
            newStyle.beardDef = oldStyle.beardDef;
            newStyle.BodyTattoo = oldStyle.BodyTattoo;
            newStyle.FaceTattoo = oldStyle.FaceTattoo;

            p.style = newStyle;


            // Create brain needs
            // Give a basic pawn Need set
            p.needs.AddOrRemoveNeedsAsAppropriate();
            innerPawn.needs.AddOrRemoveNeedsAsAppropriate();
            foreach (NeedDef needDef in brainRelatedNeeds)
            {
                Need brainNeed = innerPawn.needs.TryGetNeed(needDef);
                if (brainNeed != null)
                {
                    Need blankNeed = p.needs.TryGetNeed(needDef);
                    if (blankNeed == null)
                    {
                        blankNeed = (Need)Activator.CreateInstance(needDef.needClass, p);
                        blankNeed.def = needDef;
                        p.needs.AllNeeds.Add(blankNeed);
                    }

                    blankNeed.CurLevel = brainNeed.CurLevel;
                }
            }
            p.needs.BindDirectNeedFields();
            innerPawn.needs.BindDirectNeedFields();

            // Copy brain thoughts
            ThoughtHandler brainThoughts = innerPawn.needs.mood.thoughts;
            ThoughtHandler blankThoughts = p.needs.mood.thoughts;
            blankThoughts.memories.Memories.Clear();
            blankThoughts.memories.Memories.AddRange(brainThoughts.memories.Memories);
        }

        public Pawn GetPawn()
        {
            return innerPawn;
        }

        private WorkTags GetAvailableWorksForSkill(SkillDef s)
        {
            WorkTags t = WorkTags.None;

            if (s == SkillDefOf.Melee) t |= WorkTags.Violent | WorkTags.Hunting;
            if (s == SkillDefOf.Shooting) t |= WorkTags.Violent | WorkTags.Hunting | WorkTags.Shooting;
            if (s == SkillDefOf.Construction) return WorkTags.Constructing;
            if (s == SkillDefOf.Mining) return WorkTags.Mining;
            if (s == SkillDefOf.Cooking) return WorkTags.Cooking;
            if (s == SkillDefOf.Plants) return WorkTags.PlantWork;
            if (s == SkillDefOf.Animals) return WorkTags.Animals;
            if (s == SkillDefOf.Crafting) return WorkTags.Crafting;
            if (s == SkillDefOf.Artistic) return WorkTags.Artistic;
            //if (s == SkillDefOf.Medicine) return WorkTags.;
            if (s == SkillDefOf.Social) return WorkTags.Social;
            if (s == SkillDefOf.Intellectual) return WorkTags.Intellectual;

            return t;
        }
    }
}
