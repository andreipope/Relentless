// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    [CardTarget]
    public class ResetCardStatEffect : CardStatEffect
    {
        public override void Resolve(GameState state, RuntimeCard card)
        {
            var stat = card.stats[statId];
            stat.modifiers.Clear();
            stat.baseValue = stat.originalValue;
        }
    }
}