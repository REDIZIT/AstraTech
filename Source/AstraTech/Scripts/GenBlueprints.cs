using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AstraTech
{
    public static class GenBlueprints
    {
        private static List<ThingDef> staticAvailableDefs = new List<ThingDef>();
        private static List<ThingDef> dynamicAvailableDefs = new List<ThingDef>();

        private static HashSet<ThingDef> bannedDefs = new HashSet<ThingDef>()
        {
            AstraDefOf.astra_blueprint,
            AstraDefOf.astra_matter_merged,
            AstraDefOf.astra_matter_non_organic,
            AstraDefOf.astra_matter_organic,
        };

        public static Thing Generate(FloatRange? prefabMarketValueRange)
        {
            if (staticAvailableDefs.Count == 0)
            {
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (def.category != ThingCategory.Item) continue;
                    if (bannedDefs.Contains(def)) continue;

                    if (def.IsCorpse || def.Minifiable || def.thingClass == typeof(MinifiedThing)) continue;

                    staticAvailableDefs.Add(def);
                }
            }


            dynamicAvailableDefs.Clear();
            foreach (ThingDef def in staticAvailableDefs)
            {
                if (prefabMarketValueRange.HasValue && prefabMarketValueRange.Value.Includes(def.BaseMarketValue) == false) continue;

                dynamicAvailableDefs.Add(def);
            }


            Thing item = ThingMaker.MakeThing(AstraDefOf.astra_blueprint);

            var comp = item.TryGetComp<ThingComp_AstraBlueprint>();
            comp.prefab = dynamicAvailableDefs.RandomElement();
            
            if (comp.prefab.MadeFromStuff)
            {
                comp.prefabStuff = GenStuff.RandomStuffFor(comp.prefab);
            }

            return item;
        }
    }
}
