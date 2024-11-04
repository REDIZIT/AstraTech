using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AstraTech
{
    public class ThingWithComps_AstraBrain : ThingWithComps, IPawnContainer
    {
        public AstraBrain brain;
        public override string Label => "Astra Brain (" + brain.GetPawn().NameFullColored + ")";

        public Pawn GetPawn()
        {
            return brain.GetPawn();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref brain, nameof(brain));
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            // If just crafter or spawned via DevTools
            if (brain == null)
            {
                brain = new AstraBrain(Building_AstraPawnMachine.CreateBlank());
                brain.GetPawn().Name = new NameSingle("Unnamed");
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var o in base.GetFloatMenuOptions(selPawn))
            {
                yield return o;
            }

            yield return new FloatMenuOption("Insert into blank ..", () =>
            {
                Find.Targeter.BeginTargeting(new TargetingParameters()
                {
                    canTargetPawns = true,
                    onlyTargetControlledPawns = true,
                    mapObjectTargetsMustBeAutoAttackable = false,
                    canTargetItems = false,
                    validator = (i) => i.Thing is Pawn pawn && pawn.health.hediffSet.TryGetHediff(out Hediff_AstraBrainSocket socket) && socket.brain == null
                }, (i) =>
                {
                    Pawn pawn = (Pawn)i.Thing;
                    GenJob.TryGiveJob<JobDriver_InsertBrainIntoBlank>(selPawn, this, pawn);
                });
            });
        }
    }
}
