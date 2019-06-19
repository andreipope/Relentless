using System.Collections.Generic;

namespace Loom.ZombieBattleground.Data
{
    public class CollectionData
    {
        public List<CollectionCardData> Cards;

        public CollectionData()
        {
            Cards = new List<CollectionCardData>();
        }

        public CollectionCardData GetCardData(MouldId mouldId)
        {
            foreach (CollectionCardData cardData in Cards)
            {
                if (cardData.MouldId == mouldId)
                {
                    return cardData;
                }
            }

            return null;
        }
    }

    public class CollectionCardData
    {
        public MouldId MouldId;

        public int Amount;

        public CollectionCardData(MouldId mouldId, int amount)
        {
            MouldId = mouldId;
            Amount = amount;
        }
    }
}
