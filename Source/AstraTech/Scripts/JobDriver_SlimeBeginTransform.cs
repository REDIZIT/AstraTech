using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class JobDriver_SlimeBeginTransform : JobDriver
    {
        private Thing inactiveSlime => TargetA.Thing;
        private Thing targetItem => TargetB.Thing;

        

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(inactiveSlime, job, 1, 1, null, errorOnFailed) &&
                   pawn.Reserve(targetItem, job, 1, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoBlueprint = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return gotoBlueprint;


            job.count = 1; // Without this throwing warning about -1 default count. May be this is due to reservation -1 stackCount??
            var carryToil = Toils_Haul.StartCarryThing(TargetIndex.A);
            carryToil.AddFinishAction(() =>
            {
                HistoryEvent historyEvent = new HistoryEvent(AstraDefOf.astra_touched_slime, pawn.Named(HistoryEventArgsNames.Doer));
                Find.HistoryEventsManager.RecordEvent(historyEvent);
            });
            yield return carryToil;


            Toil gotoTarget = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return gotoTarget;


            Toil useItem = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = GenTicks.SecondsToTicks(5),
            };
            useItem.WithProgressBarToilDelay(TargetIndex.B).AddFinishAction(EncodeAction);
            yield return useItem;
        }

        private void EncodeAction()
        {
            var i = inactiveSlime.TryGetComp<ThingComp_AstraSlime>();
            i.StartTransforming(targetItem);
        }
    }
}