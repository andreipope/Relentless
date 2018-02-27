using UnityEngine;
using UnityEngine.UI;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;

namespace GrandDevs.CZB
{
    public class LoadingPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;

        private GameObject _selfPage;

        private Transform _progressBar;

        private Text _loadingText,
                     _pressAnyText;
        private Image _loaderBar;

        private float _fullFillWidth,
                        _percentage = 0;

		private bool _isLoaded;
		private Vector2 _fillSize;
        private Color _pressAnyTextColor;

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

            _fillSize = _loaderBar.rectTransform.sizeDelta;
			_fullFillWidth = _fillSize.x;
            _fillSize.x = 0;

            _loaderBar.rectTransform.sizeDelta = _fillSize;

            _pressAnyTextColor = _pressAnyText.color;

			_loadingText.text = "Loading...";

            _pressAnyText.gameObject.SetActive(false);

            Hide();
        }


        public void Update()
        {
            if (_selfPage.activeInHierarchy)
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

    }
}
