using System;
using Verse;

namespace AstraTech
{
    public static class MessageHelper
    {
        public static void ShowCustomMessage(Pawn victim, Action action)
        {
            //if (!Mod.settings.showAgain_skillExtractionWarning)
            //    return;

            string messageText = $"Skill Extractinon allows to extract only 1 skill before brain of {victim.NameFullColored} will be destroyed.";

            Find.WindowStack.Add(new Dialog_MessageBox(
                messageText,
                $"I understand, that {victim.NameShortColored} will die", action,
                "No, don't do it", null,
                "Skill extraction confirmation", true, action));
        }
    }
}
