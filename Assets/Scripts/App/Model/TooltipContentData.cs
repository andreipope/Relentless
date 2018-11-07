using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public class TooltipContentData
    {
        public List<CardTypeInfo> CardTypes;

        public List<BuffInfo> Buffs;

        public List<RankInfo> Ranks;

        public class CardTypeInfo
        {
            public string Name;

            public string Tooltip;

            public Enumerators.CardType Type;
        }

        public class BuffInfo
        {
            public string Name;

            public string Tooltip;

            public Enumerators.BuffType Type;
        }

        public class RankInfo
        {
            public string Name;

            public Enumerators.CardRank Type;

            public List<RankDescription> Info;

            public class RankDescription
            {
                public Enumerators.SetType Element;

                public string Tooltip;
            }
        }
    }
}
