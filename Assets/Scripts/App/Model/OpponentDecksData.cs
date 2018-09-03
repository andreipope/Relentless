using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using LoomNetwork.Internal;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class OpponentDecksData
    {
        public List<OpponentDeck> Decks;

        public void ParseData()
        {
            if (Decks != null)
            {
                foreach (OpponentDeck deck in Decks)
                {
                    deck.ParseData();
                }
            }
        }
    }

    public class OpponentDeck
    {
        public int Id;

        public int HeroId;

        public string Type;

        public string Actions;

        public List<DeckCardData> Cards;

        [JsonIgnore]
        public List<Enumerators.AiActionType> OpponentActions;

        public void ParseData()
        {
            OpponentActions = Utilites.CastList<Enumerators.AiActionType>(Actions);
        }

        public int GetNumCards()
        {
            int amount = 0;
            foreach (DeckCardData card in Cards)
            {
                amount += card.Amount;
            }

            return amount;
        }
    }
}
