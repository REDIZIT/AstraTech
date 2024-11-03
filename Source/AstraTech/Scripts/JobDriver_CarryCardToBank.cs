using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class JobDriver_CarryCardToBank : JobDriver
    {
        private Thing Item => TargetA.Thing;
        private Building_AstraCardsBank Buidling => TargetB.Thing as Building_AstraCardsBank;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Item, job, 1, 1, null, errorOnFailed) &&
                   pawn.Reserve(Buidling, job, 1, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil goToItem = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return goToItem;

            job.count = 1; // Without this throwing warning about -1 default count. May be this is due to reservation -1 stackCount??
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            Toil goToBuilding = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            yield return goToBuilding;


            Toil useItem = new Toil
            {
                initAction = null,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = GenTicks.SecondsToTicks(5),
            };
            useItem.WithProgressBarToilDelay(TargetIndex.B).AddFinishAction(SetCardToBank);
            yield return useItem;
        }

        private void SetCardToBank()
        {
            Buidling.SetCard(Item);
            Item.Destroy();
        }
    }

}