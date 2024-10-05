using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class Building_AstraCore : Building
    {
        public float matter => this.GetComp<CompRefuelable>().Fuel;
        //public int organicMatterInTicks;
        public int ticksLeft;

        public Building_AstraBlueprintHolder holder;

        public bool isCreationEnabled;

        public bool isDisposeEnabled;
        public DisposeMode disposeMode;


        private StringBuilder b = new StringBuilder();
        //private List<IntVec3> interactionAreaCache = new List<IntVec3>();

        private const float HOUR_PER_SILVER_MATTER_COST = 1 / 100f;


        public enum DisposeMode
        {
            OnlyAllowedItems,
            AnyItems,
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            map.GetComponent<MapComp_AstraNetManager>().RegisterCore(this);

            //// If first spawn
            //if (respawningAfterLoad == false && Prefs.DevMode)
            //{
            //    silverMatter = 1000;
            //    organicMatterInTicks = 1000 * GenDate.TicksPerDay;
            //}

            //interactionAreaCache = GenRadial.RadialCellsAround(Position, 7, true).ToList();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            //Scribe_Values.Look(ref silverMatter, nameof(silverMatter));
            //Scribe_Values.Look(ref matter, nameof(matter));
            Scribe_Values.Look(ref ticksLeft, nameof(ticksLeft));
            Scribe_Values.Look(ref isDisposeEnabled, nameof(isDisposeEnabled));
            Scribe_Values.Look(ref disposeMode, nameof(disposeMode));

            Scribe_References.Look(ref holder, nameof(holder));
        }

        public override void Tick()
        {
            base.Tick();

            if (ticksLeft > 0)
            {
                ticksLeft--;
            }
            else if (isCreationEnabled && holder != null && holder.prefab != null)
            {
                holder.CloneAndPlace(Position);
                TryStartPrinting();
            }

            //if (isDisposeEnabled)
            //{
            //    TickDispose();
            //}
            
            //if (organicMatterInTicks > 0)
            //{
            //    organicMatterInTicks = Mathf.Max(organicMatterInTicks - 1, 0);
            //}
        }

        //private void TickDispose()
        //{
        //    var pos = interactionAreaCache.RandomElement();

        //    Thing thing = Map.thingGrid.ThingAt(pos, ThingCategory.Item);
        //    if (thing == null) return;

        //    if (disposeMode == DisposeMode.OnlyAllowedItems && thing.IsForbidden(Faction)) return;

        //    Dispose(thing);
        //}

        public override string GetInspectString()
        {
            b.Clear();

            b.Append("Status: ");
            if (isCreationEnabled)
            {
                b.Append("Printing (");
                b.Append(GenDate.ToStringTicksToPeriod(ticksLeft));
                b.Append(" left)");
            }
            else
            {
                if (holder == null) b.Append("Not ready (holder is not assigned)");
                else if (holder.prefab == null) b.Append("Not ready (holder has no blueprint)");
                else b.Append("Ready, waiting activation");
            }

            b.AppendLine();
            b.Append("Matter: ");
            b.Append(matter);
            b.Append(" / ");
            b.Append(AstraDefOf.astra_matter_merged.stackLimit);

            if (matter > 0)
            {
                b.Append(" (consumption: x");
                b.Append(this.GetComp<CompRefuelable>().Props.fuelConsumptionRate);
                b.Append(" per day)");
            }


            b.AppendLine();
            b.Append("Dispose: ");
            b.Append(isDisposeEnabled ? "enabled" : "disabled");
            b.Append(" (");
            b.Append(disposeMode == DisposeMode.OnlyAllowedItems ? "Only allowed items" : "Any items");
            b.Append(")");

            return b.ToString();
        }

        //public override void DrawExtraSelectionOverlays()
        //{
        //    base.DrawExtraSelectionOverlays();
        //    GenDraw.DrawFieldEdges(interactionAreaCache);
        //}

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                defaultLabel = "Assign holder",
                defaultDesc = "",
                icon = ContentFinder<Texture2D>.Get("temp2"),
                action = BeginAssigningHolder,
            };

            if (holder != null && holder.prefab != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = isCreationEnabled ? "Disable printing" : "Enable printing",
                    defaultDesc = "Toggle printing state",
                    icon = ContentFinder<Texture2D>.Get("temp"),
                    action = () =>
                    {
                        isCreationEnabled = !isCreationEnabled;

                        // If we're now printing
                        if (isCreationEnabled)
                        {
                            TryStartPrinting();
                        }
                    }
                };
            }

            yield return new Command_ExtendedAction
            {
                defaultLabel = isDisposeEnabled ? "Disable disposing" : "Enable disposing",
                defaultDesc = "Toggle area disposing items to matter",
                icon = ContentFinder<Texture2D>.Get("temp"),
                action = () =>
                {
                    isDisposeEnabled = !isDisposeEnabled;
                },
                rightClickFloatMenuOptions = EnumerateDisposeOptions(),
            };

            //yield return new Command_Action
            //{
            //    defaultLabel = "Dispose",
            //    defaultDesc = "Dispose selected object to matter",
            //    icon = ContentFinder<Texture2D>.Get("temp"),
            //    action = () => OnButtonClicked(false),
            //};
        }
        private IEnumerable<FloatMenuOption> EnumerateDisposeOptions()
        {
            yield return new FloatMenuOption("Dispose only allowed items", () => disposeMode = DisposeMode.OnlyAllowedItems);
            yield return new FloatMenuOption("Dispose any items", () => disposeMode = DisposeMode.AnyItems);
        }

        private void TryStartPrinting()
        {
            float silverCost = GetSilverMatterCost(holder.prefab, holder.prefabStuff);
            //int organicCostInTicks = (int)((GetOrganicMatterCost(holder.prefab, holder.prefabStuff) + silverCost * 0.1f) * GenDate.TicksPerDay);

            if (matter > silverCost)
            {
                isCreationEnabled = false;
            }
            else
            {
                ticksLeft = (int)(GenDate.TicksPerHour * silverCost * HOUR_PER_SILVER_MATTER_COST);

                isCreationEnabled = true;
            }

            //if (silverCost > silverMatter || organicCostInTicks > organicMatterInTicks)
            //{
            //    isCreationEnabled = false;
            //}
            //else
            //{
            //    silverMatter -= silverCost;
            //    organicMatterInTicks -= organicCostInTicks;
            //    ticksLeft = (int)(GenDate.TicksPerHour * silverCost * HOUR_PER_SILVER_MATTER_COST);

            //    isCreationEnabled = true;
            //}
        }

        private void BeginAssigningHolder()
        {
            TargetingParameters targetingParams = new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = true,
                mapObjectTargetsMustBeAutoAttackable = false,

                validator = (i) =>
                {
                    if (i.Thing == null || i.Thing == this) return false;
                    return i.Thing is Building_AstraBlueprintHolder;
                }
            };

            Find.Targeter.BeginTargeting(targetingParams, delegate (LocalTargetInfo targetInfo)
            {
                if (targetInfo.Thing != null)
                {
                    if (targetInfo.Thing is Building_AstraBlueprintHolder holder) AssignHolder(holder);
                }
            });
        }

        //private void OnButtonClicked(bool isCreating)
        //{
        //    TargetingParameters targetingParams = new TargetingParameters
        //    {
        //        canTargetPawns = true,
        //        canTargetItems = true,

        //        canTargetBuildings = false,
        //        canTargetCorpses = isCreating == false,
        //        canTargetAnimals = false,
        //        canTargetMechs = false,
        //        canTargetPlants = false,
        //        canTargetLocations = false,

        //        mapObjectTargetsMustBeAutoAttackable = false,

        //        validator = (i) =>
        //        {
        //            if (i.Thing == null || i.Thing == this) return false;
        //            //if (interactionAreaCache.Contains(i.Cell) == false) return false;

        //            if (i.Thing.def.category == ThingCategory.Item) return true;
        //            if (i.Thing is Pawn) return true;
        //            if (i.Thing is Building_AstraBlueprintHolder) return true;

        //            return false;
        //        }
        //    };

        //    Find.Targeter.BeginTargeting(targetingParams, delegate (LocalTargetInfo targetInfo)
        //    {
        //        if (targetInfo.Thing != null)
        //        {
        //            if (targetInfo.Thing is Building_AstraBlueprintHolder holder) AssignHolder(holder);
        //            else if (isCreating) BeginCreation(targetInfo.Thing);
        //            else Dispose(targetInfo.Thing);
        //        }
        //    }, null, null);
        //}

        public void AssignHolder(Building_AstraBlueprintHolder holder)
        {
            // Remove backlink to core of disabled holder
            if (this.holder != null)
            {
                this.holder.core = null;
            }

            this.holder = holder;

            // If not reset, set backlink
            if (this.holder != null)
            {
                this.holder.core = this;
            }

            // Stop creation and reset parameters
            isCreationEnabled = false;
            ticksLeft = 0;
        }


        //private void Dispose(Thing original)
        //{
        //    //silverMatter += GetSilverMatterCost(original);
        //    //organicMatterInTicks += (int)(GetOrganicMatterCost(original) * GenDate.TicksPerDay);
        //    matter += GetSilverMatterCost(original);

        //    original.Destroy(DestroyMode.Vanish);
        //}

        //private void BeginCreation(Thing original)
        //{
        //    if (original == null) return;

        //    float cost = GetMatterCost(original);
        //    if (cost > silverMatter && DebugSettings.godMode == false)
        //    {
        //        return;
        //    }

        //    //blueprint = CopyUtils.Copy(original);

        //    ticksLeft = (int)(GenDate.TicksPerDay * 0.2f);
        //    silverMatter -= cost;
        //}



        private float GetSilverMatterCost(ThingDef def, ThingDef stuff = null)
        {
            if (def.category == ThingCategory.Item)
            {
                return def.GetStatValueAbstract(StatDefOf.MarketValue, stuff);
            }

            throw new System.Exception("Failed to get silver matter cost for unknown ThingDef - " + def.ToStringSafe());
        }
        private float GetSilverMatterCost(Thing thing)
        {
            if (thing.def.category == ThingCategory.Item)
            {
                return thing.MarketValue * thing.stackCount;
            }
            else if (thing is Pawn pawn)
            {
                return pawn.MarketValue;
            }

            throw new System.Exception("Failed to get silver matter cost for unknown Thing - " + thing.ToStringSafe());
        }


        //private float GetOrganicMatterCost(ThingDef def, ThingDef stuff = null)
        //{
        //    if (def.category == ThingCategory.Item)
        //    {
        //        return def.GetStatValueAbstract(StatDefOf.Nutrition, stuff);
        //    }

        //    throw new System.Exception("Failed to get organic matter cost for unknown ThingDef - " + def.ToStringSafe());
        //}
        //private float GetOrganicMatterCost(Thing thing)
        //{
        //    if (thing.def.category == ThingCategory.Item)
        //    {
        //        return thing.GetStatValue(StatDefOf.Nutrition) * thing.stackCount;
        //    }

        //    throw new System.Exception("Failed to get organic matter cost for unknown Thing - " + thing.ToStringSafe());
        //}
    }
}
