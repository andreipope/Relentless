// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// The authoritative game server, which is responsible for driving the game data and logic in a
    /// multiplayer game. It provides the fundamental functionality needed in an online collectible card
    /// game, namely management of the turn sequence and application of card effects. The entire project
    /// structure revolves around the fact that the server is authoritative; in order to prevent hacking,
    /// clients are fundamentally limited to sending the player input to the server and updating the visual
    /// state of the game on screen while all the critical game logic is performed on the server side.
    ///
    /// The goal is to provide useful default behavior that can be applied to a wide spectrum of games while
    /// also allowing further specialization via subclassing.
    /// </summary>
    public class Server : NetworkBehaviour
    {
        /// <summary>
        /// The duration of a turn in a game (in seconds).
        /// </summary>
        public int turnDuration { get; private set; }

        /// <summary>
        /// Index of the current player in the list of players.
        /// </summary>
        protected int currentPlayerIndex;

        /// <summary>
        /// Holds the entire state of the game.
        /// </summary>
        public GameState gameState = new GameState();

        /// <summary>
        /// The effect solver used to resolve card effects.
        /// </summary>
        public EffectSolver effectSolver { get; protected set; }

        /// <summary>
        /// List of server handler classes.
        /// </summary>
        protected List<ServerHandler> handlers = new List<ServerHandler>();

        /// <summary>
        /// Current turn.
        /// </summary>
        protected int currentTurn;

        /// <summary>
        /// True if the game has finished; false otherwise.
        /// </summary>
        protected bool gameFinished;

        /// <summary>
        /// Cached reference to the currently-executing turn coroutine.
        /// </summary>
        protected Coroutine turnCoroutine;

        /// <summary>
        /// Called when the server starts listening.
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();

            LoadGameConfiguration();
            AddServerHandlers();
            RegisterServerHandlers();
        }

        /// <summary>
        /// Loads the game configuration.
        /// </summary>
        protected virtual void LoadGameConfiguration()
        {
            var gameConfig = GameManager.Instance.config;
            turnDuration = gameConfig.properties.turnDuration;
        }

        /// <summary>
        /// Adds the server handlers that are actually responsible of implementing the server's logic.
        /// </summary>
        protected virtual void AddServerHandlers()
        {
            handlers.Add(new PlayerRegistrationHandler(this));
            handlers.Add(new TurnSequenceHandler(this));
            handlers.Add(new EffectSolverHandler(this));
            handlers.Add(new ChatHandler(this));
        }

        /// <summary>
        /// Registers the network handlers for the messages the server is interested in listening to.
        /// </summary>
        protected virtual void RegisterServerHandlers()
        {
            foreach (var handler in handlers)
            {
                handler.RegisterNetworkHandlers();
            }
        }

        /// <summary>
        /// Unregisters the network handlers for the messages the server is interested in listening to.
        /// </summary>
        protected virtual void UnregisterServerHandlers()
        {
            foreach (var handler in handlers)
            {
                handler.UnregisterNetworkHandlers();
            }
            handlers.Clear();
        }

        /// <summary>
        /// This function is called when the NetworkBehaviour will be destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            UnregisterServerHandlers();
        }

        /// <summary>
        /// Starts the multiplayer game. This is automatically called when the appropriate number of players
        /// have joined a room.
        /// </summary>
        public virtual void StartGame()
        {
            Logger.Log("Game has started.");

            // Start with turn 1.
            currentTurn = 1;

            var players = gameState.players;

            // Create an array with all the player nicknames.
            var playerNicknames = new List<string>(players.Count);
            foreach (var player in players)
            {
                playerNicknames.Add(player.nickname);
            }

            // Set the current player and opponents.
            gameState.currentPlayer = players[currentPlayerIndex];
            gameState.currentOpponent = players.Find(x => x != gameState.currentPlayer);

            var rngSeed = System.Environment.TickCount;
            effectSolver = new EffectSolver(gameState, rngSeed);

            foreach (var player in players)
            {
                effectSolver.SetTriggers(player);
                foreach (var zone in player.zones)
                {
                    foreach (var card in zone.Value.cards)
                    {
                        effectSolver.SetDestroyConditions(card);
                        effectSolver.SetTriggers(card);
                    }
                }
            }

            // Execute the game start actions.
            foreach (var action in GameManager.Instance.config.properties.gameStartActions)
            {
                ExecuteGameAction(action);
            }

            // Send a StartGame message to all the connected players.
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                var msg = new StartGameMessage();
                msg.recipientNetId = player.netId;
                msg.playerIndex = i;
                msg.turnDuration = turnDuration;
                msg.nicknames = playerNicknames.ToArray();
                msg.player = GetPlayerNetworkState(player);
                msg.opponent = GetOpponentNetworkState(players.Find(x => x != player));
                msg.rngSeed = rngSeed;
                SafeSendToClient(player, NetworkProtocol.StartGame, msg);
            }

            // Start running the turn sequence coroutine.
            turnCoroutine = StartCoroutine(RunTurn());
        }

        public virtual void SendPlayerInfoToAllClients(PlayerInfo player)
        {
            var playerStateMsg = new PlayerGameStateMessage();
            playerStateMsg.player = GetPlayerNetworkState(player);
            SafeSendToClient(player, NetworkProtocol.PlayerState, playerStateMsg);
        }

        protected virtual NetPlayerInfo GetPlayerNetworkState(PlayerInfo player)
        {
            var netPlayer = new NetPlayerInfo();
            netPlayer.id = player.id;
            netPlayer.netId = player.netId;

            // Copy player stats.
            var stats = new NetStat[player.stats.Count];
            var idx = 0;
            foreach (var entry in player.stats)
            {
                var stat = entry.Value;
                stats[idx++] = NetworkingUtils.GetNetStat(stat);
            }
            netPlayer.stats = stats;

            var gameConfig = GameManager.Instance.config;
            // Copy player zones.
            var staticZones = new List<NetStaticZone>();
            var dynamicZones = new List<NetDynamicZone>();
            foreach (var zonePair in player.zones)
            {
                var zone = zonePair.Value;
                var zoneDefinition = gameConfig.gameZones.Find(x => x.id == zone.zoneId);
                if (zoneDefinition.type == ZoneType.Static)
                {
                    var staticZone = new NetStaticZone();
                    staticZone.zoneId = zone.zoneId;
                    if (zoneDefinition.ownerVisibility == ZoneOwnerVisibility.Visible)
                    {
                        staticZone.cards = new NetStaticCard[zone.cards.Count];
                        var i = 0;
                        foreach (var card in zone.cards)
                        {
                            var netCard = new NetStaticCard();
                            netCard.cardId = card.cardId;
                            netCard.instanceId = card.instanceId;
                            staticZone.cards[i++] = netCard;
                        }
                    }
                    staticZone.numCards = zone.cards.Count;
                    staticZones.Add(staticZone);
                }
                else if (zoneDefinition.type == ZoneType.Dynamic)
                {
                    var dynamicZone = new NetDynamicZone();
                    dynamicZone.zoneId = zone.zoneId;
                    dynamicZone.cards = new NetCard[zone.cards.Count];
                    for (var j = 0; j < zone.cards.Count; j++)
                    {
                        var card = zone.cards[j];
                        var netCard = new NetCard();
                        netCard.cardId = card.cardId;
                        netCard.instanceId = card.instanceId;
                        netCard.stats = new NetStat[card.stats.Count];
                        idx = 0;
                        foreach (var entry in card.stats)
                        {
                            netCard.stats[idx++] = NetworkingUtils.GetNetStat(entry.Value);
                        }
                        netCard.keywords = new NetKeyword[card.keywords.Count];
                        idx = 0;
                        foreach (var entry in card.keywords)
                        {
                            netCard.keywords[idx++] = NetworkingUtils.GetNetKeyword(entry);
                        }
                        dynamicZone.cards[j] = netCard;
                    }
                    dynamicZone.numCards = zone.cards.Count;
                    dynamicZones.Add(dynamicZone);
                }
            }
            netPlayer.staticZones = staticZones.ToArray();
            netPlayer.dynamicZones = dynamicZones.ToArray();

            return netPlayer;
        }

        protected virtual NetPlayerInfo GetOpponentNetworkState(PlayerInfo player)
        {
            var netOpponent = new NetPlayerInfo();
            netOpponent.id = player.id;
            netOpponent.netId = player.netId;

            // Copy player stats.
            var stats = new NetStat[player.stats.Count];
            var idx = 0;
            foreach (var entry in player.stats)
            {
                var stat = entry.Value;
                stats[idx++] = NetworkingUtils.GetNetStat(stat);
            }
            netOpponent.stats = stats;

            // Copy player zones.
            var gameConfig = GameManager.Instance.config;
            var staticZones = new List<NetStaticZone>();
            var dynamicZones = new List<NetDynamicZone>();
            foreach (var zonePair in player.zones)
            {
                var zone = zonePair.Value;
                var zoneDefinition = gameConfig.gameZones.Find(x => x.id == zone.zoneId);
                if (zoneDefinition.type == ZoneType.Static)
                {
                    var staticZone = new NetStaticZone();
                    staticZone.zoneId = zone.zoneId;
                    if (zoneDefinition.opponentVisibility == ZoneOpponentVisibility.Visible)
                    {
                        staticZone.cards = new NetStaticCard[zone.cards.Count];
                        var i = 0;
                        foreach (var card in zone.cards)
                        {
                            var netCard = new NetStaticCard();
                            netCard.cardId = card.cardId;
                            netCard.instanceId = card.instanceId;
                            staticZone.cards[i++] = netCard;
                        }
                    }
                    staticZone.numCards = zone.cards.Count;
                    staticZones.Add(staticZone);
                }
                else if (zoneDefinition.type == ZoneType.Dynamic)
                {
                    var dynamicZone = new NetDynamicZone();
                    dynamicZone.zoneId = zone.zoneId;
                    if (zoneDefinition.opponentVisibility == ZoneOpponentVisibility.Visible)
                    {
                        dynamicZone.cards = new NetCard[zone.cards.Count];
                        for (var j = 0; j < zone.cards.Count; j++)
                        {
                            var card = zone.cards[j];
                            var netCard = new NetCard();
                            netCard.cardId = card.cardId;
                            netCard.instanceId = card.instanceId;
                            netCard.stats = new NetStat[card.stats.Count];
                            idx = 0;
                            foreach (var entry in card.stats)
                            {
                                netCard.stats[idx++] = NetworkingUtils.GetNetStat(entry.Value);
                            }
                            netCard.keywords = new NetKeyword[card.keywords.Count];
                            idx = 0;
                            foreach (var entry in card.keywords)
                            {
                                netCard.keywords[idx++] = NetworkingUtils.GetNetKeyword(entry);
                            }
                            dynamicZone.cards[j] = netCard;
                        }
                    }
                    dynamicZone.numCards = zone.cards.Count;
                    dynamicZones.Add(dynamicZone);
                }
            }
            netOpponent.staticZones = staticZones.ToArray();
            netOpponent.dynamicZones = dynamicZones.ToArray();

            return netOpponent;
        }

        /// <summary>
        /// Ends the current game.
        /// </summary>
        /// <param name="player">The player that has won/lost.</param>
        /// <param name="type">The result of the game (win/loss).</param>
        public virtual void EndGame(PlayerInfo player, EndGameType type)
        {
            gameFinished = true;
            var msg = new EndGameMessage();
            switch (type)
            {
                case EndGameType.Win:
                    msg.winnerPlayerIndex = player.netId;
                    break;

                case EndGameType.Loss:
                    msg.winnerPlayerIndex = gameState.players.Find(x => x != player).netId;
                    break;
            }
            NetworkServer.SendToAll(NetworkProtocol.EndGame, msg);
        }

        /// <summary>
        /// Runs the coroutine that authoritatively drives the turn sequence.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator RunTurn()
        {
            while (!gameFinished)
            {
                StartTurn();
                yield return new WaitForSeconds(turnDuration);
                EndTurn();
            }
        }

        /// <summary>
        /// Starts a new game turn.
        /// </summary>
        protected virtual void StartTurn()
        {
            Logger.Log("Start turn for player " + currentPlayerIndex + ".");

            var players = gameState.players;

            // Update the current player and opponents.
            gameState.currentPlayer = players[currentPlayerIndex];
            gameState.currentOpponent = players.Find(x => x != gameState.currentPlayer);

            gameState.currentPlayer.numTurn += 1;

            // Execute the turn start actions.
            foreach (var action in GameManager.Instance.config.properties.turnStartActions)
            {
                ExecuteGameAction(action);
            }

            // Run any code that needs to be executed at turn start time.
            PerformTurnStartStateInitialization();

            // Let the server handlers know the turn has started.
            for (var i = 0; i < handlers.Count; i++)
            {
                handlers[i].OnStartTurn();
            }

            effectSolver.OnTurnStarted();

            // Send a StartTurn message to all the connected players.
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                var msg = new StartTurnMessage();
                msg.recipientNetId = player.netId;
                msg.isRecipientTheActivePlayer = player == gameState.currentPlayer;
                msg.turn = currentTurn;
                msg.player = GetPlayerNetworkState(player);
                msg.opponent = GetOpponentNetworkState(players.Find(x => x != player));
                SafeSendToClient(player, NetworkProtocol.StartTurn, msg);
            }
        }

        /// <summary>
        /// This method can be used by subclasses to perform turn-start-specific initialization logic.
        /// </summary>
        protected virtual void PerformTurnStartStateInitialization()
        {
        }

        /// <summary>
        /// Ends the current game turn.
        /// </summary>
        protected virtual void EndTurn()
        {
            Logger.Log("End turn for player " + currentPlayerIndex + ".");

            // Let the server handlers know the turn has ended.
            for (var i = 0; i < handlers.Count; i++)
                handlers[i].OnEndTurn();

            effectSolver.OnTurnEnded();

            var players = gameState.players;

            foreach (var player in players)
            {
                foreach (var entry in player.stats)
                {
                    entry.Value.OnEndTurn();
                }
            }

            foreach (var zone in players[currentPlayerIndex].zones)
            {
                foreach (var card in zone.Value.cards)
                {
                    foreach (var stat in card.stats)
                    {
                        stat.Value.OnEndTurn();
                    }
                }
            }

            // Send the EndTurn message to all players.
            foreach (var player in players)
            {
                var msg = new EndTurnMessage();
                msg.recipientNetId = player.netId;
                msg.isRecipientTheActivePlayer = player == players[currentPlayerIndex];
                SafeSendToClient(player, NetworkProtocol.EndTurn, msg);
            }

            // Switch to next player.
            currentPlayerIndex += 1;
            if (currentPlayerIndex == players.Count)
            {
                currentPlayerIndex = 0;
                // Increase turn count.
                currentTurn += 1;
            }
        }

        /// <summary>
        /// Stops the current turn.
        /// </summary>
        public virtual void StopTurn()
        {
            if (turnCoroutine != null)
                StopCoroutine(turnCoroutine);
            EndTurn();
            turnCoroutine = StartCoroutine(RunTurn());
        }

        /// <summary>
        /// Called when a player with the specified connection identifier connects to the server.
        /// </summary>
        /// <param name="connectionId">The player's connection identifier.</param>
        public virtual void OnPlayerConnected(int connectionId)
        {
            Logger.Log("Player with id " + connectionId + " connected to server.");
            /*var player = Players.Find(x => x.ConnectionId == connectionId);
            if (player != null)
                player.IsConnected = true;*/
        }

        /// <summary>
        /// Called when a player with the specified connection identifier disconnects from the server.
        /// </summary>
        /// <param name="connectionId">The player's connection identifier.</param>
        public virtual void OnPlayerDisconnected(int connectionId)
        {
            Logger.Log("Player with id " + connectionId + " disconnected from server.");
            /*var player = Players.Find(x => x.ConnectionId == connectionId);
            if (player != null)
                player.IsConnected = false;*/
        }

        public virtual void SafeSendToClient(PlayerInfo player, short msgType, MessageBase msg)
        {
            if (player != null && player.isConnected)
            {
                NetworkServer.SendToClient(player.connectionId, msgType, msg);
            }
        }

        /// <summary>
        /// Executes the specified game action.
        /// </summary>
        /// <param name="action">Game action to execute.</param>
        protected void ExecuteGameAction(GameAction action)
        {
            var targetPlayers = new List<PlayerInfo>();
            switch (action.target)
            {
                case GameActionTarget.CurrentPlayer:
                    targetPlayers.Add(gameState.currentPlayer);
                    break;

                case GameActionTarget.CurrentOpponent:
                    targetPlayers.Add(gameState.currentOpponent);
                    break;

                case GameActionTarget.AllPlayers:
                    targetPlayers = gameState.players;
                    break;
            }

            foreach (var player in targetPlayers)
            {
                action.Resolve(gameState, player);
            }
        }
    }
}
