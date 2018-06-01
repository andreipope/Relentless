using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GrandDevs.CZB
{
    public class LoginPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;

        private GameObject _selfPage;

		private TMP_InputField _usernameInputField,
                                _passwordInputField;

        private MenuButtonNoGlow _signUpButton,
                            _loginButton;


		public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();

			_selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/LoginPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

			_usernameInputField = _selfPage.transform.Find("UsernameInputField").GetComponent<TMP_InputField>();
			_passwordInputField = _selfPage.transform.Find("PasswordInputField").GetComponent<TMP_InputField>();
			
            _signUpButton = _selfPage.transform.Find("SignUpButton").GetComponent<MenuButtonNoGlow>();
			_loginButton = _selfPage.transform.Find("LogInButton").GetComponent<MenuButtonNoGlow>();

            _signUpButton.onClickEvent.AddListener(OnSignupButtonPressed);
            _loginButton.onClickEvent.AddListener(OnLoginButtonPressed);

            Hide();
        }


        public void Update()
        {
        }

        public void Show()
        {
            _selfPage.SetActive(true);
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
        }

        public void Dispose()
        {
            
        }

		public void OnSignupButtonPressed()
		{
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK);
            //parentScene.OpenPopup<PopupSignup>("PopupSignup", popup =>{});
            OpenAlertDialog("Will be available on full version");
		}

		public void OnLoginButtonPressed()
		{
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK);
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
