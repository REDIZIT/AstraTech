﻿using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class JobDriver_ExtractBlueprint : JobDriver
    {
        private Building_AstraBlueprintHolder holder => (Building_AstraBlueprintHolder)TargetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(holder, job, 1, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoHolder = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return gotoHolder;

            Toil extract = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = GenTicks.SecondsToTicks(5),
            };
            extract.WithProgressBarToilDelay(TargetIndex.A).AddFinishAction(() =>
            {
                holder.ExtractBlueprint();
            });
            yield return extract;
        }
    }

}