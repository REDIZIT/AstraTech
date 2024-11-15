﻿using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class StockGenerator_AstraBlueprints : StockGenerator
    {
        public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
        {
            int count = Random.Range(8, 12);

            for (int i = 0; i < count; i++)
            {
                yield return GenBlueprints.TryGenerate(null);
            }
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef == AstraDefOf.astra_schematics_item;
        }
    }
}
