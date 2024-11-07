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
        public static AstraBrainDef astra_brain;
        public static ThingDef astra_cards_bank, astra_schematics_skill, astra_schematics_trait;
        public static HediffDef astra_brain_socket, astra_brain_unstable_wear;

        public static BackstoryDef astra_blank, astra_blank_adult;

        public static ThoughtDef thought_astra_brain_insert, thought_stra_brain_skill_trained, thought_stra_brain_trait_trained, thought_stra_brain_extracted_while_breakdown, thought_stra_brain_killed;


        // Vanilla Rimworld needs (somewhy there is almost nothing inside NeedDefOf)
        public static NeedDef Mood, Food, Rest, Joy, Beauty, Comfort, Outdoors, Indoors, DrugDesire, RoomSize;
    }

}