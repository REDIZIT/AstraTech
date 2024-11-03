using RimWorld;
using Verse;

namespace AstraTech
{
    public class CompProperties_AstraSkillCard : CompProperties
    {
        public CompProperties_AstraSkillCard()
        {
            compClass = typeof(ThingComp_AstraSkillCard);
        }
    }

    public class ThingComp_AstraSkillCard : ThingComp
    {
        public SkillDef skillDef;
        public int level;
        public Passion passion;

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            if (skillDef == null)
            {
                skillDef = SkillDefOf.Construction;
                level = 20;
                passion = Passion.Major;
            }
        }
    }
}
