using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class GameStateConstructor
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly IMatchManager _matchManager;
        private readonly IPvPManager _pvpManager;
        private readonly BattlegroundController _battlegroundController;

        public static GameStateConstructor Create()
        {
            return new GameStateConstructor(GameClient.Get<IGameplayManager>(), GameClient.Get<IMatchManager>(), GameClient.Get<IPvPManager>());
        }

        public GameStateConstructor(IGameplayManager gameplayManager, IMatchManager matchManager, IPvPManager pvpManager)
        {
            _gameplayManager = gameplayManager;
            _matchManager = matchManager;
            _pvpManager = pvpManager;
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
        }

        public GameState CreateCurrentGameStateFromOnlineGame(bool removeNonEssentialData)
        {
            if (_matchManager.MatchType != Enumerators.MatchType.PVP)
                throw new Exception("Game must be PVP");

            return CreateGameState(
                _pvpManager.CurrentActionIndex,
                _pvpManager.IsFirstPlayer(),
                _pvpManager.InitialGameState.Id,
                _pvpManager.InitialGameState.RandomSeed,
                _gameplayManager.CurrentPlayer.InitialPvPPlayerState.Id,
                _gameplayManager.OpponentPlayer.InitialPvPPlayerState.Id,
                removeNonEssentialData);
        }

        public GameState CreateCurrentGameStateFromLocalGame(bool removeNonEssentialData)
        {
            if (_matchManager.MatchType != Enumerators.MatchType.PVE &&
                _matchManager.MatchType != Enumerators.MatchType.LOCAL)
                throw new Exception("Game must be PVE or LOCAL");

            bool isFirstPlayer = _gameplayManager.StartingTurn == Enumerators.StartingTurn.Player;
            return CreateGameState(
                -1,
                isFirstPlayer,
                -1,
                -1,
                "CurrentPlayer",
                "OpponentPlayer",
                removeNonEssentialData);
        }

        public static PlayerState CreateFakePlayerStateFromPlayer(string playerId, Player player, bool removeNonEssentialData)
        {
            PlayerState playerState = new PlayerState
            {
                Id = playerId,
                InstanceId = player.InstanceId.ToProtobuf(),
                Defense = player.Defense,
                GooVials = player.GooVials,
                CurrentGoo = player.CurrentGoo,
                TurnTime = (int) player.TurnTime,
                CardsInPlay =
                {
                    player.CardsOnBoard
                        .Concat(player.BoardCards.Select(card => card.Model))
                        .Distinct()
                        .Select(card => card.Card.ToProtobuf())
                        .ToArray()
                },
                CardsInDeck =
                {
                    player.CardsInDeck.Select(card => card.Card.ToProtobuf()).ToArray()
                },
                CardsInHand =
                {
                    player.CardsInHand
                        .Distinct()
                        .Select(card => card.Card.ToProtobuf())
                        .ToArray()
                },
                CardsInGraveyard =
                {
                    player.CardsInGraveyard.Select(card => card.Card.ToProtobuf()).ToArray()
                }
            };

            if (removeNonEssentialData)
            {
                IEnumerable<CardInstance> allCards =
                    playerState.CardsInDeck
                    .Concat(playerState.CardsInHand)
                    .Concat(playerState.CardsInPlay)
                    .Concat(playerState.CardsInGraveyard);

                foreach (CardInstance cardInstance in allCards)
                {
                    RemoveNonEssentialData(cardInstance);
                }
            }

            return playerState;
        }

        private  GameState CreateGameState(int currentActionIndex, bool isFirstPlayer, long matchId, long randomSeed, string currentPlayerId, string opponentPlayerId, bool removeNonEssentialData)
        {
            Player player0 = isFirstPlayer ? _gameplayManager.CurrentPlayer : _gameplayManager.OpponentPlayer;
            Player player1 = !isFirstPlayer ? _gameplayManager.CurrentPlayer : _gameplayManager.OpponentPlayer;
            bool isTurnOfPlayer0 = _gameplayManager.CurrentTurnPlayer == player0;

            GameState gameState = new GameState
            {
                Id = matchId,
                RandomSeed = randomSeed,
                CurrentPlayerIndex = isTurnOfPlayer0 ? 0 : 1,
                PlayerStates =
                {
                    CreateFakePlayerStateFromPlayer(isFirstPlayer ? currentPlayerId : opponentPlayerId, player0, removeNonEssentialData),
                    CreateFakePlayerStateFromPlayer(!isFirstPlayer ? currentPlayerId : opponentPlayerId, player1, removeNonEssentialData)
                },
                CurrentActionIndex = currentActionIndex
            };

            return gameState;
        }

        private static void RemoveNonEssentialData(CardInstance cardInstance)
        {
            cardInstance.Prototype = new Protobuf.Card();
            cardInstance.AbilitiesInstances?.Clear();
        }
    }
}
