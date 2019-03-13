using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class CardInstanceSpecificData : IReadOnlyCardInstanceSpecificData
    {
        public int Attack { get; set; }

        public int Defense { get; set; }

        public Enumerators.SetType CardSetType { get; set; }

        public Enumerators.CardType CardType { get; protected set; }

        public int Cost { get; set; }

        public IList<AbilityData> Abilities { get; set; }

        public CardInstanceSpecificData(IReadOnlyCard card)
            : this(
                card.Damage,
                card.Health,
                card.CardSetType,
                card.CardType,
                card.Cost,
                card.Abilities)
        {
        }

        public CardInstanceSpecificData(CardInstanceSpecificData source)
            : this(
                source.Attack,
                source.Defense,
                source.CardSetType,
                source.CardType,
                source.Cost,
                source.Abilities)
        {
        }

        public CardInstanceSpecificData(int attack, int defense, Enumerators.SetType cardSetType, Enumerators.CardType cardType, int cost, IList<AbilityData> abilities)
        {
            Attack = attack;
            Defense = defense;
            CardSetType = cardSetType;
            CardType = cardType;
            Cost = cost;
            Abilities = abilities.Select(a => new AbilityData(a)).ToList();
        }
    }
}
