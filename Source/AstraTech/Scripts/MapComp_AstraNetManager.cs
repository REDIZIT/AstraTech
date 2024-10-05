using System.Collections.Generic;
using Verse;

namespace AstraTech
{
    public class MapComp_AstraNetManager : MapComponent
    {
        public bool isDirty;

        private Building_AstraCore core;
        private List<Building_AstraBlueprintHolder> holders = new List<Building_AstraBlueprintHolder>();

        public MapComp_AstraNetManager(Map map) : base(map)
        {
            isDirty = true;
        }

        public void RegisterCore(Building_AstraCore core)
        {
            if (this.core != null)
            {
                throw new System.Exception("You tried to register second AstraCore, but this action is not allowed. There must be only 1 AstraCore building on map.");
            }

            this.core = core;
            isDirty = true;
        }

        public void RegisterHolder(Building_AstraBlueprintHolder holder)
        {
            holders.Add(holder);
            isDirty = true;
        }
        public void DeregisterHolder(Building_AstraBlueprintHolder holder)
        {
            holders.Remove(holder);
            isDirty = true;
        }

        //public override void MapComponentUpdate()
        //{
        //    base.MapComponentUpdate();

        //    if (isDirty == false || core == null) return;
        //    isDirty = false;


        //    Log.Message("Rebuild");


        //    HashSet<Building_AstraCable> passed = new HashSet<Building_AstraCable>();

        //    for (int x = 0; x < core.RotatedSize.x + 2; x++)
        //    {
        //        for (int z = 0; z < core.RotatedSize.z + 2; z++)
        //        {
        //            IntVec3 pos = core.Position + new IntVec3(x, 0, z) - (core.RotatedSize / 2).ToIntVec3 - new IntVec3(1, 0, 1);
        //            Building building = map.edificeGrid[pos];

        //            if (building is Building_AstraCable cable)
        //            {
        //                if (passed.Contains(cable)) continue;
        //                passed.Add(cable);

        //                Log.Message(" - Found cable at " + pos);


        //            }
        //        }
        //    }
        //}

        //private IEnumerable<Building_AstraBlueprintHolder> EnumerateHolders()
        //{

        //}
    }
}
