using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB.Data
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
