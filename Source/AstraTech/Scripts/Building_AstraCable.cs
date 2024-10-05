using System.Collections.Generic;
using Verse;

namespace AstraTech
{
    public class Building_AstraCable : Building
    {
        //public override void TickRare()
        //{
        //    base.TickRare();
        //    // Логика для поиска связанных зданий
        //    List<Building> connectedBuildings = FindConnectedBuildings();

        //    // Пример взаимодействия между постройкой А и постройками Б
        //    foreach (var building in connectedBuildings)
        //    {
        //        // Здесь можно передать информацию
        //        if (building is Building_TypeA typeABuilding)
        //        {
        //            typeABuilding.UpdateConnectedBuildings(connectedBuildings);
        //        }
        //    }
        //}

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            map.GetComponent<MapComp_AstraNetManager>().isDirty = true;
        }
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            Map.GetComponent<MapComp_AstraNetManager>().isDirty = true;
        }

        public List<Building> FindConnectedBuildings()
        {
            // Логика для поиска всех зданий, подключенных к сети кабелей
            List<Building> connectedBuildings = new List<Building>();

            foreach (IntVec3 cell in GenAdj.CellsAdjacentCardinal(this)) // получаем соседние клетки
            {
                Building building = cell.GetFirstBuilding(Map);
                if (building != null && (building is Building_AstraCore || building is Building_AstraBlueprintHolder))
                {
                    connectedBuildings.Add(building);
                }
            }
            return connectedBuildings;
        }
    }
}
