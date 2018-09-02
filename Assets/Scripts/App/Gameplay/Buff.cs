// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public class Buff
    {
        public bool rankingBuff;

        public Enumerators.CardRank cardRank;

        public Enumerators.BuffType buffType;

        public object[] parameters;

        public Buff(Enumerators.BuffType type, object[] param, Enumerators.CardRank rank)
        {
            buffType = type;
            parameters = param;
            cardRank = rank;
            rankingBuff = true;
        }

        public Buff(Enumerators.BuffType type, object[] param)
        {
            buffType = type;
            parameters = param;
        }
    }
}
