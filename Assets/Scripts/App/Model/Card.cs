// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using Loom.Newtonsoft.Json;
using UnityEngine;
using LoomNetwork.CZB.Helpers;
using LoomNetwork.Internal;

namespace LoomNetwork.CZB.Data
{
    public class Card
    {
        //[JsonIgnore]
        public int id;
        public Enumerators.SetType cardSetType;
        public string kind;
        public string name;
        public int cost;
        public string description;
        public string flavorText; // new
        public string picture;
        public int damage;
        public int health;
        public string rank;
        public string type;
        public string frame;
        public List<AbilityData> abilities = new List<AbilityData>();
        public CardViewInfo cardViewInfo = new CardViewInfo();

        [JsonIgnore]
        public Enumerators.CardRank cardRank;
        [JsonIgnore]
        public Enumerators.CardType cardType;
        [JsonIgnore]
        public Enumerators.CardKind cardKind;

        public Card()
        {
        }

        public Card Clone()
        {
            Card card = new Card
            {
                id = id,
                kind = kind,
                name = name,
                cost = cost,
                description = description,
                flavorText = flavorText,
                picture = picture,
                damage = damage,
                health = health,
                rank = rank,
                type = type,
                cardSetType = cardSetType,
                cardKind = cardKind,
                cardRank = cardRank,
                cardType = cardType,
                abilities = abilities,
                cardViewInfo = cardViewInfo,
                frame = frame
            };

            return card;
        }
    }

    public class CardViewInfo
    {
        public FloatVector3 position = FloatVector3.zero;
        public FloatVector3 scale = new FloatVector3(0.38f);
    }
}