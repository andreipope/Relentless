using System.Collections.Generic;


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

        public string Type;

        public string Name;

        public List<DeckCardData> Cards;
    }
}
