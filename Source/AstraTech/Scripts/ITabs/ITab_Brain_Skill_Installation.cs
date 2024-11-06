using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class ITab_Brain_Skill_Installation : ITab
    {
        public Building_AstraPawnMachine Building => (Building_AstraPawnMachine)SelThing;
        public Pawn BrainPawn => Building.brainInside.GetPawn();
        public override bool IsVisible => Building.brainInside != null;

        private Vector2 scrollPos;
        private AstraSchematics_Skill activeSkillCard => Building.schematicsInsideBank;
        private AstraSchematics_Skill[] skillCards;

        private Texture2D PassionMinorIcon, PassionMajorIcon, rightArrow;

        public ITab_Brain_Skill_Installation()
        {
            labelKey = "Train";
        }

        public override void OnOpen()
        {
            base.OnOpen();

            PassionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor");
            PassionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor");
            rightArrow = ContentFinder<Texture2D>.Get("arrow_right");

            OnSchematicsContentChanged();
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();
            size = new Vector2(432, 300);
        }

        protected override void FillTab()
        {
            UpdateSize();

            Rect body = new Rect(17, 17, size.x - 17 * 2, size.y - 17 * 2);

            float y = body.y;
            Rect labelRect = new Rect(body.x, y, body.width, 24);
            Widgets.Label(labelRect, "Available skill cards");

            Rect scrollView = new Rect(body.x, labelRect.yMax, body.width, body.height - labelRect.yMax);
            DrawCards(scrollView, skillCards.OrderByDescending(c => c == activeSkillCard));
        }

        private void OnSchematicsContentChanged()
        {
            skillCards = Building.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading.SelectMany(t => ((Building_AstraSchematicsBank)t).GetDirectlyHeldThings()).Where(t => t is AstraSchematics_Skill).Cast<AstraSchematics_Skill>().ToArray();
        }

        private void DrawCards(Rect body, IEnumerable<Thing> items)
        {
            Rect scrollView = new Rect(body.x, body.y, body.width, body.height);

            Widgets.BeginScrollView(scrollView, ref scrollPos, new Rect(0, 0, scrollView.width, skillCards.Length * (25 + 1)));

            float offset = 0;
            foreach (Thing item in items)
            {
                AstraSchematics_Skill card = (AstraSchematics_Skill)item;

                SkillRecord brainSkill = null;
                if (Building.brainInside != null)
                {
                    brainSkill = BrainPawn.skills.GetSkill(card.skillDef);
                }


                float y = offset;
                Rect lotRect = new Rect(0, y, scrollView.width, 25);
                
                Rect lotExpandedRect = lotRect;
                if (activeSkillCard == card)
                {
                    lotExpandedRect.height *= 2;

                    Widgets.DrawBoxSolid(lotExpandedRect, new ColorInt(32, 35, 38).ToColor);
                    Widgets.DrawHighlightIfMouseover(lotExpandedRect);

                    Rect timeRect = new Rect(0, lotRect.y + lotRect.height, lotRect.width - 6, lotRect.height);
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(timeRect, "Time left: " + GenDate.ToStringTicksToPeriod(Building.ticksLeft));
                }
                else
                {
                    Widgets.DrawBoxSolid(lotRect, new ColorInt(32, 35, 38).ToColor);
                    Widgets.DrawHighlightIfMouseover(lotRect);
                }

                Rect labelRect = new Rect(12, y, 120, lotRect.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, card.skillDef.LabelCap);

                Text.Anchor = TextAnchor.MiddleLeft;
                float x = 140;

                Rect passionIconRect = new Rect(x, y + 4, 19, 19);
                Rect passionLabelRect = new Rect(passionIconRect.xMax + 2, lotRect.y, 24, lotRect.height);
                if (brainSkill != null)
                {
                    Widgets.DrawTextureFitted(passionIconRect, GetPassionIcon(brainSkill.passion), 1);
                    Widgets.Label(passionLabelRect, brainSkill.levelInt.ToString());
                }
                x = passionLabelRect.xMax;


                Rect arrowRect = new Rect(x, y + 3, 19, 19);
                Widgets.DrawTextureFitted(arrowRect, rightArrow, 1);

                x += arrowRect.width;

                passionIconRect = new Rect(x, lotRect.y + 4, 19, 19);
                Widgets.DrawTextureFitted(passionIconRect, GetPassionIcon(card.passion), 1);
                passionLabelRect = new Rect(passionIconRect.xMax + 2, lotRect.y, 24, lotRect.height);
                Widgets.Label(passionLabelRect, card.level.ToString());


                x = passionLabelRect.xMax;
                Rect actionRect = new Rect(x + 6, y + 1, lotRect.width - x - 6 * 2, lotRect.height - 2);
                if (activeSkillCard != null)
                {
                    if (activeSkillCard == card)
                    {
                        if (Widgets.ButtonText(actionRect, "Stop training"))
                        {
                            Building.StopTask_SkillTraining();
                        }
                    }
                }
                else
                {
                    if (Widgets.ButtonText(actionRect, "Start training"))
                    {
                        Building.StartTask_SkillTraining(card);
                    }
                }

                offset += lotExpandedRect.height + 1;
            }

            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.EndScrollView();
        }

        private Texture2D GetPassionIcon(Passion p)
        {
            switch (p)
            {
                case Passion.None: return BaseContent.ClearTex;
                case Passion.Minor: return PassionMinorIcon;
                case Passion.Major: return PassionMajorIcon;
                default: throw new System.Exception("Unknown passion: " + p.ToString());
            }
        }
    }
}
