using System;
using System.Linq;
using System.Numerics;
using Loom.Client;
using System.Security.Cryptography;
using System.Text;
using Loom.Client;using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class LoginPopup : IUIPopup
    {
        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IAnalyticsManager _analyticsManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private Transform _backgroundGroup;

        private Transform _loginGroup;

        private Transform _waitingGroup;

        private Transform _versionMismatchGroup;

        private TextMeshProUGUI _versionMismatchText;

        private Button _versionMismatchExitButton;


        private LoginState _state;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            OnHidePopupEvent?.Invoke();

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
            if (Self == null)
            {
                Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LoginPopup"));
            }
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _backgroundGroup = Self.transform.Find("Background");
            _loginGroup = Self.transform.Find("Login_Group");
            _waitingGroup = Self.transform.Find("Waiting_Group");
            _versionMismatchGroup = Self.transform.Find("VersionMismatch_Group");
            _versionMismatchText = _versionMismatchGroup.Find("Text_Error").GetComponent<TextMeshProUGUI>();
            _versionMismatchExitButton = _versionMismatchGroup.Find("Button_Exit").GetComponent<Button>();
            _versionMismatchExitButton.onClick.AddListener(Application.Quit);

            _state = LoginState.InitiateLogin;
            SetUIState(LoginState.InitiateLogin);
            Self.SetActive(true);
        }

        public void Show(object data)
        {
            Show();

            if (data is GameVersionMismatchException gameVersionMismatchException)
            {
                SetUIState(LoginState.RemoteVersionMismatch);
                UpdateVersionMismatchText(gameVersionMismatchException);
            }
        }

        public void Update()
        {
        }

        private void PressedLoginHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            LoginProcess();
        }

        private async void LoginProcess()
        {
            /*
            // check if field is empty. Can replace with exact value once we know if there's a set length for beta keys
            SetUIState(LoginState.BetaKeyValidateAndLogin);

            GenerateKeysAndUserFromBetaKey(betaKey, out byte[] privateKey, out byte[] _, out string userId);

            try
            {
                UserDataModel userDataModel = new UserDataModel(userId, betaKey, privateKey)
                {
                    IsValid = false
                };
                _backendDataControlMediator.SetUserDataModel(userDataModel);
                await _backendDataControlMediator.LoginAndLoadData();

                userDataModel.IsValid = true;
                _backendDataControlMediator.SetUserDataModel(userDataModel);

                SuccessfulLogin();

                _analyticsManager.SetEvent(AnalyticsManager.EventLogIn);
            }
            catch (GameVersionMismatchException e)
            {
                SetUIState(LoginState.RemoteVersionMismatch);
                UpdateVersionMismatchText(e);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                SetUIState(LoginState.BetaKeyValidationFailed);
            }
            */
        }

        private void SuccessfulLogin()
        {
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
            Hide();
        }

        private void SetUIState(LoginState state)
        {
            _state = state;
            _backgroundGroup.gameObject.SetActive(false);
            _loginGroup.gameObject.SetActive(false);
            _waitingGroup.gameObject.SetActive(false);
            _versionMismatchGroup.gameObject.SetActive(false);
            switch (_state)
            {
                case LoginState.InitiateLogin:
                    _backgroundGroup.gameObject.SetActive(false);
                    _loginGroup.gameObject.SetActive(false);
                    break;
                case LoginState.ValidateAndLogin:
                    _backgroundGroup.gameObject.SetActive(true);
                    _waitingGroup.gameObject.SetActive(true);
                    break;
                case LoginState.ValidationFailed:
                    SetUIState(LoginState.InitiateLogin);
                    _uiManager.GetPopup<WarningPopup>().Show("The process could not be completed. Please try again.");
                    break;
                case LoginState.RemoteVersionMismatch:
                    _backgroundGroup.gameObject.SetActive(true);
                    _versionMismatchGroup.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_state), _state, null);
            }
        }

        private void UpdateVersionMismatchText(GameVersionMismatchException exception)
        {
            _versionMismatchText.text =
                $"This version ({exception.LocalVersion}) is out of date.\n\nPlease download version {exception.RemoteVersion}.";
        }

        private void GenerateKeysAndUserFromBetaKey(
            string betaKey, out byte[] privateKey, out byte[] publicKey, out string userId)
        {
            betaKey = betaKey.ToLowerInvariant();

            byte[] betaKeySeed = CryptoUtils.HexStringToBytes(betaKey);

            BigInteger userIdNumber = new BigInteger(betaKeySeed) + betaKeySeed.Sum(b => b * 2);
            userId = "ZombieSlayer_" + userIdNumber;

            privateKey = CryptoUtils.GeneratePrivateKey(betaKeySeed);

            publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);
        }

        private enum LoginState
        {
            InitiateLogin,
            ValidationFailed,
            ValidateAndLogin,
            RemoteVersionMismatch
        }
    }
}
