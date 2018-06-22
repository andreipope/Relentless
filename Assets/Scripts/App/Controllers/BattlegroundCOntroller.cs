using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class BattlegrdController : IController
    {

        public int turnDuration { get; private set; }
        protected int currentPlayerIndex;
        public GameState gameState = new GameState();
        public EffectSolver effectSolver { get; protected set; }
        protected List<ServerHandler> handlers = new List<ServerHandler>();
        protected int currentTurn;
        protected bool gameFinished;
        protected Coroutine turnCoroutine;


        public BattlegrdController()
        {
            //LoadGameConfiguration();
            //AddServerHandlers();
            //RegisterServerHandlers();
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        protected virtual void LoadGameConfiguration()
        {
            var gameConfig = GameManager.Instance.config;
            turnDuration = gameConfig.properties.turnDuration;
            if (GameManager.Instance.tutorial)
                turnDuration = 100000;
        }

        public virtual void StartGame()
        {
            Debug.Log("Game has started.");

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

            if (!GameManager.Instance.tutorial)
            {
                // Execute the game start actions.
                foreach (var action in GameManager.Instance.config.properties.gameStartActions)
                {
                    ExecuteGameAction(action);
                }
            }
            else
                gameState.currentOpponent.stats[0].baseValue = 8;

            if (Constants.DEV_MODE)
                gameState.currentOpponent.stats[0].baseValue = 100;

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

        public virtual void EndGame(PlayerInfo player, EndGameType type)
        {
            if (GameManager.Instance.tutorial)
                return;
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

        protected virtual IEnumerator RunTurn()
        {
            while (!gameFinished)
            {
                StartTurn();
                yield return new WaitForSeconds(turnDuration);
                EndTurn();
            }
        }

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

        public virtual void StopTurn()
        {
            if (turnCoroutine != null)
                StopCoroutine(turnCoroutine);
            EndTurn();
            //if (!GameManager.Instance.tutorial)
            turnCoroutine = StartCoroutine(RunTurn());
        }

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