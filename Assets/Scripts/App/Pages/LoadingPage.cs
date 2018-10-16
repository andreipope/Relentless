using System;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using mixpanel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class LoadingPage : IUIElement
    {
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private ILocalizationManager _localizationManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private IAnalyticsManager _analyticsManager;

        private GameObject _selfPage, _loginForm;

        private Transform _progressBar;

        private TextMeshProUGUI _pressAnyText;

        private TextMeshProUGUI _loadingText;

        private Image _loaderBar;

        private float _percentage;

        private bool _isLoaded;

        private TMP_InputField _usernameInputField;

        private Button _signUpButton, _loginButton;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();

            _localizationManager.LanguageWasChangedEvent += LanguageWasChangedEventHandler;
            UpdateLocalization();
        }

        public async void Update()
        {
            if (_selfPage == null)
                return;

            if (!_selfPage.activeInHierarchy ||
                GameClient.Get<IAppStateManager>().AppState != Enumerators.AppState.APP_INIT)
                return;

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
                if (!Input.anyKey)
                    return;

                if (!_pressAnyText.gameObject.activeSelf)
                    return;

                _pressAnyText.gameObject.SetActive(false);

                if (_backendDataControlMediator.LoadUserDataModel() &&
                    _backendDataControlMediator.UserDataModel.IsValid)
                {
                    ConnectionPopup connectionPopup = _uiManager.GetPopup<ConnectionPopup>();

                    Func<Task> connectFunc = async () =>
                    {
                        bool success = true;
                        try
                        {
                            await _backendDataControlMediator.LoginAndLoadData();
                            SendLoginAnalytics();
                        }
                        catch (GameVersionMismatchException e)
                        {
                            success = false;
                            _uiManager.GetPopup<LoginPopup>().Show(e);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e);
                            success = false;
                            _uiManager.DrawPopup<LoginPopup>();
                        }

                        connectionPopup.Hide();

                        if (success)
                        {
                            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
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

        private void SendLoginAnalytics()
        {
            Value props = new Value();
            props[AnalyticsManager.PropertyTesterKey] = _backendDataControlMediator.UserDataModel.BetaKey;
            props[AnalyticsManager.PropertyDAppChainWalletAddress] = Application.platform.ToString();
            _analyticsManager.SetEvent(_backendDataControlMediator.UserDataModel.UserId, AnalyticsManager.EventLogIn, props);
        }

        public void Show()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.LOGO_APPEAR, Constants.SfxSoundVolume,
                false, false, true);

            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/LoadingPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _progressBar = _selfPage.transform.Find("ProgresBar");

            _loaderBar = _progressBar.Find("Fill").GetComponent<Image>();
            _loadingText = _progressBar.Find("Text").GetComponent<TextMeshProUGUI>();

            _pressAnyText = _selfPage.transform.Find("PressAnyText").GetComponent<TextMeshProUGUI>();

            _loginForm = _selfPage.transform.Find("LoginForm").gameObject;

            _usernameInputField = _loginForm.transform.Find("UsernameInputField").GetComponent<TMP_InputField>();
            _signUpButton = _loginForm.transform.Find("SignUpButton").GetComponent<Button>();
            _loginButton = _loginForm.transform.Find("LogInButton").GetComponent<Button>();

            _signUpButton.onClick.AddListener(OnSignupButtonPressed);
            _loginButton.onClick.AddListener(OnLoginButtonPressed);

            _loaderBar.fillAmount = 0.03f;

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

        public async void OnLoginButtonPressed()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            string usernameText = _usernameInputField.text;

            // Perform some basic validation of the user input locally prior to calling the
            // remote login method. This is a good way to avoid some unnecessary network
            // traffic.
            if (string.IsNullOrEmpty(usernameText))
            {
                OpenAlertDialog("Please enter your username.");
                return;
            }

            _backendDataControlMediator.UserDataModel.UserId = usernameText;
            IDataManager dataManager = GameClient.Get<IDataManager>();
            await dataManager.StartLoadCache();
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }

        private void LanguageWasChangedEventHandler(Enumerators.Language obj)
        {
            UpdateLocalization();
        }

        private void UpdateLocalization()
        {
        }

        private async void OnSignupButtonPressed()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            string usernameText = _usernameInputField.text;
            if (string.IsNullOrEmpty(usernameText))
            {
                OpenAlertDialog("Please enter your username.");
                return;
            }

            try
            {
                await _backendFacade.SignUp(usernameText);
                Debug.Log(" ====== Account Created Successfully ==== ");
                _backendDataControlMediator.UserDataModel.UserId = usernameText;

                // TODO : Removed code loading data manager
                IDataManager dataManager = GameClient.Get<IDataManager>();
                await dataManager.StartLoadCache();
                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
            }
            catch (Exception)
            {
                OpenAlertDialog("Not Able to Create Account.");
            }
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }
    }
}
