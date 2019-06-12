using System.Collections.Generic;

namespace Loom.ZombieBattleground.Data
{
    public class CollectionData
    {
        public List<CollectionCardData> Cards;

        public CollectionCardData GetCardData(CardKey cardKey)
        {
            foreach (CollectionCardData cardData in Cards)
            {
                if (cardData.CardKey == cardKey)
                {
                    return cardData;
                }
            }

            return null;
        }
    }

    public class CollectionCardData
    {
        public CardKey CardKey;

        public int Amount;

        public CollectionCardData(CardKey cardKey, int amount)
        {
            CardKey = cardKey;
            Amount = amount;
        }
    }
}
