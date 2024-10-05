using RimWorld;
using Verse;

namespace AstraTech
{
    public class IngredientValueGetter_ForMatterCombined : IngredientValueGetter
    {
        public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
        {
            return ing.GetBaseCount() + "x " + "desc 123" + " (" + ing.filter.Summary + ")";
        }

        public override float ValuePerUnitOf(ThingDef t)
        {
            float marketValue = t.GetStatValueAbstract(StatDefOf.MarketValue);
            float mass = t.GetStatValueAbstract(StatDefOf.Mass);

            return mass + marketValue;
        }
    }
}
