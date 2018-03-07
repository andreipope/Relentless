// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using GrandDevs.CZB;

namespace CCGKit
{
    /// <summary>
    /// This class is the in-game entry point to the game configuration managed from within
    /// the CCG Kit menu option in Unity.
    /// </summary>
    public sealed class GameManager
    {
        /// <summary>
        /// The configuration of this game.
        /// </summary>
        public GameConfiguration config = new GameConfiguration();

        public List<Hero> heroes = new List<Hero>();


        /// <summary>
        /// The default deck of this game.
        /// </summary>
        public Deck defaultDeck;

        /// <summary>
        /// The player decks of this game.
        /// </summary>
        public List<Deck> playerDecks = new List<Deck>();

        /// <summary>
        /// The player's nickname of this game.
        /// </summary>
        public string playerName;

        /// <summary>
        /// True if the player is logged in; false otherwise (used in Master Server Kit
        /// integration).
        /// </summary>
        public bool isPlayerLoggedIn;

        /// <summary>
        /// Current player deck id
        /// </summary>
        /// 
        public int currentDeckId = -1;
        public int opponentDeckId = -1;


        public int currentEditingDeck = -1;

        public int currentHeroId = -1;
        public int opponentHeroId = -1;


        /// <summary>
        /// Static instance.
        /// </summary>
        private static readonly GameManager instance = new GameManager();

        /// <summary>
        /// Constructor.
        /// </summary>
        private GameManager()
        {
            config.LoadGameConfigurationAtRuntime();
        }

        /// <summary>
        /// Static instance.
        /// </summary>
        public static GameManager Instance
        {
            get { return instance; }
        }
    }
}
