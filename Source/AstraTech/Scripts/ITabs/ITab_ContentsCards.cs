using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AstraTech
{
    public class ITab_ContentsCards : ITab_ContentsBase
    {
        public override IList<Thing> container => ((Building_AstraSchematicsBank)SelThing).GetDirectlyHeldThings().ToList();
        public override bool UseDiscardMessage => false;


        public ITab_ContentsCards()
        {
            labelKey = "Contents";
            containedItemsKey = "Schematics";
            size.y = 240;
        }

        protected override void OnDropThing(Thing t, int count)
        {
            base.OnDropThing(t, count);
            container.Remove(t);
        }
    }
}
