using System.Collections.Generic;
using Verse;

namespace AstraTech
{
    public class Command_ExtendedAction : Command_Action
    {
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => rightClickFloatMenuOptions;

        public IEnumerable<FloatMenuOption> rightClickFloatMenuOptions;
    }
}
