using RimWorld;
using Verse;

namespace AstraTech
{
    public class AstraBrain : IExposable, IPawnContainer
    {
        public Pawn pawn;

        public static AstraBrain CopyBrain(Pawn p)
        {
            AstraBrain b = new AstraBrain();

            b.pawn = Building_AstraPawnMachine.CreateBlank();

            b.pawn.Name = p.Name;

            //b.skills = new List<SkillRecord>();
            foreach (SkillRecord s in p.skills.skills)
            {
                SkillRecord pawnS = b.pawn.skills.GetSkill(s.def);
                pawnS.Level = s.levelInt;
                pawnS.passion = s.passion;
            }

            return b;
        }

        public static void ClearPawn(Pawn p)
        {
            p.Name = new NameSingle("Blank");

            foreach (SkillRecord pawnSkill in p.skills.skills)
            {
                pawnSkill.Level = 0;
                pawnSkill.passion = Passion.None;
            }
        }

        public void ApplyToPawn(Pawn p)
        {
            p.Name = pawn.Name;

            foreach (SkillRecord brainSkill in pawn.skills.skills)
            {
                SkillRecord pawnSkill = p.skills.GetSkill(brainSkill.def);
                pawnSkill.Level = brainSkill.levelInt;
                pawnSkill.passion = brainSkill.passion;
            }
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref pawn, nameof(pawn));
        }

        public Pawn GetPawn()
        {
            return pawn;
        }
    }
}
