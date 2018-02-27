// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// The ability types supported by the kit.
    /// </summary>
    public enum AbilityType
    {
        Triggered,
        Activated
    }

    /// <summary>
    /// The base ability class.
    /// </summary>
    public class Ability
    {
        /// <summary>
        /// The name of this ability.
        /// </summary>
        public string name;

        /// <summary>
        /// The type of this ability.
        /// </summary>
        public AbilityType type;

        /// <summary>
        /// The effect of this ability.
        /// </summary>
        public Effect effect;

        /// <summary>
        /// The target of this ability.
        /// </summary>
        public Target target;
    }

    /// <summary>
    /// Triggered abilities are abilities that get resolved when their
    /// associated trigger takes place.
    /// </summary>
    public class TriggeredAbility : Ability
    {
        /// <summary>
        /// The trigger of this ability.
        /// </summary>
        public Trigger trigger;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TriggeredAbility()
        {
            type = AbilityType.Triggered;
        }
    }

    /// <summary>
    /// Activated abilities are abilities that get resolved when the player
    /// pays a cost/s.
    /// </summary>
    public class ActivatedAbility : Ability
    {
        /// <summary>
        /// The zone of this ability.
        /// </summary>
        [GameZoneField("Zone")]
        public int zoneId;

        /// <summary>
        /// The costs of this ability.
        /// </summary>
        public List<Cost> costs = new List<Cost>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ActivatedAbility()
        {
            type = AbilityType.Activated;
        }
    }
}
