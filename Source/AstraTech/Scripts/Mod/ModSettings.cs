using Verse;

namespace AstraTech
{
    public class ModSettings : Verse.ModSettings
    {
        public bool showAgain_skillExtractionWarning = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref showAgain_skillExtractionWarning, nameof(showAgain_skillExtractionWarning), true);
        }
    }
}
