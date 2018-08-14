using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB.BackendCommunication
{
    public class ActionLogCollectorUploader : IService
    {
        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;
        private PlayerEventListener _playerEventListener;
        private PlayerEventListener _opponentEventListener;

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            
            _gameplayManager.OnGameInitializedEvent += GameplayManagerOnGameInitializedEvent;
            _gameplayManager.OnGameEndedEvent += GameplayManagerOnGameEndedEvent;
        }

        private void GameplayManagerOnGameEndedEvent(Enumerators.EndGameType obj)
        {
            _playerEventListener.OnGameEndedEventHandler(obj);
            _playerEventListener?.Dispose();
            _opponentEventListener?.Dispose();
        }

        private void GameplayManagerOnGameInitializedEvent()
        {
            _playerEventListener = new PlayerEventListener(_gameplayManager.CurrentPlayer, false);
            _opponentEventListener = new PlayerEventListener(_gameplayManager.OpponentPlayer, true);
            
            _playerEventListener.OnGameInitializedEventHandler();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            _playerEventListener?.Dispose();
            _opponentEventListener?.Dispose();
        }

        private class PlayerEventListener : IDisposable
        {
            public Player Player { get; }
            
            public bool IsOpponent { get; }
            
            private readonly BackendFacade _backendFacade;
            private readonly BackendDataControlMediator _backendDataControlMediator;
            private IDataManager _dataManager;

            public PlayerEventListener(Player player, bool isOpponent)
            {
                _backendFacade = GameClient.Get<BackendFacade>();
                _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
                _dataManager = GameClient.Get<IDataManager>();
                
                Player = player;
                IsOpponent = isOpponent;

                if (!_dataManager.BetaConfig.SaveTurnData)
                    return;
                
                Player.OnEndTurnEvent += OnEndTurnEventHandler;
                Player.OnStartTurnEvent += OnStartTurnEventHandler;
                Player.PlayerHPChangedEvent += OnPlayerHPChangedEventHandler;
                Player.PlayerGooChangedEvent += OnPlayerGooChangedEventHandler;
                Player.PlayerVialGooChangedEvent += OnPlayerVialGooChangedEventHandler;
                Player.DeckChangedEvent += OnDeckChangedEventHandler;
                Player.HandChangedEvent += OnHandChangedEventHandler;
                Player.GraveyardChangedEvent += OnGraveyardChangedEventHandler;
                Player.BoardChangedEvent += OnBoardChangedEventHandler;
                Player.CardPlayedEvent += OnCardPlayedEventHandler;
            }

            public void Dispose()
            {
                Player.OnEndTurnEvent -= OnEndTurnEventHandler;
                Player.OnStartTurnEvent -= OnStartTurnEventHandler;
                Player.PlayerHPChangedEvent -= OnPlayerHPChangedEventHandler;
                Player.PlayerGooChangedEvent -= OnPlayerGooChangedEventHandler;
                Player.PlayerVialGooChangedEvent -= OnPlayerVialGooChangedEventHandler;
                Player.DeckChangedEvent -= OnDeckChangedEventHandler;
                Player.HandChangedEvent -= OnHandChangedEventHandler;
                Player.GraveyardChangedEvent -= OnGraveyardChangedEventHandler;
                Player.BoardChangedEvent -= OnBoardChangedEventHandler;
                Player.CardPlayedEvent -= OnCardPlayedEventHandler;
            }
            
            public async void OnGameEndedEventHandler(Enumerators.EndGameType obj)
            {
                await UploadActionLogModel(
                    CreateBasicActionLogModel("GameEnded")
                        .Add("EndGameType", obj.ToString())
                    );
            }

            public async void OnGameInitializedEventHandler()
            {
                await UploadActionLogModel(CreateBasicActionLogModel("GameStarted"));
            }

            private async void OnCardPlayedEventHandler(WorkingCard obj)
            {
                await UploadActionLogModel(
                    CreateBasicActionLogModel("CardPlayed")
                        .Add("Card", WorkingCardToSimpleRepresentation(obj))
                );
            }

            private async void OnBoardChangedEventHandler(int obj)
            {
                await UploadActionLogModel(
                    CreateBasicActionLogModel("BoardChanged")
                        .Add("CardsOnBoard", Player.CardsOnBoard.Select(WorkingCardToSimpleRepresentation).ToArray())
                );
            }

            private async void OnGraveyardChangedEventHandler(int obj)
            {
                await UploadActionLogModel(
                    CreateBasicActionLogModel("GraveyardChanged")
                        .Add("CardsOnBoard", Player.CardsInGraveyard.Select(WorkingCardToSimpleRepresentation).ToArray())
                );
            }

            private async void OnHandChangedEventHandler(int obj)
            {
                await UploadActionLogModel(
                    CreateBasicActionLogModel("HandChanged")
                        .Add("CardsOnBoard", Player.CardsInHand.Select(WorkingCardToSimpleRepresentation).ToArray())
                );
            }

            private async void OnDeckChangedEventHandler(int obj)
            {
                await UploadActionLogModel(
                    CreateBasicActionLogModel("DeckChanged")
                        .Add("CardsOnBoard", Player.CardsInDeck.Select(WorkingCardToSimpleRepresentation).ToArray())
                );
            }

            private async void OnPlayerVialGooChangedEventHandler(int obj)
            {
                await UploadActionLogModel(
                    CreateBasicActionLogModel("GooOnCurrentTurnChanged")
                        .Add("Goo", obj)
                );
            }

            private async void OnPlayerGooChangedEventHandler(int obj)
            {
                await UploadActionLogModel(
                    CreateBasicActionLogModel("GooChanged")
                        .Add("Goo", obj)
                );
            }

            private async void OnPlayerHPChangedEventHandler(int obj)
            {
                await UploadActionLogModel(
                    CreateBasicActionLogModel("HealthChanged")
                        .Add("Health", obj)
                    );
            }

            private async void OnStartTurnEventHandler()
            {
                await UploadActionLogModel(CreateBasicActionLogModel("TurnStart"));
            }

            private async void OnEndTurnEventHandler()
            {
                await UploadActionLogModel(CreateBasicActionLogModel("TurnEnd"));
            }

            private ActionLogModel CreateBasicActionLogModel(string eventName)
            {
                return 
                    new ActionLogModel()
                        .Add("UserId", _backendDataControlMediator.UserDataModel.UserId)
                        .Add("CurrentTurnPlayer", IsOpponent ? "Opponent" : "Player")
                        .Add("Event", eventName);
            }

            private async Task UploadActionLogModel(ActionLogModel model)
            {
                if (!_backendFacade.IsConnected)
                    return;
                
                await _backendFacade.UploadActionLog(_backendDataControlMediator.UserDataModel.UserId, model);
            }

            private object WorkingCardToSimpleRepresentation(WorkingCard card)
            {
                return new
                {
                    card.instanceId,
                    card.cardId,
                    card.libraryCard.name,
                    card.health,
                    card.damage,
                    type = card.type.ToString()
                };
            }
        }
    }
}
