using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class JobDriver_ExtractBrain : JobDriver_Base
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.ClosestTouch);

            yield return Toils_General.WaitWith(TargetIndex.A, GenTicks.SecondsToTicks(3), true);

            yield return new Toil()
            {
                initAction = () =>
                {
                    var machine = CastA<Building_AstraPawnMachine>();
                    Thing brain = machine.ExtractBrain();

                    Job job = HaulAIUtility.HaulToStorageJob(pawn, brain);
                    if (job != null)
                    {
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
