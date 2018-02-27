// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// The available zone owners.
    /// </summary>
    public enum ZoneOwner
    {
        Player,
        Shared
    }

    /// <summary>
    /// The available zone types.
    /// </summary>
    public enum ZoneType
    {
        Static,
        Dynamic
    }

    /// <summary>
    /// The available zone owner visibilities.
    /// </summary>
    public enum ZoneOwnerVisibility
    {
        Visible,
        Hidden,
    }

    /// <summary>
    /// The available zone opponent visibilities.
    /// </summary>
    public enum ZoneOpponentVisibility
    {
        Visible,
        Hidden
    }

    /// <summary>
    /// This class represents a game zone type available in a game.
    /// </summary>
    public class GameZoneType : Resource
    {
        /// <summary>
        /// The current resource identifier.
        /// </summary>
        public static int currentId;

        /// <summary>
        /// The name of this zone.
        /// </summary>
        public string name;

        /// <summary>
        /// The owner of this zone.
        /// </summary>
        public ZoneOwner owner;

        /// <summary>
        /// The type of this zone.
        /// </summary>
        public ZoneType type;

        /// <summary>
        /// The visibility of this zone for the owner player.
        /// </summary>
        public ZoneOwnerVisibility ownerVisibility;

        /// <summary>
        /// The visibility of this zone for the opponent player.
        /// </summary>
        public ZoneOpponentVisibility opponentVisibility;

        /// <summary>
        /// True if this zone has a maximum size.
        /// </summary>
        public bool hasMaxSize;

        /// <summary>
        /// The maximum size of this number.
        /// </summary>
        public int maxSize;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameZoneType() : base(currentId++)
        {
        }
    }
}
