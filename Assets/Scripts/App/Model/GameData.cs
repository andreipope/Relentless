// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB.Data
{
    class GameData
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
            public int startManaValue;
            public int minManaValue;
            public int maxManaValue;
        }
    }
}
