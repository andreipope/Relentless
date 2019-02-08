using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground.Test
{
    public class PvPTestHelper
    {
        public WorkingCard GetCardOnBoard(Player player, string name)
        {
            WorkingCard workingCard =
                player
                .BoardCards
                .Select(boardCard => boardCard.Model.Card)
                .Concat(player.CardsOnBoard)
                .FirstOrDefault(card => CardNameEqual(name, card));

            if (workingCard == null)
            {
                throw new Exception($"No '{name}' cards found on board for player {player}");
            }

            return workingCard;
        }

        public WorkingCard GetCardInHand(Player player, string name)
        {
            WorkingCard workingCard =
                player
                    .CardsInHand
                    .FirstOrDefault(card => CardNameEqual(name, card));

            if (workingCard == null)
            {
                throw new Exception($"No '{name}' cards found in hand of player {player}");
            }

            return workingCard;
        }

        public static Deck GetDeckWithCards(string name, List<string> cardsNames, int heroId = 0)
        {
            List<DeckCardData> cards = new List<DeckCardData>();

            foreach (string card in cardsNames)
            {
                cards.Add(new DeckCardData(card, 2));
            }

            Deck deck = new Deck(
                 0,
                 heroId,
                 name,
                 cards,
                 Enumerators.OverlordSkill.NONE,
                 Enumerators.OverlordSkill.NONE
             );

            return deck;
        }
        private static bool CardNameEqual(string name, WorkingCard card)
        {
            return String.Equals(name, card.LibraryCard.Name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
