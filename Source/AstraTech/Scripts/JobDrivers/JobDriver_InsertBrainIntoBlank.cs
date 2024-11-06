using Verse;

namespace AstraTech
{
    public class JobDriver_InsertBrainIntoBlank : JobDriver_HaulAndDo
    {
        public override string GetReport()
        {
            return "Inserting brain into Blank";
        }

        protected override void FinishAction()
        {
            var brainItem = CastA<AstraBrain>();
            var blank = CastB<Pawn>();

            blank.health.hediffSet.GetFirstHediff<Hediff_AstraBrainSocket>().InsertBrain(brainItem);
            brainItem.DeSpawn();
        }
    }
}
