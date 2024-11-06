using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace AstraTech
{
    public class Building_AstraPawnMachine : Building, IPawnContainer
    {
        public Pawn pawnInside;
        public AstraBrain brainInside;
        public SkillDef skillToExtract;
        public TraitDef traitToExtract;

        public Building_AstraSchematicsBank Bank
        {
            get
            {
                return _bank;
            }
            set
            {
                if (_bank != null)
                {
                    _bank.onSchematicsDrop -= OnSchematicsBankDrop;
                }
                _bank = value;
                _bank.onSchematicsDrop += OnSchematicsBankDrop;
            }
        }
        public AstraSchematics schematicsInsideBank;

        public Task task;
        public int ticksLeft;

        private Building_AstraSchematicsBank _bank;

        public enum Task
        {
            None,
            CreateBlank,
            SkillTraining,
            SkillExtracting,
            TraitTraining,
            TraitExtracting,
            BrainToBrainCopy
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (_bank == null && (task == Task.SkillTraining || task == Task.TraitTraining))
            {
                _bank = (Building_AstraSchematicsBank)schematicsInsideBank.holdingOwner.Owner;
            }

            if (_bank != null)
            {
                _bank.onSchematicsDrop += OnSchematicsBankDrop;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref pawnInside, nameof(pawnInside));
            Scribe_Deep.Look(ref brainInside, nameof(brainInside));
            Scribe_References.Look(ref schematicsInsideBank, nameof(schematicsInsideBank));
            Scribe_Defs.Look(ref skillToExtract, nameof(skillToExtract));
            Scribe_Defs.Look(ref traitToExtract, nameof(traitToExtract));

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
                    GenJob.TryGiveJob<JobDriver_ExtractBrain>(selPawn, this);
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
                        GenJob.TryGiveJob<JobDriver_StartBlankCreation>(selPawn, this);
                    });

                    yield return new FloatMenuOption("Start task: Brain development", () =>
                    {
                        Find.Targeter.BeginTargeting(new TargetingParameters()
                        {
                            canTargetPawns = false,
                            mapObjectTargetsMustBeAutoAttackable = false,
                            canTargetItems = true,
                            canTargetBuildings = false,
                            validator = (i) => i.Thing is AstraBrain
                        }, (i) =>
                        {
                            GenJob.TryGiveJob<JobDriver_InsertBrainIntoMachine>(selPawn, i.Thing, this);
                        });
                    });

                    TargetingParameters pawnTargets = new TargetingParameters()
                    {
                        canTargetPawns = true,
                        mapObjectTargetsMustBeAutoAttackable = false,
                        canTargetItems = false,
                        onlyTargetColonistsOrPrisonersOrSlaves = true,
                        canTargetBuildings = false,
                    };

                    yield return new FloatMenuOption("Start task: Trait extraction", () =>
                    {
                        traitToExtract = null;
                        skillToExtract = null;

                        Find.Targeter.BeginTargeting(pawnTargets, (i) =>
                        {
                            Pawn victim = (Pawn)i;
                            if (victim.health.hediffSet.hediffs.Any(h => h.Bleeding))
                            {
                                Messages.Message("Target pawn is bleeding", MessageTypeDefOf.RejectInput);
                            }
                            else
                            {
                                FloatMenuUtility.MakeMenu(EnumerateTraitsForExtraction(selPawn, victim), (f) => f.Label, (f) => f.action);
                            }

                        });
                    });

                    yield return new FloatMenuOption("Start task: Skill extraction", () =>
                    {
                        traitToExtract = null;
                        skillToExtract = null;

                        Find.Targeter.BeginTargeting(pawnTargets, (i) =>
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
                            validator = (i) => i.Thing is AstraBrain
                        };

                        Find.Targeter.BeginTargeting(parameters, (i1) =>
                        {
                            Find.Targeter.BeginTargeting(parameters, (i2) =>
                            {
                                AstraBrain origin = ((AstraBrain)i1.Thing);
                                AstraBrain target = ((AstraBrain)i2.Thing);

                                origin.CopyInnerPawnToBlank(target.innerPawn);
                            });
                        });
                    });
                }
            }
            else if (task == Task.SkillExtracting || task == Task.TraitExtracting)
            {
                yield return new FloatMenuOption("Stop extraction", () =>
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
                        action = () =>
                        {
                            ticksLeft = 0;
                            Tick();
                        }
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

                    Messages.Message("Blank creation is completed", this, MessageTypeDefOf.TaskCompletion);
                }
                else if (task == Task.SkillTraining)
                {
                    AstraSchematics_Skill skillSchematics = (AstraSchematics_Skill)schematicsInsideBank;

                    SkillRecord record = brainInside.innerPawn.skills.GetSkill(skillSchematics.skillDef);
                    record.Level = skillSchematics.level;
                    record.passion = skillSchematics.passion;

                    schematicsInsideBank = null;

                    Messages.Message("Skill training is completed", this, MessageTypeDefOf.TaskCompletion);
                }
                else if (task == Task.SkillExtracting)
                {
                    SkillRecord record = pawnInside.skills.GetSkill(skillToExtract);

                    AstraSchematics_Skill schematic = (AstraSchematics_Skill)ThingMaker.MakeThing(AstraDefOf.astra_schematics_skill);
                    schematic.skillDef = record.def;
                    schematic.level = record.levelInt;
                    schematic.passion = record.passion;

                    GenPlace.TryPlaceThing(schematic, Position, Map, ThingPlaceMode.Near);
                    skillToExtract = null;

                    KillAndDropPawn();

                    Messages.Message("Skill extraction is completed", this, MessageTypeDefOf.TaskCompletion);
                }
                else if (task == Task.TraitTraining)
                {
                    Log.Message("TraitTraining completed");
                    Log.Message("schematicsInsideBank = " + schematicsInsideBank);
                    AstraSchematics_Trait traitSchematics = (AstraSchematics_Trait)schematicsInsideBank;
                    Log.Message("traitSchematics = " + traitSchematics);

                    brainInside.innerPawn.story.traits.GainTrait(new Trait(traitSchematics.traitDef, traitSchematics.degree));

                    schematicsInsideBank = null;

                    Messages.Message("Trait training is completed", this, MessageTypeDefOf.TaskCompletion);
                }
                else if (task == Task.TraitExtracting)
                {
                    Trait trait = pawnInside.story.traits.GetTrait(traitToExtract);

                    AstraSchematics_Trait schematic = (AstraSchematics_Trait)ThingMaker.MakeThing(AstraDefOf.astra_schematics_trait);
                    schematic.traitDef = trait.def;
                    schematic.degree = trait.Degree;

                    GenPlace.TryPlaceThing(schematic, Position, Map, ThingPlaceMode.Near);
                    traitToExtract = null;

                    KillAndDropPawn();

                    Messages.Message("Trait extraction is completed", this, MessageTypeDefOf.TaskCompletion);
                }

                task = Task.None;
            }
        }


        public void StartTask_CreateBlank()
        {
            task = Task.CreateBlank;
            ticksLeft = GenDate.TicksPerHour * 3;
        }

        public void StartTask_SkillTraining(AstraSchematics_Skill card)
        {
            task = Task.SkillTraining;
            ticksLeft = GenDate.TicksPerHour * (card).level;
            schematicsInsideBank = card;
            Bank = (Building_AstraSchematicsBank)card.holdingOwner.Owner;
        }
        public void StartTask_TraitTraining(AstraSchematics_Trait card)
        {
            task = Task.TraitTraining;
            ticksLeft = GenDate.TicksPerHour * 12;
            schematicsInsideBank = card;
            Bank = (Building_AstraSchematicsBank)card.holdingOwner.Owner;
        }
        public void StopTask_Training()
        {
            task = Task.None;
            ticksLeft = 0;
            schematicsInsideBank = null;
        }

        public void StartTask_Extraction(Pawn victim)
        {
            if (skillToExtract != null)
            {
                task = Task.SkillExtracting;
                ticksLeft = GenDate.TicksPerHour * 6;
            }
            else if (traitToExtract != null)
            {
                task = Task.TraitExtracting;
                ticksLeft = GenDate.TicksPerHour * 10;
            }
            
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

        public void InsertBrain(AstraBrain item)
        {
            brainInside = item;
            item.DeSpawn();
        }
        public Thing ExtractBrain()
        {
            task = Task.None;
            ticksLeft = 0;

            AstraBrain temp = brainInside;
            GenPlace.TryPlaceThing(brainInside, Position, Map, ThingPlaceMode.Near);
            brainInside = null;

            return temp;
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
                            GenJob.TryGiveJob<JobDriver_EnterToExtraction>(selPawn, pawn, this);
                        }
                        else
                        {
                            GenJob.TryGiveJob<JobDriver_CarryPawnToExtraction>(selPawn, pawn, this);
                        }
                    });
                });
            }
        }
        private IEnumerable<FloatMenuOption> EnumerateTraitsForExtraction(Pawn selPawn, Pawn pawn)
        {
            foreach (Trait trait in pawn.story.traits.allTraits)
            {
                FloatMenuOption option =  new FloatMenuOption(trait.LabelCap, () =>
                {
                    traitToExtract = trait.def;

                    MessageHelper.ShowCustomMessage(pawn, () =>
                    {
                        if (selPawn == pawn)
                        {
                            GenJob.TryGiveJob<JobDriver_EnterToExtraction>(selPawn, pawn, this);
                        }
                        else
                        {
                            GenJob.TryGiveJob<JobDriver_CarryPawnToExtraction>(selPawn, pawn, this);
                        }
                    });
                });

                if (AstraBrain.bodyRelatedTraits.Contains(trait.def))
                {
                    option.Disabled = true;
                    option.Label = "Not extractable: " + option.Label;
                }

                yield return option;
            }
        }

        private void KillAndDropPawn()
        {
            GenPlace.TryPlaceThing(pawnInside, Position, Map, ThingPlaceMode.Near);
            pawnInside.Position = Position;
            BodyPartRecord brain = pawnInside.health.hediffSet.GetBrain();
            pawnInside.health.AddHediff(HediffDefOf.MissingBodyPart, brain);
            pawnInside = null;
        }

        private void OnSchematicsBankDrop(AstraSchematics item)
        {
            if (schematicsInsideBank == item)
            {
                if (task == Task.SkillTraining)
                {
                    Messages.Message("Failed to train skill: Skill Schematics not available now", this, MessageTypeDefOf.NegativeEvent);
                    StopTask_Training();
                }
                else if (task == Task.TraitTraining)
                {
                    Messages.Message("Failed to train trait: Trait Schematics not available now", this, MessageTypeDefOf.NegativeEvent);
                    StopTask_Training();
                }                
            }
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
