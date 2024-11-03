using HarmonyLib;
using Verse;

namespace AstraTech
{
    public class Hediff_AstraBrainSocket : Hediff_Implant
    {
        public AstraBrain brain;

        public override void Tick()
        {
            base.Tick();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref brain, nameof(brain));
        }

        public void InsertBrain(AstraBrain brain)
        {
            this.brain = brain;
            brain.CopyInnerPawnToBlank(pawn);
            Severity = 1;
        }
        public void ExtractBrain()
        {
            brain.CopyReplicantToInnerPawn(pawn);

            ThingWithComps_AstraBrain item = (ThingWithComps_AstraBrain)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("astra_brain"));
            item.brain = brain;
            GenPlace.TryPlaceThing(item, pawn.Position - new IntVec3(1, 0, 0), pawn.MapHeld, ThingPlaceMode.Near);

            this.brain = null;
            AstraBrain.ClearPawn(pawn);
            Severity = 0.5f;
        }
    }

    public interface IPawnContainer
    {
        Pawn GetPawn();
    }
}
