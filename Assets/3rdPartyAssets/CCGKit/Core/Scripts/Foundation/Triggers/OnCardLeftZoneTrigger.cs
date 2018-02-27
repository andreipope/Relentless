// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    public class OnCardLeftZoneTrigger : OnCardMovedTrigger
    {
        [GameZoneField("Zone")]
        public int zoneId;

        public override bool IsTrue(GameState state, string zone)
        {
            var toZone = state.config.gameZones.Find(x => x.name == zone);
            return toZone.id == zoneId;
        }
    }
}