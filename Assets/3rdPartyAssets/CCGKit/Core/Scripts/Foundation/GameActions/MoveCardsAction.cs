// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// Game action to move cards from one zone to another.
    /// </summary>
    public class MoveCardsAction : GameAction
    {
        /// <summary>
        /// The origin zone of this action.
        /// </summary>
        [GameZoneField("Origin")]
        public int originZoneId;

        /// <summary>
        /// The destination zone of this action.
        /// </summary>
        [GameZoneField("Destination")]
        public int destinationZoneId;

        /// <summary>
        /// The number of cards to move.
        /// </summary>
        [IntField("Num cards")]
        public int numCards;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MoveCardsAction() : base("Move cards")
        {
        }

        /// <summary>
        /// Resolves this action.
        /// </summary>
        /// <param name="state">The state of the game.</param>
        /// <param name="player">The player on which to resolve this action.</param>
        public override void Resolve(GameState state, PlayerInfo player)
        {
            var fromZone = player.zones[originZoneId];
            var toZone = player.zones[destinationZoneId];
            // Do not move more cards than those available in the origin zone.
            if (numCards > fromZone.cards.Count)
            {
                numCards = fromZone.cards.Count;
            }

            // Do not move more card than those allowed in the destination zone.
            if (numCards > toZone.maxCards)
            {
                numCards = toZone.maxCards;
            }

            for (var i = 0; i < numCards; i++)
            {
                toZone.AddCard(fromZone.cards[i]);
            }
            fromZone.RemoveCards(numCards);
        }
    }
}
