using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class JobDriver_CarryPawnToSkillExtraction : JobDriver_HaulAndDo
    {
        protected override float FinishActionDurationInSeconds => 3;

        public override string GetReport()
        {
            return $"Carrying {TargetPawnA.NameShortColored} for Skill Extraction";
        }

        protected override void FinishAction()
        {
            var victim = CastA<Pawn>();
            var machine = CastB<Building_AstraPawnMachine>();

            pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
            machine.StartTask_SkillExtraction(victim);
        }
    }

    public class JobDriver_EnterToSkillExtraction : JobDriver_Base
    {
        public override string GetReport()
        {
            return $"Going for Skill Extraction";
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);

            var machine = CastB<Building_AstraPawnMachine>();

            yield return Toils_General.Wait(GenTicks.SecondsToTicks(3));

            Toil enter = ToilMaker.MakeToil("MakeNewToils");
            enter.initAction = delegate
            {
                machine.StartTask_SkillExtraction(pawn);
            };
            enter.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return enter;
        }

        protected override void FinishAction()
        {
        }
    }

    public class JobDriver_StopSkillExtraction : JobDriver_Base
    {
        public override string GetReport()
        {
            return "Stopping Skill Extraction...";
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.ClosestTouch);

            yield return GetFinishToil();
        }
        protected override void FinishAction()
        {
            var machine = CastA<Building_AstraPawnMachine>();

            machine.StopTask_SkillExtraction();
        }
    }
}
