// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using GrandDevs.CZB;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;
using System;
using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// This class represents a runtime instance of a card.
    /// </summary>
    public class RuntimeCard
    {
        /// <summary>
        /// The card identifier of this card.
        /// </summary>
        public int cardId;

        /// <summary>
        /// The instance identifier of this card.
        /// </summary>
        public int instanceId;

        /// <summary>
        /// The stats of this card, indexed by id.
        /// </summary>
        public Dictionary<int, Stat> stats = new Dictionary<int, Stat>();

        /// <summary>
        /// The stats of this card, indexed by name.
        /// </summary>
        public Dictionary<string, Stat> namedStats = new Dictionary<string, Stat>();

        /// <summary>
        /// The keywords of this card.
        /// </summary>
        public List<RuntimeKeyword> keywords = new List<RuntimeKeyword>();

        public Enumerators.CardType type;

        /// <summary>
        /// The keywords of this card.
        /// </summary>
        public List<uint> connectedAbilities = new List<uint>();

        /// <summary>
        /// The player that owns this card.
        /// </summary>
        public PlayerInfo ownerPlayer;

        /// <summary>
        /// The type of this card.
        /// </summary>
        public CardType cardType
        {
            get
            {
                var gameConfig = GameManager.Instance.config;
                var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(cardId);
                return gameConfig.cardTypes.Find(x => x.id == libraryCard.cardTypeId);
            }
        }

        /// <summary>
        /// The callback that is called when a keyword is added to this card.
        /// </summary>
        public Action<RuntimeKeyword> onKeywordAdded;

        /// <summary>
        /// The callback that is called when a keyword is removed from this card.
        /// </summary>
        public Action<RuntimeKeyword> onKeywordRemoved;

        /// <summary>
        /// Adds a keyword to this card.
        /// </summary>
        /// <param name="keyword">The identifier of the keyword.</param>
        /// <param name="value">The value of the keyword.</param>
        public void AddKeyword(int keyword, int value)
        {
            var k = keywords.Find(x => x.keywordId == keyword && x.valueId == value);
            if (k == null)
            {
                k = new RuntimeKeyword();
                k.keywordId = keyword;
                k.valueId = value;
                keywords.Add(k);
                if (onKeywordAdded != null)
                {
                    onKeywordAdded(k);
                }
            }
        }

        /// <summary>
        /// Removes a keyword from this card.
        /// </summary>
        /// <param name="keyword">The identifier of this keyword.</param>
        /// <param name="value">The value of this keyword.</param>
        public void RemoveKeyword(int keyword, int value)
        {
            var k = keywords.Find(x => x.keywordId == keyword && x.valueId == value);
            if (k != null)
            {
                keywords.Remove(k);
                if (onKeywordRemoved != null)
                {
                    onKeywordRemoved(k);
                }
            }
        }

        /// <summary>
        /// Returns true if this card has the specified keyword and false otherwise.
        /// </summary>
        /// <param name="name">The name of the keyword.</param>
        /// <returns>True if this card has the specified keyword; false otherwise.</returns>
        public bool HasKeyword(string name)
        {
            var gameConfig = GameManager.Instance.config;
            var keywordId = -1;
            var valueId = -1;
            foreach (var keyword in gameConfig.keywords)
            {
                var selectedValue = keyword.values.FindIndex(x => x.value == name);
                if (selectedValue != -1)
                {
                    keywordId = keyword.id;
                    valueId = selectedValue;
                    break;
                }
            }
            var k = keywords.Find(x => x.keywordId == keywordId && x.valueId == valueId);
            return k != null;
        }

        public void ConnectAbility(uint abilityId)
        {
            connectedAbilities.Add(abilityId);
        }

        public void DisconnectAbility(uint abilityId)
        {
            if (connectedAbilities.Contains(abilityId))
                connectedAbilities.Remove(abilityId);
        }

        public bool HasConnectedAbility(Enumerators.AbilityType type)
        {
            return connectedAbilities.Contains((uint)type);
        }
    }
}