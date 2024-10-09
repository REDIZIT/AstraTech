using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class JobDriver_BlueprintEncode : JobDriver
    {
        private Thing blueprintItem => TargetA.Thing;
        private Thing targetItem => TargetB.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(blueprintItem, job, 1, 1, null, errorOnFailed) &&
                   pawn.Reserve(targetItem, job, 1, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoBlueprint = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return gotoBlueprint;

            job.count = 1; // Without this throwing warning about -1 default count. May be this is due to reservation -1 stackCount??
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            Toil gotoTarget = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return gotoTarget;


            Toil useItem = new Toil
            {
                initAction = EncodeAction,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = GenTicks.SecondsToTicks(20),
            };
            yield return useItem;
        }

        private void EncodeAction()
        {
            var i = blueprintItem.TryGetComp<ThingComp_AstraBlueprint>();
            i.prefab = targetItem.def;
            i.prefabStuff = targetItem.Stuff;
        }
    }

}