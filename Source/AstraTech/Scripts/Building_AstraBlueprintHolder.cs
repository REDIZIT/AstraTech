using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class Building_AstraBlueprintHolder : Building
    {
        public ThingDef prefab;
        public ThingDef prefabStuff;

        public Building_AstraCore core;

        private StringBuilder b = new StringBuilder();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            map.GetComponent<MapComp_AstraNetManager>().RegisterHolder(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref prefab, nameof(prefab));
            Scribe_Defs.Look(ref prefabStuff, nameof(prefabStuff));
            //Scribe_Deep.Look(ref blueprint, nameof(blueprint));
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            if (prefab != null)
            {
                Graphic materialGraphic = prefab.graphic;

                Vector3 drawPos = drawLoc;
                drawPos.y += 0.046875f;

                drawPos.z += Mathf.Sin(Find.TickManager.TicksSinceSettle / 100f) * 0.1f;
                Rot4 rotation = Rot4.FromAngleFlat(Mathf.Sin(Find.TickManager.TicksSinceSettle / 20f));

                materialGraphic.Draw(drawPos, rotation, this);
            }
        }

        public void CloneAndPlace(IntVec3 pos)
        {
            if (prefab.category == ThingCategory.Item)
            {
                Thing item = ThingMaker.MakeThing(prefab, prefabStuff);
                GenPlace.TryPlaceThing(item, pos, Map, ThingPlaceMode.Near);
            }
        }

        public void OnBlueprintExtracted()
        {
            core?.AssignHolder(null);
        }

        public override string GetInspectString()
        {
            b.Clear();

            b.Append("Contains: ");
            b.Append(prefab == null ? "nothing" : prefab.label);
            
            if (prefabStuff != null)
            {
                b.Append(" (");
                b.Append(prefabStuff.label);
                b.Append(")");
            }

            return b.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (prefab != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Extract blueprint",
                    icon = ContentFinder<Texture2D>.Get("temp3"),
                    action = () =>
                    {
                        Thing item = ThingMaker.MakeThing(AstraDefOf.astra_blueprint);
                        var comp = item.TryGetComp<ThingComp_AstraBlueprint>();
                        comp.prefab = prefab;
                        comp.prefabStuff = prefabStuff;
                        GenPlace.TryPlaceThing(item, Position, Map, ThingPlaceMode.Near);

                        prefab = null;
                        prefabStuff = null;
                    }
                };
            }
        }
    }
}
