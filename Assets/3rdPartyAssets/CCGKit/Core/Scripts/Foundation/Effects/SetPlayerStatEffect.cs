// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    [PlayerTarget]
    public class SetPlayerStatEffect : PlayerEffect
    {
        [PlayerStatField("Player stat")]
        [Order(1)]
        public int statId;

        [ValueField("Value")]
        [Order(2)]
        public Value value;

        [IntField("Duration")]
        [Order(3)]
        public int duration;

        public override void Resolve(GameState state, PlayerInfo player)
        {
            var stat = player.stats[statId];
            var newValue = value.GetValue(state, player);
            var diff = 0;
            if (stat.effectiveValue >= newValue)
            {
                diff = -(stat.effectiveValue - newValue);
            }
            else
            {
                diff = newValue - stat.effectiveValue;
            }
            var modifier = new Modifier(diff, duration);
            stat.AddModifier(modifier);
        }
    }
}