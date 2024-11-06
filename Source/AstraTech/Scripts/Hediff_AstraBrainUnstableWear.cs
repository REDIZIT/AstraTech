using RimWorld;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class Hediff_AstraBrainUnstableWear : Hediff
    {
        public override string Label => CurStage.label + $" ({Mathf.Floor(Severity * 100)}%)";

        private AstraBrain brain;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            brain = pawn.health.hediffSet.GetFirstHediff<Hediff_AstraBrainSocket>().brain;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref brain, nameof(brain));
        }
        public override void Tick()
        {
            base.Tick();

            if (brain.unstableWorktimeInTicksLeft > 0)
            {
                brain.unstableWorktimeInTicksLeft--;
            }

            Severity = 1 - (brain.unstableWorktimeInTicksLeft / (brain.Def.unstableWorktimeInDays * GenDate.TicksPerDay));
        }
    }
}
