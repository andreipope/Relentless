// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;

namespace LoomNetwork.CZB.Data
{
    public class CollectionData
    {
        public List<CollectionCardData> cards;

        public CollectionCardData GetCardData(string id)
        {
            foreach (CollectionCardData cardData in cards)
            {
                if (cardData.cardName == id)
                {
                    return cardData;
                }
            }

            return null;
        }

        public void ChangeAmount(string idCard, int amoundChangeFactor)
        {
            GetCardData(idCard).amount += amoundChangeFactor;
        }
    }

    public class CollectionCardData
    {
        public string cardName;

        public int amount;
    }
}
