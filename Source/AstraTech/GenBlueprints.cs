using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AstraTech
{
    public static class GenBlueprints
    {
        private static List<ThingDef> availableDefs = new List<ThingDef>();

        public static Thing Generate(FloatRange? prefabMarketValueRange)
        {
            availableDefs.Clear();

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.category != ThingCategory.Item) continue;
                if (def == AstraDefOf.astra_blueprint) continue;

                if (def.IsCorpse || def.Minifiable) continue;

                if (prefabMarketValueRange.HasValue && prefabMarketValueRange.Value.Includes(def.BaseMarketValue) == false) continue;

                availableDefs.Add(def);
            }


            Thing item = ThingMaker.MakeThing(AstraDefOf.astra_blueprint);

            var comp = item.TryGetComp<ThingComp_AstraBlueprint>();
            comp.prefab = availableDefs.RandomElement();
            
            if (comp.prefab.MadeFromStuff)
            {
                comp.prefabStuff = GenStuff.RandomStuffFor(comp.prefab);
            }

            return item;
        }
    }
}
