using System;
using System.Threading.Tasks;
using LoomNetwork.CZB.BackendCommunication;
using LoomNetwork.CZB.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class LoadingPage : IUIElement
    {
        private IUIManager _uiManager;

        private IDataManager _dataManager;

        private ILoadObjectsManager _loadObjectsManager;

        private ILocalizationManager _localizationManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private GameObject _selfPage, _loginForm;

        private Transform _progressBar;

        private TextMeshProUGUI _pressAnyText;

        private TextMeshProUGUI _loadingText;

        private Image _loaderBar;

        private float _percentage;

        private bool _isLoaded;

        private Color _pressAnyTextColor;

        private TMP_InputField _usernameInputField, _passwordInputField;

        private Button _signUpButton, _loginButton;

        private int _a = 0;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _localizationManager.LanguageWasChangedEvent += LanguageWasChangedEventHandler;
            UpdateLocalization();
        }

        public async void Update()
        {
            if (_selfPage == null)

                return;

            if (_selfPage.activeInHierarchy && (GameClient.Get<IAppStateManager>().AppState == Enumerators.AppState.AppInit))
            {
                if (!_isLoaded)
                {
                    _percentage += 1f;
                    _loaderBar.fillAmount = Mathf.Clamp(_percentage / 100f, 0.03f, 1f);
                    if (_percentage >= 100)
                    {
                        _isLoaded = true;
                        _progressBar.gameObject.SetActive(false);
                        _pressAnyText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    // _pressAnyText.color = new Color(_pressAnyTextColor.r, _pressAnyTextColor.g, _pressAnyTextColor.b, Mathf.PingPong(Time.time, 1));
                    // float scalePressAnyTextValue = 1-Mathf.PingPong(Time.time*0.1f, 0.25f);
                    // _pressAnyText.transform.localScale = new Vector2(scalePressAnyTextValue, scalePressAnyTextValue);
                    if (Input.anyKey)
                    {
                        // _loginForm.SetActive(true);
                        if (_pressAnyText.gameObject.activeSelf)
                        {
                            _pressAnyText.gameObject.SetActive(false);

                            // GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.LOGIN);
                            // GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
                            if (_backendDataControlMediator.LoadUserDataModel() && _backendDataControlMediator.UserDataModel.IsValid)
                            {
                                ConnectionPopup connectionPopup = _uiManager.GetPopup<ConnectionPopup>();

                                Func<Task> connectFunc = async () =>
                                {
                                    bool success = true;
                                    try
                                    {
                                        await _backendDataControlMediator.LoginAndLoadData();
                                    } catch (GameVersionMismatchException e)
                                    {
                                        success = false;
                                        _uiManager.DrawPopup<LoginPopup>();
                                        _uiManager.GetPopup<LoginPopup>().Show(e);
                                    } catch (Exception e)
                                    {
                                        // HACK: ignore to allow offline mode
                                        Debug.LogWarning(e);
                                    }

                                    connectionPopup.Hide();

                                    if (success)
                                    {
                                        GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MainMenu);
                                    }
                                };
                                _uiManager.DrawPopup<ConnectionPopup>();
                                connectionPopup.ConnectFunc = connectFunc;
                                await connectionPopup.ExecuteConnection();
                            }
                            else
                            {
                                _uiManager.DrawPopup<LoginPopup>();
                            }
                        }
                    }
                }
            }
        }

        public void Show()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.LogoAppear, Constants.SfxSoundVolume, false, false, true);

            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/LoadingPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _progressBar = _selfPage.transform.Find("ProgresBar");

            _loaderBar = _progressBar.Find("Fill").GetComponent<Image>();
            _loadingText = _progressBar.Find("Text").GetComponent<TextMeshProUGUI>();

            _pressAnyText = _selfPage.transform.Find("PressAnyText").GetComponent<TextMeshProUGUI>();

            _loginForm = _selfPage.transform.Find("LoginForm").gameObject;

            _usernameInputField = _loginForm.transform.Find("UsernameInputField").GetComponent<TMP_InputField>();
            _passwordInputField = _loginForm.transform.Find("PasswordInputField").GetComponent<TMP_InputField>();

            _signUpButton = _loginForm.transform.Find("SignUpButton").GetComponent<Button>();
            _loginButton = _loginForm.transform.Find("LogInButton").GetComponent<Button>();

            _signUpButton.onClick.AddListener(OnSignupButtonPressed);
            _loginButton.onClick.AddListener(OnLoginButtonPressed);

            _loaderBar.fillAmount = 0.03f;

            _pressAnyTextColor = _pressAnyText.color;

