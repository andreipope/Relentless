// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine.Assertions;

namespace CCGKit
{
    /// <summary>
    /// This class represents a single card in the game.
    /// </summary>
    public class Card : Resource
    {
        /// <summary>
        /// The current resource identifier.
        /// </summary>
        public static int currentId;

        /// <summary>
        /// The type of this card.
        /// </summary>
        public int cardTypeId;

        /// <summary>
        /// The name of this card.
        /// </summary>
        public string name;

        /// <summary>
        /// The costs of this card.
        /// </summary>
        public List<Cost> costs = new List<Cost>();

        /// <summary>
        /// The properties of this card.
        /// </summary>
        public List<Property> properties = new List<Property>();

        /// <summary>
        /// The stats of this card.
        /// </summary>
        public List<Stat> stats = new List<Stat>();

        /// <summary>
        /// The keywords of this card.
        /// </summary>
        public List<RuntimeKeyword> keywords = new List<RuntimeKeyword>();

        /// <summary>
        /// The abilities of this card.
        /// </summary>
        public List<Ability> abilities = new List<Ability>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public Card() : base(currentId++)
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
