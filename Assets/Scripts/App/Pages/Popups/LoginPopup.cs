using System;
using System.Linq;
using System.Numerics;
using Loom.Client;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground.Data;
using UnityEngine.EventSystems;

namespace Loom.ZombieBattleground
{
    public class LoginPopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(LoginPopup));

        public static Action OnShowPopupEvent;
        public static Action OnHidePopupEvent;
        public static Action OnLoginSuccess;

        public bool IsRegisteredUser;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IAnalyticsManager _analyticsManager;

        private IAppStateManager _appStateManager;

        private ITutorialManager _tutorialManager;

        private IDataManager _dataManager;

        private IInputManager _inputManager;

        private INetworkActionManager _networkActionManager;

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

        private EventSystem _currentEventSystem;

        private string _lastErrorMessage;

        private LoginState _state;

        private LoginState _lastPopupState;

        private string _lastGUID;

        private bool _gameStarted = false;

        private int _onEnterInputIndex = -1;

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
            _inputManager = GameClient.Get<IInputManager>();
            _networkActionManager = GameClient.Get<INetworkActionManager>();
            _currentEventSystem = EventSystem.current;
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

            _inputManager.UnregisterInputHandler(_onEnterInputIndex);
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

            _onEnterInputIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.KEYBOARD,
                (int)KeyCode.Return, null, OnInputDownEnterButton);
                
            OnShowPopupEvent?.Invoke();
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

        private void OnInputDownEnterButton()
        {
            if (_currentEventSystem.currentSelectedGameObject == (_passwordFieldLogin.gameObject || _emailFieldLogin.gameObject))
            {
                PressedLoginHandler();
            }
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
                _uiManager.GetPopup<WarningPopup>().Show("Incorrect verification code.\nPlease try again.");
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
                _uiManager.GetPopup<WarningPopup>().Show("Please enter a valid email address.");
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
                _uiManager.GetPopup<WarningPopup>().Show("Please enter both your email and password.");
                return;
            }

            if (!Utilites.ValidateEmail(_emailFieldRegister.text))
            {
                _uiManager.GetPopup<WarningPopup>().Show("Please enter a valid email address.");
                return;
            }

            if (_passwordFieldRegister.text != _confirmFieldRegister.text)
            {
                _uiManager.GetPopup<WarningPopup>().Show("Please make sure your passwords match.");
                return;
            }

            if (_emailFieldRegister.text.Length > 0 && _passwordFieldRegister.text.Length > 0 && _confirmFieldRegister.text.Length > 0 && _passwordFieldRegister.text == _confirmFieldRegister.text)
            {
                _registerButton.enabled = false;
                RegisterProcess();
            }
            else
            {
                _uiManager.GetPopup<WarningPopup>().Show("Incorrect email or password. Please try again.");
            }
        }

        private void PressedLoginHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            if (string.IsNullOrEmpty(_emailFieldLogin.text) || string.IsNullOrEmpty(_passwordFieldLogin.text))
            {
                _uiManager.GetPopup<WarningPopup>().Show("Please enter both your email and password.");
                return;
            }

            if (!Utilites.ValidateEmail(_emailFieldLogin.text))
            {
                _uiManager.GetPopup<WarningPopup>().Show("Please enter a valid email address.");
                return;
            }

            if (_emailFieldLogin.text.Length > 0 && _passwordFieldLogin.text.Length > 0)
            {
                _loginButton.enabled = false;
                LoginProcess(false);
            }
            else
            {
                _uiManager.GetPopup<WarningPopup>().Show("Incorrect email or password. Please try again.");
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
                Helpers.ExceptionReporter.SilentReportException(e);

                if (e.Message == Constants.VaultEmptyErrorCode)
                {
                    UpdatePrivateKeyProcess(noOTP, vaultTokenData);
                }
                else
                {
                    Log.Info(e.ToString());
                    string errorMsg = string.Empty;
                    if (e.Message.Contains("Forbidden"))
                    {
                        errorMsg = "Incorrect verification code.\nPlease try again.";
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
                Helpers.ExceptionReporter.LogExceptionAsWarning(Log, e);

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
                Helpers.ExceptionReporter.LogExceptionAsWarning(Log, e);
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

                SetUIState(LoginState.ValidationFailed, "Unable to register at this time.\nPlease try again a bit later.");

                _registerButton.enabled = true;
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogExceptionAsWarning(Log, e);
                string errorMsg = string.Empty;
                if (e.Message.Contains("BadRequest"))
                {
                    errorMsg = "That email is already in use.\nPlease use a different email or login to your existing account.";
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
                await _networkActionManager.EnqueueNetworkTask(async () =>
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
                            GenerateKeysAndUserFromGUID(GUID,
                                out byte[] privateKeyFromGuID,
                                out byte[] publicKeyFromGuID,
                                out string userIDFromGuID);
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
                    },

                    onNetworkExceptionCallbackFunc: exception =>
                    {
                        SetUIState(LoginState.ValidationFailed, "Unable to login at this time.\nPlease try again a bit later.");

                        _loginButton.enabled = true;
                        return Task.CompletedTask;
                    },
                    onUnknownExceptionCallbackFunc: exception =>
                    {
                        if (exception is GameVersionMismatchException gameVersionMismatchException)
                        {
                            SetUIState(LoginState.RemoteVersionMismatch);
                            UpdateVersionMismatchText(gameVersionMismatchException);

                            _loginButton.enabled = true;
                        }
                        else
                        {
                            Log.Info(exception.ToString());
                            _lastErrorMessage = exception.Message;
                            if (exception.Message.Contains("NotFound") || exception.Message.Contains("Unauthorized"))
                            {
                                _lastErrorMessage = "Incorrect username and/or password.\nPlease try again.";
                            }
                            SetUIState(LoginState.ValidationFailed);

                            _loginButton.enabled = true;
                        }
                        return Task.CompletedTask;
                    },
                    keepCurrentAppState: true,
                    drawErrorMessage: false,
                    ignoreConnectionState: true
                );
            }
            catch
            {
                // No additional handling
            }
        }

        private async void CompleteLoginFromCurrentSetUserData()
        {
            SetUIState(LoginState.ValidateAndLogin);

            try
            {
                await _networkActionManager.EnqueueNetworkTask(async () =>
                    {
                        await _backendDataControlMediator.LoginAndLoadData();

                        _backendDataControlMediator.UserDataModel.IsValid = true;
                        _backendDataControlMediator.SetUserDataModel(_backendDataControlMediator.UserDataModel);

                        SuccessfulLogin();

                        if (!_gameStarted)
                        {
                            _analyticsManager.SetEvent(AnalyticsManager.EventGameStarted);
                            _gameStarted = true;
                        }

                        _analyticsManager.SetEvent(AnalyticsManager.EventLogIn);
                    },
                    onUnknownExceptionCallbackFunc: exception =>
                    {
                        _lastErrorMessage = exception.Message;
                        SetUIState(LoginState.ValidationFailed);
                        return Task.CompletedTask;
                    },
                    onNetworkExceptionCallbackFunc: exception =>
                    {
                        _lastErrorMessage = exception.Message;
                        SetUIState(LoginState.ValidationFailed);
                        return Task.CompletedTask;
                    },
                    keepCurrentAppState: true,
                    drawErrorMessage: false,
                    ignoreConnectionState: true
                );
            }
            catch
            {
                // No additional handling
            }
        }

        public void SetValidationFailed (string errorMessage)
        {
            _lastErrorMessage = errorMessage;
            SetUIState(LoginState.ValidationFailed);
        } 

        private async void SuccessfulLogin()
        {
            bool tutorialBegan = false;
            if (!_backendDataControlMediator.UserDataModel.IsRegistered && _dataManager.CachedUserLocalData.Tutorial)
            {
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

                    _uiManager.GetPage<GameplayPage>().CurrentDeckId = savedTutorialDeck.Id;
                    GameClient.Get<IGameplayManager>().CurrentPlayerDeck = savedTutorialDeck;

                    if(_dataManager.CachedUserLocalData.CurrentTutorialId == 0)
                    {
                        _appStateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);

                        if(_uiManager.GetPopup<YouWonYouLostPopup>().Self == null)
                        {
                            string tutorialSkipQuestion = "Welcome, Zombie Slayer!\nWould you like a tutorial to get you started?";
                            QuestionPopup questionPopup = _uiManager.GetPopup<QuestionPopup>();
                            questionPopup.ConfirmationReceived += ConfirmTutorialReceivedHandler;

                            _uiManager.DrawPopup<QuestionPopup>(new object[] { tutorialSkipQuestion, false });
                        } 
                        else 
                        {
                            _tutorialManager.SkipTutorial();
                        }
                    }
                    else
                    {
                        tutorialBegan = true;
                        await GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
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
            }
            else
            {
                _appStateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);
            }

            (int? notificationId, EndMatchResults endMatchResults) =
                await GameClient.Get<IOverlordExperienceManager>().GetEndMatchResultsFromEndMatchNotification();

            if(endMatchResults != null && !tutorialBegan)
            {
                if(_uiManager.GetPopup<QuestionPopup>().Self != null)
                {
                    _tutorialManager.SkipTutorial();
                    _uiManager.HidePopup<QuestionPopup>();
                }
                _uiManager.DrawPopup<YouWonYouLostPopup>(new object[] { endMatchResults.IsWin });
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

            Log.Info(state);
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
                    Log.Warn("Version Mismatched");
#elif UNITY_ANDROID
                    Application.OpenURL(_dataManager.ZbVersion.Version.DownloadUrlPlayStore);
#elif UNITY_IOS
                    Application.OpenURL(_dataManager.ZbVersion.Version.DownloadUrlAppStore);
#elif UNITY_STANDALONE_OSX
                    Application.OpenURL(_dataManager.ZbVersion.Version.DownloadUrlMac);
#elif UNITY_STANDALONE_WIN
                    Application.OpenURL(_dataManager.ZbVersion.Version.DownloadUrlPC);
#else
                    Log.Warn("Version Mismatched");
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
                    string msgToShow = "We were unable to verify your login credentials at this time. Please restart the game and try again later.";

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
                Log.Info(e.Message);
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
            privateKey = CryptoUtils.GeneratePrivateKey();

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
