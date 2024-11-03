using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public static class FloatMenuOptionInjector
    {
        public static IEnumerable<FloatMenuOption> AddImplantOptions(Pawn clickedPawn, Pawn selPawn, IEnumerable<FloatMenuOption> options)
        {
            foreach (var option in options)
            {
                yield return option;
            }

            yield return new FloatMenuOption("123", null);

            if (clickedPawn.health.hediffSet.HasHediff(HediffDef.Named("astra_brain_socket")))
            {
                yield return ExtractImplant(clickedPawn, selPawn);
            }
        }

        public static FloatMenuOption ExtractImplant(Pawn targetPawn, Pawn selPawn)
        {
            FloatMenuOption customOption = new FloatMenuOption("Extract Astra Brain", () =>
            {
                Job job = new Job(AstraDefOf.job_astra_brain_extract);
                job.targetA = targetPawn;
                selPawn.jobs.TryTakeOrderedJob(job);
            });

            if (selPawn == targetPawn)
            {
                customOption.Disabled = true;
                customOption.Label = "Не может выключить сам себя";
            }


            return customOption;
        }

        public static FloatMenuOption ExtractImplant(Corpse targetPawnCorpse, Pawn selPawn)
        {
            FloatMenuOption customOption = new FloatMenuOption("Extract Astra Brain", () =>
            {
                Job job = new Job(AstraDefOf.job_astra_brain_extract);
                job.targetA = targetPawnCorpse;
                selPawn.jobs.TryTakeOrderedJob(job);
            });

            return customOption;
        }
    }

    [StaticConstructorOnStartup]
    public static class ModPatcher
    {
        static ModPatcher()
        {
            var harmony = new Harmony("redizit.astratech");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetFloatMenuOptions")]
    public static class Pawn_GetFloatMenuOptions_Patch
    {
        public static void Postfix(Pawn __instance, Pawn selPawn, ref IEnumerable<FloatMenuOption> __result)
        {
            __result = FloatMenuOptionInjector.AddImplantOptions(__instance, selPawn, __result);
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor")]
    public static class Corpse_ChoicesAtFor_Patch
    {
        public static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> __result)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            Corpse corpse = c.GetFirstThing<Corpse>(pawn.Map);

            if (corpse != null)
            {
                __result.Add(FloatMenuOptionInjector.ExtractImplant(corpse, pawn));
            }
        }
    }
}
