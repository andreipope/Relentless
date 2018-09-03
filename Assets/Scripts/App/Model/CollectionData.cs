using System.Collections.Generic;

namespace LoomNetwork.CZB.Data
{
    public class CollectionData
    {
        public List<CollectionCardData> Cards;

        public CollectionCardData GetCardData(string id)
        {
            foreach (CollectionCardData cardData in Cards)
            {
                if (cardData.CardName == id)
                {
                    return cardData;
                }
            }

            return null;
        }

        public void ChangeAmount(string idCard, int amoundChangeFactor)
        {
            GetCardData(idCard).Amount += amoundChangeFactor;
        }
    }

    public class CollectionCardData
    {
        public string CardName;

        public int Amount;
    }
}
