namespace AstraTech
{
    public class JobDriver_InsertBrainIntoMachine : JobDriver_HaulAndDo
    {
        protected override void FinishAction()
        {
            var brain = CastA<ThingWithComps_AstraBrain>();
            var machine = CastB<Building_AstraPawnMachine>();

            machine.InsertBrain(brain);
        }
    }
}
