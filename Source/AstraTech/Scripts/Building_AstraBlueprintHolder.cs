using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class Building_AstraBlueprintHolder : Building
    {
        public Thing BlueprintItem => blueprintItem;

        private Thing blueprintItem;
        public ThingComp_AstraBlueprint blueprint => blueprintItem.TryGetComp<ThingComp_AstraBlueprint>();
        public bool HasBlueprint => blueprintItem != null && blueprint.prefab != null;

        public float Fuel => compRefuelable.Fuel;

        private bool isPrinting;
        private int ticksLeft, ticksTotal;

        private CompRefuelable compRefuelable;
        private StringBuilder b = new StringBuilder();

        
        /// <summary>
        /// Multiply factor for calculating printing matter cost (lower coef -> lower print cost and time)
        /// </summary>
        private const float MATTER_COST_COEF = 1 / 200f;
        private const float MATTER_COST_TO_HOURS = 20;


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compRefuelable = GetComp<CompRefuelable>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref blueprintItem, nameof(blueprintItem));
        }

        public override void Tick()
        {
            base.Tick();

            if (isPrinting)
            {
                if (ticksLeft > 0)
                {
                    ticksLeft--;
                }
                else
                {
                    isPrinting = false;
                    Messages.Message("Printing completed: " + blueprint.prefab.label.Translate(), new LookTargets(this), MessageTypeDefOf.PositiveEvent);

                    CloneAndPlace(Position - new IntVec3(0, 0, 2));
                }
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (var e in blueprint.SpecialDisplayStats())
            {
                yield return e;
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            Vector3 itemDrawPos = drawLoc + new Vector3(0, 0.01f, -0.42f);

            if (HasBlueprint)
            {
                Mesh mesh = MeshPool.GridPlane(Vector2.one * 0.7f);

                Material mat = blueprint.prefab.graphic.MatSingle;

                // TODO: Replace no-stuff color from Color.white to something else (game is coloring not to white)
                Color stuffColor = blueprint.prefabStuff != null ? blueprint.prefab.GetColorForStuff(blueprint.prefabStuff) : Color.white;

                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                propertyBlock.SetColor("_Color", stuffColor);

                Graphics.DrawMesh(mesh, itemDrawPos, Quaternion.identity, mat, 0, null, 0, propertyBlock);
            }


            float fuel = Fuel;
            Color defaultColor = new Color(190 / 255f, 190 / 255f, 190 / 255f);
            float failureT = (Mathf.Sin(GenTicks.TicksGame / 20f) + 1) / 2f;
            Color failureColor = Color.Lerp(defaultColor, new Color(1, 0.2f, 0.3f), Mathf.Lerp(0.5f, 1, failureT));
            Color fuelColor;

            if (fuel == 0)
            {
                fuelColor = failureColor;
            }
            else
            {
                if (HasBlueprint)
                {
                    float requiredFuel = GetMatterCost();
                    if (fuel < requiredFuel) fuelColor = failureColor;
                    else fuelColor = new Color(120 / 255f, 190 / 255f, 160 / 255f);
                }
                else
                {
                    fuelColor = defaultColor;
                }
            }
            

            Mesh fuelIndicatorMesh = MeshPool.GridPlane(Vector2.one * 0.45f);
            Material fuelIndicatorMat = MaterialPool.MatFrom("indicator_fuel", ShaderDatabase.Cutout);
            MaterialPropertyBlock fuelPropertyBlock = new MaterialPropertyBlock();
            fuelPropertyBlock.SetColor("_Color", fuelColor);
            Graphics.DrawMesh(fuelIndicatorMesh, itemDrawPos - new Vector3(0.725f, 0, 0), Quaternion.identity, fuelIndicatorMat, 0, null, 0, fuelPropertyBlock);


            GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest()
            {
                center = itemDrawPos + new Vector3(0.725f, 0, 0),
                rotation = Rot4.FromAngleFlat(-90),
                size = new Vector2(0.75f, 0.25f),
                fillPercent = isPrinting ? 1 - (ticksLeft / (float)ticksTotal) : 0,
                filledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(255 / 255f, 180 / 255f, 51 / 255f)),
                unfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(51 / 255f, 51 / 255f, 60 / 255f)),
                margin = 0.05f
            });

            return;
        }

        public void CloneAndPlace(IntVec3 pos)
        {
            if (blueprint.prefab.category == ThingCategory.Item)
            {
                Thing item = ThingMaker.MakeThing(blueprint.prefab, blueprint.prefabStuff);
                GenPlace.TryPlaceThing(item, pos, Map, ThingPlaceMode.Direct);
            }
        }

        public void SetBlueprint(Thing item)
        {
            blueprintItem = CopyUtils.Copy(item);
        }

        public void OnBlueprintExtracted()
        {
        }

        public override string GetInspectString()
        {
            b.Clear();

            b.Append("Status: ");
            if (isPrinting)
            {
                b.Append("Printing (");
                b.Append(GenDate.ToStringTicksToPeriod(ticksLeft));
                b.Append(" left)");
            }
            else if (blueprintItem == null || blueprint.prefab == null)
            {
                b.Append("No blueprint");
            }
            else if (Fuel < GetMatterCost())
            {
                b.Append("Not enough fuel");
            }
            else
            {
                b.Append("Ready");
            }


            b.AppendLine();
            b.Append("Contains: ");
            b.Append(blueprintItem == null ? "nothing" : blueprintItem.Label);


            b.AppendLine();
            b.Append("Matter: ");
            b.Append(Fuel.ToPrettyString());
            b.Append(" / ");
            b.Append(compRefuelable.Props.fuelCapacity.ToPrettyString());

            if (HasBlueprint)
            {
                b.Append(" (");
                b.Append(GetMatterCost().ToPrettyString());
                b.Append(" per print)");
            }
            

            return b.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (blueprintItem != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Extract blueprint",
                    icon = ContentFinder<Texture2D>.Get("temp3"),
                    action = () =>
                    {
                        GenPlace.TryPlaceThing(blueprintItem, Position - new IntVec3(0, 0, 2), Map, ThingPlaceMode.Near);
                        blueprintItem = null;
                    }
                };

                float requiredFuel = GetMatterCost();
                if (Fuel >= requiredFuel)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Start printing",
                        icon = ContentFinder<Texture2D>.Get("temp3"),
                        action = () =>
                        {
                            isPrinting = true;
                            ticksTotal = (int)(requiredFuel * MATTER_COST_TO_HOURS * GenDate.TicksPerHour);
                            ticksLeft = ticksTotal;
                            compRefuelable.ConsumeFuel(requiredFuel);
                        }
                    };
                }
            }

            if (DebugSettings.godMode && isPrinting)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Complete printing",
                    //icon = ContentFinder<Texture2D>.Get("temp3"),
                    action = () =>
                    {
                        ticksLeft = 0;
                    }
                };
            }
        }

        private float GetMatterCost()
        {
            return GetMatterCost(blueprint.prefab, blueprint.prefabStuff);
        }
        private float GetMatterCost(ThingDef def, ThingDef stuff = null)
        {
            if (def.category == ThingCategory.Item)
            {
                float marketValue = def.GetStatValueAbstract(StatDefOf.MarketValue, stuff);
                float mass = def.GetStatValueAbstract(StatDefOf.Mass, stuff);

                return (mass + marketValue) * MATTER_COST_COEF;
            }

            throw new System.Exception("Failed to get silver matter cost for unknown ThingDef - " + def.ToStringSafe());
        }
    }
}
