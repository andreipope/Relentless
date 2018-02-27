using UnityEngine;
using UnityEngine.UI;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using CCGKit;

namespace GrandDevs.CZB
{
    public class ShopPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;

        private GameObject _selfPage;

        private Button _buttonItem1,
                        _buttonItem2,
                        _buttonItem3,
                        _buttonItem4;

        private MenuButtonNoGlow _buttonOpen,
                            _buttonBack,
                            _buttonBuy;

        private Text _description,
                     _cost;

        private float[] _costs = new float[] { 1.99f, 2.99f, 4.99f, 9.99f };

        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();

			_selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/ShopPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _description = _selfPage.transform.Find("Description").GetComponent<Text>();
            _cost = _selfPage.transform.Find("Cost").GetComponent<Text>();

            _buttonItem1 = _selfPage.transform.Find("Item1").GetComponent<Button>();
            _buttonItem2 = _selfPage.transform.Find("Item2").GetComponent<Button>();
            _buttonItem3 = _selfPage.transform.Find("Item3").GetComponent<Button>();
            _buttonItem4 = _selfPage.transform.Find("Item4").GetComponent<Button>();

            _buttonOpen = _selfPage.transform.Find("OpenButton").GetComponent<MenuButtonNoGlow>();
            _buttonBack = _selfPage.transform.Find("BackButton").GetComponent<MenuButtonNoGlow>();
            _buttonBuy = _selfPage.transform.Find("BuyButton").GetComponent<MenuButtonNoGlow>();

            _buttonItem1.onClick.AddListener(() => ChooseItemHandler(0));
            _buttonItem2.onClick.AddListener(() => ChooseItemHandler(1));
            _buttonItem3.onClick.AddListener(() => ChooseItemHandler(2));
            _buttonItem4.onClick.AddListener(() => ChooseItemHandler(3));

            _buttonOpen.onClickEvent.AddListener(OpenButtonHandler);
            _buttonBack.onClickEvent.AddListener(BackButtonhandler);
            _buttonBuy.onClickEvent.AddListener(BuyButtonHandler);

            Hide();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _cost.text = string.Empty;
            _selfPage.SetActive(true);
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
        }

        public void Dispose()
        {
            
        }

#region Buttons Handlers
        public void OpenButtonHandler()
        {
            OpenAlertDialog("Coming Soon");
        }
        private void BackButtonhandler()
		{
            GameClient.Get<IAppStateManager>().BackAppState();
		}
		private void BuyButtonHandler()
		{
            OpenAlertDialog("Coming Soon");
		}

        private void ChooseItemHandler(int id)
        {
            Debug.Log(_costs[id]+ " $");
            //_description = "";
            _cost.text = _costs[id] + " $";
        }

        #endregion

        private void OpenAlertDialog(string msg)
		{
			_uiManager.DrawPopup<WarningPopup>(msg);
		}
    }
}