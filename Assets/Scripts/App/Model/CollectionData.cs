using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB.Data
{
    public class CollectionData
    {
        public List<CollectionCardData> cards;

        public CollectionData()
        {
        }

        public CollectionCardData GetCardData(int id)
        {
            foreach(var cardData in cards)
            {
                if (cardData.cardId == id)
                    return cardData;
            }
            return null;
        }

        public void ChangeAmount(int idCard, int amoundChangeFactor)
        {
            GetCardData(idCard).amount += amoundChangeFactor;
        }
    }

    public class CollectionCardData
    {
        public int cardId;
        public int amount;

        public CollectionCardData()
        {
        }
    }
}