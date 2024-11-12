using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace AstraTech
{
    public class AstraBrain : ThingWithComps, IPawnContainer
    {
        public override string Label => "Astra Brain (" + GetPawn().NameFullColored + ")";
        public bool IsAutomaton => Def.unstableWorktimeInDays > 0;
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

            if (IsAutomaton && unstableWorktimeInTicksLeft == -1)
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
            if (IsAutomaton)
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


        /// <summary>
        /// Resets pawn or dead pawn to a Blank (skills, traits, story, relates, thoughts, needs, Ideo) <br/>
        /// Use this if you want:<br/>
        /// 1. Extract Brain from pawn (but after <see cref="CopyReplicantToInnerPawn"/>)<br/>
        /// 2. Reset pawn without Brain extraction, just make it Blank<br/>
        /// </summary>
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

                // Remove Ideo
                RemoveIdeoAndPsy(p);
            }
        }

        /// <summary>
        /// Copy brain needs, thoughts and Ideo from Replicant to Brain's inner pawn.<br/>
        /// Use this if you want:<br/>
        /// 1. Extract Brain from pawn (but before <see cref="ClearPawn"/>)<br/>
        /// </summary>
        public void CopyReplicantToInnerPawn(Pawn replicant)
        {
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
                CopyThoughts_InnerToBlank(replicant, innerPawn, IsAutomaton);

                // Copy Ideology
                CopyIdeoAndPsy(replicant, innerPawn);
            }            
        }

        /// <summary>
        /// Copy story, traits, skills, relates, needs, thoughts and Ideo from Brain's inner pawn to <paramref name="p"/><br/>
        /// Use this if you want:<br/>
        /// 1. Insert Brain into pawn (note: this method only copy - Socket insertion, Item despawning and etc are up to you)
        /// </summary>
        public void CopyInnerPawnToBlank(Pawn p)
        {
            p.Name = innerPawn.Name;
            p.ageTracker.AgeChronologicalTicks = innerPawn.ageTracker.AgeChronologicalTicks;

            ConvertToColonistIfNot(p);

            // Copy story
            CopyStory(p);

            // Add only brain related traits
            CopyTraits(p, true);

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

            //// Copy style (is it really brain stuff??)
            //CopyStyle(p);


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
            CopyThoughts_InnerToBlank(innerPawn, p, IsAutomaton);

            // Copy Ideology
            CopyIdeoAndPsy(innerPawn, p);
        }


        /// <summary>
        /// Copy Brain's inner pawn into another Brain's inner pawn but asuming that it is Automaton (Automatons have some limitaions)
        /// Use this if you want:<br/>
        /// 1. Brain-to-Brain copy from full-filled Brain into limited Automaton Brain
        /// </summary>
        public void CopyInnerPawnToAutomaton(AstraBrain automatonBrain, HashSet<SkillDef> skillsToCopy)
        {
            Pawn p = automatonBrain.GetPawn();

            p.Name = new NameSingle(innerPawn.Name + "'s Automaton");
            p.ageTracker.AgeChronologicalTicks = innerPawn.ageTracker.AgeChronologicalTicks;

            // Copy story
            CopyStory(p);

            // Remove any traits
            CopyTraits(p, false);
            
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

            //// Copy style (is it really brain stuff??)
            //CopyStyle(p);
            

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
            CopyThoughts_InnerToBlank(innerPawn, p, IsAutomaton);

            CopyIdeoAndPsy(innerPawn, p);
        }

        public Pawn GetPawn()
        {
            return innerPawn;
        }

        private void CopyStory(Pawn p)
        {
            Pawn_StoryTracker newStory = new Pawn_StoryTracker(p);
            Pawn_StoryTracker oldStory = p.story;
            
            newStory.bodyType = oldStory.bodyType;
            newStory.headType = oldStory.headType;
            newStory.hairDef = oldStory.hairDef;
            newStory.furDef = oldStory.furDef;
            newStory.SkinColorBase = oldStory.SkinColorBase;
            newStory.HairColor = oldStory.HairColor;

            p.story = newStory;
        }
        private void CopyTraits(Pawn p, bool addBrainTraits)
        {
            Pawn_StoryTracker newStory = p.story;
            Pawn_StoryTracker brainStory = innerPawn.story;

            newStory.traits.allTraits.Clear();
            if (addBrainTraits) newStory.traits.allTraits.AddRange(brainStory.traits.allTraits);

            newStory.AllBackstories.Clear();
            newStory.Childhood = brainStory.Childhood;
            newStory.Adulthood = brainStory.Adulthood;
        }
        //private void CopyStyle(Pawn p)
        //{
        //    Pawn_StyleTracker newStyle = new Pawn_StyleTracker(p);
        //    Pawn_StyleTracker oldStyle = p.style;
        //    newStyle.beardDef = oldStyle.beardDef;
        //    newStyle.BodyTattoo = oldStyle.BodyTattoo;
        //    newStyle.FaceTattoo = oldStyle.FaceTattoo;

            //Pawn_StyleTracker newStyle = new Pawn_StyleTracker(p);
            //Pawn_StyleTracker oldStyle = p.style;
            //newStyle.beardDef = oldStyle.beardDef;
            //newStyle.BodyTattoo = oldStyle.BodyTattoo;
            //newStyle.FaceTattoo = oldStyle.FaceTattoo;
            //newStyle.nextBodyTatooDef = oldStyle.nextBodyTatooDef;
            //newStyle.nextFaceTattooDef = oldStyle.nextFaceTattooDef;
            //newStyle.nextHairDef = oldStyle.nextHairDef;
            //newStyle.nextHairColor = oldStyle.nextHairColor;
            //newStyle.nextStyleChangeAttemptTick = oldStyle.nextStyleChangeAttemptTick;

        //    p.style = newStyle;
        //}
        private void CopyThoughts_InnerToBlank(Pawn source, Pawn target, bool justClear)
        {
            ThoughtHandler sourceThoughts = source.needs.mood.thoughts;
            ThoughtHandler targetThoughts = target.needs.mood.thoughts;
            targetThoughts.memories.Memories.Clear();
            if (justClear == false) targetThoughts.memories.Memories.AddRange(sourceThoughts.memories.Memories);
        }
        private void CopyIdeoAndPsy(Pawn source, Pawn target)
        {
            if (ModsConfig.IdeologyActive)
            {
                target.ideo.SetIdeo(source.Ideo);
            }
            if (ModsConfig.RoyaltyActive)
            {
                if (IsAutomaton)
                {
                    //
                    // Remove target Psylinks coz Automaton can not use Psycasts
                    //
                    if (target.HasPsylink)
                    {
                        // Vanilla Psylink
                        target.health.RemoveHediff(target.psychicEntropy.Psylink);

                        // Vanilla Psycasts Expanded Psylink
                        if (ModCompatibility.VPEIsActive)
                        {
                            ModCompatibility.RemoveVPE(target);
                        }
                    }
                }
                else if (source.HasPsylink)
                {
                    //
                    // Copy Psylinks (Persona module insertion)
                    //
                    BodyPartRecord brain = target.health.hediffSet.GetBrain();

                    // Vanilla Psylink
                    Hediff_Psylink targetPsylink;
                    if (target.HasPsylink == false)
                    {
                        targetPsylink = (Hediff_Psylink)target.health.AddHediff(DefDatabase<HediffDef>.GetNamed("PsychicAmplifier"), brain);
                    }
                    else
                    {
                        targetPsylink = target.psychicEntropy.Psylink;
                    }
                    targetPsylink.CopyFrom(source.psychicEntropy.Psylink);
                    target.abilities.abilities.Clear();
                    foreach (Ability ability in source.abilities.abilities)
                    {
                        target.abilities.GainAbility(ability.def);
                    }


                    // Vanilla Psycasts Expanded Psylink
                    if (ModCompatibility.VPEIsActive)
                    {
                        ModCompatibility.CopyVPE(source, target);
                    }
                }
                
            }
        }
        private static void RemoveIdeoAndPsy(Pawn target)
        {
            if (ModsConfig.IdeologyActive)
            {
                target.ideo.SetIdeo(null);
            }
            if (ModsConfig.RoyaltyActive)
            {
                if (target.HasPsylink)
                {
                    // Vanilla Psycasts Expanded Psylink
                    if (ModCompatibility.VPEIsActive)
                    {
                        ModCompatibility.RemoveVPE(target);
                    }

                    // Vanilla Psylink
                    Hediff_Psylink hediff = target.psychicEntropy.Psylink;
                    target.health.RemoveHediff(hediff);
                }
            }
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

        private void ConvertToColonistIfNot(Pawn enemy)
        {
            if (enemy.Faction == Faction.OfPlayer) return;
            RecruitUtility.Recruit(enemy, Faction.OfPlayer);
        }
    }
}
