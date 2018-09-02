using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public class Buff
    {
        public bool RankingBuff;

        public Enumerators.CardRank CardRank;

        public Enumerators.BuffType BuffType;

        public object[] Parameters;

        public Buff(Enumerators.BuffType type, object[] param, Enumerators.CardRank rank)
        {
            BuffType = type;
            Parameters = param;
            CardRank = rank;
            RankingBuff = true;
        }

        public Buff(Enumerators.BuffType type, object[] param)
        {
            BuffType = type;
            Parameters = param;
        }
    }
}
