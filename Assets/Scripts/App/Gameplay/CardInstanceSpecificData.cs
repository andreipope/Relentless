using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class CardInstanceSpecificData : IReadOnlyCardInstanceSpecificData
    {
        public int Damage { get; set; }

        public int Defense { get; set; }

        public Enumerators.Faction Faction { get; set; }

        public Enumerators.CardType CardType { get; protected set; }

        public int Cost { get; set; }

        public IList<AbilityData> Abilities { get; set; }


        public CardInstanceSpecificData(IReadOnlyCard card)
            : this(
                card.Damage,
                card.Defense,
                card.Faction,
                card.Type,
                card.Cost,
                card.Abilities)
        {
        }
        public CardInstanceSpecificData(
            int damage,
            int defense,
            Enumerators.Faction faction,
            Enumerators.CardType cardType,
            int cost,
            IReadOnlyList<AbilityData> abilities)
        {
            CopyFrom(damage, defense, faction, cardType, cost, abilities);
        }

        public void CopyFrom(IReadOnlyCard card)
        {
            CopyFrom(
                card.Damage,
                card.Defense,
                card.Faction,
                card.Type,
                card.Cost,
                card.Abilities);
        }

        private void CopyFrom(
            int damage,
            int defense,
            Enumerators.Faction faction,
            Enumerators.CardType cardType,
            int cost,
            IReadOnlyList<AbilityData> abilities)
        {
            Damage = damage;
            Defense = defense;
            Faction = faction;
            CardType = cardType;
            Cost = cost;
            Abilities = abilities.Select(a => new AbilityData(a)).ToList();
        }
    }
}
