using System;
using System.Threading.Tasks;
using App.Utilites;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private GameObject _selfPage;

        private MenuButtonNoGlow _buttonArmy;

        private Button _buttonPlay, _buttonDeck;

        private ButtonShiftingContent _buttonBuy, _buttonOpen, _buttonCredits, _buttonTutorial, _buttonQuit;

        private MenuButtonToggle _buttonMusic, _buttonSfx;

        private TextMeshProUGUI _packsCount;

        private Animator _logoAnimator;

        private TextMeshProUGUI _connectionStatusText;

        private Button _buttonReconnect;

        private Button _buttonLogout;

        private GameObject _markerOffline;

        private GameObject _markerOnline;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _backendFacade.ContractCreated += LoomManagerOnContractCreated;
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MainMenuPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonPlay = _selfPage.transform.Find("Button_Play").GetComponent<Button>();
            _buttonDeck = _selfPage.transform.Find("Button_Deck").GetComponent<Button>();
            _buttonArmy = _selfPage.transform.Find("Button_Army").GetComponent<MenuButtonNoGlow>();
            _buttonCredits = _selfPage.transform.Find("BackMetalLeft2/Button_Credits")
                .GetComponent<ButtonShiftingContent>();
            _buttonQuit = _selfPage.transform.Find("BackMetalLeft/Button_Quit").GetComponent<ButtonShiftingContent>();
            _buttonTutorial = _selfPage.transform.Find("Button_Tutorial").GetComponent<ButtonShiftingContent>();
            _buttonBuy = _selfPage.transform.Find("Button_Shop").GetComponent<ButtonShiftingContent>();
            _buttonOpen = _selfPage.transform.Find("Button_OpenPacks").GetComponent<ButtonShiftingContent>();
            _packsCount = _selfPage.transform.Find("Button_OpenPacks/Count").GetComponent<TextMeshProUGUI>();
            _buttonMusic = _selfPage.transform.Find("Button_Music").GetComponent<MenuButtonToggle>();
            _buttonSfx = _selfPage.transform.Find("Button_SFX").GetComponent<MenuButtonToggle>();

            _logoAnimator = _selfPage.transform.Find("Logo").GetComponent<Animator>();

            _connectionStatusText = _selfPage.transform.Find("ConnectionPanel/ConnectionStatusText")
                .GetComponent<TextMeshProUGUI>();
            _buttonReconnect = _selfPage.transform.Find("ConnectionPanel/Button_Reconnect").GetComponent<Button>();
            _buttonLogout = _selfPage.transform.Find("ConnectionPanel/Button_Logout").GetComponent<Button>();
            _markerOffline = _selfPage.transform.Find("ConnectionPanel/Marker_Status_Offline").gameObject;
            _markerOnline = _selfPage.transform.Find("ConnectionPanel/Marker_Status_Online").gameObject;

            _buttonPlay.onClick.AddListener(OnClickPlay);
            _buttonDeck.onClick.AddListener(OnClickPlay);
            _buttonArmy.Clicked.AddListener(OnClickCollection);
            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonOpen.onClick.AddListener(OpenButtonHandler);
            _buttonCredits.onClick.AddListener(CreditsButtonOnClickHandler);
            _buttonQuit.onClick.AddListener(QuitButtonOnClickHandler);
            _buttonTutorial.onClick.AddListener(TutorialButtonOnClickHandler);
            _buttonReconnect.onClick.AddListener(ReconnectButtonOnClickHandler);
            _buttonLogout.onClick.AddListener(LogoutButtonOnClickHandler);
            _buttonMusic.ValueChanged.AddListener(OnValueChangedEventMusic);
            _buttonSfx.ValueChanged.AddListener(OnValueChangedEventSfx);

            _buttonArmy.Interactable = true;

            _packsCount.text = _playerManager.LocalUser.PacksCount <= 99 ?
                _playerManager.LocalUser.PacksCount.ToString() :
                "99";

            _logoAnimator.SetBool("LogoShow", true);

            _buttonMusic.SetStatus(!_soundManager.MusicMuted);
            _buttonSfx.SetStatus(!_soundManager.SfxMuted);

            if (!_dataManager.CachedUserLocalData.AgreedTerms)
            {
                _uiManager.DrawPopup<TermsPopup>();
            }

            UpdateConnectionStateUI();
        }

        public void Hide()
        {
            if (_backendFacade.Contract != null)
            {
                _backendFacade.Contract.Client.ReadClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
                _backendFacade.Contract.Client.WriteClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
            }

            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
        }

        public void Dispose()
        {
        }

        private void RpcClientOnConnectionStateChanged(IRpcClient sender, RpcConnectionState state)
        {
            UnitySynchronizationContext.Instance.Post(o => UpdateConnectionStateUI(), null);
        }

        private void UpdateConnectionStateUI()
        {
            if (_selfPage == null)
                return;

            _connectionStatusText.text = _backendFacade.IsConnected ?
                "<color=green>Online</color>" :
                "<color=red>Offline</color>";

            _buttonReconnect.gameObject.SetActive(!_backendFacade.IsConnected);
            _markerOffline.gameObject.SetActive(_buttonReconnect.gameObject.activeSelf);
            _buttonLogout.gameObject.SetActive(!_buttonReconnect.gameObject.activeSelf);
            _markerOnline.gameObject.SetActive(!_buttonReconnect.gameObject.activeSelf);
        }

        private void LoomManagerOnContractCreated(Contract oldContract, Contract newContract)
        {
            if (oldContract != null)
            {
                oldContract.Client.ReadClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
                oldContract.Client.WriteClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
            }

            newContract.Client.ReadClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;
            newContract.Client.WriteClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;

            UpdateConnectionStateUI();
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        #region Buttons Handlers

        private void OnClickPlay()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            if (GameClient.Get<IDataManager>().CachedUserLocalData.Tutorial)
            {
                _uiManager.GetPage<GameplayPage>().CurrentDeckId = 0;

                GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
            }
            else
            {
                _stateManager.ChangeAppState(Enumerators.AppState.DECK_SELECTION);
            }
        }

        private void TutorialButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IDataManager>().CachedUserLocalData.Tutorial = true;
            GameClient.Get<IGameplayManager>().IsTutorial = true;
            _uiManager.GetPage<GameplayPage>().CurrentDeckId = 0;
            GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
        }

        private void OnClickCollection()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.COLLECTION);
        }

        private void BuyButtonHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.SHOP);
        }

        private void CreditsButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.CREDITS);
        }

        private void QuitButtonOnClickHandler()
        {
            Action[] actions = new Action[2];
            actions[0] = () =>
            {
                Application.Quit();
            };
            actions[1] = () => { };

            _uiManager.DrawPopup<ConfirmationPopup>(actions);
        }

        private void OpenButtonHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.PACK_OPENER);
        }

        private async void ReconnectButtonOnClickHandler()
        {
            try
            {
                ConnectionPopup connectionPopup = _uiManager.GetPopup<ConnectionPopup>();

                Func<Task> connectFunc = async () =>
                {
                    bool success = true;
                    try
                    {
                        await _backendDataControlMediator.LoginAndLoadData();
                    }
                    catch (Exception)
                    {
                        // HACK: ignore to allow offline mode
                    }

                    if (!_backendFacade.IsConnected)
                    {
                        success = false;
                    }

                    if (success)
                    {
                        connectionPopup.Hide();
                    }
                    else
                    {
                        connectionPopup.ShowFailedOnMenu();
                    }
                };
                _uiManager.DrawPopup<ConnectionPopup>();
                connectionPopup.ConnectFunc = connectFunc;
                await connectionPopup.ExecuteConnection();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                OpenAlertDialog(
                    $"Reconnect failed. Please check your Internet connection.\n\nAdditional info: {e.GetType().Name} [{e.Message}]");
            }
        }

        private void LogoutButtonOnClickHandler()
        {
            _dataManager.DeleteData();
            _backendDataControlMediator.UserDataModel = null;
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.APP_INIT);
        }

        private void OnValueChangedEventMusic(bool value)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _soundManager.SetMusicMuted(!value);
        }

        private void OnValueChangedEventSfx(bool value)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _soundManager.SetSoundMuted(!value);
        }

        #endregion

    }
}
