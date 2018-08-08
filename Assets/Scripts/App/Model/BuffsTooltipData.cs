// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections.Generic;

namespace LoomNetwork.CZB.Data
{
    public class BuffsTooltipData
    {
        public List<BuffInfo> buffs;
    }

    public class BuffInfo
    {
        public string name;
        public string tooltip;
        public string type;
    }
}
