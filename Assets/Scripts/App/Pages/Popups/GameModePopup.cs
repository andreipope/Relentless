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
    public class GameModePopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;

        private ISoundManager _soundManager;

        private IDataManager _dataManager;

        private Button _buttonBack,
                       _buttonSoloMode,
                       _buttonPvPMode;

        private ButtonShiftingContent _buttonTutorial;

        private BackendDataControlMediator _backendDataControlMediator;

        #region IUIPopup

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/GameModePopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);
            
            _buttonTutorial = Self.transform.Find("Button_Tutorial").GetComponent<ButtonShiftingContent>();
            _buttonSoloMode = Self.transform.Find("Button_SoloMode").GetComponent<Button>();
            _buttonPvPMode = Self.transform.Find("Button_PvPMode").GetComponent<Button>();
            _buttonBack = Self.transform.Find("Button_Back").GetComponent<Button>();

            _buttonTutorial.onClick.AddListener(ButtonTutorialHandler);
            _buttonSoloMode.onClick.AddListener(ButtonSoloModeHandler);
            _buttonPvPMode.onClick.AddListener(ButtonPvPModeHandler);
            _buttonBack.onClick.AddListener(ButtonBackHandler);

            _buttonPvPMode.interactable = _dataManager.ConfigData.EnablePvP;
            _buttonPvPMode.interactable = true;
#if DISABLE_PVP
            _buttonPvPMode.interactable = false;
#endif
        }
        
        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        #endregion

        #region Buttons Handlers

        private void ButtonTutorialHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonTutorial.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _uiManager.HidePopup<GameModePopup>();
            
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

        private void ButtonSoloModeHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonSoloMode.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }
            
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);            
            
            _uiManager.GetPage<MainMenuWithNavigationPage>().SetGameMode(MainMenuWithNavigationPage.GameMode.SOLO);
            _uiManager.HidePopup<GameModePopup>();
        }

        private void ButtonPvPModeHandler()
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
                _uiManager.GetPage<MainMenuWithNavigationPage>().SetGameMode(MainMenuWithNavigationPage.GameMode.VS);
                _uiManager.HidePopup<GameModePopup>();
                //_stateManager.ChangeAppState(Enumerators.AppState.PvPSelection);
            //}
        }

        private void ButtonBackHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonBack.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _uiManager.HidePopup<GameModePopup>();
        }

        #endregion      
    }
}
