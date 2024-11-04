namespace AstraTech
{
    public class JobDriver_CarrySchematicsToBank : JobDriver_HaulAndDo
    {
        protected override bool DropBeforeFinishAction => false;

        public override string GetReport()
        {
            return "Carrying schematics to bank";
        }

        protected override void FinishAction()
        {
            var item = CastA<AstraSchematics>();
            var bank = CastB<Building_AstraSchematicsBank>();

            bank.InsertItem(item);
        }
    }
}
