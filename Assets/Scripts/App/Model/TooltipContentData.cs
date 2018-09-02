using System.Collections.Generic;

namespace LoomNetwork.CZB.Data
{
    public class TooltipContentData
    {
        public List<BuffInfo> Buffs;

        public List<RankInfo> Ranks;

        public class BuffInfo
        {
            public string Name;

            public string Tooltip;

            public string Type;
        }

        public class RankInfo
        {
            public string Name;

            public string Type;

            public List<RankDescription> Info;

            public class RankDescription
            {
                public string Element;

                public string Tooltip;
            }
        }
    }
}
