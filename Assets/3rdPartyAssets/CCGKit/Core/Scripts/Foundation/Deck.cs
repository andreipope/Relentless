// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// A single entry in a deck.
    /// </summary>
    [Serializable]
    public class DeckEntry
    {
        /// <summary>
        /// The unique identifier of the card.
        /// </summary>
        public int id;

        /// <summary>
        /// The number of copies of the card.
        /// </summary>
        public int amount;
    }

    /// <summary>
    /// A deck is a collection of cards that players use when entering a game.
    /// </summary>
    [Serializable]
    public class Deck
    {
        /// <summary>
        /// The name of this deck.
        /// </summary>
        public string name = "New deck";

        /// <summary>
        /// The entries of this deck.
        /// </summary>
        public List<DeckEntry> cards = new List<DeckEntry>();

        /// <summary>
        /// Returns the number of cards in this deck.
        /// </summary>
        /// <returns>The number of cards in this deck.</returns>
        public int GetNumCards()
        {
            var total = 0;
            foreach (var card in cards)
            {
                total += card.amount;
            }
            return total;
        }

        /// <summary>
        /// Returns the number of cards of the specified type in this deck.
        /// </summary>
        /// <param name="config">The game's configuration.</param>
        /// <param name="cardTypeId">The card type.</param>
        /// <returns>The number of cards of the specified type in this deck.</returns>
        public int GetNumCards(GameConfiguration config, int cardTypeId)
        {
            var total = 0;
            foreach (var card in cards)
            {
                foreach (var set in config.cardSets)
                {
                    var libraryCard = set.cards.Find(x => x.id == card.id);
                    if (libraryCard != null && libraryCard.cardTypeId == cardTypeId)
                    {
                        total += card.amount;
                        break;
                    }
                }
            }
            return total;
        }

        /// <summary>
        /// Adds the specified card to this deck.
        /// </summary>
        /// <param name="card">The card to add to this deck.</param>
        public void AddCard(Card card)
        {
            var existingCard = cards.Find(x => x.id == card.id);
            if (existingCard != null)
            {
                existingCard.amount += 1;
            }
            else
            {
                cards.Add(new DeckEntry { id = card.id, amount = 1 });
            }
        }

        /// <summary>
        /// Removes this card from this deck.
        /// </summary>
        /// <param name="card">The card to remove from this deck.</param>
        public void RemoveCards(Card card)
        {
            var existingCard = cards.Find(x => x.id == card.id);
            if (existingCard != null)
            {
                cards.Remove(existingCard);
            }
        }
    }
}
