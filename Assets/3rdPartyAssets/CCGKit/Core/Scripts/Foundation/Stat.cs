// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// The modifier of a stat.
    /// </summary>
    public class Modifier
    {
        /// <summary>
        /// The constant value to identify a permanent modifier.
        /// </summary>
        private const int PERMANENT = 0;

        /// <summary>
        /// The value of this modifier.
        /// </summary>
        public int value;

        /// <summary>
        /// The duration of this modifier.
        /// </summary>
        public int duration;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">The value of the modifier.</param>
        /// <param name="duration">The duration of the modifier.</param>
        public Modifier(int value, int duration = PERMANENT)
        {
            this.value = value;
            this.duration = duration;
        }

        /// <summary>
        /// Returns true if this modifier is permanent and false otherwise.
        /// </summary>
        /// <returns>True if this modifier is permanent; false otherwise.</returns>
        public bool IsPermanent()
        {
            return duration == PERMANENT;
        }
    }

    /// <summary>
    /// Stats are a fundamental concept in CCG Kit. They represent integer values that can change over
    /// the course of a game and are used in both players and cards. For example, a player could have
    /// life and mana stats and a creature card could have cost, attack and defense stats. Stats are
    /// transmitted over the network, which means you should only use them to represent values that can
    /// actually change over the course of a game in order to save bandwidth.
    /// </summary>
    public class Stat
    {
        /// <summary>
        /// The identifier of this stat.
        /// </summary>
        public int statId;

        /// <summary>
        /// The name of this stat.
        /// </summary>
        public string name;

        /// <summary>
        /// The base value of this stat.
        /// </summary>
        private int _baseValue;

        /// <summary>
        /// The base value of this stat.
        /// </summary>
        [SerializeField]
        public int baseValue
        {
            get { return _baseValue; }
            set
            {
                var oldValue = _baseValue;
                _baseValue = value;
                if (onValueChanged != null && oldValue != _baseValue)
                {
                    onValueChanged(oldValue, value);
                }
            }
        }

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
        /// The modifiers of this stat.
        /// </summary>
        public List<Modifier> modifiers = new List<Modifier>();

        /// <summary>
        /// The callback that is called when the value of this stat changes.
        /// </summary>
        public Action<int, int> onValueChanged;

        /// <summary>
        /// The effective value of this stat.
        /// </summary>
        public int effectiveValue
        {
            get
            {
                // Start with the base value.
                var value = baseValue;

                // Apply all the modifiers.
                foreach (var modifier in modifiers)
                {
                    value += modifier.value;
                }

                // Clamp to [minValue, maxValue] if needed.
                if (value < minValue)
                {
                    value = minValue;
                }
                else if (value > maxValue)
                {
                    value = maxValue;
                }

                // Return the effective value.
                return value;
            }
        }

        /// <summary>
        /// Adds a modifier to this stat.
        /// </summary>
        /// <param name="modifier">The modifier to add.</param>
        public void AddModifier(Modifier modifier)
        {
            var oldValue = effectiveValue;
            modifiers.Add(modifier);
            if (onValueChanged != null)
            {
                onValueChanged(oldValue, effectiveValue);
            }
        }

        /// <summary>
        /// This method is automatically called when the turn ends.
        /// </summary>
        public void OnEndTurn()
        {
            var oldValue = effectiveValue;

            var modifiersToRemove = new List<Modifier>(modifiers.Count);

            var temporaryModifiers = modifiers.FindAll(x => !x.IsPermanent());
            foreach (var modifier in temporaryModifiers)
            {
                modifier.duration -= 1;
                if (modifier.duration <= 0)
                {
                    modifiersToRemove.Add(modifier);
                }
            }

            foreach (var modifier in modifiersToRemove)
            {
                modifiers.Remove(modifier);
            }
            if (modifiersToRemove.Count > 0 && onValueChanged != null)
            {
                onValueChanged(oldValue, effectiveValue);
            }
        }
    }
}
