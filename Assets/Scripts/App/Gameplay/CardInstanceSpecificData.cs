using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class CardInstanceSpecificData : IReadOnlyCardInstanceSpecificData
    {
        public int Damage { get; set; }

        public int Defense { get; set; }

        public Enumerators.Faction Faction { get; set; }

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

        public CardInstanceSpecificData(CardInstanceSpecificData source)
            : this(
                source.Damage,
                source.Defense,
                source.Faction,
                source.CardType,
                source.Cost)
        {
        }

        public CardInstanceSpecificData(int damage, int defense, Enumerators.Faction faction, Enumerators.CardType cardType, int cost)
        {
            Damage = damage;
            Defense = defense;
            Faction = faction;
            CardType = cardType;
            Cost = cost;
        }
    }
}
