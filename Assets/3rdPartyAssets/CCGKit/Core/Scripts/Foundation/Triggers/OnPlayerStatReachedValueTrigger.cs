// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    public class OnPlayerStatReachedValueTrigger : OnPlayerStatChangedTrigger
    {
        [IntField("Value")]
        public int value;

        public override bool IsTrue(Stat stat, int newValue, int oldValue)
        {
            return stat.statId == statId && newValue == value;
        }
    }
}