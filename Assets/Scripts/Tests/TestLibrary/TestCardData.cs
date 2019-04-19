using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground.Test
{
    public class TestCardData
    {
        public string CardName { get; }

        public int MouldId { get; }

        public int Amount { get; }

        public TestCardData(string cardName, int amount)
        {
            CardName = cardName;
            MouldId = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCardFromName(cardName).MouldId;
            Amount = amount;
        }

        public DeckCardData ToDeckCardData()
        {
            return new DeckCardData(MouldId, Amount);
        }
    }
}
