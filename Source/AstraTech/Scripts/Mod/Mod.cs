using Verse;

namespace AstraTech
{
    public class Mod : Verse.Mod
    {
        public static ModSettings settings;

        public Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<ModSettings>();
        }
    }
}
