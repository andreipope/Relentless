using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB.Data
{
    internal class GameData
    {
        public class GameProperties
        {
            public int TurnDuration;

            public int DeckSize;

            public int StartCardsAmount;

            public Enumerators.GameEndCondition EndGameCondition;

            public int StartLifeValue;

            public int MinLifeValue;

            public int MaxLifeValue;

            public int StartGooValue;

            public int MinGooValue;

            public int MaxGooValue;
        }
    }
}