#if UNITY_IOS || UNITY_ANDROID
            _pressAnyText.text = "TAP TO CONTINUE";
#else
            _pressAnyText.text = "PRESS ANY KEY";
#endif
            _loadingText.text = "LOADING...";

            _pressAnyText.gameObject.SetActive(false);
            _loginForm.SetActive(false);

            if (_isLoaded)
            {
                _pressAnyText.gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if (_selfPage == null)

                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;

            _percentage = 0f;
            _isLoaded = false;
        }

        public void Dispose()
        {
        }

        public void OnLoginButtonPressed()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);
            string usernameText = _usernameInputField.text;
            string passwordText = _passwordInputField.text;

            // Perform some basic validation of the user input locally prior to calling the
            // remote login method. This is a good way to avoid some unnecessary network
            // traffic.
            if (string.IsNullOrEmpty(usernameText))
            {
                OpenAlertDialog("Please enter your username.");
                return;
            }

            /*if (string.IsNullOrEmpty(passwordText))
            {
                OpenAlertDialog("Please enter your password.");
                return;
            }*/
            _backendDataControlMediator.UserDataModel.UserId = usernameText;
            IDataManager dataManager = GameClient.Get<IDataManager>();
            dataManager.OnLoadCacheCompletedEvent += OnLoadCacheComplete;
            dataManager.StartLoadCache();

            // GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
            /*ClientAPI.Login(usernameText, passwordText,
				() =>
				{
					GameManager.Instance.isPlayerLoggedIn = true;
					GameManager.Instance.playerName = ClientAPI.masterServerClient.username;
					Close();
				},
				error =>
				{
					var errorMsg = "";
					switch (error)
					{
						case LoginError.DatabaseConnectionError:
							errorMsg = "There was an error connecting to the database.";
							break;

						case LoginError.NonexistingUser:
							errorMsg = "This user does not exist.";
							break;

						case LoginError.InvalidCredentials:
							errorMsg = "Invalid credentials.";
							break;

						case LoginError.ServerFull:
							errorMsg = "The server is full.";
							break;

						case LoginError.AuthenticationRequired:
							errorMsg = "Authentication is required.";
							break;

						case LoginError.UserAlreadyLoggedIn:
							errorMsg = "This user is already logged in.";
							break;
					}
					OpenAlertDialog(errorMsg);
				});*/
        }

        private void LanguageWasChangedEventHandler(Enumerators.Language obj)
        {
            UpdateLocalization();
        }

        private void UpdateLocalization()
        {
            // _loginText.text = _localizationManager.GetUITranslation("KEY_START_SCREEN_LOGIN");
        }

        private async void OnSignupButtonPressed()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);

            // parentScene.OpenPopup<PopupSignup>("PopupSignup", popup =>{});
            // OpenAlertDialog("Will be available on full version");
            string usernameText = _usernameInputField.text;
            string passwordText = _passwordInputField.text;
            if (string.IsNullOrEmpty(usernameText))
            {
                OpenAlertDialog("Please enter your username.");
                return;
            }

            /*if (string.IsNullOrEmpty(passwordText))
            {
                OpenAlertDialog("Please enter your password.");
                return;
            }*/
            try
            {
                await _backendFacade.SignUp(usernameText);
                Debug.Log(" ====== Account Created Successfully ==== ");
                _backendDataControlMediator.UserDataModel.UserId = usernameText;

                // OpenAlertDialog("Account Created Successfully");
                // TODO : Removed code loading data manager
                IDataManager dataManager = GameClient.Get<IDataManager>();
                dataManager.OnLoadCacheCompletedEvent += OnLoadCacheComplete;
                dataManager.StartLoadCache();
            } catch (Exception e)
            {
                OpenAlertDialog("Not Able to Create Account..");
            }
        }

        private void OnLoadCacheComplete()
        {
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MainMenu);
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }
    }
}
