using System;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Loom.ZombieBattleground.BackendCommunication;
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

            _buttonPvPMode.interactable = _dataManager.ConfigData.EnablePvP;
            _buttonPvPMode.interactable = true;
#if DISABLE_PVP
            _buttonPvPMode.interactable = false;
#endif
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
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _dataManager.CachedUserLocalData.CurrentTutorialId = 0;
            _dataManager.CachedUserLocalData.Tutorial = true;
            GameClient.Get<IGameplayManager>().IsTutorial = true;
            _uiManager.GetPage<GameplayPage>().CurrentDeckId = 0;
            GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
        }

        private void SoloModeButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IMatchManager>().MatchType = Enumerators.MatchType.LOCAL;
            _stateManager.ChangeAppState(Enumerators.AppState.HordeSelection);
        }

        private void PvPModeButtonOnClickHandler()
        {

            Version pvpVersion = Version.Parse(_dataManager.CachedVersions.PvpVersion);
            if (!BuildMetaInfo.Instance.CheckBackendVersionMatch(pvpVersion))
            {
                Action[] actions = new Action[2];
                actions[0] = () =>
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("Version Mismatched");
                    #elif UNITY_ANDROID
                    Application.OpenURL(Constants.GameLinkForAndroid);
                    #elif UNITY_IOS
                    Application.OpenURL(Constants.GameLinkForIOS);
                    #elif UNITY_STANDALONE_OSX
                    Application.OpenURL(Constants.GameLinkForOSX);
                    #elif UNITY_STANDALONE_WIN
                    Application.OpenURL(Constants.GameLinkForWindows);
                    #else
                    Debug.LogWarning("Version Mismatched");
                    #endif
                };
                actions[1] = () =>
                {
                    Application.Quit();
                };

                _uiManager.DrawPopup<UpdatePopup>(actions);
                return;
            }
            
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            if (!Constants.AlwaysGuestLogin && !_backendDataControlMediator.UserDataModel.IsRegistered)
            {
                _uiManager.GetPopup<LoginPopup>().Show();
            }
            else
            {
                _stateManager.ChangeAppState(Enumerators.AppState.PvPSelection);
            }
        }

        private void BackButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }

        #endregion

    }
}
