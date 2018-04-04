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

        public Deck defaultDeck;
        /// <summary>
        /// Static instance.
        /// </summary>
        private static readonly GameManager instance = new GameManager();


        public bool tutorial;
        public int tutorialStep;
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
