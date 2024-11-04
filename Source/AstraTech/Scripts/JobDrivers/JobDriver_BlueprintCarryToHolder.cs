using Verse;

namespace AstraTech
{
    public class JobDriver_BlueprintCarryToHolder : JobDriver_HaulAndDo
    {
        public override string GetReport()
        {
            return "carrying blueprint to the holder";
        }

        protected override void FinishAction()
        {
            var item = CastA<Thing>();
            var building = CastB<Building_AstraBlueprintHolder>();

            building.SetBlueprint(item);
            item.Destroy();
        }
    }
}
