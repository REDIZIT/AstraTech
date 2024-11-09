using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace AstraTech
{
    public class Hediff_AstraBrainSocket : Hediff_Implant
    {
        public AstraBrain brain;
        public override string LabelInBrackets => brain == null ? "empty" : brain.innerPawn.Name + "'s persona installed";

        //private static FieldInfo cachedThoughtsField = typeof(SituationalThoughtHandler).GetField("cachedThoughts", BindingFlags.Instance | BindingFlags.NonPublic);

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            AstraBrain.ClearPawn(pawn);
        }
        public override void Tick()
        {
            base.Tick();

            //if (brain != null && brain.IsAutomaton)
            //{
            //    var thoughts = pawn.needs.mood.thoughts;

            //    // Clear memories (saw a corpse, ate good meal)
            //    thoughts.memories.Memories.Clear();

            //    //// Clear situational thoughts (dark, cold, wet)
            //    //List<Thought_Situational> replicantSituationalThoughts = (List<Thought_Situational>)cachedThoughtsField.GetValue(thoughts.situational);
            //    //replicantSituationalThoughts.Clear();
            //    ////Log.Message(replicantSituationalThoughts.Count);
            //    //cachedThoughtsField.SetValue(thoughts.situational, replicantSituationalThoughts);/* // Be aware to not copy Ref to list, but elements of list*/
            //}
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref brain, nameof(brain));
        }

        // Invoked before Notify_PawnDied (where needs = null)
        public override void Notify_PawnKilled()
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(AstraDefOf.thought_astra_brain_killed);

            // Brain can be null if Blank died
            if (brain != null)
            {
                brain.CopyReplicantToInnerPawn(pawn);
            }

            base.Notify_PawnKilled();
        }


        public void InsertBrain(AstraBrain brain)
        {
            this.brain = brain;
            brain.CopyInnerPawnToBlank(pawn);
            Severity = 1;

            if (brain.IsAutomaton)
            {
                pawn.health.AddHediff(AstraDefOf.astra_brain_unstable_wear, pawn.health.hediffSet.GetBrain());
            }
        }
        public void ExtractBrain()
        {
            // Make sure needs are not null
            if (pawn.Dead == false)
            {
                if (pawn.InMentalState)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AstraDefOf.thought_astra_brain_extracted_while_breakdown);
                }

                brain.CopyReplicantToInnerPawn(pawn);
            }


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
