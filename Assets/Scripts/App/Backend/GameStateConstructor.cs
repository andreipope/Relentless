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

        public GameState CreateCurrentGameState()
        {
            if (_matchManager.MatchType != Enumerators.MatchType.PVP)
                throw new Exception("Game must be PVP");

            Player player0 = _pvpManager.IsFirstPlayer() ? _gameplayManager.CurrentPlayer : _gameplayManager.OpponentPlayer;
            Player player1 = !_pvpManager.IsFirstPlayer() ? _gameplayManager.CurrentPlayer : _gameplayManager.OpponentPlayer;


            /*IEnumerable<WorkingCard> extraCardsInHand0 =
                _pvpManager.IsFirstPlayer() ?
                    _battlegroundController.PlayerHandCards
                        .Select(card => card.WorkingCard) :
                    _battlegroundController.OpponentHandCards
                        .Select(card => card.WorkingCard);
            IEnumerable<WorkingCard> extraCardsOnBoard0 =
                _pvpManager.IsFirstPlayer() ?
                    _battlegroundController.PlayerBoardCards.Select(card => card.Model.Card) :
                    _battlegroundController.OpponentBoardCards.Select(card => card.Model.Card);

            IEnumerable<WorkingCard> extraCardsInHand1 =
                !_pvpManager.IsFirstPlayer() ?
                    _battlegroundController.PlayerHandCards
                        .Select(card => card.WorkingCard) :
                    _battlegroundController.OpponentHandCards
                        .Select(card => card.WorkingCard);
            IEnumerable<WorkingCard> extraCardsOnBoard1 =
                !_pvpManager.IsFirstPlayer() ?
                    _battlegroundController.PlayerBoardCards.Select(card => card.Model.Card) :*/
                    _battlegroundController.OpponentBoardCards.Select(card => card.Model.Card);

            GameState gameState = new GameState
            {
                Id = _pvpManager.InitialGameState.Id,
                RandomSeed = _pvpManager.InitialGameState.RandomSeed,
                PlayerStates =
                {
                    CreateFakePlayerStateFromPlayer(player0.InitialPvPPlayerState.Id, player0),
                    CreateFakePlayerStateFromPlayer(player1.InitialPvPPlayerState.Id, player1)
                }
            };

            return gameState;
        }

        public static PlayerState CreateFakePlayerStateFromPlayer(string playerId, Player player, IEnumerable<WorkingCard> extraCardsInHand = null, IEnumerable<WorkingCard> extraCardsOnBoard = null)
        {
            if (extraCardsInHand == null)
            {
                extraCardsInHand = Array.Empty<WorkingCard>();
            }

            if (extraCardsOnBoard == null)
            {
                extraCardsOnBoard = Array.Empty<WorkingCard>();
            }
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
                        .Concat(extraCardsOnBoard)
                        .Distinct()
                        .Select(card => RemoveNonEssentialData(card.ToProtobuf()))
                        .ToArray()
                },
                CardsInDeck =
                {
                    player.CardsInDeck.Select(card => RemoveNonEssentialData(card.ToProtobuf())).ToArray()
                },
                CardsInHand =
                {
                    player.CardsInHand
                        .Concat(extraCardsInHand)
                        .Distinct()
                        .Select(card => RemoveNonEssentialData(card.ToProtobuf()))
                        .ToArray()
                },
                CardsInGraveyard =
                {
                    player.CardsInGraveyard.Select(card => RemoveNonEssentialData(card.ToProtobuf())).ToArray()
                }
            };
            return playerState;
        }

        private static CardInstance RemoveNonEssentialData(CardInstance cardInstance)
        {
            cardInstance.Prototype = new Protobuf.Card();
            cardInstance.AbilitiesInstances?.Clear();
            return cardInstance;
        }
    }
}
