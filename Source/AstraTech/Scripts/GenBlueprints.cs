using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AstraTech
{
    public static class GenBlueprints
    {
        public static List<ThingDef> StaticAvailableDefs
        {
            get
            {
                if (_staticAvailableDefs == null)
                {
                    _staticAvailableDefs = new List<ThingDef>(32);
                    foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                    {
                        if (def.category != ThingCategory.Item) continue;
                        if (bannedDefs.Contains(def)) continue;

                        if (def.IsCorpse || def.Minifiable || def.thingClass == typeof(MinifiedThing)) continue;

                        _staticAvailableDefs.Add(def);
                    }
                }
                return _staticAvailableDefs;
            }
        }

        private static List<ThingDef> dynamicAvailableDefs = new List<ThingDef>();

        private static List<ThingDef> _staticAvailableDefs;

        private static HashSet<ThingDef> bannedDefs = new HashSet<ThingDef>()
        {
            AstraDefOf.astra_schematics_item,
            AstraDefOf.astra_matter_merged,
            AstraDefOf.astra_matter_non_organic,
            AstraDefOf.astra_matter_organic,
        };

        public static Thing TryGenerate(FloatRange? prefabMarketValueRange)
        {
            float blueprintPrice = AstraDefOf.astra_schematics_item.BaseMarketValue;

            dynamicAvailableDefs.Clear();
            foreach (ThingDef def in StaticAvailableDefs)
            {
                float blueprintWithDefPrice = blueprintPrice + def.BaseMarketValue * 5;
                blueprintWithDefPrice *= 0.5f;

                if (prefabMarketValueRange.HasValue && prefabMarketValueRange.Value.Includes(blueprintWithDefPrice) == false) continue;

                dynamicAvailableDefs.Add(def);
            }

            if (dynamicAvailableDefs.Count > 0)
            {
                Thing item = ThingMaker.MakeThing(AstraDefOf.astra_schematics_item);

                var comp = item.TryGetComp<ThingComp_AstraBlueprint>();
                comp.prefab = dynamicAvailableDefs.RandomElement();

                if (comp.prefab.MadeFromStuff)
                {
                    comp.prefabStuff = GenStuff.RandomStuffFor(comp.prefab);
                }

                return item;
            }

            return null;
        }
    }
}
