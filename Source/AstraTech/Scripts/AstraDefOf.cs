using RimWorld;
using Verse;

namespace AstraTech
{
    [DefOf]
    public static class AstraDefOf
    {
        public static JobDef job_astra_blueprint_assign, job_astra_blueprint_encode, job_astra_blueprint_extract, job_astra_start_printing;
        public static JobDef job_astra_slime_begin_transform;
        public static JobDef job_astra_brain_extract;
        public static ThingDef astra_blueprint, astra_blueprint_empty, astra_slime;
        public static ThingDef astra_matter_organic, astra_matter_non_organic, astra_matter_merged;
        public static ThingDef astra_brain;
        public static ThoughtDef astra_held_slime_disgust, astra_held_slime_adore;
        //public static PreceptDef astra_precept;
        public static HistoryEventDef astra_touched_slime;
    }

}