// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// The base class for conditions.
    /// </summary>
    public abstract class Condition
    {
        /// <summary>
        /// Returns a readable string representing this condition.
        /// </summary>
        /// <param name="config">The game's configuration.</param>
        /// <returns>A readable string that represents this condition.</returns>
        public abstract string GetReadableString(GameConfiguration config);

        /// <summary>
        /// Returns a readable string representing the specified condition operator.
        /// </summary>
        /// <param name="op">The condition operator.</param>
        /// <returns>A readable string that represents the specified condition operator.</returns>
        public static string GetReadableConditionOperator(ConditionOperator op)
        {
            switch (op)
            {
                case ConditionOperator.LessThan:
                    return "<";

                case ConditionOperator.LessThanOrEqualTo:
                    return "<=";

                case ConditionOperator.EqualTo:
                    return "==";

                case ConditionOperator.GreaterThanOrEqualTo:
                    return ">=";

                default:
                    return ">";
            }
        }
    }

    /// <summary>
    /// The base class for player conditions.
    /// </summary>
    public abstract class PlayerCondition : Condition
    {
        /// <summary>
        /// Returns true if this condition has been met on the specified player and false otherwise.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>True if this condition has been met on the specified player; false otherwise.</returns>
        public abstract bool IsTrue(PlayerInfo player);
    }

    /// <summary>
    /// The base class for card conditions.
    /// </summary>
    public abstract class CardCondition : Condition
    {
        /// <summary>
        /// Returns true if this condition has been met on the specified card and false otherwise.
        /// </summary>
        /// <param name="card">The card.</param>
        /// <returns>True if this condition has been met on the specified card; false otherwise.</returns>
        public abstract bool IsTrue(RuntimeCard card);
    }
}
