using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public abstract class JobDriver_HaulAndDo : JobDriver
    {
        protected virtual float FinishActionDurationInSeconds => 3;
        protected abstract void FinishAction();


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return 
                pawn.Reserve(TargetA, job, 1, 1, null, errorOnFailed) && 
                pawn.Reserve(TargetB, job, 1, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoA = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedOrNull(TargetIndex.A);
            yield return gotoA;

            job.count = 1;
            Toil carryA = Toils_Haul.StartCarryThing(TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A);
            yield return carryA;

            Toil gotoB = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedOrNull(TargetIndex.B);
            yield return gotoB;


            Toil finishJob = new Toil()
            {
                initAction = null,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = GenTicks.SecondsToTicks(FinishActionDurationInSeconds),
            };
            finishJob.WithProgressBarToilDelay(TargetIndex.B).AddFinishAction(FinishAction);

            yield return finishJob;
        }
    }
}
