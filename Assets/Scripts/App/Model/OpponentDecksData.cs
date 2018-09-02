// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using Loom.Newtonsoft.Json;
using LoomNetwork.CZB.Common;
using LoomNetwork.Internal;

namespace LoomNetwork.CZB.Data
{
    public class OpponentDecksData
    {
        public List<OpponentDeck> decks;

        public void ParseData()
        {
            if (decks != null)
            {
                foreach (OpponentDeck deck in decks)
                {
                    deck.ParseData();
                }
            }
        }
    }

    public class OpponentDeck
    {
        public int id;

        public int heroId;

        public string type;

        public string actions;

        public List<DeckCardData> cards;

        [JsonIgnore]
        public List<Enumerators.AIActionType> opponentActions;

        public void ParseData()
        {
            opponentActions = Utilites.CastList<Enumerators.AIActionType>(actions);
        }

        public int GetNumCards()
        {
            int amount = 0;
            foreach (DeckCardData card in cards)
            {
                amount += card.amount;
            }

            return amount;
        }
    }
}
