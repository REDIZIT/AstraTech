using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class ITab_Brain_Copy : ITab
    {
        public Building_AstraPawnMachine Building => (Building_AstraPawnMachine)SelThing;
        public Pawn BrainPawn => Building.brainInside.GetPawn();
        public override bool IsVisible => true;

        private Vector2 scrollPos;

        private Texture2D PassionMinorIcon, PassionMajorIcon;

        private HashSet<SkillDef> selectedSkills => Building.skillsToCopy;
        private int maxSkills = 3;

        public ITab_Brain_Copy()
        {
            labelKey = "Copy";
        }

        public override void OnOpen()
        {
            base.OnOpen();

            PassionMinorIcon = WidgetsWork.PassionWorkboxMinorIcon;
            PassionMajorIcon = WidgetsWork.PassionWorkboxMajorIcon;

            selectedSkills.Clear();
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();
            size = new Vector2(432, 420);
        }

        protected override void FillTab()
        {
            UpdateSize();

            Rect body = new Rect(17, 17, size.x - 17 * 2, size.y - 17 * 2);

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Rect labelRect = new Rect(body.x, body.y, body.width, 32);
            Widgets.Label(labelRect, "Brain-to-brain copy");
            Text.Font = GameFont.Small;

            float y = labelRect.yMax;
            float spacing = 3;
            float brainBoxWidth = body.width / 2f - spacing;
            DrawBrainBox(body.x, y, brainBoxWidth, true);
            DrawBrainBox(body.x + body.width / 2f + spacing, y, brainBoxWidth, false);
            y += 22 + 42;


            y += 1;

            labelRect = new Rect(body.x, y, body.width, 26);

            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(labelRect, "Available skills");

            Text.Anchor = TextAnchor.MiddleRight;
            Rect actionRect = new Rect(body.x + body.width / 2f + spacing, y, brainBoxWidth, labelRect.height - 1);

            if (selectedSkills.Count >= maxSkills)
            {
                if (Building.task == Building_AstraPawnMachine.Task.BrainToBrainCopy)
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(actionRect, "Time left: " + GenDate.ToStringTicksToPeriod(Building.ticksLeft));
                }
                else
                {
                    if (Widgets.ButtonText(actionRect, "Start process"))
                    {
                        Building.StartTask_Copy();
                    }
                }
            }
            else
            {
                Text.Anchor = TextAnchor.LowerRight;
                Widgets.Label(labelRect, selectedSkills.Count + " / " + maxSkills);
            }

            Text.Anchor = 0;
            y = labelRect.yMax;


            

            

            Rect scrollOut = new Rect(body.x, y, body.width, body.height - y + body.y);

            if(Building.brainInside == null)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(scrollOut, "No Persona module");
            }
            else if (Building.secondBrainInside == null)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(scrollOut, "No Automaton module");
            }
            else
            {
                Pawn persona = Building.brainInside.GetPawn();
                var skills = persona.skills.skills;

                int elementsCount = skills.Count;
                float scrollViewHeight = elementsCount * (26 + 1);
                Rect scrollView = new Rect(0, 0, scrollViewHeight > scrollOut.height ? scrollOut.width - 16 : scrollOut.width, scrollViewHeight);
                Widgets.BeginScrollView(scrollOut, ref scrollPos, scrollView);

                float offset = 0;
                foreach (SkillRecord skill in skills)
                {
                    Rect lotRect = new Rect(0, offset, scrollView.width, 26);
                    Widgets.DrawBoxSolid(lotRect, Colors.buttonDefault);

                    labelRect = new Rect(6, offset, lotRect.width, lotRect.height);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(labelRect, skill.def.LabelCap + ", " + skill.levelInt + ", " + skill.passion.GetLabel());

                    bool wasSelected = selectedSkills.Contains(skill.def);
                    bool checkboxOn = wasSelected;
                    Widgets.Checkbox(lotRect.xMax - 26 - 6, lotRect.y, ref checkboxOn, size: 22, disabled: wasSelected == false && selectedSkills.Count >= maxSkills);
                    if (checkboxOn != wasSelected)
                    {
                        if (wasSelected) selectedSkills.Remove(skill.def);
                        else selectedSkills.Add(skill.def);
                    }

                    offset += lotRect.height + 1;
                }

                Widgets.EndScrollView();
            }


            Text.Anchor = 0;
            Text.Font = GameFont.Small;
        }
        private void DrawBrainBox(float x, float y, float width, bool isPersonaModule)
        {
            Rect labelRect = new Rect(x, y, width, 22);
            Text.Anchor = 0;
            Widgets.Label(labelRect, isPersonaModule ? "Persona module" : "Automaton module");
            y = labelRect.yMax;

            Rect body = new Rect(x, y, width, 42);
            Widgets.DrawBoxSolidWithOutline(body, Colors.buttonDefault, Colors.buttonOutline);

            AstraBrain brain = isPersonaModule ? Building.brainInside : Building.secondBrainInside;

            if (brain != null)
            {
                Rect iconRect = new Rect(body.x, body.y, 42, body.height);
                Widgets.DefIcon(iconRect, brain.def);

                labelRect = new Rect(iconRect.xMax, body.y, body.width - iconRect.width, body.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, brain.GetPawn().Name.ToStringShort);
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(body, "Not inserted");
            }
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
