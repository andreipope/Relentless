using DG.Tweening;
using LoomNetwork.CZB.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class ShopPage : IUIElement
    {
        private readonly float[] _costs =
        {
            1.99f, 2.99f, 4.99f, 9.99f
        };

        private readonly int[] _amount =
        {
            1, 2, 5, 10
        };

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IPlayerManager _playerManager;

        private GameObject _selfPage;

        private Button _buttonItem1, _buttonItem2, _buttonItem3, _buttonItem4;

        private Button _buttonOpen, _buttonCollection, _buttonBack, _buttonBuy;

        private TextMeshProUGUI _costItem1, _costItem2, _costItem3, _costItem4, _wallet;

        private int _currentPackId = -1;

        private GameObject[] _packsObjects;

        private Image[] _imageObjects;

        private Color _deselectedColor;

        private Color _selectedColor;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _playerManager = GameClient.Get<IPlayerManager>();

            _selectedColor = Color.white;
            _deselectedColor = new Color(0.5f, 0.5f, 0.5f);

            Hide();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/ShopPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _wallet = _selfPage.transform.Find("Wallet").GetComponent<TextMeshProUGUI>();

            _buttonItem1 = _selfPage.transform.Find("Item1").GetComponent<Button>();
            _buttonItem2 = _selfPage.transform.Find("Item2").GetComponent<Button>();
            _buttonItem3 = _selfPage.transform.Find("Item3").GetComponent<Button>();
            _buttonItem4 = _selfPage.transform.Find("Item4").GetComponent<Button>();

            _costItem1 = _buttonItem1.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
            _costItem2 = _buttonItem2.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
            _costItem3 = _buttonItem3.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
            _costItem4 = _buttonItem4.transform.Find("Cost").GetComponent<TextMeshProUGUI>();

            _buttonBack = _selfPage.transform.Find("Image_Header/BackButton").GetComponent<Button>();
            _buttonBuy = _selfPage.transform.Find("BuyNowPanel/Button_Buy").GetComponent<Button>();
            _buttonOpen = _selfPage.transform.Find("Button_Open").GetComponent<Button>();
            _buttonCollection = _selfPage.transform.Find("Button_Collection").GetComponent<Button>();

            _buttonItem1.onClick.AddListener(() => ChooseItemHandler(0));
            _buttonItem2.onClick.AddListener(() => ChooseItemHandler(1));
            _buttonItem3.onClick.AddListener(() => ChooseItemHandler(2));
            _buttonItem4.onClick.AddListener(() => ChooseItemHandler(3));

            _buttonBack.onClick.AddListener(BackButtonhandler);
            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonOpen.onClick.AddListener(OpenButtonHandler);
            _buttonCollection.onClick.AddListener(CollectionButtonHandler);

            _packsObjects = new[]
            {
                _buttonItem1.gameObject, _buttonItem2.gameObject, _buttonItem3.gameObject, _buttonItem4.gameObject
            };

            _imageObjects = new[]
            {
                _buttonItem1.transform.Find("Image").GetComponent<Image>(),
                _buttonItem2.transform.Find("Image").GetComponent<Image>(),
                _buttonItem3.transform.Find("Image").GetComponent<Image>(),
                _buttonItem4.transform.Find("Image").GetComponent<Image>()
            };

            foreach (Image img in _imageObjects)
            {
                img.color = _deselectedColor;
            }

            _playerManager.LocalUser.Wallet = 1000;
            _wallet.text = _playerManager.LocalUser.Wallet.ToString("0.00") + " $";
            if (_currentPackId > -1)
            {
                _packsObjects[_currentPackId].transform.Find("Highlight").GetComponent<Image>().DOFade(0f, 0f);
                _currentPackId = -1;
            }

            _costItem1.text = "$ " + _costs[0];
            _costItem2.text = "$ " + _costs[1];
            _costItem3.text = "$ " + _costs[2];
            _costItem4.text = "$ " + _costs[3];
            _buttonItem1.transform.Find("Text_Value").GetComponent<Text>().text = "x" + _amount[0];
            _buttonItem2.transform.Find("Text_Value").GetComponent<Text>().text = "x" + _amount[1];
            _buttonItem3.transform.Find("Text_Value").GetComponent<Text>().text = "x" + _amount[2];
            _buttonItem4.transform.Find("Text_Value").GetComponent<Text>().text = "x" + _amount[3];

            _selfPage.SetActive(true);
        }

        public void Hide()
        {
            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
        }

        public void Dispose()
        {
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        #region Buttons Handlers

        public void OpenButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.PACK_OPENER);
        }

        public void CollectionButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.COLLECTION);
        }

        private void BackButtonhandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IAppStateManager>().BackAppState();
        }

        private void BuyButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            if (_currentPackId >= _costs.Length || _currentPackId < 0)
            {
                Debug.LogError("No pack chosen");
                return;
            }

            _playerManager.LocalUser.Wallet -= _costs[_currentPackId];
            _wallet.text = _playerManager.LocalUser.Wallet.ToString("0.00") + " $";
            GameObject prefab;
            for (int i = 0; i < _amount[_currentPackId]; i++)
            {
                _playerManager.LocalUser.PacksCount++;

                prefab = _packsObjects[_currentPackId].transform.Find("PackItemPrefab").gameObject;

                GameObject packItem = Object.Instantiate(prefab);
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

            if (_currentPackId > -1)
            {
                _packsObjects[_currentPackId].transform.Find("Highlight").GetComponent<Image>().DOFade(0f, 0.3f);
            }

            _currentPackId = id;
            for (int i = 0; i < _imageObjects.Length; i++)
            {
                if (i == _currentPackId)
                {
                    _imageObjects[i].color = _selectedColor;
                }
                else
                {
                    _imageObjects[i].color = _deselectedColor;
                }
            }

            _packsObjects[_currentPackId].transform.Find("Highlight").GetComponent<Image>().DOFade(0.8f, 0.3f);
        }

        #endregion

    }
}
