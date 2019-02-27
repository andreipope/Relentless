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
using Loom.Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class LoginPopup : IUIPopup
    {
        public static Action OnHidePopupEvent;
        public static Action OnLoginSuccess;

        public bool IsRegisteredUser;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IAnalyticsManager _analyticsManager;

        private IAppStateManager _appStateManager;

        private ITutorialManager _tutorialManager;

        private IDataManager _dataManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private Transform _backgroundGroup;

        private Transform _loginGroup;

        private Transform _registerGroup;

        private Transform _forgottenGroup;

        private Transform _forgottenSuccessGroup;

        private Transform _waitingGroup;

        private Transform _OTPGroup;

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

        private Button _cancelOTPButton;

        private Button _sendOTPButton;

        private InputField _emailFieldLogin;
        private InputField _passwordFieldLogin;

        private InputField _emailFieldRegister;
        private InputField _passwordFieldRegister;
        private InputField _confirmFieldRegister;

        private InputField _emailFieldForgot;

        private InputField _OTPFieldOTP;
        private Image _backgroundDarkImage;

        private string _lastErrorMessage;


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
            _appStateManager = GameClient.Get<IAppStateManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _dataManager = GameClient.Get<IDataManager>();
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

            _backgroundDarkImage = Self.GetComponent<Image>();

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

            _OTPGroup = Self.transform.Find("OTP_Group");
            _sendOTPButton = _OTPGroup.transform.Find("Button_Send_BG/Button_Send").GetComponent<Button>();
            _cancelOTPButton = _OTPGroup.transform.Find("Button_Cancel_BG/Button_Cancel").GetComponent<Button>();
            _OTPFieldOTP = _OTPGroup.transform.Find("OTP_BG/OTP_InputField").GetComponent<InputField>();

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
            _cancelOTPButton.onClick.AddListener(PressedGoToLoginHandler);
            _sendOTPButton.onClick.AddListener(PressedSendOTPHandler);

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

        public void SetLoginAsGuestState(string GUID = null)
        {
            _lastGUID = GUID;
            SetUIState(LoginState.LoginAsGuest);
        }

        public void SetLoginFromDataState()
        {
            SetUIState(LoginState.LoginFromCurrentSetOfData);
        }

        public void SetLoginFieldsData(string _email, string _password)
        {
            _emailFieldLogin.text = _email;
            _passwordFieldLogin.text = _password;
            SetUIState(LoginState.InitiateLogin);
        }

        public void SetRegistrationFieldsData(string _email, string _password)
        {
            _emailFieldRegister.text = _email;
            _passwordFieldRegister.text = _password;
            _confirmFieldRegister.text = _password;
        }

        public void Logout() 
        {
            Show();
            SetLoginAsGuestState();
        }

        private void PressedSendOTPHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            if (_OTPFieldOTP.text.Length > 0)
            {
                ConfirmOTPProcess();
            }
            else
            {
                _uiManager.GetPopup<WarningPopup>().Show("Please input a valid OTP.");
            }
        }

        private void PressedSendForgotPasswordHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            if (_emailFieldForgot.text.Length > 0 && Utilites.ValidateEmail(_emailFieldForgot.text))
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

            if (string.IsNullOrEmpty(_emailFieldRegister.text) || string.IsNullOrEmpty(_passwordFieldRegister.text) || string.IsNullOrEmpty(_confirmFieldRegister.text))
            {
                _uiManager.GetPopup<WarningPopup>().Show("No Email or Password Entered.");
                return;
            }

            if (!Utilites.ValidateEmail(_emailFieldRegister.text))
            {
                _uiManager.GetPopup<WarningPopup>().Show("Please input valid Email.");
                return;
            }

            if (_passwordFieldRegister.text != _confirmFieldRegister.text)
            {
                _uiManager.GetPopup<WarningPopup>().Show("Password Mismatch - Password and Confirm Password must be the same.");
                return;
            }

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

            if (string.IsNullOrEmpty(_emailFieldLogin.text) || string.IsNullOrEmpty(_passwordFieldLogin.text))
            {
                _uiManager.GetPopup<WarningPopup>().Show("No Email or Password Entered.");
                return;
            }

            if (!Utilites.ValidateEmail(_emailFieldLogin.text))
            {
                _uiManager.GetPopup<WarningPopup>().Show("Please input valid Email.");
                return;
            }

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

        private async void ConfirmOTPProcess(bool noOTP = false)
        {
            SetUIState(LoginState.ValidateAndLogin);
            CreateVaultTokenData vaultTokenData = new CreateVaultTokenData();
            try
            {
                if (noOTP)
                {
                    vaultTokenData = await _backendFacade.CreateVaultTokenForNon2FAUsers(_backendDataControlMediator.UserDataModel.AccessToken);
                }
                else
                {
                    vaultTokenData = await _backendFacade.CreateVaultToken(_OTPFieldOTP.text, _backendDataControlMediator.UserDataModel.AccessToken);
                }
                GetVaultDataResponse vaultDataData = await _backendFacade.GetVaultData(vaultTokenData.auth.client_token);
                _backendDataControlMediator.UserDataModel.PrivateKey = Convert.FromBase64String(vaultDataData.data.privatekey);
                CompleteLoginFromCurrentSetUserData();
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogException(e);

                if (e.Message == Constants.VaultEmptyErrorCode)
                {
                    UpdatePrivateKeyProcess(noOTP, vaultTokenData);
                }
                else
                {
                    Debug.Log(e.ToString());
                    string errorMsg = string.Empty;
                    if (e.Message.Contains("Forbidden"))
                    {
                        errorMsg = "Invalid OTP. \n Please Enter correct OTP.";
                    }
                    _lastErrorMessage = e.Message;
                    SetUIState(LoginState.ValidationFailed, errorMsg);
                }
            }
        }

        private async void UpdatePrivateKeyProcess(bool noOTP, CreateVaultTokenData vaultPreviousData = null)
        {
            SetUIState(LoginState.ValidateAndLogin);
            try
            {
                CreateVaultTokenData vaultTokenData;
                if (noOTP)
                {
                    vaultTokenData = await _backendFacade.CreateVaultTokenForNon2FAUsers(_backendDataControlMediator.UserDataModel.AccessToken);
                }
                else
                {
                    vaultTokenData = vaultPreviousData;
                }
                bool setVaultTokenResponse = await _backendFacade.SetVaultData(vaultTokenData.auth.client_token, Convert.ToBase64String(_backendDataControlMediator.UserDataModel.PrivateKey));
                CompleteLoginFromCurrentSetUserData();
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogException(e);

                Debug.Log(e.ToString());
                _lastErrorMessage = e.Message;
                SetUIState(LoginState.ValidationFailed);
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
                Helpers.ExceptionReporter.LogException(e);

                Debug.Log(e.ToString());
                _lastErrorMessage = e.Message;
                SetUIState(LoginState.ValidationFailed);
            }
        }

        private async void RegisterProcess()
        {
            SetUIState(LoginState.ValidateAndLogin);
            try
            {
                RegisterData registerData = await _backendFacade.InitiateRegister(_emailFieldRegister.text, _passwordFieldRegister.text);

                SetLoginFieldsData(_emailFieldRegister.text, _passwordFieldRegister.text);

                LoginProcess(false);
                return;
            }
            catch (RpcClientException e)
            {
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true, false);

                SetUIState(LoginState.ValidationFailed, "Registration was failed.\nPlease try again later.");

                _registerButton.enabled = true;
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogException(e);

                Debug.Log(e.ToString());
                string errorMsg = string.Empty;
                if (e.Message.Contains("BadRequest"))
                {
                    errorMsg = "This email already exists, \n " +
                               "Please try a different email to register or \n " +
                               "login to your existing account.";
                }

                _lastErrorMessage = e.Message;
                SetUIState(LoginState.ValidationFailed, errorMsg);

                _registerButton.enabled = true;
            }
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
                int authyId = 0;
                string accessToken = "";

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

                    string payload = loginData.accessToken.Split('.')[1];

                    string decodedText = Encoding.UTF8.GetString(Utilites.Base64UrlDecode(payload));

                    AccessTokenData accessTokenData = JsonConvert.DeserializeObject<AccessTokenData>(decodedText);

                    authyId = accessTokenData.authy_id;

                    accessToken = loginData.accessToken;

                    userId = "ZombieSlayer_" + accessTokenData.user_id;
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
                    GUID = GUID,
                    AccessToken = accessToken
                };

                _backendDataControlMediator.SetUserDataModel(userDataModel);

                _loginButton.enabled = true;

                if (authyId != 0)
                {
                    SetUIState(LoginState.PromptOTP);
                    return;
                }

                _OTPFieldOTP.text = "";

                if (isGuest)
                {
                    CompleteLoginFromCurrentSetUserData();
                }
                else
                {
                    ConfirmOTPProcess(true);
                }

                return;
            }
            catch (GameVersionMismatchException e)
            {
                Helpers.ExceptionReporter.LogException(e);

                SetUIState(LoginState.RemoteVersionMismatch);
                UpdateVersionMismatchText(e);

                _loginButton.enabled = true;
            }
            catch (TimeoutException e)
            {
                Helpers.ExceptionReporter.LogException(e);

                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true, false);

                SetUIState(LoginState.ValidationFailed, "Login failed due to: " + e.Message);

                _loginButton.enabled = true;
            }
            catch (RpcClientException e)
            {
                Helpers.ExceptionReporter.LogException(e);

                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true, false);

                SetUIState(LoginState.ValidationFailed, "Login failed due to: " + e.Message);

                _loginButton.enabled = true;
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogException(e);

                Debug.Log(e.ToString());
                _lastErrorMessage = e.Message;
                if (e.Message.Contains("NotFound") || e.Message.Contains("Unauthorized"))
                {
                    _lastErrorMessage = "\n The Username and/or Password are not correct. \n";
                }
                SetUIState(LoginState.ValidationFailed);

                _loginButton.enabled = true;
            }
        }

        private async void CompleteLoginFromCurrentSetUserData()
        {
            SetUIState(LoginState.ValidateAndLogin);

            try
            {
                await _backendDataControlMediator.LoginAndLoadData();

                _backendDataControlMediator.UserDataModel.IsValid = true;
                _backendDataControlMediator.SetUserDataModel(_backendDataControlMediator.UserDataModel);

                SuccessfulLogin();

                _analyticsManager.SetEvent(AnalyticsManager.EventLogIn);
            }
            catch (TimeoutException e)
            {
                Helpers.ExceptionReporter.LogException(e);

                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true, false);

                _lastErrorMessage = e.Message;
                SetUIState(LoginState.ValidationFailed);
            }
            catch (RpcClientException e)
            {
                Helpers.ExceptionReporter.LogException(e);

                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true, false);

                _lastErrorMessage = e.Message;
                SetUIState(LoginState.ValidationFailed);
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogException(e);

                Debug.LogWarning(e);
                _lastErrorMessage = e.Message;
                SetUIState(LoginState.ValidationFailed);
            }
        }

        private async void SuccessfulLogin()
        {
            if (!_backendDataControlMediator.UserDataModel.IsRegistered && _dataManager.CachedUserLocalData.Tutorial)
            {
#if USE_REBALANCE_BACKEND
                GameClient.Get<IDataManager>().CachedUserLocalData.Tutorial = false;
                _appStateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);
#else
                GameClient.Get<IGameplayManager>().IsTutorial = true;
                (_tutorialManager as TutorialManager).CheckAvailableTutorial();

                _tutorialManager.SetupTutorialById(_dataManager.CachedUserLocalData.CurrentTutorialId);

                if (_tutorialManager.CurrentTutorial.IsGameplayTutorial())
                {
                    Data.Deck savedTutorialDeck = _dataManager.CachedUserLocalData.TutorialSavedDeck;

                    if (savedTutorialDeck != null)
                    {
                        if (_dataManager.CachedDecksData.Decks.Find(deck => deck.Id == savedTutorialDeck.Id) == null)
                        {
                            _dataManager.CachedDecksData.Decks.Add(savedTutorialDeck);
                            await _backendFacade.AddDeck(_backendDataControlMediator.UserDataModel.UserId, savedTutorialDeck);
                        }
                    }
                    else
                    {
                        savedTutorialDeck = _dataManager.CachedDecksData.Decks.Last();
                    }

                    if(_dataManager.CachedUserLocalData.CurrentTutorialId == 0)
                    {
                        _appStateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);
                        _uiManager.GetPage<GameplayPage>().CurrentDeckId = (int)savedTutorialDeck.Id;
                        GameClient.Get<IGameplayManager>().CurrentPlayerDeck = savedTutorialDeck;

                        string tutorialSkipQuestion = "Welcome, Zombie Slayer!\nWould you like a tutorial to get you started?";
                        QuestionPopup questionPopup = _uiManager.GetPopup<QuestionPopup>();
                        questionPopup.ConfirmationReceived += ConfirmTutorialReceivedHandler;

                        _uiManager.DrawPopup<QuestionPopup>(new object[] { tutorialSkipQuestion, false });
                    }
                    else
                    {
                        GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
                    }
                }
                else
                {
                    _appStateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);

                    if (!_tutorialManager.IsTutorial)
                    {
                        _tutorialManager.StartTutorial();
                    }
                }
