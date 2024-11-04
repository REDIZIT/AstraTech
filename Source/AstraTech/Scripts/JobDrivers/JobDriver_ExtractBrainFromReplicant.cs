using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class JobDriver_ExtractBrainFromReplicant : JobDriver
    {
        private bool IsTargetAlivePawn => TargetA.Thing is Pawn;
        private Pawn TargetPawn => (Pawn)TargetA.Thing;
        private Corpse TargetCorpse => (Corpse)TargetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            yield return gotoTarget;


            // Останавливаем действия цели
            Toil waitForInitiator = new Toil();
            waitForInitiator.initAction = () =>
            {
                if (TargetA.Thing is Pawn pawn && pawn.jobs != null)
                {
                    pawn.jobs.SuspendCurrentJob(JobCondition.InterruptForced);
                    pawn.jobs.CaptureAndClearJobQueue();
                }
            };
            waitForInitiator.defaultCompleteMode = ToilCompleteMode.Delay;
            waitForInitiator.defaultDuration = 100;
            yield return waitForInitiator;


            Toil finishJob = new Toil();
            finishJob.initAction = () =>
            {
                Pawn p;
                if (IsTargetAlivePawn)
                {
                    p = TargetPawn;
                }
                else
                {
                    p = TargetCorpse.InnerPawn;
                }

                p.health.hediffSet.GetFirstHediff<Hediff_AstraBrainSocket>().ExtractBrain();

                if (IsTargetAlivePawn == false)
                {
                    // After brain extraction name of pawn is changed, so we need to update corpse cache
                    typeof(Corpse).GetField("cachedLabel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(TargetCorpse, null);
                }               
            };
            finishJob.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finishJob;
        }
    }
}
