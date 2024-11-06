using Verse;

namespace AstraTech
{
    public class Hediff_AstraBrainSocket : Hediff_Implant
    {
        public AstraBrain brain;
        public override string LabelInBrackets => brain == null ? "empty" : brain.innerPawn.Name + "'s persona installed";

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            AstraBrain.ClearPawn(pawn);
        }
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

            if (brain.IsUnstable)
            {
                pawn.health.AddHediff(AstraDefOf.astra_brain_unstable_wear, pawn.health.hediffSet.GetBrain());
            }
        }
        public void ExtractBrain()
        {
            brain.CopyReplicantToInnerPawn(pawn);

            //AstraBrain item = (AstraBrain)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("astra_brain"));
            //item.brain = brain;
            GenPlace.TryPlaceThing(brain, pawn.Position - new IntVec3(0, 0, 1), pawn.MapHeld, ThingPlaceMode.Near);

            brain = null;
            AstraBrain.ClearPawn(pawn);
            Severity = 0.5f;

            if (pawn.health.hediffSet.TryGetHediff(out Hediff_AstraBrainUnstableWear wear))
            {
                pawn.health.RemoveHediff(wear);
            }
        }
    }
}
