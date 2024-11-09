using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class ITab_Brain_Train : ITab_Brain_Copy
    {
        protected override bool IsCopyTab => false;

        public ITab_Brain_Train()
        {
            labelKey = "Train";
        }
    }
    public class ITab_Brain_Copy : ITab
    {
        public Building_AstraPawnMachine Building => (Building_AstraPawnMachine)SelThing;
        private HashSet<SkillDef> selectedSkills => Building.skillsToCopy;
        private Pawn BrainPawn => Building.brainInside.GetPawn();
        private AstraSchematics activeSkillCard => Building.schematicsInsideBank;

        protected virtual bool IsCopyTab => true;

        private Vector2 scrollPos;
        private Texture2D PassionMinorIcon, PassionMajorIcon, rightArrow;      
        private AstraSchematics[] availableSchematics;

        private int maxSkills = 3;

        public ITab_Brain_Copy()
        {
            labelKey = "Copy";
        }

        public override void OnOpen()
        {
            base.OnOpen();

            PassionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor");
            PassionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor");
            rightArrow = ContentFinder<Texture2D>.Get("arrow_right");

            if (Building.task == Building_AstraPawnMachine.Task.None)
            {
                selectedSkills.Clear();
            }

            OnSchematicsContentChanged();
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

            //
            // Header
            //
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Rect labelRect = new Rect(body.x, body.y, body.width, 32);
            Widgets.Label(labelRect, IsCopyTab ? "Persona-to-automaton copy" : "Persona module training");
            Text.Font = GameFont.Small;

            float y = labelRect.yMax;
            float spacing = 3;
            float brainBoxWidth = body.width / 2f - spacing;
            DrawBrainBox(body.x, y, brainBoxWidth, true);
            if (IsCopyTab) DrawBrainBox(body.x + body.width / 2f + spacing, y, brainBoxWidth, false);
            y += 22 + 42;



            //
            // Body
            //

            //
            // ScrollView label and action
            //
            y += 1;

            labelRect = new Rect(body.x, y, body.width, 26);

            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(labelRect, IsCopyTab ? "Available skills" : "Available skills and traits");

            Text.Anchor = TextAnchor.MiddleRight;
            Rect actionRect = new Rect(body.x + body.width / 2f + spacing, y, brainBoxWidth, labelRect.height - 1);


            if (Building.task != Building_AstraPawnMachine.Task.None)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(actionRect, "Time left: " + GenDate.ToStringTicksToPeriod(Building.ticksLeft));
            }
            else
            {
                if (IsCopyTab)
                {
                    if (Building.secondBrainInside != null)
                    {
                        if (selectedSkills.Count >= maxSkills)
                        {
                            if (Widgets.ButtonText(actionRect, "Start process"))
                            {
                                Building.StartTask_Copy();
                            }
                        }
                        else
                        {
                            Text.Anchor = TextAnchor.LowerRight;
                            Widgets.Label(labelRect, selectedSkills.Count + " / " + maxSkills);
                        }
                    }
                }
                else
                {

                }
            }

            Text.Anchor = 0;
            y = labelRect.yMax;


            
            //
            // ScrollView
            //
            Rect scrollOut = new Rect(body.x, y, body.width, body.height - y + body.y);

            if (IsCopyTab)
            {
                if (Building.brainInside == null)
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
                    DrawCopyTabScrollView(scrollOut);
                }
            }
            else
            {
                DrawTrainTabScrollView(scrollOut);
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

        private void DrawCopyTabScrollView(Rect scrollOut)
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

                Rect labelRect = new Rect(6, offset, lotRect.width, lotRect.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, skill.def.LabelCap + ", " + skill.levelInt + ", " + skill.passion.GetLabel());

                bool wasSelected = selectedSkills.Contains(skill.def);
                bool checkboxOn = wasSelected;
                bool isDisabled = (Building.task != Building_AstraPawnMachine.Task.None) || (wasSelected == false && selectedSkills.Count >= maxSkills);
                Widgets.Checkbox(lotRect.xMax - 26 - 6, lotRect.y, ref checkboxOn, size: 22, disabled: isDisabled);
                if (checkboxOn != wasSelected)
                {
                    if (wasSelected) selectedSkills.Remove(skill.def);
                    else selectedSkills.Add(skill.def);
                }

                offset += lotRect.height + 1;
            }

            Widgets.EndScrollView();
        }
        
        private void DrawTrainTabScrollView(Rect scrollOut)
        {
            int elementsCount = availableSchematics.Length;
            float scrollViewHeight = elementsCount * (26 + 1);
            Rect scrollView = new Rect(0, 0, scrollViewHeight > scrollOut.height ? scrollOut.width - 16 : scrollOut.width, scrollViewHeight);
            Widgets.BeginScrollView(scrollOut, ref scrollPos, scrollView);

            float offset = 0;
            foreach (Thing item in availableSchematics)
            {
                if (item is AstraSchematics_Skill card)
                {
                    SkillRecord brainSkill = null;
                    if (Building.brainInside != null)
                    {
                        brainSkill = BrainPawn.skills.GetSkill(card.skillDef);
                    }


                    float y = offset;
                    Rect lotRect = new Rect(0, y, scrollView.width, 26);

                    Rect lotExpandedRect = lotRect;
                    if (activeSkillCard == card)
                    {
                        lotExpandedRect.height *= 2;

                        Widgets.DrawBoxSolid(lotExpandedRect, Colors.buttonDefault);
                        Widgets.DrawHighlightIfMouseover(lotExpandedRect);

                        Rect timeRect = new Rect(0, lotRect.y + lotRect.height, lotRect.width - 6, lotRect.height);
                        Text.Anchor = TextAnchor.MiddleRight;
                        Widgets.Label(timeRect, "Time left: " + GenDate.ToStringTicksToPeriod(Building.ticksLeft));
                    }
                    else
                    {
                        Widgets.DrawBoxSolid(lotRect, Colors.buttonDefault);
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
                                Building.StopTask_Training();
                            }
                        }
                    }
                    else if (brainSkill.levelInt < card.level)
                    {
                        if (Widgets.ButtonText(actionRect, "Start training"))
                        {
                            Building.StartTask_SkillTraining(card);
                        }
                    }

                    offset += lotExpandedRect.height + 1;
                }
                else if (item is AstraSchematics_Trait trait)
                {
                    bool hasSameTrait = true;
                    if (Building.brainInside != null)
                    {
                        hasSameTrait = BrainPawn.story.traits.GetTrait(trait.traitDef) != null;
                    }


                    float y = offset;
                    Rect lotRect = new Rect(0, y, scrollView.width, 26);

                    Rect lotExpandedRect = lotRect;
                    if (activeSkillCard == item)
                    {
                        lotExpandedRect.height *= 2;

                        Widgets.DrawBoxSolid(lotExpandedRect, Colors.buttonDefault);
                        Widgets.DrawHighlightIfMouseover(lotExpandedRect);

                        Rect timeRect = new Rect(0, lotRect.y + lotRect.height, lotRect.width - 6, lotRect.height);
                        Text.Anchor = TextAnchor.MiddleRight;
                        Widgets.Label(timeRect, "Time left: " + GenDate.ToStringTicksToPeriod(Building.ticksLeft));
                    }
                    else
                    {
                        Widgets.DrawBoxSolid(lotRect, Colors.buttonDefault);
                        Widgets.DrawHighlightIfMouseover(lotRect);
                    }

                    float actionXOffset = 67;
                    Rect labelRect = new Rect(12, y, 120 + actionXOffset, lotRect.height);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(labelRect, trait.TraitLabel);
                    Text.Anchor = 0;

                    float x = 140 + actionXOffset;


                    Rect actionRect = new Rect(x + 6, y + 1, lotRect.width - x - 6 * 2, lotRect.height - 2);
                    if (activeSkillCard != null)
                    {
                        if (activeSkillCard == item)
                        {
                            if (Widgets.ButtonText(actionRect, "Stop training"))
                            {
                                Building.StopTask_Training();
                            }
                        }
                    }
                    else
                    {
                        if (AstraBrain.bodyRelatedTraits.Contains(trait.traitDef))
                        {
                            Text.Anchor = TextAnchor.MiddleLeft;
                            Widgets.Label(actionRect, "Not applicable");
                            Text.Anchor = 0;
                        }
                        else
                        {
                            if (hasSameTrait == false)
                            {
                                if (Widgets.ButtonText(actionRect, "Start training"))
                                {
                                    Building.StartTask_TraitTraining(trait);
                                }
                            }
                            else
                            {
                                Text.Anchor = TextAnchor.MiddleCenter;
                                Widgets.Label(actionRect, "Already has");
                                Text.Anchor = 0;
                            }
                        }
                    }

                    offset += lotExpandedRect.height + 1;
                }
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

        private void OnSchematicsContentChanged()
        {
            availableSchematics = Building.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading
                .SelectMany(t => ((Building_AstraSchematicsBank)t).GetDirectlyHeldThings())
                .Where(t => t is AstraSchematics_Skill || t is AstraSchematics_Trait)
                .Cast<AstraSchematics>().ToArray();
        }
    }
}
