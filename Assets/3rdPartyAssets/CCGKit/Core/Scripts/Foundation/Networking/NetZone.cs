// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    public struct NetStaticCard
    {
        public int cardId;
        public int instanceId;
    }

    public struct NetStaticZone
    {
        public int zoneId;
        public NetStaticCard[] cards;
        public int numCards;
    }

    public struct NetDynamicZone
    {
        public int zoneId;
        public NetCard[] cards;
        public int numCards;
    }
}