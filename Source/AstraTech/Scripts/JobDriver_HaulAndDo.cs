using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public abstract class JobDriver_Base : JobDriver
    {
        protected virtual float FinishActionDurationInSeconds => 3;
        protected abstract void FinishAction();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(TargetA, job, 1, 1, null, errorOnFailed) == false) return false;

            if (TargetB.IsValid)
            {
                if (pawn.Reserve(TargetB, job, 1, 1, null, errorOnFailed) == false) return false;
            }

            return true;
        }

        protected T CastA<T>() where T : Thing
        {
            return Cast<T>(TargetIndex.A);
        }
        protected T CastB<T>() where T : Thing
        {
            return Cast<T>(TargetIndex.B);
        }
        protected T Cast<T>(TargetIndex index) where T : Thing
        {
            Thing targetThing = job.GetTarget(index).Thing;
            return (T)targetThing;
        }

        protected Toil GetFinishToil()
        {
            Toil finishToil = new Toil()
            {
                initAction = null,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = GenTicks.SecondsToTicks(FinishActionDurationInSeconds),
            };
            finishToil.WithProgressBarToilDelay(TargetIndex.B).AddFinishAction(FinishAction);

            return finishToil;
        }
    }
    public abstract class JobDriver_HaulAndDo : JobDriver_Base
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoA = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedOrNull(TargetIndex.A);
            yield return gotoA;

            job.count = 1;
            Toil carryA = Toils_Haul.StartCarryThing(TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A);
            yield return carryA;

            Toil gotoB = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedOrNull(TargetIndex.B);
            yield return gotoB;

            yield return GetFinishToil();
        }
    }
}
