// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB.Data
{
    internal class GameData
    {
        public class GameProperties
        {
            public int turnDuration;

            public int deckSize;

            public int startCardsAmount;

            public Enumerators.GameEndCondition endGameCondition;

            public int startLifeValue;

            public int minLifeValue;

            public int maxLifeValue;

            public int startGooValue;

            public int minGooValue;

            public int maxGooValue;
        }
    }
}
