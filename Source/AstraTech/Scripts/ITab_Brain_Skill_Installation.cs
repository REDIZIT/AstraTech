using RimWorld;
using System.Collections.Generic;
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
        private ThingComp_AstraSkillCard activeSkillCard => Building.activeSkillCard;
        private List<ThingComp_AstraSkillCard> skillCards;

        private static Texture2D PassionMinorIcon, PassionMajorIcon, rightArrow;

        public override void OnOpen()
        {
            base.OnOpen();

            labelKey = "Train";

            PassionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor");
            PassionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor");
            rightArrow = ContentFinder<Texture2D>.Get("arrow_right");

            skillCards = new List<ThingComp_AstraSkillCard>()
            {
                new ThingComp_AstraSkillCard()
                {
                    skillDef = SkillDefOf.Construction,
                    level = 19,
                    passion = Passion.Minor,
                },
                new ThingComp_AstraSkillCard()
                {
                    skillDef = SkillDefOf.Melee,
                    level = 3,
                    passion = Passion.Major,
                },
                new ThingComp_AstraSkillCard()
                {
                    skillDef = SkillDefOf.Shooting,
                    level = 5,
                    passion = Passion.None,
                },
            };
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
            DrawCards(scrollView, skillCards);
        }

        private void DrawCards(Rect body, IEnumerable<ThingComp_AstraSkillCard> cards)
        {
            Rect scrollView = new Rect(body.x, body.y, body.width, body.height);

            Widgets.BeginScrollView(scrollView, ref scrollPos, new Rect(0, 0, scrollView.width, skillCards.Count * (25 + 1)));

            float offset = 0;
            foreach (ThingComp_AstraSkillCard card in cards)
            {
                SkillRecord brainSkill = BrainPawn.skills.GetSkill(card.skillDef);


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
                Widgets.DrawTextureFitted(passionIconRect, PassionMajorIcon, 1);
                Rect passionLabelRect = new Rect(passionIconRect.xMax + 2, lotRect.y, 24, lotRect.height);
                Widgets.Label(passionLabelRect, brainSkill.levelInt.ToString());


                x = passionLabelRect.xMax;

                Rect arrowRect = new Rect(x, y + 3, 19, 19);
                Widgets.DrawTextureFitted(arrowRect, rightArrow, 1);

                x += arrowRect.width;

                passionIconRect = new Rect(x, lotRect.y + 4, 19, 19);
                Widgets.DrawTextureFitted(passionIconRect, PassionMinorIcon, 1);
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
    }
}
