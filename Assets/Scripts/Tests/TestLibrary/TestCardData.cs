using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground.Test
{
    public class TestCardData
    {
        public string CardName { get; }

        public CardKey CardKey { get; }

        public int Amount { get; }

        public TestCardData(string cardName, int amount)
        {
            CardName = cardName;
            CardKey = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCardByName(cardName).CardKey;
            Amount = amount;
        }

        public DeckCardData ToDeckCardData()
        {
            return new DeckCardData(CardKey, Amount);
        }
    }
}
