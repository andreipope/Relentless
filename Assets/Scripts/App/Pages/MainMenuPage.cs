using System;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Loom.ZombieBattleground.BackendCommunication;

namespace Loom.ZombieBattleground
{
    public class MainMenuPage : IUIElement
    {
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;

        private ISoundManager _soundManager;

        private IPlayerManager _playerManager;

        private IDataManager _dataManager;

        private GameObject _selfPage;

        private MenuButtonNoGlow _buttonArmy;

        private Button _buttonPlay, _buttonSettings, _buttonCredits;

        private ButtonShiftingContent _buttonBuy, _buttonOpen;

        private Button _buttonLogin;

        private TextMeshProUGUI _packsCount;

        private Animator _logoAnimator;

        private BackendDataControlMediator _backendDataControlMediator;

        private bool _isReturnToTutorial;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        public void Update()
        {
            if (_selfPage != null)
            {
                if (!Constants.AlwaysGuestLogin && 
                    _backendDataControlMediator.UserDataModel != null && 
                    (!_backendDataControlMediator.UserDataModel.IsRegistered || !_backendDataControlMediator.UserDataModel.IsValid))
                {
                    if (!_buttonLogin.gameObject.activeSelf)
                    {
                        _buttonLogin.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (_buttonLogin.gameObject.activeSelf)
                    {
                        _buttonLogin.gameObject.SetActive(false);
                    }
                }
            }
        }  

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MainMenuPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonPlay = _selfPage.transform.Find("Button_Play").GetComponent<Button>();

            _buttonArmy = _selfPage.transform.Find("Button_Army").GetComponent<MenuButtonNoGlow>();
            _buttonCredits = _selfPage.transform.Find("Button_Credits").GetComponent<Button>();
            _buttonBuy = _selfPage.transform.Find("Button_Shop").GetComponent<ButtonShiftingContent>();
            _buttonOpen = _selfPage.transform.Find("Button_OpenPacks").GetComponent<ButtonShiftingContent>();
            _packsCount = _selfPage.transform.Find("Button_OpenPacks/Count").GetComponent<TextMeshProUGUI>();
            _buttonSettings = _selfPage.transform.Find("Button_Settings").GetComponent<Button>();
            _buttonLogin = _selfPage.transform.Find("Button_Login").GetComponent<Button>();

            _logoAnimator = _selfPage.transform.Find("Logo").GetComponent<Animator>();

            _buttonPlay.onClick.AddListener(OnClickPlay);
            _buttonArmy.Clicked.AddListener(OnClickCollection);
            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonOpen.onClick.AddListener(OpenButtonHandler);
            _buttonLogin.onClick.AddListener(PressedLoginHandler);

            _buttonCredits.onClick.AddListener(CreditsButtonOnClickHandler);
            _buttonSettings.onClick.AddListener(SettingsButtonOnClickHandler);

            _buttonArmy.Interactable = true;

            _packsCount.text = _playerManager.LocalUser.PacksCount <= 99 ?
                _playerManager.LocalUser.PacksCount.ToString() :
                "99";

            if (_logoAnimator.gameObject.activeInHierarchy)
            {
                _logoAnimator.SetBool("LogoShow", true);
            }

            //Hide for current beta release
            //if (!_dataManager.CachedUserLocalData.AgreedTerms)
            //{
            //    _uiManager.DrawPopup<TermsPopup>();
            //}

            _isReturnToTutorial = GameClient.Get<ITutorialManager>().UnfinishedTutorial;
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

        private void PressedLoginHandler() 
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonBuy.name) || _isReturnToTutorial)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            LoginPopup popup = _uiManager.GetPopup<LoginPopup>();
            popup.Show();
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        #region Buttons Handlers

        private void OnClickPlay()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonPlay.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.TriedToPressPlayButton);
                return;
            }
            else if (_isReturnToTutorial)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.BattleStarted);

                GameClient.Get<IMatchManager>().FindMatch();
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _stateManager.ChangeAppState(Enumerators.AppState.PlaySelection);
        }

        private void OnClickCollection()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonArmy.name) || _isReturnToTutorial)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.ARMY);
        }

        private void BuyButtonHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonBuy.name) || _isReturnToTutorial)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.SHOP);
        }

        private void CreditsButtonOnClickHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonCredits.name) || _isReturnToTutorial)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.CREDITS);
        }

        private void OpenButtonHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonOpen.name) || _isReturnToTutorial)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.PACK_OPENER);
        }

        private void SettingsButtonOnClickHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonSettings.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _uiManager.DrawPopup<SettingsPopup>(true);
        }

        #endregion

    }
}
