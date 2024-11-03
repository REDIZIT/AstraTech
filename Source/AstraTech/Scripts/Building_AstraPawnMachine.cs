using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace AstraTech
{
    public class Building_AstraPawnMachine : Building, IPawnContainer
    {
        public Pawn pawnInside;
        public AstraBrain brainInside;
        public Thing activeSkillCard;
        public SkillDef skillToExtract;

        public Task task;
        public int ticksLeft;

        public enum Task
        {
            None,
            CreateBlank,
            SkillTraining,
            SkillExtracting,
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref pawnInside, nameof(pawnInside));
            Scribe_Deep.Look(ref brainInside, nameof(brainInside));
            Scribe_References.Look(ref activeSkillCard, nameof(activeSkillCard));
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var o in base.GetFloatMenuOptions(selPawn))
            {
                yield return o;
            }

            if (brainInside != null)
            {
                yield return new FloatMenuOption("Extract brain", () =>
                {
                    task = Task.None;
                    ticksLeft = 0;

                    ThingWithComps_AstraBrain item = (ThingWithComps_AstraBrain)ThingMaker.MakeThing(AstraDefOf.astra_brain);
                    item.brain = brainInside;
                    GenPlace.TryPlaceThing(item, Position, Map, ThingPlaceMode.Near);
                    brainInside = null;
                });
            }

            if (task == Task.None)
            {
                yield return new FloatMenuOption("Start task: Blank creation", () =>
                {
                    StartTask_CreateBlank();
                });

                yield return new FloatMenuOption("Start task: Brain development", () =>
                {
                    Find.Targeter.BeginTargeting(new TargetingParameters()
                    {
                        canTargetPawns = false,
                        mapObjectTargetsMustBeAutoAttackable = false,
                        canTargetItems = true,
                        validator = (i) => i.Thing is ThingWithComps_AstraBrain
                    }, (i) =>
                    {
                        brainInside = ((ThingWithComps_AstraBrain)i.Thing).brain;
                        i.Thing.Destroy();
                    });
                });

                yield return new FloatMenuOption("Start task: Skill extraction", () =>
                {
                    Find.Targeter.BeginTargeting(new TargetingParameters()
                    {
                        canTargetPawns = true,
                        mapObjectTargetsMustBeAutoAttackable = false,
                        canTargetItems = false,
                        onlyTargetControlledPawns = true,
                    }, (i) =>
                    {
                        FloatMenuUtility.MakeMenu(EnumerateSkillsForExtraction((Pawn)i), (f) => f.Label, (f) => f.action);
                        
                    });
                });
            }
            else if (task == Task.SkillTraining)
            {
                
            }
            else if (task == Task.SkillExtracting)
            {
                yield return new FloatMenuOption("Stop skill extraction", () =>
                {
                    StopTask_SkillExtraction();
                });
            }
        }

        public override string GetInspectString()
        {
            StringBuilder b = new StringBuilder();

            b.Append("Task: ");
            b.Append(task.ToString());
            if (task != Task.None)
            {
                b.Append(" (time left: ");
                b.Append(GenDate.ToStringTicksToPeriod(ticksLeft));
                b.Append(")");
            }

            b.AppendLine();
            b.Append("Contains: ");
            if (pawnInside != null)
            {
                b.Append(pawnInside.NameFullColored);
            }
            else if (brainInside != null)
            {
                b.Append("Astra Brain (");
                b.Append(brainInside.innerPawn.NameFullColored);
                b.Append(")");
            }
            else
            {
                b.Append("nothing");
            }

            b.Append(GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading.Count.ToString());

            return b.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (DebugSettings.godMode)
            {
                if (task != Task.None && ticksLeft != 0)
                {
                    yield return new Command_Action()
                    {
                        defaultLabel = "Dev: Complete task",
                        action = () => ticksLeft = 0
                    };
                }
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (ticksLeft > 0)
            {
                ticksLeft--;
            }
            else
            {
                if (task == Task.CreateBlank)
                {
                    Pawn p = CreateBlankWithSocket();
                    GenSpawn.Spawn(p, Position, Map, WipeMode.Vanish);
                }
                else if (task == Task.SkillTraining)
                {
                    ThingComp_AstraSkillCard skillCard = activeSkillCard.TryGetComp<ThingComp_AstraSkillCard>();

                    SkillRecord record = brainInside.innerPawn.skills.GetSkill(skillCard.skillDef);
                    record.Level = skillCard.level;
                    record.passion = skillCard.passion;

                    activeSkillCard = null;
                }
                else if (task == Task.SkillExtracting)
                {
                    SkillRecord record = pawnInside.skills.GetSkill(skillToExtract);
                    
                    Thing item = ThingMaker.MakeThing(AstraDefOf.astra_skill_card);
                    var comp = item.TryGetComp<ThingComp_AstraSkillCard>();
                    comp.skillDef = record.def;
                    comp.level = record.levelInt;
                    comp.passion = record.passion;

                    GenPlace.TryPlaceThing(item, Position, Map, ThingPlaceMode.Near);

                    GenPlace.TryPlaceThing(pawnInside, Position, Map, ThingPlaceMode.Near);
                    BodyPartRecord brain = pawnInside.health.hediffSet.GetBrain();
                    pawnInside.health.AddHediff(HediffDefOf.MissingBodyPart, brain);
                    pawnInside = null;
                    skillToExtract = null;
                }

                task = Task.None;
            }
        }

        private IEnumerable<FloatMenuOption> EnumerateSkillsForExtraction(Pawn pawn)
        {
            foreach (SkillRecord skill in pawn.skills.skills)
            {
                yield return new FloatMenuOption(skill.def.LabelCap + " - " + skill.levelInt, () =>
                {
                    StartTask_SkillExtraction(pawn, skill.def);
                });
            }
        }

        public void StartTask_CreateBlank()
        {
            task = Task.CreateBlank;
            ticksLeft = GenDate.TicksPerHour * 3;
        }

        public void StartTask_SkillTraining(Thing card)
        {
            task = Task.SkillTraining;
            ticksLeft = GenDate.TicksPerHour * card.TryGetComp<ThingComp_AstraSkillCard>().level;
            activeSkillCard = card;
        }
        public void StopTask_SkillTraining()
        {
            task = Task.None;
            ticksLeft = 0;
            activeSkillCard = null;
        }

        public void StartTask_SkillExtraction(Pawn victim, SkillDef skillToExtract)
        {
            task = Task.SkillExtracting;
            ticksLeft = GenDate.TicksPerHour * 1;

            this.pawnInside = victim;
            victim.DeSpawn();

            this.skillToExtract = skillToExtract;
        }
        public void StopTask_SkillExtraction()
        {
            task = Task.None;

            GenPlace.TryPlaceThing(pawnInside, Position, Map, ThingPlaceMode.Near);
            pawnInside = null;

            skillToExtract = null;
        }



        public static Pawn CreateBlankWithSocket()
        {
            Pawn p = CreateBlank();

            // Add brain socket implant
            HediffDef implantDef = HediffDef.Named("astra_brain_socket");
            BodyPartRecord partToReplace = p.health.hediffSet.GetBrain();

            Hediff_AstraBrainSocket hediff = (Hediff_AstraBrainSocket)HediffMaker.MakeHediff(implantDef, p, partToReplace);
            p.health.AddHediff(hediff);

            return p;
        }
        public static Pawn CreateBlank()
        {
            Pawn p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                PawnKindDefOf.Colonist,
                Faction.OfPlayer,
                forceGenerateNewPawn: true
            ));


            p.Name = new NameSingle("Blank");
            p.gender = Gender.None;

            p.ageTracker.AgeBiologicalTicks = GenDate.TicksPerYear * 20;
            p.ageTracker.AgeChronologicalTicks = 0;

            p.health.Reset();
                        

            p.story = new Pawn_StoryTracker(p);
            p.story.bodyType = BodyTypeDefOf.Male;
            p.story.headType = DefDatabase<HeadTypeDef>.GetNamed("Male_AverageNormal");
            p.story.hairDef = HairDefOf.Bald;
            p.story.SkinColorBase = PawnSkinColors.GetSkinColor(0);

            p.story.traits.allTraits.Clear();
            p.story.AllBackstories.Clear();

            p.skills.Notify_SkillDisablesChanged();
            p.skills.DirtyAptitudes();


            foreach (SkillRecord s in p.skills.skills)
            {
                s.Level = 0;
                s.passion = Passion.None;
            }

            p.workSettings.Notify_DisabledWorkTypesChanged();
            p.workSettings.Notify_UseWorkPrioritiesChanged();
            
            p.Notify_DisabledWorkTypesChanged();


            p.relations.ClearAllRelations();


            p.style.beardDef = BeardDefOf.NoBeard;
            p.style.BodyTattoo = TattooDefOf.NoTattoo_Body;
            p.style.FaceTattoo = TattooDefOf.NoTattoo_Face;

            p.inventory.DestroyAll();
            p.apparel.DestroyAll();
            p.equipment.DestroyAllEquipment();


            AstraBrain.ClearPawn(p);

            return p;
        }

        public Pawn GetPawn()
        {
            if (pawnInside != null) return pawnInside;
            if (brainInside != null) return brainInside.GetPawn();
            return null;
        }
    }
}
