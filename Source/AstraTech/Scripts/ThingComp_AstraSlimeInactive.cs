using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class CompProperties_AstraSlimeInactive : CompProperties
    {
        public CompProperties_AstraSlimeInactive()
        {
            compClass = typeof(ThingComp_AstraSlimeInactive);
        }
    }
    public class ThingComp_AstraSlimeInactive : ThingComp
    {
        public override string CompInspectStringExtra()
        {
            return "Sleeping";
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            yield return new FloatMenuOption("Begin transformation into ..", () => BeginEncodeJob(selPawn));
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
                Job job = new Job(AstraDefOf.job_astra_slime_begin_transform, parent, targetInfo);
                pawn.jobs.TryTakeOrderedJob(job);

            }, null, null);
        }
    }
}
