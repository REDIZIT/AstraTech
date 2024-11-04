﻿using Verse;

namespace AstraTech
{
    public class JobDriver_CarryCardToBank : JobDriver_HaulAndDo
    {
        protected override bool DropBeforeFinishAction => false;

        public override string GetReport()
        {
            return "Carrying schematics to bank";
        }

        protected override void FinishAction()
        {
            var item = CastA<Thing>();
            var bank = CastB<Building_AstraCardsBank>();

            bank.InsertItem(item);
        }
    }
}