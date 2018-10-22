using System;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;

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

        private BackendFacade _backendFacade;

        private Enumerators.AppState _finishMatchAppState;

        private float lookingForOpponentTimeout;

        public Enumerators.MatchType MatchType { get; set; }

        // TODO : Find another solution, right now its tempraoray only....
        private bool _checkPlayerStatus;

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

        public async Task StopLookingForOpponent () {
            await _backendFacade.UnsubscribeEvent();
            if (_uiManager.GetPopup<ConnectionPopup>().Self != null)
            {
                _uiManager.HidePopup<ConnectionPopup>();
            }
            _uiManager.DrawPopup<WarningPopup>("Couldn't find an opponent.");
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
                            GameClient.Get<IQueueManager>().StartNetworkThread();
                            _uiManager.DrawPopup<ConnectionPopup>();
                            _uiManager.GetPopup<ConnectionPopup>().ShowLookingForMatch();
                            await _pvpManager.FindMatch();

                            if (_pvpManager.MatchMetadata.Status == Match.Types.Status.Started)
                            {
                                OnStartGamePvP();
                            }
                            else
                            {
                                lookingForOpponentTimeout = Constants.matchmakingTimeOut;
                                _pvpManager.GameStartedActionReceived += OnStartGamePvP;
                            }
                        } 
                        catch (Exception e) {
                            Debug.LogWarning(e);
                            if (_uiManager.GetPopup<ConnectionPopup>().Self != null) {
                                _uiManager.HidePopup<ConnectionPopup>();
                            }

                            _uiManager.DrawPopup<ConnectionPopup>($"Error while finding a match:\n{e}");
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException(MatchType + " not implemented yet.");
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
            _backendFacade = GameClient.Get<BackendFacade>();

            _sceneManager.SceneForAppStateWasLoadedEvent += SceneForAppStateWasLoadedEventHandler;

            lookingForOpponentTimeout = 0;
        }

        public async void Update()
        {
            if (_checkPlayerStatus)
            {
                _checkPlayerStatus = false;
                GetGameState();
            }

            if (lookingForOpponentTimeout > 0) {
                lookingForOpponentTimeout -= Time.deltaTime;
                if (lookingForOpponentTimeout <= 0) {
                    lookingForOpponentTimeout = 0;
                    await StopLookingForOpponent();
                }
            }
        }

        private void CreateLocalMatch()
        {
            StartLoadMatch();
        }

        private void OnStartGamePvP()
        {
            lookingForOpponentTimeout = 0;
            _checkPlayerStatus = true;
        }

        private async void GetGameState()
        {
            // TODO : Quick fix... something wrong with backend side..
            // Need to remove delay
            await Task.Delay(3000);

            _uiManager.HidePopup<ConnectionPopup>();
            CreateLocalMatch();
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
