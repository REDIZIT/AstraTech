using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class Building_AstraCardsBank : Building, IThingHolder
    {
        public bool HasFreeSpace => itemsInside.Count + ReservedSpace < 4;
        public float ReservedSpace => Map.reservationManager.ReservationsReadOnly.Where(r => r.Target == this).Sum(r => r.StackCount);

        public ThingOwner<Thing> itemsInside;

        public Building_AstraCardsBank()
        {
            itemsInside = new ThingOwner<Thing>(this, true);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            itemsInside.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
            base.DeSpawn(mode);
        }
        
        public override string GetInspectString()
        {
            return "Contains: " + (itemsInside.Count == 0 ? "nothing" : itemsInside.Count + " schematics") + " / Reserved: " + ReservedSpace;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref itemsInside, nameof(itemsInside), this);
        }

        public void InsertItem(Thing card)
        {
            Log.Message("Owner: " + card.holdingOwner.ToStringSafe());
            Log.Message("CanAcceptAnyOf: " + itemsInside.CanAcceptAnyOf(card));
            Log.Message("GetCountCanAccept: " + itemsInside.GetCountCanAccept(card));
            Log.Message("TotalStackCount: " + itemsInside.TotalStackCount);
            bool success = itemsInside.TryAddOrTransfer(card);
            Log.Message("Success: " + success);
            //card.DeSpawn();
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return itemsInside;
        }

        
    }
}
