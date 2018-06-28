using GrandDevs.CZB.Common;
using System;

namespace GrandDevs.CZB
{
    public class MatchManager : IService, IMatchManager
    {
        private IUIManager _uiManager;
        private IScenesManager _sceneManager;
        private IAppStateManager _appStateManager;
        private IGameplayManager _gameplayManager;
        private ITutorialManager _tutorialManager;

        private Enumerators.AppState _finishMatchAppState;


        public Enumerators.MatchType MatchType { get; set; }

        public void Dispose()
        {
            _sceneManager.SceneForAppStateWasLoadedEvent -= SceneForAppStateWasLoadedEventHandler;
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _sceneManager = GameClient.Get<IScenesManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _sceneManager.SceneForAppStateWasLoadedEvent += SceneForAppStateWasLoadedEventHandler;
        }
        public void Update()
        {
        }

        public void FinishMatch(Enumerators.AppState appStateAfterMatch, bool tutorialCancel = false)
        {
            if (_tutorialManager.IsTutorial)
            {
                if (!tutorialCancel)
                    _tutorialManager.StopTutorial();
                else
                    _tutorialManager.CancelTutorial();
            }

            _finishMatchAppState = appStateAfterMatch;

            _uiManager.HideAllPages();
            _uiManager.DrawPopup<PreparingForBattlePopup>();

            _sceneManager.ChangeScene(Enumerators.AppState.APP_INIT);
        }

        public void FindMatch(Enumerators.MatchType matchType)
        {
            switch (matchType)
            {
                case Enumerators.MatchType.LOCAL:
                    CreateLocalMatch();
                    break;
                default:
                    throw new NotImplementedException(matchType.ToString() + " not implemented yet.");
            }

            MatchType = matchType;
        }

        private void CreateLocalMatch()
        {
            //todo write logic

            StartLoadMatch();
        }

        private void CreatePVEMatch()
        {
            //todo write logic

            StartLoadMatch();
        }

        private void CreateNetworkMatch()
        {
            //todo write logic

            StartLoadMatch();
        }

        private void StartLoadMatch()
        {
            _uiManager.HideAllPages();
            _uiManager.DrawPopup<PreparingForBattlePopup>();

            _sceneManager.ChangeScene(Enumerators.AppState.GAMEPLAY);
        }

        private void SceneForAppStateWasLoadedEventHandler(Enumerators.AppState state)
        {
            if (state == Enumerators.AppState.GAMEPLAY)
            {
                _appStateManager.ChangeAppState(Enumerators.AppState.GAMEPLAY);

                _uiManager.HidePopup<PreparingForBattlePopup>();

                _gameplayManager.StartGameplay();
            }
            else if(state == Enumerators.AppState.APP_INIT)
            {
                _appStateManager.ChangeAppState(_finishMatchAppState);

                _uiManager.HidePopup<PreparingForBattlePopup>();

                _gameplayManager.StopGameplay();
            }
        }
    }
}