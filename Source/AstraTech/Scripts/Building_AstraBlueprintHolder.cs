using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AstraTech
{

    public class Building_AstraBlueprintHolder : Building_WorkTable
    {
        public Thing BlueprintItem => blueprintItem;
        public ThingComp_AstraBlueprint blueprint => blueprintItem.TryGetComp<ThingComp_AstraBlueprint>();
        public bool HasBlueprint => blueprintItem != null && blueprint.prefab != null;
        public float Fuel => compRefuelable.Fuel;

        private Thing blueprintItem;
        private bool isPrinting;
        private int ticksLeft, ticksTotal;
        private bool isLoopEnabled;

        private CompRefuelable compRefuelable;
        private StringBuilder b = new StringBuilder();
        private Graphic prefabGraphics;
        
        /// <summary>
        /// Multiply factor for calculating printing matter cost (lower coef -> lower print cost and time)
        /// </summary>
        private const float MATTER_COST_COEF = 1 / 200f;
        private const float MATTER_COST_TO_HOURS = 20;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compRefuelable = GetComp<CompRefuelable>();
            RefreshGraphics();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref blueprintItem, nameof(blueprintItem));
            Scribe_Values.Look(ref isPrinting, nameof(isPrinting));
            Scribe_Values.Look(ref ticksLeft, nameof(ticksLeft));
            Scribe_Values.Look(ref ticksTotal, nameof(ticksTotal));
            Scribe_Values.Look(ref isLoopEnabled, nameof(isLoopEnabled));
        }

        public override void Tick()
        {
            base.Tick();

            // Keep refuelable component always with non-zero fuel amount
            // Why?
            // Building_WorkTable does not give jobs if it have refuelable component and has no fuel.
            // I don't know who to change this behaviour, that's why I just keep fuel level always above zero.
            // float.Epsilon is the lowest posible value to make it not affecting gameplay
            if (compRefuelable.HasFuel == false)
            {
                compRefuelable.Refuel(float.Epsilon);
            }

            if (isPrinting)
            {
                if (ticksLeft > 0)
                {
                    ticksLeft--;
                }
                else
                {
                    isPrinting = false;

                    CloneAndPlace(Position - new IntVec3(0, 0, 2));
                    Messages.Message("Printing completed: " + blueprint.prefab.label.Translate(), new LookTargets(this), MessageTypeDefOf.PositiveEvent);

                    if (isLoopEnabled)
                    {
                        TryStartPrinting();
                    }
                }
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            if (HasBlueprint)
            {
                foreach (var e in blueprint.SpecialDisplayStats())
                {
                    yield return e;
                }
            }
        }
        
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var o in base.GetFloatMenuOptions(selPawn))
            {
                yield return o;
            }

            if (selPawn.IsColonistPlayerControlled == false) yield break;

            
            if (isPrinting == false)
            {
                var startPrinting = new FloatMenuOption("Start printing", () =>
                {
                    Job job = new Job(AstraDefOf.job_astra_start_printing, this);
                    selPawn.jobs.TryTakeOrderedJob(job);
                });
                if (HasBlueprint == false)
                {
                    startPrinting.Disabled = true;
                    startPrinting.Label = "Cannot start printing: Has no blueprint";
                }
                else
                {
                    float requiredFuel = GetMatterCost();
                    if (Fuel < requiredFuel)
                    {
                        startPrinting.Disabled = true;
                        startPrinting.Label = "Cannot start printing: Not enough matter";
                    }
                }
                yield return startPrinting;
            }

            if (HasBlueprint)
            {
                var extractBlueprint = new FloatMenuOption("Extract blueprint", () =>
                {
                    Job job = new Job(AstraDefOf.job_astra_blueprint_extract, this);
                    selPawn.jobs.TryTakeOrderedJob(job);
                });
                if (isPrinting)
                {
                    extractBlueprint.Disabled = true;
                    extractBlueprint.Label = "Cannot extract blueprint: Printing";
                }
                yield return extractBlueprint;
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            //// Render top part of building
            //Mesh topMesh = this.Graphic.MeshAt(Rot4.North);
            //Material topMaterial = MaterialPool.MatFrom("astra_holder_top", ShaderDatabase.Cutout);
            //Vector3 topDrawLoc = new Vector3(drawLoc.x + 0.25f, AltitudeLayer.Pawn.AltitudeFor(), drawLoc.z);
            //Graphics.DrawMesh(topMesh, topDrawLoc + Graphic.data.drawOffset, Quaternion.identity, topMaterial, 0);


            Vector3 itemDrawPos = drawLoc + new Vector3(0, 0.01f, -0.42f);

            if (HasBlueprint)
            {
                prefabGraphics.Draw(itemDrawPos, Rot4.North, this);
            }


            float fuel = Fuel;
            Color defaultColor = new ColorInt(190, 190, 190).ToColor;
            float failureT = (Mathf.Sin(GenTicks.TicksGame / 60f) + 1) / 2f;
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
                    else fuelColor = new ColorInt(120, 190, 160).ToColor;
                }
                else
                {
                    fuelColor = defaultColor;
                }
            }

            Mesh fuelIndicatorMesh = MeshPool.GridPlane(Vector2.one * 0.45f);
            Material fuelIndicatorMat = MaterialPool.MatFrom("indicator_fuel", ShaderDatabase.Transparent);
            MaterialPropertyBlock fuelPropertyBlock = new MaterialPropertyBlock();
            fuelPropertyBlock.SetColor("_Color", fuelColor);
            Graphics.DrawMesh(fuelIndicatorMesh, itemDrawPos - new Vector3(0.725f, 0, 0), Quaternion.identity, fuelIndicatorMat, 0, null, 0, fuelPropertyBlock);


            //GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest()
            //{
            //    center = itemDrawPos + new Vector3(0.725f, 0, 0),
            //    rotation = Rot4.FromAngleFlat(-90),
            //    size = new Vector2(0.75f, 0.25f),
            //    fillPercent = isPrinting ? 1 - (ticksLeft / (float)ticksTotal) : 0,
            //    filledMat = SolidColorMaterials.SimpleSolidColorMaterial(new ColorInt(255, 180, 51).ToColor),
            //    unfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new ColorInt(51, 51, 60).ToColor),
            //    margin = 0.05f
            //});
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
            if (blueprintItem != null) ExtractBlueprint();
            blueprintItem = CopyUtils.Copy(item);
            RefreshGraphics();
        }
        public void ExtractBlueprint()
        {
            if (blueprintItem != null) GenPlace.TryPlaceThing(blueprintItem, Position - new IntVec3(0, 0, 2), Map, ThingPlaceMode.Near);
            blueprintItem = null;
        }
        public void TryStartPrinting()
        {
            float requiredFuel = GetMatterCost();
            if (Fuel >= requiredFuel)
            {
                isPrinting = true;
                ticksTotal = (int)(requiredFuel * MATTER_COST_TO_HOURS * GenDate.TicksPerHour);
                ticksLeft = ticksTotal;
                compRefuelable.ConsumeFuel(requiredFuel);
            }
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
            b.Append(Fuel.ToPrettyString(ToStringUtils.RoundType.Floor));
            b.Append(" / ");
            b.Append(compRefuelable.Props.fuelCapacity.ToPrettyString());

            if (HasBlueprint)
            {
                b.Append(" (");
                b.Append(GetMatterCost().ToPrettyString(ToStringUtils.RoundType.Ceil));
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

            yield return new Command_Action
            {
                defaultLabel = isLoopEnabled ? "Disable loop" : "Enable loop",
                icon = ContentFinder<Texture2D>.Get("temp3"),
                action = () =>
                {
                    isLoopEnabled = !isLoopEnabled;
                }
            };

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

        private void RefreshGraphics()
        {
            if (HasBlueprint)
            {
                Graphic g = blueprint.prefab.graphic.GetColoredVersion(blueprint.prefab.graphic.Shader, blueprint.prefabColor, Color.white);
                prefabGraphics = g.GetCopy(Vector2.one * 0.7f, null);
            }
            else
            {
                prefabGraphics = null;
            }
        }
    }
}
