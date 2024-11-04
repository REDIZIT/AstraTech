using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class AstraSchematics : ThingWithComps
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var o in base.GetFloatMenuOptions(selPawn))
            {
                yield return o;
            }


            ThingRequest req = ThingRequest.ForDef(AstraDefOf.astra_cards_bank);
            TraverseParms traverse = TraverseParms.For(selPawn);

            Thing bank = GenClosest.ClosestThingReachable(Position, MapHeld, req, PathEndMode.ClosestTouch, traverse, validator: (t) =>
            {
                return t is Building_AstraSchematicsBank b && b.HasFreeSpace && selPawn.CanReserve(t) && ReservationUtility.CanReserve(selPawn, b);
            });


            FloatMenuOption carry = new FloatMenuOption("Carry to a bank", () =>
            {
                GenJob.TryGiveJob<JobDriver_CarrySchematicsToBank>(selPawn, this, bank);
            });

            if (bank == null)
            {
                carry.Disabled = true;
                carry.Label = "No free bank found: " + carry.Label;
            }

            yield return carry;
        }
    }
}
