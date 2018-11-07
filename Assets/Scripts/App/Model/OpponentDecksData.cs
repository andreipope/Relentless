using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public class OpponentDecksData
    {
        public List<OpponentDeck> Decks;
    }

    public class OpponentDeck
    {
        public int Id;

        public int HeroId;

        public Enumerators.AiType Type;

        public string Name;

        public List<DeckCardData> Cards;
    }
}
