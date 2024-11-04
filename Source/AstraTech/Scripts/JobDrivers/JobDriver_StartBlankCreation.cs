using System.Collections.Generic;
using Verse.AI;

namespace AstraTech
{
    public class JobDriver_StartBlankCreation : JobDriver_Base
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return GetFinishToil();
        }

        protected override void FinishAction()
        {
            var machine = CastA<Building_AstraPawnMachine>();
            machine.StartTask_CreateBlank();
        }
    }
}
