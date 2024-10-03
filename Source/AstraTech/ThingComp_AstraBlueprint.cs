using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class ThingComp_AstraBlueprint : ThingComp
    {
        public ThingDef prefab;
        public ThingDef prefabStuff;

        public CompProperties_AstraBlueprint Props => (CompProperties_AstraBlueprint)props;

        public override bool AllowStackWith(Thing other)
        {
            if (other.TryGetComp(out ThingComp_AstraBlueprint comp))
            {
                return comp.prefab == prefab;
            }
            return false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref prefab, nameof(prefab));
            Scribe_Defs.Look(ref prefabStuff, nameof(prefabStuff));
        }

        public override string CompInspectStringExtra()
        {
            if (prefabStuff != null)
            {
                return $"Contains: {prefab.label} ({prefabStuff.label})";
            }
            return $"Contains: {prefab.label}";
        }

        public override string TransformLabel(string label)
        {
            if (prefabStuff != null)
            {
                return $"{base.TransformLabel(label)} ({prefab.label} ({prefabStuff.label}))";
            }
            return $"{base.TransformLabel(label)} ({prefab.label})";
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Blueprint of", prefab.ToStringSafe(), "", -1);

            if (prefabStuff != null)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Blueprint item made of", prefabStuff.ToStringSafe(), "", 0);
            }
        }

        public override float GetStatOffset(StatDef stat)
        {
            if (stat == StatDefOf.MarketValue)
            {
                return prefab.BaseMarketValue * 5;
            }
            return base.GetStatOffset(stat);
        }
        public override void GetStatsExplanation(StatDef stat, StringBuilder sb)
        {
            base.GetStatsExplanation(stat, sb);

            if (stat == StatDefOf.MarketValue)
            {
                sb.Append($"Blueprinted item market value ({prefab.BaseMarketValue}) * 5 = +{prefab.BaseMarketValue * 5}");
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // If placed by Dev mode
            if (prefab == null)
            {
                prefab = DefDatabase<ThingDef>.GetNamed("MedicineIndustrial");
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            yield return new FloatMenuOption("Assign to a holder", () => BeginCarryJob(selPawn));
        }

        private void BeginCarryJob(Pawn pawn)
        {
            TargetingParameters targetingParams = new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = true,
                mapObjectTargetsMustBeAutoAttackable = false,

                validator = (i) => i.Thing is Building_AstraBlueprintHolder
            };

            Find.Targeter.BeginTargeting(targetingParams, delegate (LocalTargetInfo targetInfo)
            {
                if (targetInfo.Thing is Building_AstraBlueprintHolder b)
                {
                    Job job = new Job(AstraDefOf.astra_set_blueprint, parent, b);
                    pawn.jobs.TryTakeOrderedJob(job);
                }
            }, null, null);
        }
    }

    public class CompProperties_AstraBlueprint : CompProperties
    {
        public CompProperties_AstraBlueprint()
        {
            this.compClass = typeof(ThingComp_AstraBlueprint);
        }
    }

}