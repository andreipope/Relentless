// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Loom.Unity3d.Zb;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public class LoadingPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;

        private GameObject _selfPage, _loginForm;

        private Transform _progressBar;

        private TextMeshProUGUI _pressAnyText;
        private TextMeshProUGUI _loadingText;
        private Image _loaderBar;

        private float _percentage = 0;

		private bool _isLoaded;
        private Color _pressAnyTextColor;

        private TMP_InputField _usernameInputField,
                                _passwordInputField;

        private Button _signUpButton,
                            _loginButton;

        private int a = 0;
        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();

			_selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/LoadingPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

			_localizationManager.LanguageWasChangedEvent += LanguageWasChangedEventHandler;
			UpdateLocalization();

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

			_loadingText.text = "LOADING...";

            _pressAnyText.gameObject.SetActive(false);
            _loginForm.SetActive(false);

            Hide();
        }


        public void Update()
        {
            if (_selfPage.activeInHierarchy && GameClient.Get<IAppStateManager>().AppState == Enumerators.AppState.APP_INIT)
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
                    //_pressAnyText.color = new Color(_pressAnyTextColor.r, _pressAnyTextColor.g, _pressAnyTextColor.b, Mathf.PingPong(Time.time, 1));
					float scalePressAnyTextValue = 1-Mathf.PingPong(Time.time*0.1f, 0.25f);
					_pressAnyText.transform.localScale = new Vector2(scalePressAnyTextValue, scalePressAnyTextValue);
                    if (Input.GetMouseButtonUp(0))
                    {
                        _loginForm.SetActive(true);
                        _pressAnyText.gameObject.SetActive(false);

                        //GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.LOGIN);
                        GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
                    }
                }
            }
        }

        public void Show()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.LOGO_APPEAR, Constants.SFX_SOUND_VOLUME, false, false, true);
            _selfPage.SetActive(true);
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
        }

        public void Dispose()
        {
            
        }

		private void LanguageWasChangedEventHandler(Enumerators.Language obj)
		{
			UpdateLocalization();
		}

		private void UpdateLocalization()
		{
			//  _loginText.text = _localizationManager.GetUITranslation("KEY_START_SCREEN_LOGIN");
		}

	    private async void OnSignupButtonPressed()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            //parentScene.OpenPopup<PopupSignup>("PopupSignup", popup =>{});
            //OpenAlertDialog("Will be available on full version");
	        
	        var usernameText = _usernameInputField.text;
	        var passwordText = _passwordInputField.text;
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
	        
			await LoomManager.Instance.SignUp(usernameText, result => {
				if(!string.IsNullOrEmpty(result))
					OpenAlertDialog("Not Able to Create Account..");
				else
				{
					CustomDebug.Log(" ====== Account Created Successfully ==== ");
					LoomManager.UserId = usernameText;
					//OpenAlertDialog("Account Created Successfully");
					// TODO : Removed code loading data manager
					var dataManager = GameClient.Get<IDataManager>();
					dataManager.OnLoadCacheCompletedEvent += OnLoadCacheComplete;
					dataManager.StartLoadCache();
				}
					
			});
        }

	    private void OnLoadCacheComplete()
	    {
		    GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
	    }

        public void OnLoginButtonPressed()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            var usernameText = _usernameInputField.text;
            var passwordText = _passwordInputField.text;

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
	        
	        LoomManager.UserId = usernameText;
	        var dataManager = GameClient.Get<IDataManager>();
	        dataManager.OnLoadCacheCompletedEvent += OnLoadCacheComplete;
	        dataManager.StartLoadCache();

            //GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
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

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

    }
}
