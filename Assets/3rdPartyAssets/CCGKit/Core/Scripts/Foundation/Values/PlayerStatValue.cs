// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// A value that is equal to the value of a player stat.
    /// </summary>
    public class PlayerStatValue : Value
    {
        /// <summary>
        /// The stat of this value.
        /// </summary>
        [PlayerStatField("Player stat")]
        public int statId;

        /// <summary>
        /// Returns the integer value of this value.
        /// </summary>
        /// <param name="state">The state of the game.</param>
        /// <param name="player">The state of the player.</param>
        /// <returns>The integer value of this value.</returns>
        public override int GetValue(GameState state, PlayerInfo player)
        {
            var stat = player.stats[statId];
            return stat.effectiveValue;
        }
    }
}
