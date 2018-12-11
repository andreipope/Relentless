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

        private Button _loginButton;

        private InputField _emailField;
        private InputField _passwordField;


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
            _loginButton = _loginGroup.transform.Find("Button_Login").GetComponent<Button>();
            _emailField = _loginGroup.transform.Find("Email_InputField").GetComponent<InputField>();
            _passwordField = _loginGroup.transform.Find("Password_InputField").GetComponent<InputField>();
            _loginButton = _loginGroup.transform.Find("Button_Login").GetComponent<Button>();
            _waitingGroup = Self.transform.Find("Waiting_Group");
            _versionMismatchGroup = Self.transform.Find("VersionMismatch_Group");
            _versionMismatchText = _versionMismatchGroup.Find("Text_Error").GetComponent<TextMeshProUGUI>();
            _versionMismatchExitButton = _versionMismatchGroup.Find("Button_Exit").GetComponent<Button>();
            _versionMismatchExitButton.onClick.AddListener(Application.Quit);

            _loginButton.onClick.AddListener(PressedLoginHandler);

            _state = LoginState.InitiateLogin;
            SetUIState(LoginState.InitiateLogin);
            Self.SetActive(true);

            if (Constants.AutomaticLoginEnabled) {
                LoginProcess();
            }
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

            if (_emailField.text.Length > 0 && _passwordField.text.Length > 0)
            {
                _loginButton.enabled = false;
                LoginProcess();
            }
            else
            {
                _uiManager.GetPopup<WarningPopup>().Show("Please input valid data.");
            }
        }

        private async void LoginProcess()
        {
            SetUIState(LoginState.ValidateAndLogin);
            try
            {
                byte[] privateKey;
                byte[] publicKey;
                LoginData loginData;
                string userId;

                if (Constants.AutomaticLoginEnabled)
                {
                    GenerateKeysAndUserFromGUID(Guid.NewGuid().ToString(), out byte[] privateKeyFromGuID, out byte[] publicKeyFromGuID, out string userIDFromGuID);
                    privateKey = privateKeyFromGuID;
                    publicKey = publicKeyFromGuID;
                    userId = userIDFromGuID;
                }
                else 
                {
                    loginData = await _backendFacade.InitiateLogin(_emailField.text, _passwordField.text);
                    Debug.Log(loginData.accessToken);
                    UserInfo userInfo = await _backendFacade.GetUserInfo(loginData.accessToken);

                    userId = "ZombieSlayer_" + userInfo.UserId;
                    GenerateKeysAndUserFromUserID(userId, out byte[] privateKeyFromUserId, out byte[] publicKeyFromUserID);

                    privateKey = privateKeyFromUserId;
                    publicKey = publicKeyFromUserID;
                }

                UserDataModel userDataModel = new UserDataModel(userId, privateKey)
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
            catch (Exception e) {
                SetUIState(LoginState.InitiateLogin);
                Debug.Log(e.ToString());
                _uiManager.GetPopup<WarningPopup>().Show("Login could not be completed. Please ensure your details are correct and try again.");
            }

            _loginButton.enabled = true;
        }

        private void SuccessfulLogin()
        {
            if (GameClient.Get<IDataManager>().CachedUserLocalData.Tutorial)
            {
                _uiManager.GetPage<GameplayPage>().CurrentDeckId = 0;

                GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
            }
            else
            {
                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
            }
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
                    _loginGroup.gameObject.SetActive(true);
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

        private void GenerateKeysAndUserFromUserID(
            string userId, out byte[] privateKey, out byte[] publicKey)
        {
            userId = "ZombieSlayer_" + userId;

            string seedString =
                CryptoUtils.BytesToHexString(
                    new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(userId))) +
                CryptoUtils.BytesToHexString(
                    new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(userId)));

            byte[] seedByte = CryptoUtils.HexStringToBytes(seedString);

            privateKey = CryptoUtils.GeneratePrivateKey(seedByte);

            publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);
        }

        private void GenerateKeysAndUserFromGUID(
            string guID, out byte[] privateKey, out byte[] publicKey, out string userId)
        {
            string guidKey =
                CryptoUtils.BytesToHexString(
                    new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(guID))) +
                CryptoUtils.BytesToHexString(
                    new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(guID)));

            byte[] seedByte = CryptoUtils.HexStringToBytes(guidKey);

            BigInteger userIdNumber = new BigInteger(seedByte) + seedByte.Sum(b => b * 2);
            userId = "ZombieSlayer_" + userIdNumber;

            privateKey = CryptoUtils.GeneratePrivateKey(seedByte);

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
