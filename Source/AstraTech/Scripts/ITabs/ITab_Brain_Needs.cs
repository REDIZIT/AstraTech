using RimWorld;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class ITab_Brain_Needs : ITab
    {
        private Vector2 thoughtScrollPosition;

        protected override Pawn SelPawn => ((IPawnContainer)SelThing).GetPawn();

        public override bool IsVisible
        {
            get
            {
                if (SelPawn.RaceProps.Animal && SelPawn.Faction == null)
                {
                    return false;
                }

                if (SelPawn.RaceProps.Insect && SelPawn.Faction != Faction.OfPlayer)
                {
                    return false;
                }

                CompHoldingPlatformTarget compHoldingPlatformTarget = SelPawn.TryGetComp<CompHoldingPlatformTarget>();
                if (compHoldingPlatformTarget != null && compHoldingPlatformTarget.CurrentlyHeldOnPlatform)
                {
                    return false;
                }

                if (SelPawn.needs != null)
                {
                    return SelPawn.needs.AllNeeds.Count > 0;
                }

                return false;
            }
        }

        public ITab_Brain_Needs()
        {
            labelKey = "TabNeeds";
            tutorTag = "Needs";
        }

        public override void OnOpen()
        {
            thoughtScrollPosition = default(Vector2);
        }

        protected override void FillTab()
        {
            NeedsCardUtility.DoNeedsMoodAndThoughts(new Rect(0f, 0f, size.x, size.y), SelPawn, ref thoughtScrollPosition);
        }

        protected override void UpdateSize()
        {
            size = NeedsCardUtility.GetSize(SelPawn);
        }
    }
}
