using RimWorld;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class ITab_Brain_Character : ITab
    {
        private Pawn PawnToShowInfoAbout
        {
            get
            {
                Pawn pawn = ((IPawnContainer)SelThing).GetPawn();
                return pawn;
            }
        }

        public override bool IsVisible => PawnToShowInfoAbout != null;

        public ITab_Brain_Character()
        {
            labelKey = "TabCharacter";
            tutorTag = "Character";
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();
            size = CharacterCardUtility.PawnCardSize(PawnToShowInfoAbout) + new Vector2(17f, 17f) * 2f;
        }

        protected override void FillTab()
        {
            UpdateSize();
            Vector2 vector = CharacterCardUtility.PawnCardSize(PawnToShowInfoAbout);
            CharacterCardUtility.DrawCharacterCard(new Rect(17f, 17f, vector.x, vector.y), PawnToShowInfoAbout);
        }
    }
}
