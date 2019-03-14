using System.Collections.Generic;

namespace Loom.ZombieBattleground.Data
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

        public void ChangeAmount(string idCard, int amountChangeFactor)
        {
            GetCardData(idCard).Amount += amountChangeFactor;
        }
    }

    public class CollectionCardData
    {
        public string CardName;

        public int Amount;
    }
}
