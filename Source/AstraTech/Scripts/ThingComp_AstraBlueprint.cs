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

        private const float EMPTY_BLUEPRINT_MARKET_VALUE_OFFSET = 4000;

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
            if (prefab == null)
            {
                return "Contains: nothing";
            }
            else
            {
                if (prefabStuff != null)
                {
                    return $"Contains: {prefab.label} ({prefabStuff.label})";
                }
                return $"Contains: {prefab.label}";
            }
        }

        public override string TransformLabel(string label)
        {
            if (prefab == null)
            {
                return $"{base.TransformLabel(label)} (empty)";
            }
            else 
            {
                if (prefabStuff != null)
                {
                    return $"{base.TransformLabel(label)} ({prefab.label} ({prefabStuff.label}))";
                }
                return $"{base.TransformLabel(label)} ({prefab.label})";
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            if (prefab == null)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Blueprint of", prefab.label, "Empty.\n\nThis blueprint is not encoded yet. You can encode, almost, any item into this blueprint.", -1);
            }
            else
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
            
        }

        public override float GetStatOffset(StatDef stat)
        {
            if (prefab == null)
            {
                return EMPTY_BLUEPRINT_MARKET_VALUE_OFFSET;
            }
            else if (stat == StatDefOf.MarketValue)
            {
                return prefab.BaseMarketValue * 5;
            }

            return base.GetStatOffset(stat);
        }
        public override void GetStatsExplanation(StatDef stat, StringBuilder sb)
        {
            base.GetStatsExplanation(stat, sb);

            if (prefab == null)
            {
                sb.Append($"Blueprint is empty = +" + EMPTY_BLUEPRINT_MARKET_VALUE_OFFSET);
            }
            else if (stat == StatDefOf.MarketValue)
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

            if (prefab != null)
            {
                yield return new FloatMenuOption("Assign to a holder", () => BeginCarryJob(selPawn));
            }
            else
            {
                yield return new FloatMenuOption("Encode item", () => BeginEncodeJob(selPawn));
            }
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
                    Job job = new Job(AstraDefOf.job_astra_blueprint_assign, parent, b);
                    pawn.jobs.TryTakeOrderedJob(job);
                }
            }, null, null);
        }

        private void BeginEncodeJob(Pawn pawn)
        {
            TargetingParameters targetingParams = new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = false,
                canTargetItems = true,
                canTargetSelf = false,
                mapObjectTargetsMustBeAutoAttackable = false,

                validator = (i) =>
                {
                    if (i.Thing == null) return false;
                    return GenBlueprints.StaticAvailableDefs.Contains(i.Thing.def);
                }
            };

            Find.Targeter.BeginTargeting(targetingParams, delegate (LocalTargetInfo targetInfo)
            {
                Job job = new Job(AstraDefOf.job_astra_blueprint_encode, parent, targetInfo);
                pawn.jobs.TryTakeOrderedJob(job);

            }, null, null);
        }

        public override string GetDescriptionPart()
        {
            return $"Blueprinted item: {prefab.label} - {prefab.description}";
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