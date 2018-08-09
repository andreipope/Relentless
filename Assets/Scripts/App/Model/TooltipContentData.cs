// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections.Generic;

namespace LoomNetwork.CZB.Data
{
    public class TooltipContentData
    {
        public List<BuffInfo> buffs;
        public List<RankInfo> ranks;

        public class BuffInfo
        {
            public string name;
            public string tooltip;
            public string type;
        }

        public class RankInfo
        {
            public string name;
            public string type;
            public List<RankDescription> info;

            public class RankDescription
            {
                public string element;
                public string tooltip;
            }
        }
    }
}
