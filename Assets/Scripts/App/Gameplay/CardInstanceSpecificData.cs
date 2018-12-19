using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class CardInstanceSpecificData
    {
        public int Damage { get; protected set; }

        public int Health { get; protected set; }

        public Enumerators.SetType CardSetType { get; set; }

        public Enumerators.CardType CardType { get; protected set; }

        public int Cost { get; set; }

        public CardInstanceSpecificData(IReadOnlyCard card)
            : this(
                card.Damage,
                card.Health,
                card.CardSetType,
                card.CardType,
                card.Cost)
        {
        }

        public CardInstanceSpecificData(int damage, int health, Enumerators.SetType cardSetType, Enumerators.CardType cardType, int cost)
        {
            Damage = damage;
            Health = health;
            CardSetType = cardSetType;
            CardType = cardType;
            Cost = cost;
        }
    }
}