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
            Thing item = GenBlueprints.TryGenerate(parms.totalMarketValueRange);
            if (item != null) outThings.Add(item);
        }
    }
}
