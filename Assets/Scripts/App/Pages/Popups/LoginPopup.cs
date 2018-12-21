using System;
using System.Linq;
using System.Numerics;
using Loom.Client;
using System.Security.Cryptography;
using System.Text;
using Loom.ZombieBattleground.BackendCommunication;
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

        public bool IsRegisteredUser;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IAnalyticsManager _analyticsManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private Transform _backgroundGroup;

        private Transform _loginGroup;

        private Transform _registerGroup;

        private Transform _forgottenGroup;

        private Transform _forgottenSuccessGroup;

        private Transform _waitingGroup;

        private Transform _versionMismatchGroup;

        private TextMeshProUGUI _versionMismatchText;

        private Button _versionMismatchExitButton;

        private Button _loginButton;

        private Button _toRegisterButton;

        private Button _closeLoginButton;

        private Button _forgotPasswordLoginButton;

        private Button _registerButton;

        private Button _toLoginButton;

        private Button _closeRegisterButton;

        private Button _cancelForgotButton;

        private Button _sendForgotButton;

        private Button _gotitForgotSuccessButton;

        private InputField _emailFieldLogin;
        private InputField _passwordFieldLogin;

        private InputField _emailFieldRegister;
        private InputField _passwordFieldRegister;
        private InputField _confirmFieldRegister;

        private InputField _emailFieldForgot;

        private LoginState _state;

        private LoginState _lastPopupState;

        private string _lastGUID;

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
            _loginButton = _loginGroup.transform.Find("Button_Login_BG/Button_Login").GetComponent<Button>();
            _emailFieldLogin = _loginGroup.transform.Find("Email_BG/Email_InputField").GetComponent<InputField>();
            _passwordFieldLogin = _loginGroup.transform.Find("Password_BG/Password_InputField").GetComponent<InputField>();
            _loginButton = _loginGroup.transform.Find("Button_Login_BG/Button_Login").GetComponent<Button>();
            _toRegisterButton = _loginGroup.transform.Find("Button_Register_BG/Button_Register").GetComponent<Button>();
            _forgotPasswordLoginButton = _loginGroup.transform.Find("Button_ForgotPassword").GetComponent<Button>();
            _closeLoginButton = _loginGroup.transform.Find("Button_Close_BG/Button_Close").GetComponent<Button>();

            _registerGroup = Self.transform.Find("Register_Group");
            _registerButton = _registerGroup.transform.Find("Button_Register_BG/Button_Register").GetComponent<Button>();
            _toLoginButton = _registerGroup.transform.Find("Button_Login").GetComponent<Button>();
            _emailFieldRegister = _registerGroup.transform.Find("Email_BG/Email_InputField").GetComponent<InputField>();
            _passwordFieldRegister = _registerGroup.transform.Find("Password_BG/Password_InputField").GetComponent<InputField>();
            _confirmFieldRegister = _registerGroup.transform.Find("Confirm_BG/Confirm_InputField").GetComponent<InputField>();
            _closeRegisterButton = _registerGroup.transform.Find("Button_Close_BG/Button_Close").GetComponent<Button>();

            _forgottenGroup = Self.transform.Find("Forgot_Group");
            _cancelForgotButton = _forgottenGroup.transform.Find("Button_Cancel_BG/Button_Cancel").GetComponent<Button>();
            _sendForgotButton = _forgottenGroup.transform.Find("Button_Send_BG/Button_Send").GetComponent<Button>();
            _emailFieldForgot = _forgottenGroup.transform.Find("Email_BG/Email_InputField").GetComponent<InputField>();

            _forgottenSuccessGroup = Self.transform.Find("SuccessForgot_Group");
            _gotitForgotSuccessButton = _forgottenSuccessGroup.transform.Find("Button_Confirm_BG/Button_Confirm").GetComponent<Button>();

            _waitingGroup = Self.transform.Find("Waiting_Group");
            _versionMismatchGroup = Self.transform.Find("VersionMismatch_Group");
            _versionMismatchText = _versionMismatchGroup.Find("Text_Error").GetComponent<TextMeshProUGUI>();
            _versionMismatchExitButton = _versionMismatchGroup.Find("Button_Exit").GetComponent<Button>();
            _versionMismatchExitButton.onClick.AddListener(Application.Quit);

            _loginButton.onClick.AddListener(PressedLoginHandler);
            _registerButton.onClick.AddListener(PressedRegisterHandler);
            _toRegisterButton.onClick.AddListener(PressedGoToRegisterHandler);
            _toLoginButton.onClick.AddListener(PressedGoToLoginHandler);
            _forgotPasswordLoginButton.onClick.AddListener(PressedGoToForgotPasswordHandler);
            _cancelForgotButton.onClick.AddListener(PressedGoToLoginHandler);
            _gotitForgotSuccessButton.onClick.AddListener(PressedGoToLoginHandler);
            _sendForgotButton.onClick.AddListener(PressedSendForgotPasswordHandler);
            _closeLoginButton.onClick.AddListener(Hide);
            _closeRegisterButton.onClick.AddListener(Hide);

            if (!Constants.AlwaysGuestLogin)
            {
                _state = LoginState.InitiateLogin;
                SetUIState(LoginState.InitiateLogin);
                Self.SetActive(true);
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

        public void SetLoginAsGuestState (string GUID = null) 
        {
            _lastGUID = GUID;
            SetUIState(LoginState.LoginAsGuest);
        }

        public void SetLoginFieldsDataAndInitiateLogin (string _email, string _password) 
        {
            _emailFieldLogin.text = _email;
            _passwordFieldLogin.text = _password;
            SetUIState(LoginState.InitiateLogin);
            if (!Constants.AlwaysGuestLogin)
            {
                LoginProcess(false);
            }
        }

        private void PressedSendForgotPasswordHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            if (_emailFieldForgot.text.Length > 0) 
            {
                ForgottenPasswordProcess();
            }
            else
            {
                _uiManager.GetPopup<WarningPopup>().Show("Please input valid data.");
            }
        }

        private void PressedGoToForgotPasswordHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            SetUIState(LoginState.ForgotPassword);
        }


        private void PressedGoToLoginHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            SetUIState(LoginState.InitiateLogin);
        }

        private void PressedGoToRegisterHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            SetUIState(LoginState.InitiateRegistration);
        }

        private void PressedRegisterHandler() 
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            if (_emailFieldRegister.text.Length > 0 && _passwordFieldRegister.text.Length > 0 && _confirmFieldRegister.text.Length > 0 && _passwordFieldRegister.text == _confirmFieldRegister.text)
            {
                _registerButton.enabled = false;
                RegisterProcess();
            }
            else
            {
                _uiManager.GetPopup<WarningPopup>().Show("Please input valid data.");
            }
        }

        private void PressedLoginHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            if (_emailFieldLogin.text.Length > 0 && _passwordFieldLogin.text.Length > 0)
            {
                _loginButton.enabled = false;
                LoginProcess(false);
            }
            else
            {
                _uiManager.GetPopup<WarningPopup>().Show("Please input valid data.");
            }
        }

        private async void ForgottenPasswordProcess()
        {
            SetUIState(LoginState.ValidateAndLogin);
            try
            {
                await _backendFacade.InitiateForgottenPassword(_emailFieldForgot.text);

                SetUIState(LoginState.SuccessForgotPassword);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                SetUIState(LoginState.ValidationFailed);
            }
        }

        private async void RegisterProcess () 
        {
            SetUIState(LoginState.ValidateAndLogin);
            try
            {
                RegisterData registerData = await _backendFacade.InitiateRegister(_emailFieldRegister.text, _passwordFieldRegister.text);

                SetLoginFieldsDataAndInitiateLogin(_emailFieldRegister.text, _passwordFieldRegister.text);

                return;
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                SetUIState(LoginState.ValidationFailed);
            }

            _registerButton.enabled = true;
        }

        private async void LoginProcess(bool isGuest)
        {
            SetUIState(LoginState.ValidateAndLogin);
            try
            {
                byte[] privateKey;
                byte[] publicKey;
                LoginData loginData;
                string userId;

                string GUID = _lastGUID ?? Guid.NewGuid().ToString();


                if (isGuest)
                {
                    GenerateKeysAndUserFromGUID(GUID, out byte[] privateKeyFromGuID, out byte[] publicKeyFromGuID, out string userIDFromGuID);
                    privateKey = privateKeyFromGuID;
                    publicKey = publicKeyFromGuID;
                    userId = userIDFromGuID;
                }
                else 
                {
                    loginData = await _backendFacade.InitiateLogin(_emailFieldLogin.text, _passwordFieldLogin.text);
                    Debug.Log(loginData.accessToken);
                    UserInfo userInfo = await _backendFacade.GetUserInfo(loginData.accessToken);

                    userId = "ZombieSlayer_" + userInfo.UserId;
                    GenerateKeysAndUserFromUserID(userId, out byte[] privateKeyFromUserId, out byte[] publicKeyFromUserID);

                    privateKey = privateKeyFromUserId;
                    publicKey = publicKeyFromUserID;
                }

                UserDataModel userDataModel = new UserDataModel(userId, privateKey)
                {
                    IsValid = false,
                    IsRegistered = !isGuest,
                    Email = _emailFieldLogin.text,
                    Password = _passwordFieldLogin.text,
                    GUID = GUID
                };

                _backendDataControlMediator.SetUserDataModel(userDataModel);
                await _backendDataControlMediator.LoginAndLoadData();

                userDataModel.IsValid = true;
                _backendDataControlMediator.SetUserDataModel(userDataModel);

                SuccessfulLogin();

                _analyticsManager.SetEvent(AnalyticsManager.EventLogIn);

                return;
            }
            catch (GameVersionMismatchException e)
            {
                SetUIState(LoginState.RemoteVersionMismatch);
                UpdateVersionMismatchText(e);
            }
            catch (Exception e) 
            {
                Debug.Log(e.ToString());
                SetUIState(LoginState.ValidationFailed);
            }

            _loginButton.enabled = true;
        }

        private void SuccessfulLogin()
        {
            if (!_backendDataControlMediator.UserDataModel.IsRegistered && GameClient.Get<IDataManager>().CachedUserLocalData.Tutorial)
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
            if (Constants.AlwaysGuestLogin) 
            {
                if (state == LoginState.InitiateLogin || state == LoginState.InitiateRegistration) 
                {
                    state = LoginState.LoginAsGuest;
                }
            }

            if (Self == null) 
                return;
            
            Debug.Log(state);
            _state = state;
            _backgroundGroup.gameObject.SetActive(false);
            _loginGroup.gameObject.SetActive(false);
            _registerGroup.gameObject.SetActive(false);
            _waitingGroup.gameObject.SetActive(false);
            _versionMismatchGroup.gameObject.SetActive(false);
            _forgottenGroup.gameObject.SetActive(false);
            _forgottenSuccessGroup.gameObject.SetActive(false);
            switch (_state)
            {
                case LoginState.InitiateLogin:
                    _lastPopupState = _state;
                    _backgroundGroup.gameObject.SetActive(false);
                    _loginGroup.gameObject.SetActive(true);
                    break;
                case LoginState.InitiateRegistration:
                    _lastPopupState = _state;
                    _backgroundGroup.gameObject.SetActive(false);
                    _registerGroup.gameObject.SetActive(true);
                    break;
                case LoginState.ValidateAndLogin:
                    _backgroundGroup.gameObject.SetActive(true);
                    _waitingGroup.gameObject.SetActive(true);
                    break;
                case LoginState.ValidationFailed:
                    WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
                    popup.Show("The process could not be completed. Please try again.");
                    _uiManager.GetPopup<WarningPopup>().ConfirmationReceived += WarningPopupClosedOnAutomatedLogin;
                    break;
                case LoginState.RemoteVersionMismatch:
                    _backgroundGroup.gameObject.SetActive(true);
                    _versionMismatchGroup.gameObject.SetActive(true);
                    break;
                case LoginState.LoginAsGuest:
                    _lastPopupState = _state;
                    LoginProcess(true);
                    break;
                case LoginState.ForgotPassword:
                    _lastPopupState = _state;
                    _backgroundGroup.gameObject.SetActive(false);
                    _forgottenGroup.gameObject.SetActive(true);
                    break;
                case LoginState.SuccessForgotPassword:
                    _backgroundGroup.gameObject.SetActive(false);
                    _forgottenSuccessGroup.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_state), _state, null);
            }
        }

        private void WarningPopupClosedOnAutomatedLogin()
        {
            SetUIState(_lastPopupState);
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
            InitiateRegistration,
            ValidationFailed,
            ValidateAndLogin,
            RemoteVersionMismatch,
            LoginAsGuest,
            ForgotPassword,
            SuccessForgotPassword
        }
    }
}
