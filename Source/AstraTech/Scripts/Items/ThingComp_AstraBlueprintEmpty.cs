using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class CompProperties_AstraBlueprintEmpty : CompProperties
    {
        public CompProperties_AstraBlueprintEmpty()
        {
            compClass = typeof(ThingComp_AstraBlueprintEmpty);
        }
    }
    public class ThingComp_AstraBlueprintEmpty : ThingComp
    {
        public override string CompInspectStringExtra()
        {
            return "Contains: nothing";
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Blueprint of", "nothing", "Empty.\n\nThis blueprint is not encoded yet. You can encode, almost, any item into this blueprint.", -1);
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            yield return new FloatMenuOption("Encode item", () => BeginEncodeJob(selPawn));
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
            return "Blueprinted item: nothing";
        }
    }
}