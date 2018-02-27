// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// This class represents a condition for a card to be destroyed based on one of its stats
    /// reaching a certain value.
    /// </summary>
    public class StatDestroyCardCondition : DestroyCardCondition
    {
        /// <summary>
        /// The type of the card.
        /// </summary>
        [CardTypeField("Type")]
        [Order(1)]
        public int typeId;

        /// <summary>
        ///  The stat of the card.
        /// </summary>
        [CardStatField("Stat")]
        [Order(2)]
        public int statId;

        /// <summary>
        /// The operator of this condition.
        /// </summary>
        [EnumField("Operator", width = 150)]
        [Order(3)]
        public ConditionOperator op;

        /// <summary>
        /// The value of this condition.
        /// </summary>
        [IntField("Value")]
        [Order(4)]
        public int value;

        /// <summary>
        /// Returns true if this condition has been met on the specified card and false otherwise.
        /// </summary>
        /// <param name="card">The card.</param>
        /// <returns>True if this condition has been met on the specified card; false otherwise.</returns>
        public bool IsTrue(RuntimeCard card)
        {
            var stat = card.stats[statId];
            switch (op)
            {
                case ConditionOperator.LessThan:
                    return stat.effectiveValue < value;

                case ConditionOperator.LessThanOrEqualTo:
                    return stat.effectiveValue <= value;

                case ConditionOperator.EqualTo:
                    return stat.effectiveValue == value;

                case ConditionOperator.GreaterThanOrEqualTo:
                    return stat.effectiveValue >= value;

                case ConditionOperator.GreaterThan:
                    return stat.effectiveValue > value;
            }
            return false;
        }

        /// <summary>
        /// Returns a readable string representing this condition.
        /// </summary>
        /// <param name="config">The game's configuration.</param>
        /// <returns>A readable string that represents this condition.</returns>
        public override string GetReadableString(GameConfiguration config)
        {
            var type = config.cardTypes.Find(x => x.id == typeId);
            var stat = type.stats.Find(x => x.id == statId);
            if (stat != null)
            {
                return stat.name + " " + GetReadableConditionOperator(op) + " " + value;
            }
            return "";
        }
    }
}
