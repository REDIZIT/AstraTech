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
        public ThingComp_AstraSkillCard activeSkillCard;

        public Task task;
        public int ticksLeft;

        public enum Task
        {
            None,
            CreateBlank,
            SkillTraining,
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var o in base.GetFloatMenuOptions(selPawn))
            {
                yield return o;
            }

            yield return new FloatMenuOption("Start task: Create blank", () =>
            {
                StartTask_CreateBlank();               
            });

            if (brainInside == null)
            {
                yield return new FloatMenuOption("Start task: Brain edit", () =>
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
            }
            else
            {
                yield return new FloatMenuOption("Extract brain", () =>
                {
                    ThingWithComps_AstraBrain item = (ThingWithComps_AstraBrain)ThingMaker.MakeThing(AstraDefOf.astra_brain);
                    item.brain = brainInside;
                    GenPlace.TryPlaceThing(item, Position, Map, ThingPlaceMode.Near);
                });
            }
        }

        public override string GetInspectString()
        {
            StringBuilder b = new StringBuilder();

            b.Append("Contains: ");
            if (pawnInside != null)
            {
                b.Append(pawnInside.NameFullColored);
            }
            else if (brainInside != null)
            {
                b.Append("Astra Brain (");
                b.Append(brainInside.pawn.NameFullColored);
                b.Append(")");
            }
            else
            {
                b.Append("nothing");
            }

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
                    task = Task.None;

                    Pawn p = CreateBlankWithSocket();
                    GenSpawn.Spawn(p, Position, Map, WipeMode.Vanish);
                }
                else if (task == Task.SkillTraining)
                {
                    task = Task.None;

                    SkillRecord record = brainInside.pawn.skills.GetSkill(activeSkillCard.skillDef);
                    record.Level = activeSkillCard.level;
                    record.passion = activeSkillCard.passion;

                    activeSkillCard = null;
                }
            }
        }

        public void StartTask_CreateBlank()
        {
            task = Task.CreateBlank;
            ticksLeft = GenDate.TicksPerHour * 3;
        }
        public void StartTask_SkillTraining(ThingComp_AstraSkillCard card)
        {
            task = Task.SkillTraining;
            ticksLeft = GenDate.TicksPerHour * card.level;
            activeSkillCard = card;
        }
        public void StopTask_SkillTraining()
        {
            task = Task.None;
            ticksLeft = 0;
            activeSkillCard = null;
        }

        public static Pawn CreateBlankWithSocket(bool debugBrain = false)
        {
            Pawn p = CreateBlank();

            // Add brain socket implant
            HediffDef implantDef = HediffDef.Named("astra_brain_socket");
            BodyPartRecord partToReplace = p.health.hediffSet.GetBrain();

            Hediff_AstraBrainSocket hediff = (Hediff_AstraBrainSocket)HediffMaker.MakeHediff(implantDef, p, partToReplace);
            p.health.AddHediff(hediff);

            if (debugBrain)
            {
                // Insert brain into socket
                p.skills.GetSkill(SkillDefOf.Shooting).Level = 8;

                AstraBrain brain = AstraBrain.CopyBrain(p);
                hediff.InsertBrain(brain);
            }

            return p;
        }
        public static Pawn CreateBlank()
        {
            Pawn p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                PawnKindDefOf.Colonist,
                Faction.OfPlayer,
                forceGenerateNewPawn: true
            ));


            p.Name = new NameSingle("Replicant");
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
