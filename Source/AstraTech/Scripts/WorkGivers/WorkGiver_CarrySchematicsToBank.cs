using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class WorkGiver_CarrySchematicsToBank : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var items = pawn.Map.listerThings.ThingsOfDef(AstraDefOf.astra_schematics_item);
            var items_empty = pawn.Map.listerThings.ThingsOfDef(AstraDefOf.astra_schematics_item_empty);
            var skills = pawn.Map.listerThings.ThingsOfDef(AstraDefOf.astra_schematics_skill);
            var traits = pawn.Map.listerThings.ThingsOfDef(AstraDefOf.astra_schematics_trait);

            return items.Union(items_empty).Union(skills).Union(traits);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Thing freeBank = TryFindClosestFreeBank(pawn);
            if (freeBank == null) return false;

            return t != null && !t.IsForbidden(pawn) && pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly);
        }

        public override Job JobOnThing(Pawn pawn, Thing schematicsItem, bool forced = false)
        {
            Thing bank = TryFindClosestFreeBank(pawn);

            return GenJob.CreateJob<JobDriver_CarrySchematicsToBank>(pawn, schematicsItem, bank);
        }

        private Thing TryFindClosestFreeBank(Pawn pawn)
        {
            ThingRequest req = ThingRequest.ForDef(AstraDefOf.astra_cards_bank);
            TraverseParms traverse = TraverseParms.For(pawn);

            Thing bank = GenClosest.ClosestThingReachable(pawn.Position, pawn.MapHeld, req, PathEndMode.ClosestTouch, traverse, validator: (t) =>
            {
                return t is Building_AstraSchematicsBank b && b.HasFreeSpace && pawn.CanReserve(t) && ReservationUtility.CanReserve(pawn, b);
            });

            return bank;
        }
    }
}
