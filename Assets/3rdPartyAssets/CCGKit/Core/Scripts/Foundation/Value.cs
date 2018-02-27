// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// The base class for values.
    /// </summary>
    public abstract class Value
    {
        /// <summary>
        /// Returns the integer value of this value.
        /// </summary>
        /// <param name="state">The state of the game.</param>
        /// <param name="player">The state of the player.</param>
        /// <returns></returns>
        public abstract int GetValue(GameState state, PlayerInfo player);
    }
}
