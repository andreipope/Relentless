using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class CardInstanceSpecificData
    {
        public int Damage { get; set; }

        public int Health { get; set; }

        public Enumerators.Faction CardSetType { get; set; }

        public Enumerators.CardType CardType { get; protected set; }

        public int Cost { get; set; }

        public CardInstanceSpecificData(IReadOnlyCard card)
            : this(
                card.Damage,
                card.Defense,
                card.Faction,
                card.CardType,
                card.Cost)
        {
        }

        public CardInstanceSpecificData(int damage, int health, Enumerators.Faction cardSetType, Enumerators.CardType cardType, int cost)
        {
            Damage = damage;
            Health = health;
            CardSetType = cardSetType;
            CardType = cardType;
            Cost = cost;
        }
    }
}
