using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AstraTech
{
    public class Building_AstraSchematicsBank : Building, IThingHolder
    {
        public bool HasFreeSpace => itemsInside.Count + ReservedSpace < 4;
        public float ReservedSpace => Map.reservationManager.ReservationsReadOnly.Where(r => r.Target == this).Sum(r => r.StackCount);

        public ThingOwner<AstraSchematics> itemsInside;

        public Building_AstraSchematicsBank()
        {
            itemsInside = new ThingOwner<AstraSchematics>(this, false);
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

        public void InsertItem(AstraSchematics card)
        {
            //Log.Message("Owner: " + card.holdingOwner.ToStringSafe());
            //Log.Message("CanAcceptAnyOf: " + itemsInside.CanAcceptAnyOf(card));
            //Log.Message("GetCountCanAccept: " + itemsInside.GetCountCanAccept(card));
            //Log.Message("TotalStackCount: " + itemsInside.TotalStackCount);
            bool success = itemsInside.TryAddOrTransfer(card);
            //Log.Message("Success: " + success);
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
