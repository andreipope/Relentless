// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// This class represents a runtime instance of a zone.
    /// </summary>
    public class RuntimeZone
    {
        /// <summary>
        /// The identifier of this zone.
        /// </summary>
        public int zoneId;

        /// <summary>
        /// The instance identifier of this zone.
        /// </summary>
        public int instanceId;

        /// <summary>
        /// The name of this zone.
        /// </summary>
        public string name;

        /// <summary>
        /// The cards of this zone.
        /// </summary>
        public List<RuntimeCard> cards = new List<RuntimeCard>();

        /// <summary>
        /// The number of cards of this zone.
        /// </summary>
        protected int _numCards;

        /// <summary>
        /// The number of cards of this zone.
        /// </summary>
        public int numCards
        {
            get
            {
                return _numCards;
            }
            set
            {
                _numCards = value;
                if (onZoneChanged != null)
                {
                    onZoneChanged(_numCards);
                }
            }
        }

        /// <summary>
        /// The maximum number of cards of this zone.
        /// </summary>
        public int maxCards;

        /// <summary>
        /// The callback that is called when this zone changes.
        /// </summary>
        public Action<int> onZoneChanged;

        /// <summary>
        /// The callback that is called when a card is added to this zone.
        /// </summary>
        public Action<RuntimeCard> onCardAdded;

        /// <summary>
        /// The callback that is called when a card is removed from this zone.
        /// </summary>
        public Action<RuntimeCard> onCardRemoved;

        /// <summary>
        /// Adds a card to this zone.
        /// </summary>
        /// <param name="card">The card to add.</param>
        public void AddCard(RuntimeCard card)
        {
            if (cards.Count < maxCards && !cards.Contains(card))
            {
                cards.Add(card);
                _numCards += 1;
                if (onZoneChanged != null)
                {
                    onZoneChanged(numCards);
                }
                if (onCardAdded != null)
                {
                    onCardAdded(card);
                }
            }
        }

        /// <summary>
        /// Removes a card from this zone.
        /// </summary>
        /// <param name="card">The card to remove.</param>
        public void RemoveCard(RuntimeCard card)
        {
            if (cards.Contains(card))
            {
                cards.Remove(card);
                _numCards -= 1;
                if (onZoneChanged != null)
                {
                    onZoneChanged(numCards);
                }
                if (onCardRemoved != null)
                {
                    onCardRemoved(card);
                }
            }
        }

        /// <summary>
        /// Removes a number of cards from this zone.
        /// </summary>
        /// <param name="amount">The number of cards to remove.</param>
        public void RemoveCards(int amount)
        {
            cards.RemoveRange(0, amount);
            _numCards -= amount;
            if (onZoneChanged != null)
            {
                onZoneChanged(numCards);
            }
        }
    }
}
