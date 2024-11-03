using Verse;

namespace AstraTech
{
    public class Building_AstraCardsBank : Building
    {
        public ThingComp_AstraSkillCard SkillCardComp => skillCard == null ? null : skillCard.TryGetComp<ThingComp_AstraSkillCard>();

        public Thing skillCard;

        public bool HasFreeSpace => skillCard == null;

        public void SetCard(Thing card)
        {
            skillCard = card;
        }
    }
}
