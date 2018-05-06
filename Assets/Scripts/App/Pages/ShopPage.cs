using UnityEngine;
using UnityEngine.UI;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using CCGKit;
using DG.Tweening;
using TMPro;

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

        private TextMeshProUGUI _description,
                     _costItem_1,
                     _costItem_2,
                     _costItem_3,
                     _costItem_4,
                    _wallet;

        private int _currentPackId = -1;

        private float[] _costs = new float[] { 1.99f, 2.99f, 4.99f, 9.99f };
        private int[] _amount = new int[] { 1, 2, 5, 10 };
        private GameObject[] _packsObjects;
        private Image[] _imageObjects;

        private float _itemYstartPos;
        private Color _deselectedColor;
        private Color _selectedColor;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _playerManager = GameClient.Get<IPlayerManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/ShopPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

			_description = _selfPage.transform.Find("BuyNowPanel/Description").GetComponent<TextMeshProUGUI>();
			_wallet = _selfPage.transform.Find("Wallet").GetComponent<TextMeshProUGUI>();
            _selectedColor = Color.white;
            _deselectedColor = new Color(0.5f, 0.5f, 0.5f);


            _buttonItem1 = _selfPage.transform.Find("Item1").GetComponent<Button>();
            _buttonItem2 = _selfPage.transform.Find("Item2").GetComponent<Button>();
            _buttonItem3 = _selfPage.transform.Find("Item3").GetComponent<Button>();
            _buttonItem4 = _selfPage.transform.Find("Item4").GetComponent<Button>();

            _costItem_1 = _buttonItem1.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
            _costItem_2 = _buttonItem2.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
            _costItem_3 = _buttonItem3.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
            _costItem_4 = _buttonItem4.transform.Find("Cost").GetComponent<TextMeshProUGUI>();

            _buttonBack = _selfPage.transform.Find("Image_Header/BackButton").GetComponent<MenuButtonNoGlow>();
            _buttonBuy = _selfPage.transform.Find("BuyNowPanel/Button_Buy").GetComponent<MenuButtonNoGlow>();

            _buttonItem1.onClick.AddListener(() => ChooseItemHandler(0));
            _buttonItem2.onClick.AddListener(() => ChooseItemHandler(1));
            _buttonItem3.onClick.AddListener(() => ChooseItemHandler(2));
            _buttonItem4.onClick.AddListener(() => ChooseItemHandler(3));

            _buttonBack.onClickEvent.AddListener(BackButtonhandler);
            _buttonBuy.onClickEvent.AddListener(BuyButtonHandler);

            _itemYstartPos = _buttonItem1.gameObject.transform.position.y;

            _packsObjects = new GameObject[] { _buttonItem1.gameObject,
                                        _buttonItem2.gameObject,
                                        _buttonItem3.gameObject,
                                        _buttonItem4.gameObject};

            _imageObjects = new Image[] { _buttonItem1.transform.Find("Image").GetComponent<Image>(),
                                        _buttonItem2.transform.Find("Image").GetComponent<Image>(),
                                        _buttonItem3.transform.Find("Image").GetComponent<Image>(),
                                        _buttonItem4.transform.Find("Image").GetComponent<Image>()
            };

            foreach(Image img in _imageObjects)
            {
                img.color = _deselectedColor;
            }

            Hide();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _playerManager.LocalUser.wallet = 1000;
            _wallet.text = _playerManager.LocalUser.wallet.ToString("0.00") + " $";
            if (_currentPackId > -1)
            {
                _packsObjects[_currentPackId].transform.Find("Highlight").GetComponent<Image>().DOFade(0f, 0f);
                _currentPackId = -1;
            }

            _costItem_1.text = "$ " + _costs[0];
            _costItem_2.text = "$ " + _costs[1];
            _costItem_3.text = "$ " + _costs[2];
            _costItem_4.text = "$ " + _costs[3];
            _buttonItem1.transform.Find("Text_Value").GetComponent<Text>().text = "x" + _amount[0].ToString();
            _buttonItem2.transform.Find("Text_Value").GetComponent<Text>().text = "x" + _amount[1].ToString();
            _buttonItem3.transform.Find("Text_Value").GetComponent<Text>().text = "x" + _amount[2].ToString();
            _buttonItem4.transform.Find("Text_Value").GetComponent<Text>().text = "x" + _amount[3].ToString();

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
            GameObject prefab = null;
            for (int i = 0; i < _amount[_currentPackId]; i++)
            {
                _playerManager.LocalUser.packsCount++;

                prefab = _packsObjects[_currentPackId].transform.Find("PackItemPrefab").gameObject;

                GameObject packItem = MonoBehaviour.Instantiate(prefab) as GameObject;
                packItem.transform.position = prefab.transform.position;
                packItem.transform.SetParent(_selfPage.transform, true);
                packItem.transform.localScale = Vector3.one;
				packItem.SetActive(true);


				Sequence animationSequence = DOTween.Sequence();
                animationSequence.AppendInterval(_amount[_currentPackId] * 0.1f - 0.1f * i);
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
           for(int i= 0; i < _imageObjects.Length; i++)
            {
                if(i == _currentPackId)
                {
                    _imageObjects[i].color = _selectedColor;
                }
                else
                {
                    _imageObjects[i].color = _deselectedColor;
                }
            }
            //_description = "";
            //_cost.text = _costs[_currentPackId] + " $";
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