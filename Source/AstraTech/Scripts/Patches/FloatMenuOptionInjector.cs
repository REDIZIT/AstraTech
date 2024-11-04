using System.Collections.Generic;
using Verse;

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
                GenJob.TryGiveJob<JobDriver_ExtractBrainFromReplicant>(selPawn, targetPawn);
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
                GenJob.TryGiveJob<JobDriver_ExtractBrainFromReplicant>(selPawn, targetPawnCorpse);
            });

            return customOption;
        }
    }
}
