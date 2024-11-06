using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AstraTech
{
    public class Building_AstraSchematicsBank : Building, IThingHolder, IThingHolderEvents<AstraSchematics>
    {
        public bool HasFreeSpace => itemsInside.Count + ReservedSpace < 4;
        public float ReservedSpace => Map.reservationManager.ReservationsReadOnly.Where(r => r.Target == this).Sum(r => r.StackCount);

        public ThingOwner<AstraSchematics> itemsInside;
        public Action<AstraSchematics> onSchematicsAdd, onSchematicsDrop;

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

        public bool TryInsertItem(AstraSchematics card)
        {
            return itemsInside.TryAddOrTransfer(card);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return itemsInside;
        }

        public void Notify_ItemAdded(AstraSchematics item)
        {
            onSchematicsAdd?.Invoke(item);
        }

        public void Notify_ItemRemoved(AstraSchematics item)
        {
            onSchematicsDrop?.Invoke(item);
        }
    }
}
