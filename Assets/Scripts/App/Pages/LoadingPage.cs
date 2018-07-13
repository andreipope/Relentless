using System.Text;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using Loom.Unity3d.Samples;
using Loom.Unity3d.Zb;

namespace GrandDevs.CZB
{
    public class LoadingPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;

        private GameObject _selfPage, _loginForm;

        private Transform _progressBar;

        private Text _loadingText,
                     _pressAnyText;
        private Image _loaderBar;

        private float _fullFillWidth,
                        _percentage = 0;

		private bool _isLoaded;
		private Vector2 _fillSize;
        private Color _pressAnyTextColor;

        private TMP_InputField _usernameInputField,
                                _passwordInputField;

        private MenuButtonNoGlow _signUpButton,
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
            _loadingText = _progressBar.Find("Text").GetComponent<Text>();

            _pressAnyText = _selfPage.transform.Find("PressAnyText").GetComponent<Text>();

            _loginForm = _selfPage.transform.Find("LoginForm").gameObject;

            _usernameInputField = _loginForm.transform.Find("UsernameInputField").GetComponent<TMP_InputField>();
            _passwordInputField = _loginForm.transform.Find("PasswordInputField").GetComponent<TMP_InputField>();

            _signUpButton = _loginForm.transform.Find("SignUpButton").GetComponent<MenuButtonNoGlow>();
            _loginButton = _loginForm.transform.Find("LogInButton").GetComponent<MenuButtonNoGlow>();

            _signUpButton.onClickEvent.AddListener(OnSignupButtonPressed);
            _loginButton.onClickEvent.AddListener(OnLoginButtonPressed);

            _fillSize = _loaderBar.rectTransform.sizeDelta;
			_fullFillWidth = _fillSize.x;
            _fillSize.x = 0;

            _loaderBar.rectTransform.sizeDelta = _fillSize;

            _pressAnyTextColor = _pressAnyText.color;

			_loadingText.text = "Loading...";

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
                    _fillSize.x = _fullFillWidth * _percentage / 100f;
                    _loaderBar.rectTransform.sizeDelta = _fillSize;
                    if (_percentage >= 100)
                    {
                        _isLoaded = true;
                        _progressBar.gameObject.SetActive(false);
                        _pressAnyText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    _pressAnyText.color = new Color(_pressAnyTextColor.r, _pressAnyTextColor.g, _pressAnyTextColor.b, Mathf.PingPong(Time.time, 1));
                    if (Input.GetMouseButtonUp(0))
                    {
                        _loginForm.SetActive(true);
                        _pressAnyText.gameObject.SetActive(false);

                        GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.LOGIN);
                        /*a++;
                        if(a == 1)
						MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/GameNetworkManager"));
                        if(a==2)
                        {
							GameNetworkManager.Instance.isSinglePlayer = true;
							GameNetworkManager.Instance.StartHost();
						}
*/

                    }
                    //GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.LOGIN);
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

        public void OnSignupButtonPressed()
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
	        
	        var accountTx = new UpsertAccountRequest {
		        UserId = usernameText
	        };
	        
	        LoomManager.Instance.SignUp(accountTx, result =>
	        {
		        if (result != null)
		        {
			        if (result.CheckTx.Code != 0)
			        {
				        if (!string.IsNullOrEmpty(result.CheckTx.Error))
					        OpenAlertDialog(result.CheckTx.Error);
			        }
			        else if (result.DeliverTx.Code != 0)
			        {
				        if (!string.IsNullOrEmpty(result.DeliverTx.Error))
					        OpenAlertDialog(result.DeliverTx.Error);
			        }
			        else
				        OpenAlertDialog("Account Create Successfully.");
		        } 
		        else
		        {
			        OpenAlertDialog("Connection Not Found.");
		        }
	        });
	        
	         
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

            if (string.IsNullOrEmpty(passwordText))
            {
                OpenAlertDialog("Please enter your password.");
                return;
            }
	        
	        /*var mapEntry = new MapEntry();
	        mapEntry.Key = "Crypto";
	        mapEntry.Value = "Currency";
	        LoomManager.Instance.SetMessageWithResult(mapEntry, result =>
	        {
		        if (result != null)
		        {
			        if (result.CheckTx.Code != 0)
			        {
				        if (!string.IsNullOrEmpty(result.CheckTx.Error))
					        OpenAlertDialog(result.CheckTx.Error);
			        }
			        else if (result.DeliverTx.Code != 0)
			        {
				        if (!string.IsNullOrEmpty(result.DeliverTx.Error))
					        OpenAlertDialog(result.DeliverTx.Error);
			        }
			        else
				        OpenAlertDialog("result = " + result);
		        } 
		        else
		        {
			        OpenAlertDialog("Connection Not Found.");
		        }
	        });*/
	        
	        /*LoomManager.Instance.GetMessage(mapEntry, result =>
	        {
		        if(result != null)
			       Debug.Log("========= Result = " + result.Value);
		        else
			       Debug.Log("======= Key Not found ==== ");
	        });*/
	        

            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
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
