using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AstraTech
{
    public class AstraSchematics_Skill : AstraSchematics
    {
        public SkillDef skillDef;
        public int level;
        public Passion passion;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (skillDef == null)
            {
                skillDef = EnumerateAllSkils().RandomElement();
                level = Rand.RangeInclusive(0, 20);
                passion = (Passion)Rand.RangeInclusive(0, 2);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref skillDef, nameof(skillDef));
            Scribe_Values.Look(ref level, nameof(level));
            Scribe_Values.Look(ref passion, nameof(passion));
        }

        public override string GetInspectString()
        {
            return skillDef.LabelCap + ", " + level + ", " + passion.GetLabel();
        }

        public override string Label
        {
            get
            {
                if (skillDef == null) return base.Label + " (empty)";
                else return base.Label + " (" + skillDef.LabelCap + ", " + level + ")";
            }
        }

        private IEnumerable<SkillDef> EnumerateAllSkils()
        {
            yield return SkillDefOf.Construction;
            yield return SkillDefOf.Plants;
            yield return SkillDefOf.Intellectual;
            yield return SkillDefOf.Mining;
            yield return SkillDefOf.Shooting;
            yield return SkillDefOf.Melee;
            yield return SkillDefOf.Social;
            yield return SkillDefOf.Animals;
            yield return SkillDefOf.Cooking;
            yield return SkillDefOf.Medicine;
            yield return SkillDefOf.Artistic;
            yield return SkillDefOf.Crafting;
        }
    }
}
