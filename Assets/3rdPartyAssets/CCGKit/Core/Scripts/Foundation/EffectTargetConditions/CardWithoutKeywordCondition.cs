// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// This class represents a condition in which a card stat is checked to determine if
    /// it does not contain a certain keyword.
    /// </summary>
    public class CardWithoutKeywordCondition : CardCondition
    {
        /// <summary>
        /// The keyword of this condition.
        /// </summary>
        [KeywordTypeField("Keyword")]
        [Order(2)]
        public int keywordId;

        /// <summary>
        /// The value of this condition.
        /// </summary>
        [KeywordValueField("Value")]
        [Order(3)]
        public int valueId;

        /// <summary>
        /// Returns a readable string representing this condition.
        /// </summary>
        /// <param name="config">The game's configuration.</param>
        /// <returns>A readable string that represents this condition.</returns>
        public override string GetReadableString(GameConfiguration config)
        {
            var keyword = config.keywords.Find(x => x.id == keywordId);
            if (keyword != null)
            {
                return "Without " + keyword.values[valueId].value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Returns true if this condition has been met on the specified card and false otherwise.
        /// </summary>
        /// <param name="card">The card.</param>
        /// <returns>True if this condition has been met on the specified card; false otherwise.</returns>
        public override bool IsTrue(RuntimeCard card)
        {
            return card.keywords.Find(x => x.keywordId == keywordId && x.valueId == valueId) == null;
        }
    }
}