#endif
            }
            else
            {
                _appStateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);
            }
            Hide();
            OnLoginSuccess?.Invoke();
        }

        private void ConfirmTutorialReceivedHandler(bool state)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmTutorialReceivedHandler;
            if (state)
            {
                GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
            }
            else
            {
                _tutorialManager.SkipTutorial();
            }
        }

        private void SetUIState(LoginState state, string errorMsg = "")
        {
            if (Constants.AlwaysGuestLogin)
            {
                if (state == LoginState.InitiateLogin || state == LoginState.InitiateRegistration || state == LoginState.LoginFromCurrentSetOfData)
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
            _OTPGroup.gameObject.SetActive(false);
            _backgroundDarkImage.enabled = true;

            if (_backendFacade.BackendEndpoint.IsForceUpdate)
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
                _backgroundDarkImage.enabled = false;
                _uiManager.DrawPopup<UpdatePopup>(actions);
                return;
            }

            if (_backendFacade.BackendEndpoint.IsMaintenanceMode && _state != LoginState.ValidationFailed)
            {
                _lastPopupState = _state;
                SetUIState(LoginState.ValidationFailed, Constants.ErrorMessageForMaintenanceMode);
                return;
            }

            if (_backendFacade.BackendEndpoint.IsConnectionImpossible && _state != LoginState.ValidationFailed)
            {
                _lastPopupState = _state;
                SetUIState(LoginState.ValidationFailed, Constants.ErrorMessageForConnectionImpossible);
                return;
            }

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
                    if (_appStateManager.AppState == Enumerators.AppState.APP_INIT)
                    {
                        _backgroundDarkImage.enabled = false;
                    }
                    else
                    {
                        _backgroundGroup.gameObject.SetActive(true);
                        _waitingGroup.gameObject.SetActive(true);
                    }
                    break;
                case LoginState.ValidationFailed:
                    WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
                    string msgToShow = "The process could not be completed with error:" + _lastErrorMessage +
                                       "\nPlease try again.";

                    if (!string.IsNullOrEmpty(errorMsg))
                        msgToShow = errorMsg;
                    popup.Show(msgToShow);
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
                case LoginState.PromptOTP:
                    _lastPopupState = _state;
                    _backgroundGroup.gameObject.SetActive(false);
                    _OTPGroup.gameObject.SetActive(true);
                    break;
                case LoginState.LoginFromCurrentSetOfData:
                    _lastPopupState = _state;
                    CompleteLoginFromCurrentSetUserData();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_state), _state, null);
            }
        }

        private async void WarningPopupClosedOnAutomatedLogin()
        {
            _uiManager.GetPopup<WarningPopup>().ConfirmationReceived -= WarningPopupClosedOnAutomatedLogin;
            try
            {
                if (_backendFacade.BackendEndpoint == BackendEndpointsContainer.Endpoints[BackendPurpose.Production])
                {
                    _backendFacade.BackendEndpoint = await _backendFacade.GetServerURLs();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                _backendFacade.BackendEndpoint = BackendEndpointsContainer.Endpoints[BackendPurpose.Production];
            }
            finally
            {
                SetUIState(_lastPopupState);
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
            userIdNumber = BigInteger.Abs(userIdNumber);
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
            SuccessForgotPassword,
            PromptOTP,
            LoginFromCurrentSetOfData
        }
    }
}
