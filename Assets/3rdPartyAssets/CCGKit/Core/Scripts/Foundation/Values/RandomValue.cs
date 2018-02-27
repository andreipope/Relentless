// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// A random number.
    /// </summary>
    public class RandomValue : Value
    {
        /// <summary>
        /// The minimum value of this value.
        /// </summary>
        [IntField("Min")]
        public int min;

        /// <summary>
        /// The maximum value of this value.
        /// </summary>
        [IntField("Max")]
        public int max;

        /// <summary>
        /// Returns the integer value of this value.
        /// </summary>
        /// <param name="state">The state of the game.</param>
        /// <param name="player">The state of the player.</param>
        /// <returns>The integer value of this value.</returns>
        public override int GetValue(GameState state, PlayerInfo player)
        {
            return state.effectSolver.GetRandomNumber(min, max + 1);
        }
    }
}
