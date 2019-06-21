using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground.Test
{
    public class PvpTestContext {
        public readonly Deck Player1Deck;
        public readonly Deck Player2Deck;
        public bool Player1HasFirstTurn = true;
        public bool UseBackendLogic;
        public bool IsReversed;

        public PvpTestContext(Deck player1Deck, Deck player2Deck) {
            if (player1Deck == null)
                throw new ArgumentNullException(nameof(player1Deck));

            if (player2Deck == null)
                throw new ArgumentNullException(nameof(player2Deck));

            if (player1Deck == player2Deck)
                throw new Exception("player1Deck == player2Deck");

            ValidateDeck(player1Deck);
            ValidateDeck(player2Deck);

            Player1Deck = player1Deck;
            Player2Deck = player2Deck;
        }

        public Player GetOpponentPlayer()
        {
            return IsReversed ? TestHelper.Instance.GetCurrentPlayer() : TestHelper.Instance.GetOpponentPlayer();
        }

        public Player GetCurrentPlayer()
        {
            return !IsReversed ? TestHelper.Instance.GetCurrentPlayer() : TestHelper.Instance.GetOpponentPlayer();
        }

        /// <summary>
        /// Calculates the instance ID of a card in deck by its name and number.
        /// Example: "in this deck, get the ID of a second 'Slab' card."
        /// </summary>
        /// <param name="deck"></param>
        /// <param name="name">Name of the card</param>
        /// <param name="number">The number of a card with that name</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public InstanceId GetCardInstanceIdByName(Deck deck, string name, int number) {
            AssertKnownDeck(deck);
            if (number <= 0)
                throw new Exception("number must be >= 1");

            int count = 0;
            foreach (DeckCardData deckCard in deck.Cards)
            {
                string cardName =
                    GameClient.Get<IDataManager>()
                        .CachedCardsLibraryData
                        .GetCardByCardKey(deckCard.CardKey)
                        .Name;

                if (PvPTestUtility.CardNameEqual(name, cardName))
                {
                    if (deckCard.Amount <= 0)
                        throw new Exception($"deckCard.Amount <= 0 for card {name}");

                    if (number > deckCard.Amount)
                        throw new Exception($"number > deckCard.Amount for card {name}");

                    return GetCardInstanceIdByIndex(deck, count + number - 1);
                }

                count += deckCard.Amount;
            }

            throw new Exception($"card with name {name} not found in deck");
        }

        /// <summary>
        /// Calculates the instance ID of a card by its index in deck.
        /// </summary>
        /// <param name="deck"></param>
        /// <param name="indexInDeck"></param>
        /// <returns></returns>
        public InstanceId GetCardInstanceIdByIndex(Deck deck, int indexInDeck) {
            return new InstanceId(GetDeckStartingInstanceId(deck).Id + indexInDeck);
        }

        /// <summary>
        /// Calculates the instance ID of the first card in a deck.
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        public InstanceId GetDeckStartingInstanceId(Deck deck) {
            AssertKnownDeck(deck);

            bool isPlayer1Deck = deck == Player1Deck;
            isPlayer1Deck = IsReversed ? !isPlayer1Deck : isPlayer1Deck;
            Deck otherDeck = isPlayer1Deck ? Player2Deck : Player1Deck;

            bool condition = Player1HasFirstTurn && isPlayer1Deck || !Player1HasFirstTurn && !isPlayer1Deck;
            condition = IsReversed ? !condition : condition;
            if (condition)
            {
                return new InstanceId(2);
            } else
            {
                return new InstanceId(2 + otherDeck.GetNumCards());
            }
        }

        private void AssertKnownDeck(Deck deck) {
            if (deck != Player1Deck && deck != Player2Deck)
                throw new Exception("deck != Player1Deck && deck != Player2Deck");
        }

        private static void ValidateDeck(Deck deck)
        {
            IEnumerable<IGrouping<CardKey, DeckCardData>> groupBy = deck.Cards.GroupBy(card => card.CardKey);
            foreach (IGrouping<CardKey, DeckCardData> grouping in groupBy)
            {
                int count = grouping.Count();
                if (count > 1)
                    throw new Exception($"Card {grouping.Key} has multiple definitions in deck, only one is allowed");
            }
        }
    }
}
