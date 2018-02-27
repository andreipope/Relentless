// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Assertions;

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// This class represents a single card type in the game.
    /// </summary>
    public class CardType : Resource
    {
        /// <summary>
        /// The current resource identifier.
        /// </summary>
        public static int currentId;

        /// <summary>
        /// The name of this card type.
        /// </summary>
        public string name;

        /// <summary>
        /// The properties of this card type.
        /// </summary>
        public List<Property> properties = new List<Property>();

        /// <summary>
        /// The stats of this card type.
        /// </summary>
        public List<DefinitionStat> stats = new List<DefinitionStat>();

        /// <summary>
        /// The destroy conditions of this card type.
        /// </summary>
        public List<DestroyCardCondition> destroyConditions = new List<DestroyCardCondition>();

        /// <summary>
        /// True if this card should move to another zone after triggering its effect
        /// (useful for spell-like cards); false otherwise.
        /// </summary>
        public bool moveAfterTriggeringEffect;

        /// <summary>
        /// The zone to which this card should move after triggering its effect.
        /// </summary>
        [GameZoneField("Zone")]
        public int zoneId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public CardType() : base(currentId++)
        {
        }

        /// <summary>
        /// Returns the value of the integer property with the specified name.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>The value of the property.</returns>
        public int GetIntProperty(string name)
        {
            var property = properties.Find(x => x.name == name && x is IntProperty);
            Assert.IsNotNull(property);
            return (property as IntProperty).value;
        }

        /// <summary>
        /// Returns the value of the string property with the specified name.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>The value of the property.</returns>
        public string GetStringProperty(string name)
        {
            var property = properties.Find(x => x.name == name && x is StringProperty);
            Assert.IsNotNull(property);
            return (property as StringProperty).value;
        }
    }
}
