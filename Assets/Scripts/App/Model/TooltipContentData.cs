using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public class TooltipContentData
    {
        public List<CardTypeInfo> CardTypes;

        public List<GameMechanicInfo> Mechanics;

        public List<RankInfo> Ranks;

        public class CardTypeInfo
        {
            public string Name;

            public string Tooltip;

            public Enumerators.CardType Type;
        }

        public class GameMechanicInfo
        {
            public string Name;

            public string Tooltip;

            public Enumerators.GameMechanicDescriptionType Type;
        }

        public class RankInfo
        {
            public string Name;

            public Enumerators.CardRank Type;

            public List<RankDescription> Info;

            public class RankDescription
            {
                public Enumerators.Faction Element;

                public string Tooltip;
            }
        }
    }
}
