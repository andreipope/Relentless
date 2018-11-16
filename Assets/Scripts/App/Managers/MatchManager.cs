using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using Deck = Loom.ZombieBattleground.Data.Deck;

namespace Loom.ZombieBattleground
{
    public class MatchManager : IService, IMatchManager
    {
        private IUIManager _uiManager;

        private IScenesManager _sceneManager;

        private IAppStateManager _appStateManager;

        private IGameplayManager _gameplayManager;

        private ITutorialManager _tutorialManager;

        private IPvPManager _pvpManager;

        private Enumerators.AppState _finishMatchAppState;

        private int _onPvPManagerGameStartedActionHandlerCounter;

        public Enumerators.MatchType MatchType { get; set; }

        public void FinishMatch(Enumerators.AppState appStateAfterMatch)
        {
            _tutorialManager.StopTutorial();

            if (_gameplayManager.IsTutorial &&
                !_tutorialManager.IsTutorial &&
                appStateAfterMatch != Enumerators.AppState.MAIN_MENU)
            {
                _sceneManager.ChangeScene(Enumerators.AppState.GAMEPLAY, true);
                return;
            }

            _finishMatchAppState = appStateAfterMatch;

            _uiManager.HideAllPages();
            _uiManager.DrawPopup<LoadingGameplayPopup>();

            _gameplayManager.ResetWholeGameplayScene();

            _sceneManager.ChangeScene(Enumerators.AppState.APP_INIT);
        }

