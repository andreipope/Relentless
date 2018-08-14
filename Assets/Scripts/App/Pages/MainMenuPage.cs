// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using System.Threading.Tasks;
using App.Utilites;
using Loom.Client;
using LoomNetwork.CZB.BackendCommunication;
using UnityEngine;
using UnityEngine.UI;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Gameplay;
using TMPro;
using System;

namespace LoomNetwork.CZB
{
    public class MainMenuPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;
		private IAppStateManager _stateManager;
		private ISoundManager _soundManager;
        private IPlayerManager _playerManager;
		private IDataManager _dataManager;
        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;

        private GameObject _selfPage;

        private MenuButtonNoGlow _buttonArmy;

        private Button _buttonPlay, _buttonDeck;

        private ButtonShiftingContent _buttonBuy, _buttonOpen,
                       _buttonCredits, _buttonTutorial, _buttonQuit;

        private MenuButtonToggle _buttonMusic,
                                 _buttonSFX;

        private TextMeshProUGUI _packsCount;

        private Animator _logoAnimator;

        private bool _logoShowed;
        private TextMeshProUGUI _connectionStatusText;
        private Button _buttonReconnect;
		private Button _buttonLogout;

        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
			_dataManager = GameClient.Get<IDataManager> ();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MainMenuPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonPlay = _selfPage.transform.Find("Button_Play").GetComponent<Button>();
            _buttonDeck = _selfPage.transform.Find("Button_Deck").GetComponent<Button>();
            _buttonArmy = _selfPage.transform.Find("Button_Army").GetComponent<MenuButtonNoGlow>();
            _buttonCredits = _selfPage.transform.Find("BackMetalLeft2/Button_Credits").GetComponent<ButtonShiftingContent>();
            _buttonQuit = _selfPage.transform.Find("BackMetalLeft/Button_Quit").GetComponent<ButtonShiftingContent>();
            _buttonTutorial = _selfPage.transform.Find("Button_Tutorial").GetComponent<ButtonShiftingContent>();
            _buttonBuy = _selfPage.transform.Find("Button_Shop").GetComponent<ButtonShiftingContent>();
            _buttonOpen = _selfPage.transform.Find("Button_OpenPacks").GetComponent<ButtonShiftingContent>();
            _packsCount = _selfPage.transform.Find("Button_OpenPacks/Count").GetComponent<TextMeshProUGUI>();
            _buttonMusic = _selfPage.transform.Find("Button_Music").GetComponent<MenuButtonToggle>();
            _buttonSFX = _selfPage.transform.Find("Button_SFX").GetComponent<MenuButtonToggle>();

            _logoAnimator = _selfPage.transform.Find("Logo").GetComponent<Animator>();
            
            _connectionStatusText = _selfPage.transform.Find("ConnectionPanel/ConnectionStatusText").GetComponent<TextMeshProUGUI>();
            _buttonReconnect = _selfPage.transform.Find("ConnectionPanel/Button_Reconnect").GetComponent<Button>();
			_buttonLogout = _selfPage.transform.Find("ConnectionPanel/Button_Logout").GetComponent<Button>();

            _buttonPlay.onClick.AddListener(OnClickPlay);
            _buttonDeck.onClick.AddListener(OnClickPlay);
            _buttonArmy.onClickEvent.AddListener(OnClickCollection);
            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonOpen.onClick.AddListener(OpenButtonHandler);
            _buttonCredits.onClick.AddListener(CreditsButtonOnClickHandler);
            _buttonQuit.onClick.AddListener(QuitButtonOnClickHandler);
            _buttonTutorial.onClick.AddListener(TutorialButtonOnClickHandler);
            _buttonReconnect.onClick.AddListener(ReconnectButtonOnClickHandler);
            _buttonLogout.onClick.AddListener(LogoutButtonOnClickHandler);            _buttonMusic.onValueChangedEvent.AddListener(OnValueChangedEventMusic);
            _buttonSFX.onValueChangedEvent.AddListener(OnValueChangedEventSFX);
            
            _backendFacade.ContractCreated += LoomManagerOnContractCreated;
            
