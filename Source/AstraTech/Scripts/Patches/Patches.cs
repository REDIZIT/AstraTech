using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AstraTech
{
    [StaticConstructorOnStartup]
    public static class ModPatcher
    {
        static ModPatcher()
        {
            var harmony = new Harmony("REDIZIT.AstraTech");
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

            if (corpse != null && corpse.InnerPawn.health.hediffSet.TryGetHediff(out Hediff_AstraBrainSocket socket) && socket.brain != null)
            {
                __result.Add(FloatMenuOptionInjector.ExtractImplant(corpse, pawn));
            }
        }
    }

    [HarmonyPatch(typeof(Job), "MakeDriver")]
    public static class Job_MakeDriver_Patch
    {
        public static bool Prefix(Job __instance, Pawn driverPawn, ref JobDriver __result)
        {
            if (__instance.def == AstraDefOf.job_astra_haul_and_do)
            {
                // GenJob will create custom JobDriver and place it into cachedDriver field
                // To make sure, that Job will not overwrite it by MakeDriver, we block this method and force to use cachedDriver
                __result = __instance.GetCachedDriverDirect;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetDisabledWorkTypes")]
    public static class Patch_GetDisabledWorkTypes
    {
        static void Postfix(Pawn __instance, ref List<WorkTypeDef> __result, bool permanentOnly)
        {
            if (__instance.health.hediffSet.TryGetHediff(out Hediff_AstraBrainSocket socket) && socket.brain != null && socket.brain.IsAutomaton)
            {
                if (socket.brain.availableSkills != null && socket.brain.availableSkills.Count > 0)
                {
                    foreach (WorkTypeDef workType in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                    {
                        if (workType.relevantSkills.All(s => socket.brain.availableSkills.Contains(s)) == false)
                        {
                            if (__result.Contains(workType) == false)
                            {
                                __result.Add(workType);
                            }
                        }
                    }
                }
            }
        }
    }
}