        public async void FindMatch()
        {
            switch (MatchType)
            {
                case Enumerators.MatchType.LOCAL:
                    CreateLocalMatch();
                    break;
                case Enumerators.MatchType.PVP:
                    {
                        try
                        {
                            GameClient.Get<IQueueManager>().Clear();

                            if (_onPvPManagerGameStartedActionHandlerCounter < 0) {
                                _onPvPManagerGameStartedActionHandlerCounter = 0;
                                Debug.Log("OnPvPManagerGameStartedActionReceived was unsubscribed more than required.");
                            }

                            while (_onPvPManagerGameStartedActionHandlerCounter > 0) {
                                _pvpManager.GameStartedActionReceived -= OnPvPManagerGameStartedActionReceived;
                                _onPvPManagerGameStartedActionHandlerCounter--;
                                Debug.Log("Unsubscribing on PVP, OnPvPManagerGameStartedActionReceived.");
                            }

                            _uiManager.DrawPopup<MatchMakingPopup>();

                            MatchMakingPopup matchMakingPopup = _uiManager.GetPopup<MatchMakingPopup>();
                            await matchMakingPopup.InitiateRegisterPlayerToPool(_uiManager.GetPage<GameplayPage>().CurrentDeckId);
                            matchMakingPopup.CancelMatchmakingClicked += MatchMakingPopupOnCancelMatchmakingClicked;

                            _pvpManager.GameStartedActionReceived += OnPvPManagerGameStartedActionReceived;
                            _onPvPManagerGameStartedActionHandlerCounter++;
                        }
                        catch (Exception e) {
                            Debug.LogWarning(e);
                            MatchMakingPopup matchMakingPopup = _uiManager.GetPopup<MatchMakingPopup>();
                            matchMakingPopup.CancelMatchmakingClicked -= MatchMakingPopupOnCancelMatchmakingClicked;
                            matchMakingPopup.Hide();
                            _uiManager.DrawPopup<WarningPopup>($"Error while finding a match:\n{e.Message}");
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException(MatchType + " not implemented yet.");
            }

        }

        public async void DebugFindPvPMatch(Deck deck)
        {
            /*
            try
            {
                _uiManager.DrawPopup<ConnectionPopup>();

                ConnectionPopup connectionPopup = _uiManager.GetPopup<ConnectionPopup>();
                connectionPopup.ShowLookingForMatch();
                connectionPopup.CancelMatchmakingClicked += ConnectionPopupOnCancelMatchmakingClicked;

                bool success = await _pvpManager.DebugFindMatch(deck);
                if (!success)
                    return;

                if (_pvpManager.MatchMetadata.Status == Match.Types.Status.Started)
                {
                    StartPvPMatch();
                }
                else
                {
                    _pvpManager.GameStartedActionReceived += OnPvPManagerGameStartedActionReceived;
                }
            }
            catch (Exception e) {
                Debug.LogWarning(e);
                _uiManager.GetPopup<ConnectionPopup>().Hide();
                _uiManager.DrawPopup<WarningPopup>($"Error while finding a match:\n{e.Message}");
            }
            */
        }

        private async void MatchMakingPopupOnCancelMatchmakingClicked()
        {
            try
            {
                _pvpManager.GameStartedActionReceived -= OnPvPManagerGameStartedActionReceived;
                _onPvPManagerGameStartedActionHandlerCounter--;
                MatchMakingPopup matchMakingPopup = _uiManager.GetPopup<MatchMakingPopup>();
                matchMakingPopup.CancelMatchmakingClicked -= MatchMakingPopupOnCancelMatchmakingClicked;
                matchMakingPopup.Hide();
                await _pvpManager.CancelFindMatch();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                _uiManager.GetPopup<MatchMakingPopup>().Hide();
                _uiManager.DrawPopup<WarningPopup>($"Error while canceling finding a match:\n{e.Message}");
            }
        }

        public void FindMatch(Enumerators.MatchType matchType)
        {
            MatchType = matchType;
            FindMatch();
        }

        public void Dispose()
        {
            _sceneManager.SceneForAppStateWasLoadedEvent -= SceneForAppStateWasLoadedEventHandler;
        }

        public void Init()
        {
            MatchType = Enumerators.MatchType.LOCAL;
            _uiManager = GameClient.Get<IUIManager>();
            _sceneManager = GameClient.Get<IScenesManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _pvpManager = GameClient.Get<IPvPManager>();

            _sceneManager.SceneForAppStateWasLoadedEvent += SceneForAppStateWasLoadedEventHandler;
            _pvpManager.MatchingFailed += OnPvPManagerMatchingFailed;
        }

        public void Update()
        {

        }

        private void CreateLocalMatch()
        {
            StartLoadMatch();
        }

        private void StartPvPMatch()
        {
            _uiManager.HidePopup<ConnectionPopup>();
            CreateLocalMatch();
        }

        public void OnPvPManagerGameStartedActionReceived()
        {
            MatchMakingPopup matchMakingPopup = _uiManager.GetPopup<MatchMakingPopup>();
            matchMakingPopup.CancelMatchmakingClicked -= MatchMakingPopupOnCancelMatchmakingClicked;
            matchMakingPopup.Hide();
            _pvpManager.GameStartedActionReceived -= OnPvPManagerGameStartedActionReceived;
            _onPvPManagerGameStartedActionHandlerCounter--;
            StartPvPMatch();
        }

        private void OnPvPManagerMatchingFailed()
        {
            _uiManager.GetPopup<ConnectionPopup>().Hide();
            _uiManager.DrawPopup<WarningPopup>("Couldn't find an opponent.");
        }

        private void StartLoadMatch()
        {
            _uiManager.HideAllPages();
            _uiManager.DrawPopup<LoadingGameplayPopup>();

            _sceneManager.ChangeScene(Enumerators.AppState.GAMEPLAY);
        }

        private void SceneForAppStateWasLoadedEventHandler(Enumerators.AppState state)
        {
            switch (state)
            {
                case Enumerators.AppState.GAMEPLAY:
                    {
                        ForceStartGameplay(_gameplayManager.IsTutorial);
                    }
                    break;
                case Enumerators.AppState.APP_INIT:
                    {
                        _appStateManager.ChangeAppState(_finishMatchAppState);
                    }
                    break;
            }
        }

        private void ForceStartGameplay(bool force = false)
        {
            if (_gameplayManager.IsTutorial)
            {
                _tutorialManager.SetupTutorialById(GameClient.Get<IDataManager>().CachedUserLocalData.CurrentTutorialId);
            }

            _appStateManager.ChangeAppState(Enumerators.AppState.GAMEPLAY, force);

            _uiManager.HidePopup<LoadingGameplayPopup>();

            _gameplayManager.StartGameplay();
        }
    }
}
