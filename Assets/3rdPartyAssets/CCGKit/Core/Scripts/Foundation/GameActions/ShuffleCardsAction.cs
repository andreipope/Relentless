// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// Game action to shuffle the cards in a zone.
    /// </summary>
    public class ShuffleCardsAction : GameAction
    {
        /// <summary>
        /// The zone of this action.
        /// </summary>
        [GameZoneField("Zone")]
        public int zoneId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ShuffleCardsAction() : base("Shuffle cards")
        {
        }

        /// <summary>
        /// Resolves this action.
        /// </summary>
        /// <param name="state">The state of the game.</param>
        /// <param name="player">The player on which to resolve this action.</param>
        public override void Resolve(GameState state, PlayerInfo player)
        {
            var zone = player.zones[zoneId];
            zone.cards.Shuffle();
        }
    }
}
