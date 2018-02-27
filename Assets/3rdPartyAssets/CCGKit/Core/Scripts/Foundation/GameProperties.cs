// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// The general properties of a game.
    /// </summary>
    public class GameProperties
    {
        /// <summary>
        /// Duration of a game turn (in seconds).
        /// </summary>
        public int turnDuration;

        /// <summary>
        /// Minimum number of cards that need to be in a deck.
        /// </summary>
        public int minDeckSize;

        /// <summary>
        /// Maximum number of cards that can be in a deck.
        /// </summary>
        public int maxDeckSize;

        /// <summary>
        /// List of actions to perform when a game starts.
        /// </summary>
        public List<GameAction> gameStartActions = new List<GameAction>();

        /// <summary>
        /// List of actions to perform when a turn starts.
        /// </summary>
        public List<GameAction> turnStartActions = new List<GameAction>();

        /// <summary>
        /// List of actions to perform when a turn ends.
        /// </summary>
        public List<GameAction> turnEndActions = new List<GameAction>();

        /// <summary>
        /// List of end game conditions of this game.
        /// </summary>
        public List<EndGameCondition> endGameConditions = new List<EndGameCondition>();
    }
}
