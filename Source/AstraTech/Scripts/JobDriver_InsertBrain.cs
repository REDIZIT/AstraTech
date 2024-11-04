using Verse;

namespace AstraTech
{
    public class JobDriver_InsertBrain : JobDriver_HaulAndDo
    {
        public override string GetReport()
        {
            return "Inserting brain...";
        }

        protected override void FinishAction()
        {
            var brainItem = CastA<ThingWithComps_AstraBrain>();
            var blank = CastB<Pawn>();

            blank.health.hediffSet.GetFirstHediff<Hediff_AstraBrainSocket>().InsertBrain(brainItem.brain);
            brainItem.Destroy();
        }
    }
}
