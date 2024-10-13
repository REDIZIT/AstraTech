﻿using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class Building_AstraCore : Building
    {
        public float matter => compRefuelable.Fuel;
        public int ticksLeft;

        public Building_AstraBlueprintHolder holder;

        public bool isCreationEnabled, isPrintning;

        private StringBuilder b = new StringBuilder();
        private CompRefuelable compRefuelable;

        private const float MATTER_COST_TO_HOURS = 20;

        /// <summary>
        /// Multiply factor for calculating printing matter cost (lower coef -> lower print cost and time)
        /// </summary>
        private const float MATTER_COST_COEF = 1 / 200f;

        public enum DisposeMode
        {
            OnlyAllowedItems,
            AnyItems,
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            map.GetComponent<MapComp_AstraNetManager>().RegisterCore(this);
            compRefuelable = GetComp<CompRefuelable>();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref ticksLeft, nameof(ticksLeft));
            Scribe_Values.Look(ref isCreationEnabled, nameof(isCreationEnabled));
            Scribe_Values.Look(ref isPrintning, nameof(isPrintning));
            Scribe_References.Look(ref holder, nameof(holder));
        }

        public override void Tick()
        {
            base.Tick();

            if (ticksLeft > 0)
            {
                ticksLeft--;
                if (holder == null || holder.BlueprintItem == null)
                {
                    isPrintning = false;
                }
            }
            else if (isPrintning && holder != null && holder.BlueprintItem != null)
            {
                isPrintning = false;
                holder.CloneAndPlace(Position - new IntVec3(0, 0, 3));

                if (isCreationEnabled)
                {
                    TryStartPrinting();
                }
            }
        }

        public override string GetInspectString()
        {
            b.Clear();

            b.Append("Status: ");
            if (isPrintning)
            {
                b.Append("Printing ");
                b.Append(holder.blueprint.prefab.label);
                b.Append(" (");
                b.Append(GenDate.ToStringTicksToPeriod(ticksLeft));
                b.Append(" left)");

                if (isCreationEnabled == false)
                {
                    b.Append("[ Stop scheduled ]");
                }
            }
            else
            {
                if (holder == null) b.Append("Not ready (holder is not assigned)");
                else if (holder.BlueprintItem == null) b.Append("Not ready (holder has no blueprint)");
                else b.Append("Ready, waiting activation");
            }

            b.AppendLine();
            b.Append("Matter: ");
            b.Append(matter >= 10 ? matter.ToString("F0") : matter.ToString("0.0"));
            b.Append(" / ");
            b.Append(AstraDefOf.astra_matter_merged.stackLimit);

            float consumption = this.GetComp<CompRefuelable>().Props.fuelConsumptionRate;
            if (matter > 0 && consumption > 0)
            {
                b.Append(" (consumption: x");
                b.Append(consumption);
                b.Append(" per day)");
            }

            if (holder != null && holder.BlueprintItem != null)
            {
                float matterCost = GetMatterCost(holder);

                b.AppendLine();
                b.Append("Print matter cost: ");
                b.Append(matterCost >= 10 ? (Mathf.Ceil(matterCost * 10) / 10f).ToString("F0") : matterCost.ToString("0.0"));
                
                if (matterCost > matter)
                {
                    b.Append(" (not enough matter)");
                }
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
                defaultLabel = "Assign holder",
                defaultDesc = "",
                icon = ContentFinder<Texture2D>.Get("temp2"),
                action = BeginAssigningHolder,
            };

            if (holder != null && holder.BlueprintItem != null)
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
        }

        private void TryStartPrinting()
        {
            float matterCost = GetMatterCost(holder);

            if (matterCost > matter)
            {
                isPrintning = false;
            }
            else
            {
                ticksLeft = (int)(GenDate.TicksPerHour * matterCost * MATTER_COST_TO_HOURS);
                compRefuelable.ConsumeFuel(matterCost);

                isPrintning = true;
            }
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

        public void AssignHolder(Building_AstraBlueprintHolder holder)
        {
            throw new System.NotImplementedException();
            //// Remove backlink to core of disabled holder
            //if (this.holder != null)
            //{
            //    this.holder.core = null;
            //}

            //this.holder = holder;

            //// If not reset, set backlink
            //if (this.holder != null)
            //{
            //    this.holder.core = this;
            //}

            //// Stop creation and reset parameters
            //isCreationEnabled = false;
            //isPrintning = false;
            //ticksLeft = 0;
        }


        private float GetMatterCost(Building_AstraBlueprintHolder holder)
        {
            return GetMatterCost(holder.blueprint.prefab, holder.blueprint.prefabStuff);
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
