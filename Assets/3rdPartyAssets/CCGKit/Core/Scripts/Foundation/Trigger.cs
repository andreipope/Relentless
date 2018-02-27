// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// The base trigger class.
    /// </summary>
    public abstract class Trigger
    {
    }

    /// <summary>
    /// The base trigger class for triggers related to player stat changes.
    /// </summary>
    public abstract class OnPlayerStatChangedTrigger : Trigger
    {
        /// <summary>
        /// The stat of this trigger.
        /// </summary>
        [PlayerStatField("Stat")]
        public int statId;

        /// <summary>
        /// Returns true if this trigger is true and false otherwise.
        /// </summary>
        /// <param name="stat">The stat.</param>
        /// <param name="newValue">The new value of the stat.</param>
        /// <param name="oldValue">The old value of the stat.</param>
        /// <returns>True if this trigger is true; false otherwise.</returns>
        public abstract bool IsTrue(Stat stat, int newValue, int oldValue);
    }

    /// <summary>
    /// The base trigger class for triggers related to card stat changes.
    /// </summary>
    public abstract class OnCardStatChangedTrigger : Trigger
    {
        /// <summary>
        /// The card type of this trigger.
        /// </summary>
        [CardTypeField("Card type")]
        public int cardTypeId;

        /// <summary>
        /// The stat of this trigger.
        /// </summary>
        [CardStatField("Stat")]
        public int statId;

        /// <summary>
        /// Returns true if this trigger is true and false otherwise.
        /// </summary>
        /// <param name="stat">The stat.</param>
        /// <param name="newValue">The new value of the stat.</param>
        /// <param name="oldValue">The old value of the stat.</param>
        /// <returns>True if this trigger is true; false otherwise.</returns>
        public abstract bool IsTrue(Stat stat, int newValue, int oldValue);
    }

    /// <summary>
    /// The base trigger class for triggers related to card movements.
    /// </summary>
    public abstract class OnCardMovedTrigger : Trigger
    {
        /// <summary>
        /// Returns true if this trigger is true and false otherwise.
        /// </summary>
        /// <param name="stat">The stat.</param>
        /// <param name="newValue">The new value of the stat.</param>
        /// <param name="oldValue">The old value of the stat.</param>
        /// <returns>True if this trigger is true; false otherwise.</returns>
        public abstract bool IsTrue(GameState state, string zone);
    }
}
