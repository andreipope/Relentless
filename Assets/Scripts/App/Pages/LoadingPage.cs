using System;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
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

        private GameObject _selfPage;

        private Transform _progressBar;

        private TextMeshProUGUI _pressAnyText;

        private TextMeshProUGUI _loadingText;

        private Image _loaderBar;

        private float _percentage;

        private bool _isLoaded;

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
                            _analyticsManager.SetEvent(AnalyticsManager.EventLogIn);
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
                    await connectionPopup.ExecuteConnection(ConnectionPopup.ConnectionState.FirstConnect);
                }
                else
                {
                    _uiManager.DrawPopup<LoginPopup>();
                }
            }
        }

        public void Show()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.LOGO_APPEAR, Constants.SfxSoundVolume, false, false, true);

            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/LoadingPageOptimized"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _progressBar = _selfPage.transform.Find("ProgresBar");

            _loaderBar = _progressBar.Find("Fill").GetComponent<Image>();
            _loadingText = _progressBar.Find("Text").GetComponent<TextMeshProUGUI>();
            _loadingText.text = "LOADING...";

            _loaderBar.fillAmount = 0.03f;
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
