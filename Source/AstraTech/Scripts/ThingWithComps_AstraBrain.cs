using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AstraTech
{
    public class ThingWithComps_AstraBrain : ThingWithComps, IPawnContainer
    {
        public AstraBrain brain;

        public Pawn GetPawn()
        {
            return brain.GetPawn();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (brain == null) brain = new AstraBrain(Building_AstraPawnMachine.CreateBlank());
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var o in base.GetFloatMenuOptions(selPawn))
            {
                yield return o;
            }

            yield return new FloatMenuOption("Insert into ...", () =>
            {
                Find.Targeter.BeginTargeting(new TargetingParameters()
                {
                    canTargetPawns = true,
                    onlyTargetControlledPawns = true,
                    mapObjectTargetsMustBeAutoAttackable = false,
                    canTargetItems = false,
                    validator = (i) => i.Thing is Pawn pawn && pawn.health.hediffSet.HasHediff<Hediff_AstraBrainSocket>()
                }, (i) =>
                {
                    Pawn pawn = (Pawn)i.Thing;
                    pawn.health.hediffSet.GetFirstHediff<Hediff_AstraBrainSocket>().InsertBrain(brain);
                    Destroy();
                });
            });
        }
    }
}
