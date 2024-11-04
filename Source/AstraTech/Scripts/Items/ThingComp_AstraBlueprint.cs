using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class CompProperties_AstraBlueprint : CompProperties
    {
        public CompProperties_AstraBlueprint()
        {
            compClass = typeof(ThingComp_AstraBlueprint);
        }
    }

    public class AstraSchematics_Item : AstraSchematics
    {
    }

    public class ThingComp_AstraBlueprint : ThingComp
    {
        public ThingDef prefab;
        public ThingDef prefabStuff;
        public Color prefabColor;

        public CompProperties_AstraBlueprint Props => (CompProperties_AstraBlueprint)props;


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // If spawned by Dev mode.
            if (prefab == null)
            {
                prefab = GenBlueprints.StaticAvailableDefs.RandomElement();
                if (prefab.MadeFromStuff)
                {
                    prefabStuff = GenStuff.RandomStuffFor(prefab);
                }
            }
        }

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
            Scribe_Values.Look(ref prefabColor, nameof(prefabColor));
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
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Blueprint of", prefab.label, "Item that is ready to be printed by Astra code using this blueprint: " + prefab.label, -1, hyperlinks: new Dialog_InfoCard.Hyperlink[1]
                {
                    new Dialog_InfoCard.Hyperlink(prefab)
                });

            if (prefabStuff != null)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Blueprint item made of", prefabStuff.label, "Printed item will be made from material: " + prefabStuff.label, 0, hyperlinks: new Dialog_InfoCard.Hyperlink[1]
                {
                        new Dialog_InfoCard.Hyperlink(prefabStuff)
                });
            }
        }

        public override float GetStatOffset(StatDef stat)
        {
            if (stat == StatDefOf.MarketValue && prefab != null)
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

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            yield return new FloatMenuOption("Assign schematics to a holder: " + prefab.label, () => BeginCarryJob(selPawn));
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
                    GenJob.TryGiveJob<JobDriver_BlueprintCarryToHolder>(pawn, parent, b);
                }
            }, null, null);
        }

        public override string GetDescriptionPart()
        {
            return $"Blueprinted item: {prefab.label} - {prefab.description}";
        }
    }
}