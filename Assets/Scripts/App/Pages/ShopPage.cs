using UnityEngine;
using UnityEngine.UI;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using CCGKit;
using DG.Tweening;


namespace GrandDevs.CZB
{
    public class ShopPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;
		private IPlayerManager _playerManager;

        private GameObject _selfPage;

        private Button _buttonItem1,
                        _buttonItem2,
                        _buttonItem3,
                        _buttonItem4;

        private MenuButtonNoGlow _buttonOpen,
                            _buttonBack,
                            _buttonBuy;

        private Text _description,
                     _cost,
                     _wallet;

        private int _currentPackId = -1;

		private float[] _costs = new float[] { 1.99f, 2.99f, 4.99f, 9.99f };
		private int[] _amount = new int[] { 1, 3, 5, 8 };
        private GameObject[] _packsObjects;

        private float _itemYstartPos;

        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();
            _playerManager = GameClient.Get<IPlayerManager>();

			_selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/ShopPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _description = _selfPage.transform.Find("Description").GetComponent<Text>();
            _cost = _selfPage.transform.Find("Cost").GetComponent<Text>();
            _wallet = _selfPage.transform.Find("Wallet").GetComponent<Text>();
            

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

            _itemYstartPos = _buttonItem1.gameObject.transform.position.y;

            _packsObjects = new GameObject[] { _buttonItem1.gameObject,
                                        _buttonItem2.gameObject,
                                        _buttonItem3.gameObject,
                                        _buttonItem4.gameObject};

            Hide();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _cost.text = string.Empty;
            _playerManager.LocalUser.wallet = 1000;
            _wallet.text = _playerManager.LocalUser.wallet.ToString("0.00") + " $";
            if (_currentPackId > -1)
            {
                _packsObjects[_currentPackId].transform.Find("Highlight").GetComponent<Image>().DOFade(0f, 0f);
                _currentPackId = -1;
            }


			_buttonItem1.transform.Find("Amount/Value").GetComponent<Text>().text = _amount[0].ToString();
			_buttonItem2.transform.Find("Amount/Value").GetComponent<Text>().text = _amount[1].ToString();
			_buttonItem3.transform.Find("Amount/Value").GetComponent<Text>().text = _amount[2].ToString();
			_buttonItem4.transform.Find("Amount/Value").GetComponent<Text>().text = _amount[3].ToString();

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
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
        }
        private void BackButtonhandler()
		{
            GameClient.Get<IAppStateManager>().BackAppState();
		}
		private void BuyButtonHandler()
		{
            _playerManager.LocalUser.wallet -= _costs[_currentPackId];
			_wallet.text = _playerManager.LocalUser.wallet.ToString("0.00") + " $";

            for (int i = 0; i < _amount[_currentPackId]; i++)
            {
                _playerManager.LocalUser.packsCount++;

                GameObject packItem = MonoBehaviour.Instantiate(_packsObjects[_currentPackId]) as GameObject;
                packItem.transform.position = _packsObjects[_currentPackId].transform.position;
                packItem.transform.SetParent(_selfPage.transform, true);
                packItem.transform.localScale = Vector3.one;
                packItem.transform.Find("Highlight").gameObject.SetActive(false);
                packItem.GetComponent<Button>().interactable = false;

                Sequence animationSequence = DOTween.Sequence();
                animationSequence.AppendInterval(0.1f * i);
                animationSequence.Append(packItem.transform.DOMove(Vector3.up * -10, .3f));
            }
        }

        private void ChooseItemHandler(int id)
        {
            if (_currentPackId == id)
                return;
            Vector3 pos;
            if (_currentPackId > -1)
            {
                pos = _packsObjects[_currentPackId].transform.position;
                pos.y = _itemYstartPos;
                _packsObjects[_currentPackId].transform.Find("Highlight").GetComponent<Image>().DOFade(0f, 0.3f);
            }
            _currentPackId = id;
            //_description = "";
            _cost.text = _costs[_currentPackId] + " $";
            pos = _packsObjects[_currentPackId].GetComponent<RectTransform>().position;
            pos.y = _itemYstartPos - 1;
            _packsObjects[_currentPackId].transform.Find("Highlight").GetComponent<Image>().DOFade(0.8f, 0.3f);

        }

        #endregion

        private void OpenAlertDialog(string msg)
		{
			_uiManager.DrawPopup<WarningPopup>(msg);
		}
    }
}