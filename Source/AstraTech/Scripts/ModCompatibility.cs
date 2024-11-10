using Verse;
using VanillaPsycastsExpanded;
using System.Linq;
using VFECore.Abilities;

namespace AstraTech
{
    [StaticConstructorOnStartup]
    public static class ModCompatibility
    {
        public static bool VPEIsActive;

        static ModCompatibility()
        {
            VPEIsActive = ModsConfig.IsActive("VanillaExpanded.VPsycastsE");
        }

        public static void CopyVPE(Pawn source, Pawn target)
        {
            Hediff_PsycastAbilities targetAbilities = PsycastUtility.Psycasts(target);
            Hediff_PsycastAbilities sourceAbilities = PsycastUtility.Psycasts(source);

            if (targetAbilities == null) throw new System.Exception("Failed to get Hediff_PsycastAbilities for target pawn: No hediff found");
            if (sourceAbilities == null) throw new System.Exception("Failed to get Hediff_PsycastAbilities for source pawn: No hediff found");

            targetAbilities.psysets.Clear();
            targetAbilities.psysets.AddRange(sourceAbilities.psysets);

            CompAbilities targetCompAbilities = target.GetComp<CompAbilities>();
            CompAbilities sourceCompAbilities = source.GetComp<CompAbilities>();

            targetCompAbilities.LearnedAbilities.RemoveAll(IsPsyAbility);
            foreach (Ability ability in sourceCompAbilities.LearnedAbilities.Where(IsPsyAbility))
            {
                targetCompAbilities.GiveAbility(ability.def);
            }

            targetAbilities.unlockedMeditationFoci.Clear();
            targetAbilities.unlockedMeditationFoci.AddRange(sourceAbilities.unlockedMeditationFoci);

            targetAbilities.unlockedPaths.Clear();
            targetAbilities.unlockedPaths.AddRange(sourceAbilities.unlockedPaths);

            targetAbilities.SetLevelTo(sourceAbilities.level);
            targetAbilities.points = sourceAbilities.points;
            targetAbilities.experience = sourceAbilities.experience;
        }

        public static void RemoveVPE(Pawn target)
        {
            PsycastUtility.Psycasts(target).Reset();
            target.health.RemoveHediff(PsycastUtility.Psycasts(target));

            CompAbilities targetCompAbilities = target.GetComp<CompAbilities>();
            targetCompAbilities.LearnedAbilities.RemoveAll(IsPsyAbility);
        }

        // HUMAN, REMEMBER: JIT compiler compiles lamda-functions even if them are unreachable; Do not use VPE classes and structs in method params (thats why here a is object, but no Ability)
        private static bool IsPsyAbility(object a)
        {
            return AbilityExtensionPsycastUtility.Psycast(((Ability)a).def) != null;
        }
    }
}
