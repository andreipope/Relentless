using System.Collections.Generic;

namespace Loom.ZombieBattleground.Data
{
    public class CollectionData
    {
        public List<CollectionCardData> Cards;

        public CollectionCardData GetCardData(int id)
        {
            foreach (CollectionCardData cardData in Cards)
            {
                if (cardData.MouldId == id)
                {
                    return cardData;
                }
            }

            return null;
        }

        public void ChangeAmount(int idCard, int amountChangeFactor)
        {
            GetCardData(idCard).Amount += amountChangeFactor;
        }
    }

    public class CollectionCardData
    {
        public int MouldId;

        public int Amount;
    }
}
