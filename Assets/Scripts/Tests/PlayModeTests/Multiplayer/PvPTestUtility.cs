using System;
using System.Linq;

namespace Loom.ZombieBattleground.Test
{
    public static class PvPTestUtility
    {
        public static WorkingCard GetCardOnBoard(Player player, string name)
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

        public static WorkingCard GetCardInHand(Player player, string name)
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

        public static bool CardNameEqual(string name1, string name2)
        {
            return String.Equals(name1, name2, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool CardNameEqual(string name, WorkingCard card)
        {
            return CardNameEqual(name, card.LibraryCard.Name);
        }
    }
}
