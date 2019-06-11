
using log4net;
using UnityEngine;
using UnityEngine.UI;
using NotImplementedException = System.NotImplementedException;

namespace Loom.ZombieBattleground
{
    public class ViewDeckPage : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(ShopWithNavigationPage));

        private const float BoardCardScale = 0.35f;

        private GameObject _selfPage;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;

        private Button _backButton;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/ViewDeckPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _backButton = _selfPage.transform.Find("ViewDeck/Top_Panel/Left_Panel/Button_Back").GetComponent<Button>();
            _backButton.onClick.AddListener(BackButtonHandler);


            UpdatePageScaleToMatchResolution();
        }

        private void BackButtonHandler()
        {
            Dispose();
        }

        private void UpdatePageScaleToMatchResolution()
        {
            float screenRatio = (float) Screen.width / Screen.height;
            if (screenRatio < 1.76f)
            {
                _selfPage.transform.localScale = Vector3.one * 0.93f;
            }
        }

        public void Hide()
        {

        }

        public void Update()
        {

        }

        public void Dispose()
        {
            if (_selfPage == null)
                return;

            Object.Destroy(_selfPage);
        }


    }
}


