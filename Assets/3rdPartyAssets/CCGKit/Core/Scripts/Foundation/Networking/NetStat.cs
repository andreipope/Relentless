// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    public struct NetModifier
    {
        public int value;
        public int duration;
    }

    public struct NetStat
    {
        public int statId;
        public int baseValue;
        public int originalValue;
        public int minValue;
        public int maxValue;
        public NetModifier[] modifiers;
    }
}