            Hide();
            
        }

        private void RpcClientOnConnectionStateChanged(IRpcClient sender, RpcConnectionState state) {
            UnitySynchronizationContext.Instance.Post(o => UpdateConnectionStateUI(), null);
        }

        private void UpdateConnectionStateUI() {
            if (!_selfPage.activeSelf)
                return;

            _connectionStatusText.text = 
                _backendFacade.IsConnected ? 
                    "<color=green>Online</color>" : 
                    "<color=red>Offline</color>";
            
            _buttonReconnect.gameObject.SetActive(!_backendFacade.IsConnected);
			_buttonLogout.gameObject.SetActive (!_buttonReconnect.gameObject.activeSelf);
        }

        public void Update()
        {

        }

        public void Show()
        {
            _selfPage.SetActive(true);
            _buttonArmy.interactable = true;

            _packsCount.text = _playerManager.LocalUser.packsCount <= 99 ? _playerManager.LocalUser.packsCount.ToString() : "99";

            _logoAnimator.SetBool("LogoShow", true);

            _buttonMusic.SetStatus(!_soundManager.MusicMuted);
            _buttonSFX.SetStatus(!_soundManager.SfxMuted);

			if (!_dataManager.CachedUserLocalData.agreedTerms) {
				_uiManager.DrawPopup<TermsPopup> ();
			}

            /*if (_logoShowed && !_logoAnimator.GetBool("LogoShow"))
                _logoAnimator.SetBool("LogoShow", true);

            if (!_logoShowed)
            {
                GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.LOGO_APPEAR);
                _logoShowed = true;
            } */
            

            /*if (LoomManager.Instance.Contract != null)
            {
                LoomManagerOnContractCreated(null, LoomManager.Instance.Contract);
            }*/

            UpdateConnectionStateUI();
        }

        private void LoomManagerOnContractCreated(Contract oldContract, Contract newContract) {
            if (oldContract != null)
            {
                oldContract.Client.ReadClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
                oldContract.Client.WriteClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
            }
            newContract.Client.ReadClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;
            newContract.Client.WriteClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;

            UpdateConnectionStateUI();
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
        }

        public void Dispose()
        {
            if (_backendFacade.Contract != null)
            {
                _backendFacade.Contract.Client.ReadClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
                _backendFacade.Contract.Client.WriteClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
            }
        }

        #region Buttons Handlers
        private void OnClickPlay()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            if (GameClient.Get<IDataManager>().CachedUserLocalData.tutorial)
            {
                (_uiManager.GetPage<GameplayPage>() as GameplayPage).CurrentDeckId = 0;

                GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
            }
            else
                _stateManager.ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
        }

        private void TutorialButtonOnClickHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IDataManager>().CachedUserLocalData.tutorial = true;
            GameClient.Get<IGameplayManager>().IsTutorial = true;
            (_uiManager.GetPage<GameplayPage>() as GameplayPage).CurrentDeckId = 0;
            GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
        }

		private void OnClickCollection()
		{
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _stateManager.ChangeAppState(Common.Enumerators.AppState.COLLECTION);
		}

        private void BuyButtonHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
			_stateManager.ChangeAppState (Common.Enumerators.AppState.SHOP);
        }

        private void CreditsButtonOnClickHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _stateManager.ChangeAppState(Common.Enumerators.AppState.CREDITS);
        }

        private void QuitButtonOnClickHandler()
        {
            Action callback = () =>
            {
                Application.Quit();
            };

            _uiManager.DrawPopup<ConfirmationPopup>(callback);
        }

        private void OpenButtonHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
			_stateManager.ChangeAppState (Common.Enumerators.AppState.PACK_OPENER);
        }
        
        private async void ReconnectButtonOnClickHandler() {
            try
            {
                // FIXME: add waiting popup
                await _backendDataControlMediator.LoginAndLoadData();
            } catch (Exception e)
            {
                Debug.LogException(e);
                OpenAlertDialog("Reconnect failed. Reason: " + e.GetType().Name);
            }
        }

		private void LogoutButtonOnClickHandler() {
			_dataManager.DeleteData ();
			_backendDataControlMediator.UserDataModel = null;
			GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.APP_INIT);
		}

        private void OnValueChangedEventMusic(bool value)
		{
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
           // _soundManager.SetMusicVolume(value ? Constants.BACKGROUND_SOUND_VOLUME : 0);
            _soundManager.SetMusicMuted(!value);
        }

        private void OnValueChangedEventSFX(bool value)
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
          //  _soundManager.SetSoundVolume(value ? Constants.SFX_SOUND_VOLUME : 0);
            _soundManager.SetSoundMuted(!value);
        }

        #endregion

        private void OpenAlertDialog(string msg)
		{
			_uiManager.DrawPopup<WarningPopup>(msg);
		}
    }
}