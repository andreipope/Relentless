using System;
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

        private static bool CardNameEqual(string name, WorkingCard card)
        {
            return String.Equals(name, card.LibraryCard.Name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
