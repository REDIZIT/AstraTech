using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

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
                skillDef = EnumerateAllSkils().RandomElement();
                level = Rand.RangeInclusive(0, 20);
                passion = (Passion)Rand.RangeInclusive(0, 2);
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var o in base.CompFloatMenuOptions(selPawn))
            {
                yield return o;
            }

            yield return new FloatMenuOption("Carry to a bank", () =>
            {
                Job job = new Job(AstraDefOf.job_astra_carry_card_to_bank);
                job.targetA = parent;

                job.targetB = GenClosest.ClosestThingReachable(parent.Position, parent.MapHeld, new ThingRequest()
                {
                    singleDef = AstraDefOf.astra_cards_bank
                }, PathEndMode.ClosestTouch, TraverseParms.For(selPawn), validator: (t) => t is Building_AstraCardsBank bank && bank.HasFreeSpace && selPawn.CanReserve(t));

                selPawn.jobs.TryTakeOrderedJob(job);
            });
        }

        public override string CompInspectStringExtra()
        {
            return skillDef.LabelCap + ", " + level + ", " + passion.GetLabel();
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
