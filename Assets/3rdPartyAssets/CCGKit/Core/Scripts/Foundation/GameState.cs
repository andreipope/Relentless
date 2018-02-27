// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// This class stores the current, complete state of a game.
    /// </summary>
    public class GameState
    {
        /// <summary>
        /// The configuration of this game.
        /// </summary>
        public GameConfiguration config;

        /// <summary>
        /// The players of this game.
        /// </summary>
        public List<PlayerInfo> players = new List<PlayerInfo>();

        /// <summary>
        /// The current player of this game.
        /// </summary>
        public PlayerInfo currentPlayer;

        /// <summary>
        /// The current opponent of this game.
        /// </summary>
        public PlayerInfo currentOpponent;

        /// <summary>
        /// The effect solver to use in this game.
        /// </summary>
        public EffectSolver effectSolver;
    }
}
