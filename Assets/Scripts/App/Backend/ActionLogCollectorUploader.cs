using System;
using System.Linq;
using System.Threading.Tasks;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB.BackendCommunication
{
    public class ActionLogCollectorUploader : IService
    {
        private IGameplayManager _gameplayManager;

        private IAnalyticsManager _analyticsManager;

        private PlayerEventListener _playerEventListener;

        private PlayerEventListener _opponentEventListener;

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();

            _gameplayManager.OnGameInitializedEvent += GameplayManagerOnGameInitializedEvent;
            _gameplayManager.OnGameEndedEvent += GameplayManagerOnGameEndedEvent;
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            _playerEventListener?.Dispose();
            _opponentEventListener?.Dispose();
        }

        private void GameplayManagerOnGameEndedEvent(Enumerators.EndGameType obj)
        {
            _playerEventListener?.OnGameEndedEventHandler(obj);
            _playerEventListener?.Dispose();
            _opponentEventListener?.Dispose();

            _analyticsManager.NotifyFinishedMatch(obj);
        }

        private void GameplayManagerOnGameInitializedEvent()
        {
            _playerEventListener?.Dispose();
            _opponentEventListener?.Dispose();

            _playerEventListener = new PlayerEventListener(_gameplayManager.CurrentPlayer, false);
            _opponentEventListener = new PlayerEventListener(_gameplayManager.OpponentPlayer, true);

            _playerEventListener.OnGameInitializedEventHandler();

            _analyticsManager.NotifyStartedMatch();
        }

        private class PlayerEventListener : IDisposable
        {
            private readonly BackendFacade _backendFacade;

            private readonly BackendDataControlMediator _backendDataControlMediator;

            public PlayerEventListener(Player player, bool isOpponent)
            {
                _backendFacade = GameClient.Get<BackendFacade>();
                _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
                IDataManager dataManager = GameClient.Get<IDataManager>();

                Player = player;
                IsOpponent = isOpponent;

                if (!dataManager.BetaConfig.SaveTurnData)
                    return;

                Player.OnEndTurnEvent += OnEndTurnEventHandler;
                Player.OnStartTurnEvent += OnStartTurnEventHandler;
                Player.PlayerHpChangedEvent += OnPlayerHPChangedEventHandler;
                Player.PlayerGooChangedEvent += OnPlayerGooChangedEventHandler;
                Player.PlayerVialGooChangedEvent += OnPlayerVialGooChangedEventHandler;
                Player.DeckChangedEvent += OnDeckChangedEventHandler;
                Player.HandChangedEvent += OnHandChangedEventHandler;
                Player.GraveyardChangedEvent += OnGraveyardChangedEventHandler;
                Player.BoardChangedEvent += OnBoardChangedEventHandler;
                Player.CardPlayedEvent += OnCardPlayedEventHandler;
            }

            public Player Player { get; }

            public bool IsOpponent { get; }

            public void Dispose()
            {
                UnsubscribeFromPlayerEvents();
            }

            public async void OnGameEndedEventHandler(Enumerators.EndGameType obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("GameEnded").Add("EndGameType", obj.ToString()));
            }

            public async void OnGameInitializedEventHandler()
            {
                await UploadActionLogModel(CreateBasicActionLogModel("GameStarted"));
            }

            private void UnsubscribeFromPlayerEvents()
            {
                Player.OnEndTurnEvent -= OnEndTurnEventHandler;
                Player.OnStartTurnEvent -= OnStartTurnEventHandler;
                Player.PlayerHpChangedEvent -= OnPlayerHPChangedEventHandler;
                Player.PlayerGooChangedEvent -= OnPlayerGooChangedEventHandler;
                Player.PlayerVialGooChangedEvent -= OnPlayerVialGooChangedEventHandler;
                Player.DeckChangedEvent -= OnDeckChangedEventHandler;
                Player.HandChangedEvent -= OnHandChangedEventHandler;
                Player.GraveyardChangedEvent -= OnGraveyardChangedEventHandler;
                Player.BoardChangedEvent -= OnBoardChangedEventHandler;
                Player.CardPlayedEvent -= OnCardPlayedEventHandler;
            }

            private async void OnCardPlayedEventHandler(WorkingCard obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("CardPlayed").Add("Card", WorkingCardToSimpleRepresentation(obj)));
            }

            private async void OnBoardChangedEventHandler(int obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("BoardChanged").Add("CardsOnBoard", Player.CardsOnBoard.Select(WorkingCardToSimpleRepresentation).ToArray()));
            }

            private async void OnGraveyardChangedEventHandler(int obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("GraveyardChanged").Add("CardsOnBoard", Player.CardsInGraveyard.Select(WorkingCardToSimpleRepresentation).ToArray()));
            }

            private async void OnHandChangedEventHandler(int obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("HandChanged").Add("CardsOnBoard", Player.CardsInHand.Select(WorkingCardToSimpleRepresentation).ToArray()));
            }

            private async void OnDeckChangedEventHandler(int obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("DeckChanged").Add("CardsOnBoard", Player.CardsInDeck.Select(WorkingCardToSimpleRepresentation).ToArray()));
            }

            private async void OnPlayerVialGooChangedEventHandler(int obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("GooOnCurrentTurnChanged").Add("Goo", obj));
            }

            private async void OnPlayerGooChangedEventHandler(int obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("GooChanged").Add("Goo", obj));
            }

            private async void OnPlayerHPChangedEventHandler(int obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("HealthChanged").Add("Health", obj));
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
                return new ActionLogModel().Add("UserId", _backendDataControlMediator.UserDataModel.UserId).Add("CurrentTurnPlayer", IsOpponent?"Opponent":"Player").Add("Event", eventName);
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
                    instanceId = card.InstanceId,
                    cardId = card.CardId,
                    name = card.LibraryCard.Name,
                    health = card.Health,
                    damage = card.Damage,
                    type = card.Type.ToString()
                };
            }
        }
    }
}
