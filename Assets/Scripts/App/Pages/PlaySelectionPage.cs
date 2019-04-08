using System;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.Experimental.PlayerLoop;

namespace Loom.ZombieBattleground
{
    public class PlaySelectionPage : IUIElement
    {
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;

        private ISoundManager _soundManager;

        private IDataManager _dataManager;

        private GameObject _selfPage;

        private Button _backButton,
                       _buttonSoloMode,
                       _buttonPvPMode;

        private ButtonShiftingContent _buttonTutorial;

        private BackendDataControlMediator _backendDataControlMediator;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/PlaySelectionPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonTutorial = _selfPage.transform.Find("Button_Tutorial").GetComponent<ButtonShiftingContent>();
            _buttonSoloMode = _selfPage.transform.Find("Button_SoloMode").GetComponent<Button>();
            _buttonPvPMode = _selfPage.transform.Find("Button_PvPMode").GetComponent<Button>();
            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();

            _buttonTutorial.onClick.AddListener(TutorialButtonOnClickHandler);
            _buttonSoloMode.onClick.AddListener(SoloModeButtonOnClickHandler);
            _buttonPvPMode.onClick.AddListener(PvPModeButtonOnClickHandler);
            _backButton.onClick.AddListener(BackButtonOnClickHandler);
        }

        public void Hide()
        {
            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
        }

        public void Dispose()
        {
        }

        #region Buttons Handlers

        private void TutorialButtonOnClickHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonTutorial.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            if (!GameClient.Get<ITutorialManager>().CheckNextTutorial())
            {
                _dataManager.CachedUserLocalData.CurrentTutorialId = 0;
                _dataManager.CachedUserLocalData.Tutorial = true;
                GameClient.Get<IGameplayManager>().IsTutorial = true;
                _uiManager.GetPage<GameplayPage>().CurrentDeckId = 0;
                GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
            }
        }

        private void SoloModeButtonOnClickHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonSoloMode.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IMatchManager>().MatchType = Enumerators.MatchType.LOCAL;
            _stateManager.ChangeAppState(Enumerators.AppState.HordeSelection);
        }

        private void PvPModeButtonOnClickHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonPvPMode.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            //FIXME we're removing the check just for now, we need to bring this back at some point
            /*
            if (!Constants.AlwaysGuestLogin && !_backendDataControlMediator.UserDataModel.IsRegistered)
            {
                _uiManager.GetPopup<LoginPopup>().Show();
            }
            else
            {*/
                _stateManager.ChangeAppState(Enumerators.AppState.PvPSelection);
            //}
        }

        private void BackButtonOnClickHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_backButton.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }

        #endregion

    }
}
