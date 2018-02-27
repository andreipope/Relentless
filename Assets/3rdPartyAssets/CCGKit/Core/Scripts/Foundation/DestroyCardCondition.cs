// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// This class represents a condition for a card to be destroyed.
    /// </summary>
    public class DestroyCardCondition
    {
        /// <summary>
        /// Returns a readable string representing this condition.
        /// </summary>
        /// <param name="config">The game's configuration.</param>
        /// <returns>A readable string that represents this condition.</returns>
        public virtual string GetReadableString(GameConfiguration config)
        {
            return "Destroy card condition";
        }

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
}
