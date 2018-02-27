// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// This card represents a cost in the game. Costs are used when playing cards and when
    /// activating activated abilities.
    /// </summary>
    public class Cost
    {
        /// <summary>
        /// Returns a readable string representing this cost.
        /// </summary>
        /// <param name="config">The game's configuration.</param>
        /// <returns>A readable string that represents this cost.</returns>
        public virtual string GetReadableString(GameConfiguration config)
        {
            return "Card cost";
        }
    }
}
