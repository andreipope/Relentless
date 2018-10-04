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

        private BackendDataControlMediator _backendDataControlMediator;

        private Enumerators.AppState _finishMatchAppState;

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

        public async void FindMatch()
        {
            switch (MatchType)
            {
                case Enumerators.MatchType.LOCAL:
                    CreateLocalMatch();
                    break;
                case Enumerators.MatchType.PVP:
                    {
                        GameClient.Get<IQueueManager>().StartNetworkThread();
                        _uiManager.DrawPopup<ConnectionPopup>();
                        _uiManager.GetPopup<ConnectionPopup>().ShowLookingForMatch();
                        _pvpManager.MatchResponse = await GetBackendFacade(_backendFacade).FindMatch(
                            _backendDataControlMediator.UserDataModel.UserId,
                            _uiManager.GetPage<GameplayPage>().CurrentDeckId);

                        Debug.LogWarning("=== Response = " + _pvpManager.MatchResponse);
                        _backendFacade.SubscribeEvent(_pvpManager.MatchResponse.Match.Topics.ToList());

                        if (_pvpManager.MatchResponse.Match.Status == Match.Types.Status.Started)
                        {
                            OnStartGamePvP();
                        }
                        else
                        {
                            _pvpManager.GameStartedActionReceived += OnStartGamePvP;
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException(MatchType + " not implemented yet.");
            }

        }

        public async void FindMatch(Enumerators.MatchType matchType)
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
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _sceneManager.SceneForAppStateWasLoadedEvent += SceneForAppStateWasLoadedEventHandler;
        }

        public void Update()
        {
            if (_checkPlayerStatus)
            {
                _checkPlayerStatus = false;
                GetGameState();
            }
        }

        private void CreateLocalMatch()
        {
            StartLoadMatch();
        }

        private static BackendFacade GetBackendFacade(BackendFacade backendFacade)
        {
            return backendFacade;
        }

        private void OnStartGamePvP()
        {
            _checkPlayerStatus = true;
        }

        private async void GetGameState()
        {

            // TODO : Quick fix... something wrong with backend side..
            // Need to remove delay
            await Task.Delay(3000);
            _pvpManager.GameStateResponse = await _backendFacade.GetGameState((int)_pvpManager.MatchResponse.Match.Id);

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
