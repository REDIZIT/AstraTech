
namespace AstraTech
{
    public class JobDriver_InsertBrainIntoMachine : JobDriver_HaulAndDo
    {
        public override string GetReport()
        {
            return "Inserting brain into machine";
        }

        protected override void FinishAction()
        {            
            var brain = CastA<AstraBrain>();
            var machine = CastB<Building_AstraPawnMachine>();

            machine.InsertBrain(brain);
        }
    }
}
