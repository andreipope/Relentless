using System;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Iap;
using OneOf;
using OneOf.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class LoadingWithAnimationPage : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(LoadingWithAnimationPage));

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private ILocalizationManager _localizationManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private IAnalyticsManager _analyticsManager;

        private GameObject _selfPage;

        private Transform _progressBar;

        private TextMeshProUGUI _pressAnyText;

        private TextMeshProUGUI _loadingText;

        private Image _loaderBar;

        private float _percentage;

        private bool _isLoaded,
                     _isHasInternetConnection;

        private IDataManager _dataManager;

        private bool _dataLoading = false;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _dataManager = GameClient.Get<IDataManager>();

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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            //this makes us skip the initial "bar fill"
            _percentage = 100f;
#endif

            if(!_isHasInternetConnection)
                return;

            if (!_isLoaded)
            {
                _percentage += 1.4f;
                _loaderBar.fillAmount = Mathf.Clamp(_percentage / 100f, 0.03f, 1f);
                if (_percentage >= 100)
                {
                    _isLoaded = true;
                }
            }
            if(!_dataLoading)
            {
                _dataLoading = true;

                await _backendDataControlMediator.UpdateEndpointsFromZbVersion();

                try
                {
                    await GameClient.Get<IDataManager>().LoadZbVersionData();
                }
                catch (Exception e)
                {
                    Log.Info(e.Message);
                }

                OneOf<Success, IapException> beginInitialization = await GameClient.Get<IapMediator>().BeginInitialization();
                if (!beginInitialization.IsT0)
                {
                    Log.Warn("IAP initialization failed, it'll be retried next time. Failure: " + beginInitialization.Value);
                }

                if (_backendDataControlMediator.UserDataModel != null)
                {
                    GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
                    return;
                }

                if (_backendDataControlMediator.LoadUserDataModel() &&
                    _backendDataControlMediator.UserDataModel.IsValid)
                {
                    LoginPopup popup = _uiManager.GetPopup<LoginPopup>();
                    popup.Show();

                    if (!_backendDataControlMediator.UserDataModel.IsRegistered)
                    {
                        popup.SetLoginAsGuestState(_backendDataControlMediator.UserDataModel.GUID);
                    }
                    else
                    {
                        popup.SetLoginFieldsData(_backendDataControlMediator.UserDataModel.Email, _backendDataControlMediator.UserDataModel.Password);
                        popup.SetLoginFromDataState();
                    }
                }
                else
                {
                    LoginPopup popup = _uiManager.GetPopup<LoginPopup>();
                    popup.Show();
                    popup.SetLoginAsGuestState();
                }
            }
        }

        public void Show()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.LOGO_APPEAR, Constants.SfxSoundVolume, false, false, true);

            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/LoadingWithAnimationPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _progressBar = _selfPage.transform.Find("ProgresBar");

            _loaderBar = _progressBar.Find("Fill").GetComponent<Image>();
            _loadingText = _progressBar.Find("Text").GetComponent<TextMeshProUGUI>();
            _loadingText.text = "LOADING...";

            _loaderBar.fillAmount = 0.03f;

            _progressBar.gameObject.SetActive(false);

            CheckForMinimumSystemRequirement();
            CheckForInternetConnection();          
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

        private void CheckForMinimumSystemRequirement()
        {
            if (!SystemRequirementTool.CheckIfMeetMinimumSystemRequirement())
            {
                OpenAlertDialog("Your device does not meet with the minimum system requirements. If you choose to continue with game you may face difficulties or may not be able to play");
            }
        }
        
        private void CheckForInternetConnection()
        {
            if (GameClient.GetDefaultBackendPurpose() == BackendPurpose.Local)
            {
                _isHasInternetConnection = true;
                return;
            }

            if (SystemRequirementTool.CheckInternetConnectionReachability())
            {
                _isHasInternetConnection = true;
            }
            else
            {
                _isHasInternetConnection = false;
                InternetConnectionPopup popup = _uiManager.GetPopup<InternetConnectionPopup>();
                popup.ConfirmationReceived += ConfirmRetryIfNoConnection;
                popup.Show("Please check your internet connection and try again.");            
            }
        }

        private async void ConfirmRetryIfNoConnection(bool status)
        {
            _uiManager.GetPopup<InternetConnectionPopup>().ConfirmationReceived -= ConfirmRetryIfNoConnection;
            _uiManager.HidePopup<InternetConnectionPopup>();
            if(status)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                CheckForInternetConnection();
            }
            else
            {
                GameClient.Get<IAppStateManager>().QuitApplication();
            }
        }

        private void LanguageWasChangedEventHandler(Enumerators.Language obj)
        {
            UpdateLocalization();
        }

        private void UpdateLocalization()
        {
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }
    }
}
