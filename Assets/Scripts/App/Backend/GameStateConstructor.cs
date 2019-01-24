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

        public GameState CreateCurrentGameState(bool removeNonEssentialData)
        {
            if (_matchManager.MatchType != Enumerators.MatchType.PVP)
                throw new Exception("Game must be PVP");

            Player player0 = _pvpManager.IsFirstPlayer() ? _gameplayManager.CurrentPlayer : _gameplayManager.OpponentPlayer;
            Player player1 = !_pvpManager.IsFirstPlayer() ? _gameplayManager.CurrentPlayer : _gameplayManager.OpponentPlayer;

            GameState gameState = new GameState
            {
                Id = _pvpManager.InitialGameState.Id,
                RandomSeed = _pvpManager.InitialGameState.RandomSeed,
                PlayerStates =
                {
                    CreateFakePlayerStateFromPlayer(player0.InitialPvPPlayerState.Id, player0, removeNonEssentialData),
                    CreateFakePlayerStateFromPlayer(player1.InitialPvPPlayerState.Id, player1, removeNonEssentialData)
                }
            };

            return gameState;
        }

        public static PlayerState CreateFakePlayerStateFromPlayer(string playerId, Player player, bool removeNonEssentialData)
        {
            PlayerState playerState = new PlayerState
            {
                Id = playerId,
                Defense = player.Defense,
                GooVials = player.GooVials,
                CurrentGoo = player.CurrentGoo,
                TurnTime = (int) player.TurnTime,
                CardsInPlay =
                {
                    player.CardsOnBoard
                        .Concat(player.BoardCards.Select(card => card.Model.Card))
                        .Distinct()
                        .Select(card => card.ToProtobuf())
                        .ToArray()
                },
                CardsInDeck =
                {
                    player.CardsInDeck.Select(card => card.ToProtobuf()).ToArray()
                },
                CardsInHand =
                {
                    player.CardsInHand
                        .Distinct()
                        .Select(card => card.ToProtobuf())
                        .ToArray()
                },
                CardsInGraveyard =
                {
                    player.CardsInGraveyard.Select(card => card.ToProtobuf()).ToArray()
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

        private static void RemoveNonEssentialData(CardInstance cardInstance)
        {
            cardInstance.Prototype = new Protobuf.Card();
            cardInstance.AbilitiesInstances?.Clear();
        }
    }
}
