using System.Collections.Generic;
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

            if (clickedPawn.health.hediffSet.TryGetHediff(out Hediff_AstraBrainSocket socket) && socket.brain != null)
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
                customOption.Label = "Can not extract it's own brain";
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
}
