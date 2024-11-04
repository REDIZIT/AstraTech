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
        public AstraBrain brainInside;/*, secondBrainInside;*/
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
            BrainToBrainCopy
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref pawnInside, nameof(pawnInside));
            Scribe_Deep.Look(ref brainInside, nameof(brainInside));
            //Scribe_Deep.Look(ref secondBrainInside, nameof(secondBrainInside));
            Scribe_Deep.Look(ref activeSkillCard, nameof(activeSkillCard));
            Scribe_Defs.Look(ref skillToExtract, nameof(skillToExtract));

            Scribe_Values.Look(ref task, nameof(task));
            Scribe_Values.Look(ref ticksLeft, nameof(ticksLeft));
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
                if (Map.reservationManager.IsReserved(this))
                {
                    var o = new FloatMenuOption("Not available: Someone is already using this machine", null);
                    o.Disabled = true;
                    yield return o;
                }
                else
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
                            canTargetBuildings = false,
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
                            onlyTargetColonistsOrPrisonersOrSlaves = true,
                            canTargetBuildings = false,
                        }, (i) =>
                        {
                            Pawn victim = (Pawn)i;
                            if (victim.health.hediffSet.hediffs.Any(h => h.Bleeding))
                            {
                                Messages.Message("Target pawn is bleeding", MessageTypeDefOf.RejectInput);
                            }
                            else
                            {
                                FloatMenuUtility.MakeMenu(EnumerateSkillsForExtraction(selPawn, victim), (f) => f.Label, (f) => f.action);
                            }

                        });
                    });

                    yield return new FloatMenuOption("Start task: Brain to Brain copy", () =>
                    {
                        TargetingParameters parameters = new TargetingParameters()
                        {
                            canTargetPawns = false,
                            mapObjectTargetsMustBeAutoAttackable = false,
                            canTargetItems = true,
                            validator = (i) => i.Thing is ThingWithComps_AstraBrain
                        };

                        Find.Targeter.BeginTargeting(parameters, (i1) =>
                        {
                            Find.Targeter.BeginTargeting(parameters, (i2) =>
                            {
                                AstraBrain origin = ((ThingWithComps_AstraBrain)i1.Thing).brain;
                                AstraBrain target = ((ThingWithComps_AstraBrain)i2.Thing).brain;

                                origin.CopyInnerPawnToBlank(target.innerPawn);
                            });
                        });
                    });
                }
            }
            else if (task == Task.SkillTraining)
            {
                
            }
            else if (task == Task.SkillExtracting)
            {
                yield return new FloatMenuOption("Stop skill extraction", () =>
                {
                    GenJob.TryGiveJob<JobDriver_StopSkillExtraction>(selPawn, this);
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

            //b.Append(GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading.Count.ToString());

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

                    Messages.Message("Blank creation is completed", MessageTypeDefOf.TaskCompletion);
                }
                else if (task == Task.SkillTraining)
                {
                    AstraSchematics_Skill skillCard = (AstraSchematics_Skill)activeSkillCard;

                    SkillRecord record = brainInside.innerPawn.skills.GetSkill(skillCard.skillDef);
                    record.Level = skillCard.level;
                    record.passion = skillCard.passion;

                    activeSkillCard = null;

                    Messages.Message("Skill training is completed", MessageTypeDefOf.TaskCompletion);
                }
                else if (task == Task.SkillExtracting)
                {
                    SkillRecord record = pawnInside.skills.GetSkill(skillToExtract);
                    
                    Thing item = ThingMaker.MakeThing(AstraDefOf.astra_schematics_skill);
                    var comp = (AstraSchematics_Skill)item;
                    comp.skillDef = record.def;
                    comp.level = record.levelInt;
                    comp.passion = record.passion;

                    GenPlace.TryPlaceThing(item, Position, Map, ThingPlaceMode.Near);

                    GenPlace.TryPlaceThing(pawnInside, Position, Map, ThingPlaceMode.Near);
                    pawnInside.Position = Position;
                    BodyPartRecord brain = pawnInside.health.hediffSet.GetBrain();
                    pawnInside.health.AddHediff(HediffDefOf.MissingBodyPart, brain);
                    pawnInside = null;
                    skillToExtract = null;

                    Messages.Message("Skill extraction is completed", MessageTypeDefOf.TaskCompletion);
                }

                task = Task.None;
            }
        }

        private IEnumerable<FloatMenuOption> EnumerateSkillsForExtraction(Pawn selPawn, Pawn pawn)
        {
            foreach (SkillRecord skill in pawn.skills.skills)
            {
                yield return new FloatMenuOption(skill.def.LabelCap + ", " + skill.levelInt + ", " + skill.passion.GetLabel(), () =>
                {
                    skillToExtract = skill.def;

                    MessageHelper.ShowCustomMessage(pawn, () =>
                    {
                        if (selPawn == pawn)
                        {
                            GenJob.TryGiveJob<JobDriver_EnterToSkillExtraction>(selPawn, pawn, this);
                        }
                        else
                        {
                            GenJob.TryGiveJob<JobDriver_CarryPawnToSkillExtraction>(selPawn, pawn, this);
                        }
                    });         
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
            ticksLeft = GenDate.TicksPerHour * ((AstraSchematics_Skill)card).level;
            activeSkillCard = card;
        }
        public void StopTask_SkillTraining()
        {
            task = Task.None;
            ticksLeft = 0;
            activeSkillCard = null;
        }

        public void StartTask_SkillExtraction(Pawn victim)
        {
            task = Task.SkillExtracting;
            ticksLeft = GenDate.TicksPerHour * 6;

            pawnInside = victim;
            victim.DeSpawn();
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
