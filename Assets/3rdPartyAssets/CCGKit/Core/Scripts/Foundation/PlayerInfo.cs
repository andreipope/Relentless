// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This class stores the current stat of a player in a game.
    /// </summary>
    public class PlayerInfo
    {
        /// <summary>
        /// The unique identifier of this player.
        /// </summary>
        public int id;

        /// <summary>
        /// The unique connection identifier of this player.
        /// </summary>
        public int connectionId;

        /// <summary>
        /// The unique network instance identifier of this player.
        /// </summary>
        public NetworkInstanceId netId;

        /// <summary>
        /// The nickname of this player.
        /// </summary>
        public string nickname;

        /// <summary>
        /// True if this player is currently connected to the server; false otherwise.
        /// </summary>
        public bool isConnected;

        /// <summary>
        /// True if this player is controlled by a human; false otherwise (AI).
        /// </summary>
        public bool isHuman;

        /// <summary>
        /// The stats of this player, indexed by id.
        /// </summary>
        public Dictionary<int, Stat> stats = new Dictionary<int, Stat>();

        /// <summary>
        /// The stats of this player, indexed by name.
        /// </summary>
        public Dictionary<string, Stat> namedStats = new Dictionary<string, Stat>();

        /// <summary>
        /// The zones of this player, indexed by id.
        /// </summary>
        public Dictionary<int, RuntimeZone> zones = new Dictionary<int, RuntimeZone>();

        /// <summary>
        /// The zones of this player, indexed by name.
        /// </summary>
        public Dictionary<string, RuntimeZone> namedZones = new Dictionary<string, RuntimeZone>();

        /// <summary>
        /// The current card instance identifier of this player.
        /// </summary>
        public int currentCardInstanceId;

        /// <summary>
        /// The current turn number of this player.
        /// </summary>
        public int numTurn;

        /// <summary>
        /// Returns the card with the specified instance identifier in the specified zone.
        /// </summary>
        /// <param name="cardInstanceId">The instance identifier of this card.</param>
        /// <param name="zoneId">The zone identifier of this card.</param>
        /// <returns>The card with the specified instance identifier in the specified zone.</returns>
        public RuntimeCard GetCard(int cardInstanceId, int zoneId)
        {
            var card = zones[zoneId].cards.Find(x => x.instanceId == cardInstanceId);
            return card;
        }
    }
}
