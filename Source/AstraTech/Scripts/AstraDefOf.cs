using RimWorld;
using Verse;

namespace AstraTech
{
    [DefOf]
    public static class AstraDefOf
    {
        public static JobDef job_astra_blueprint_encode, job_astra_blueprint_extract, job_astra_start_printing;
        public static JobDef job_astra_slime_begin_transform;
        public static ThingDef astra_schematics_item, astra_schematics_item_empty, astra_slime;
        public static ThingDef astra_matter_organic, astra_matter_non_organic, astra_matter_merged;
        public static ThoughtDef astra_held_slime_disgust, astra_held_slime_adore;
        public static HistoryEventDef astra_touched_slime;


        public static JobDef job_astra_haul_and_do;
        public static ThingDef astra_brain, astra_cards_bank, astra_schematics_skill;
        public static HediffDef astra_brain_socket;


        // Vanilla Rimworld needs (somewhy there is almost nothing inside NeedDefOf)
        public static NeedDef Mood, Food, Rest, Joy, Beauty, Comfort, Outdoors, Indoors, DrugDesire, RoomSize;
    }

}