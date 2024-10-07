using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class JobDriver_SetBlueprintToHolder : JobDriver
    {
        private Thing item => TargetA.Thing;
        private Building_AstraBlueprintHolder building => TargetB.Thing as Building_AstraBlueprintHolder;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(item, job, 1, 1, null, errorOnFailed) &&
                   pawn.Reserve(building, job, 1, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil goToItem = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return goToItem;

            //job.count = 1; // Without this throwing warning about -1 default count. May be this is due to reservation -1 stackCount??
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            Toil goToBuilding = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return goToBuilding;


            Toil useItem = new Toil
            {
                initAction = SetBlueprintToHolder,
                defaultCompleteMode = ToilCompleteMode.Instant,
                defaultDuration = GenTicks.SecondsToTicks(5)
            };
            yield return useItem;
        }

        private void SetBlueprintToHolder()
        {
            var i = item.TryGetComp<ThingComp_AstraBlueprint>();
            building.prefab = i.prefab;
            building.prefabStuff = i.prefabStuff;
            item.Destroy();
        }
    }

}