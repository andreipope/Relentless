// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    [PlayerTarget]
    public class ResetPlayerStatEffect : PlayerEffect
    {
        [PlayerStatField("Player stat")]
        [Order(1)]
        public int statId;

        public override void Resolve(GameState state, PlayerInfo player)
        {
            var stat = player.stats[statId];
            stat.modifiers.Clear();
            stat.baseValue = stat.originalValue;
        }
    }
}