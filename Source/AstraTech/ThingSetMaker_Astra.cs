using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AstraTech
{
    public class ThingSetMaker_Astra : ThingSetMaker
    {
        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            throw new System.NotImplementedException();
        }

        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            outThings.Add(GenBlueprints.Generate(parms.totalMarketValueRange));
        }
    }
}
