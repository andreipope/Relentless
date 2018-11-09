using System.Collections.Generic;


namespace Loom.ZombieBattleground.Data
{
    public class OpponentDecksData
    {
        public List<Deck> Decks;
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
