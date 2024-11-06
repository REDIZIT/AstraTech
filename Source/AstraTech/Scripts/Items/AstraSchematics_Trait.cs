using RimWorld;
using System.Linq;
using System.Reflection;
using Verse;

namespace AstraTech
{
    public class AstraSchematics_Trait : AstraSchematics
    {
        public TraitDef traitDef;
        public int degree;

        public string TraitLabel => traitDef.DataAtDegree(degree).LabelCap;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (traitDef == null)
            {
                FieldInfo field = typeof(TraitDefOf).GetFields(BindingFlags.Static | BindingFlags.Public).Where(f => f.GetCustomAttribute<MayRequireAttribute>() == null).RandomElement();

                traitDef = (TraitDef)field.GetValue(null);
                degree = 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref traitDef, nameof(traitDef));
            Scribe_Values.Look(ref degree, nameof(degree));
        }

        public override string GetInspectString()
        {
            return TraitLabel;
        }

        public override string Label
        {
            get
            {
                if (traitDef == null) return base.Label + " (empty)";
                else return base.Label + " (" + TraitLabel + ")";
            }
        }

    }
}
