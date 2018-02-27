// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// The base stat class used in player and card definitions.
    /// </summary>
    public class DefinitionStat : Resource
    {
        /// <summary>
        /// The name of this stat.
        /// </summary>
        public string name;

        /// <summary>
        /// The base value of this stat.
        /// </summary>
        public int baseValue;

        /// <summary>
        /// The original value of this stat.
        /// </summary>
        public int originalValue;

        /// <summary>
        /// The minimum value of this stat.
        /// </summary>
        public int minValue;

        /// <summary>
        /// The maximum value of this stat.
        /// </summary>
        public int maxValue;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The identifier of this resource.</param>
        public DefinitionStat(int id) : base(id)
        {
        }
    }

    /// <summary>
    /// The stat of a player definition.
    /// </summary>
    public class PlayerStat : DefinitionStat
    {
        /// <summary>
        /// The current resource identifier.
        /// </summary>
        public static int currentId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PlayerStat() : base(currentId++)
        {
        }
    }

    /// <summary>
    /// The stat of a card definition.
    /// </summary>
    public class CardStat : DefinitionStat
    {
        /// <summary>
        /// The current resource identifier.
        /// </summary>
        public static int currentId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public CardStat() : base(currentId++)
        {
        }
    }
}
