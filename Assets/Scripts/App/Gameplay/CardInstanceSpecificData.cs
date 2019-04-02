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
        public int Damage { get; set; }

        public int Defense { get; set; }

        public Enumerators.Faction Faction { get; set; }

        public Enumerators.CardType CardType { get; protected set; }

        public int Cost { get; set; }

        public List<CardAbilitiesCombination> Abilities { get; set; }

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

        public CardInstanceSpecificData(CardInstanceSpecificData source)
            : this(
                source.Damage,
                source.Defense,
                source.Faction,
                source.CardType,
                source.Cost,
                source.Abilities)
        {
        }

        public CardInstanceSpecificData(int damage, int defense, Enumerators.Faction faction, Enumerators.CardType cardType, int cost, List<CardAbilitiesCombination> abilities)
        {
            Damage = damage;
            Defense = defense;
            Faction = faction;
            CardType = cardType;
            Cost = cost;
            Abilities = abilities;
        }
    }
}
