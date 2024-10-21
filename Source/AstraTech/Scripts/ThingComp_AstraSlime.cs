using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public class ThingWithComps_AstraSlime : ThingWithComps
    {
        private ThingComp_AstraSlime comp;
        private CompHeatPusher heatComp;
        private CompGlower glowComp;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            comp = GetComp<ThingComp_AstraSlime>();
            heatComp = GetComp<CompHeatPusher>();
            glowComp = GetComp<CompGlower>();

            heatComp.enabled = comp.IsTransforming;
            glowComp.GlowRadius = comp.IsTransforming ? 2 : 0;

            base.SpawnSetup(map, respawningAfterLoad);
        }
        public override Graphic Graphic
        {
            get
            {
                int tick = Find.TickManager.TicksGame + (1 * 60) * (Position.x + Position.z);
                float t = Mathf.Repeat(tick / (5 * 60f), 1);
                float tsin = Mathf.Sin(Mathf.PI * 2 * t);
                float tsinmap = (tsin + 1) / 2f;
                Color a = Color.red * 0.6f;
                Color b = Color.red * 1f;
                a.a = 1;
                b.a = 1;
                Color color = comp.IsTransforming ? Color.Lerp(a, b, tsinmap) : new ColorInt(60, 40, 80, 255).ToColor;
                return GraphicDatabase.Get<Graphic_Single>("astra_slime_bw", ShaderDatabase.Cutout, Vector2.one * 0.65f, color);
            }
        }
    }
    public class CompProperties_AstraSlime : CompProperties
    {
        public CompProperties_AstraSlime()
        {
            compClass = typeof(ThingComp_AstraSlime);
        }
    }
    public class ThingComp_AstraSlime : ThingComp
    {
        public bool IsTransforming => prefab != null;

        public ThingDef prefab;
        public ThingDef prefabStuff;
        public Color prefabColor;
        public QualityCategory prefabQuality;
        public int ticksLeft;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref prefab, nameof(prefab));
            Scribe_Defs.Look(ref prefabStuff, nameof(prefabStuff));
            Scribe_Values.Look(ref prefabColor, nameof(prefabColor));
            Scribe_Values.Look(ref prefabQuality, nameof(prefabQuality));
            Scribe_Values.Look(ref ticksLeft, nameof(ticksLeft));
        }

        public override string CompInspectStringExtra()
        {
            if (prefab == null)
            {
                return "Sleeping";
            }
            if (prefabStuff != null)
            {
                return $"Transforming into {prefab.label} ({prefabStuff.label}) in {GenDate.ToStringTicksToPeriod(ticksLeft)}";
            }
            return $"Transforming into {prefab.label} in {GenDate.ToStringTicksToPeriod(ticksLeft)}";
        }

        public override void CompTick()
        {
            base.CompTick();

            if (IsTransforming)
            {
                if (ticksLeft > 0)
                {
                    ticksLeft--;
                }
                else
                {
                    if (prefab.category == ThingCategory.Item)
                    {
                        Map map = parent.Map;
                        IntVec3 pos = parent.Position;

                        parent.DeSpawn();

                        Thing item = ThingMaker.MakeThing(prefab, prefabStuff);
                        item.stackCount = prefab.stackLimit;

                        if (item.HasComp<CompColorable>())
                        {
                            item.DrawColor = prefabColor;
                        }

                        if (item.HasComp<CompQuality>())
                        {
                            item.TryGetComp<CompQuality>().SetQuality(prefabQuality, ArtGenerationContext.Colony);
                        }
                        
                        GenPlace.TryPlaceThing(item, pos, map, ThingPlaceMode.Direct);
                    }
                }
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            if (IsTransforming == false)
            {
                yield return new FloatMenuOption("Begin transforming into ..", () => BeginEncodeJob(selPawn));
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            if (DebugSettings.ShowDevGizmos && IsTransforming)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Dev: Complete tranforming",
                    action = () =>
                    {
                        ticksLeft = 0;
                    }
                };
            }
        }

        public void StartTransforming(Thing targetItem)
        {
            prefab = targetItem.def;
            prefabStuff = targetItem.Stuff;
            prefabColor = targetItem.DrawColor;
            
            if (targetItem.HasComp<CompQuality>())
            {
                prefabQuality = targetItem.TryGetComp<CompQuality>().Quality;
            }

            ticksLeft = (int)(Building_AstraBlueprintHolder.GetMatterCost(prefab, prefabStuff) * Building_AstraBlueprintHolder.MATTER_COST_TO_HOURS * GenDate.TicksPerHour);
            ticksLeft *= prefab.stackLimit;

            parent.GetComp<CompHeatPusher>().enabled = true;
            parent.GetComp<CompGlower>().GlowRadius = IsTransforming ? 2 : 0;
        }

        private void BeginEncodeJob(Pawn pawn)
        {
            TargetingParameters targetingParams = new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = false,
                canTargetItems = true,
                canTargetSelf = false,
                mapObjectTargetsMustBeAutoAttackable = false,

                validator = (i) =>
                {
                    if (i.Thing == null) return false;
                    if (i.Thing.def == AstraDefOf.astra_slime) return false;
                    return GenBlueprints.StaticAvailableDefs.Contains(i.Thing.def);
                }
            };

            Find.Targeter.BeginTargeting(targetingParams, delegate (LocalTargetInfo targetInfo)
            {
                Job job = new Job(AstraDefOf.job_astra_slime_begin_transform, parent, targetInfo);
                pawn.jobs.TryTakeOrderedJob(job);

            }, null, null);
        }
    }
}
