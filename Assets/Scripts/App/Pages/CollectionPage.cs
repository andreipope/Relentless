using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using TMPro;
using DG.Tweening;
using GrandDevs.Internal;

namespace GrandDevs.CZB
{
    public class CollectionPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;
		private IDataManager _dataManager;

        private GameObject _selfPage;

        private MenuButtonNoGlow _buttonBuy,
                                _buttonOpen,
                                _buttonBack;
        private Button  _buttonArrowLeft,
                        _buttonArrowRight;

		public List<Transform> cardPositions;

        public GameObject _cardCreaturePrefab,
                          _cardSpellPrefab,
                          _cardPlaceholdersPrefab,
                          _cardPlaceholders;

        private TextMeshProUGUI gooValueText;

        private Slider _cardSetsSlider;

		private int numPages;
		private int currentPage;

        private int numSets;
        private int currentSet;

        private CardView _selectedCard;
        private CardView _selectedCollectionCard;

        private bool _isPopupChangedStart;

        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();
			_dataManager = GameClient.Get<IDataManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/CollectionPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            gooValueText = _selfPage.transform.Find("GooValue/Value").GetComponent<TextMeshProUGUI>();

			_buttonBuy = _selfPage.transform.Find("BuyButton").GetComponent<MenuButtonNoGlow>();
			_buttonOpen = _selfPage.transform.Find("OpenButton").GetComponent<MenuButtonNoGlow>();
			_buttonBack = _selfPage.transform.Find("Panel_Header/BackButton").GetComponent<MenuButtonNoGlow>();
            _buttonArrowLeft = _selfPage.transform.Find("ArrowLeftButton").GetComponent<Button>();
            _buttonArrowRight = _selfPage.transform.Find("ArrowRightButton").GetComponent<Button>();

            _cardSetsSlider = _selfPage.transform.Find("Panel_Header/Elements").GetComponent<Slider>();

            _buttonBuy.onClickEvent.AddListener(BuyButtonHandler);
            _buttonOpen.onClickEvent.AddListener(OpenButtonHandler);
            _buttonBack.onClickEvent.AddListener(BackButtonHandler);
            _buttonArrowLeft.onClick.AddListener(ArrowLeftButtonHandler);
            _buttonArrowRight.onClick.AddListener(ArrowRightButtonHandler);

            _cardSetsSlider.onValueChanged.AddListener(CardSetsSliderOnValueChangedHandler);

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CreatureCard");
			_cardSpellPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/SpellCard");
			_cardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholders");

            Hide();
        }

        public void Update()
        {
            if (_selfPage.activeInHierarchy)
            {
                if (!_uiManager.GetPopup<CardInfoPopup>().Self.activeSelf && !_uiManager.GetPopup<DesintigrateCardPopup>().Self.activeSelf)
                {
                    if (!_isPopupChangedStart && _selectedCard != null)
                    {
                        ClosePopupInfo();
                    }
                    if (!_isPopupChangedStart && Input.GetMouseButtonDown(0))
                    {
                        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        var hit = Physics2D.Raycast(mousePos, Vector2.zero);
                        if (hit.collider != null)
                        {
                            foreach (var card in MonoBehaviour.FindObjectsOfType<CardView>())
                            {
                                if (hit.collider.gameObject == card.gameObject)
                                {
                                    CardSelected(card);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Show()
        {
            //_uiManager.Canvas.GetComponent<Canvas>().worldCamera = GameObject.Find("Camera2").GetComponent<Camera>();
            gooValueText.text = GameClient.Get<IPlayerManager>().LocalUser.gooValue.ToString();

            _selfPage.SetActive(true);
            InitObjects();
		}

        public void Hide()
        {
            _selfPage.SetActive(false);
            MonoBehaviour.Destroy(_cardPlaceholders);
            foreach (var card in MonoBehaviour.FindObjectsOfType<CardView>())
            {
                MonoBehaviour.Destroy(card.gameObject);
            }
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(_cardPlaceholders);
            foreach (var card in MonoBehaviour.FindObjectsOfType<CardView>())
            {
                MonoBehaviour.Destroy(card.gameObject);
            }
        }

        private void InitObjects()
        {
			_cardPlaceholders = MonoBehaviour.Instantiate(_cardPlaceholdersPrefab);
			cardPositions = new List<Transform>();
			foreach (Transform placeholder in _cardPlaceholders.transform)
			{
				cardPositions.Add(placeholder);
			}
            //pageText.text = "Page " + (currentPage + 1) + "/" + numPages;

            numSets = _dataManager.CachedCardsLibraryData.sets.Count - 1; //1 - tutorial
            numPages = Mathf.CeilToInt(_dataManager.CachedCardsLibraryData.sets[currentSet].cards.Count / (float)cardPositions.Count);

            _cardSetsSlider.value = 0;
            LoadCards(0, 0);
        }

        private void ClosePopupInfo()
        {
            ChangeStatePopup(true);

			var amount = _dataManager.CachedCollectionData.GetCardData(_selectedCollectionCard.libraryCard.id).amount;
			_selectedCollectionCard.UpdateAmount(amount);

            UpdateGooValue();

			Sequence sequence = DOTween.Sequence();
            sequence.Append(_selectedCard.transform.DOScale(_selectedCollectionCard.transform.localScale, .3f));
			sequence.Join(_selectedCard.transform.DOMove(_selectedCollectionCard.transform.position, .3f));
			sequence.Join(_selectedCard.transform.DORotate(_selectedCollectionCard.transform.eulerAngles, .3f));
			sequence.OnComplete(() =>
			{
				MonoBehaviour.Destroy(_selectedCard.gameObject);
				_selectedCard = null;
                ChangeStatePopup(false);
            });
        }

        public void UpdateGooValue()
        {
            gooValueText.text = GameClient.Get<IPlayerManager>().LocalUser.gooValue.ToString();
        }

#region Buttons Handlers

        private void CardSelected(CardView card)
        {
            ChangeStatePopup(true);
            _selectedCollectionCard = card;
            _selectedCard = MonoBehaviour.Instantiate(card.gameObject).GetComponent<CardView>();
            _selectedCard.name = "CardPreview";
            Utilites.SetLayerRecursively(_selectedCard.gameObject, 11);

			Sequence mySequence = DOTween.Sequence();
			mySequence.Append(_selectedCard.transform.DORotate(new Vector3(-20, 30, -20), .2f));
			mySequence.Append(_selectedCard.transform.DORotate(new Vector3(0, 0, 0), .4f));

			Sequence mySequence2 = DOTween.Sequence();
			mySequence2.Append(_selectedCard.transform.DOMove(new Vector3(-4.3f, 1.2f, 5), .4f));
			mySequence2.Append(_selectedCard.transform.DOMove(new Vector3(-4.3f, .8f, 5), .2f));

			Sequence mySequence3 = DOTween.Sequence();
			mySequence3.Append(_selectedCard.transform.DOScale(new Vector3(.9f, .9f, .9f), .4f));
			mySequence3.Append(_selectedCard.transform.DOScale(new Vector3(.72f, .72f, .72f), .2f));
            mySequence3.OnComplete(() =>
            {
                ChangeStatePopup(false);
            });


			_uiManager.DrawPopup<CardInfoPopup>(card.libraryCard);
            (_uiManager.GetPopup<CardInfoPopup>() as CardInfoPopup).cardTransform = _selectedCard.transform;
        }

        private void ChangeStatePopup(bool isStart)
        {
            _isPopupChangedStart = isStart;
            _cardSetsSlider.interactable = !isStart;
            _buttonBuy.interactable = !isStart;
            _buttonOpen.interactable = !isStart;
            _buttonArrowLeft.interactable = !isStart;
            _buttonArrowRight.interactable = !isStart;
            _buttonBack.interactable = !isStart;
        }

        private void CardSetsSliderOnValueChangedHandler(float value)
        {
            currentPage = 0;
            currentSet = (int)value;
            LoadCards(0, (int)value);
        }

        private void BuyButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.SHOP);
        }
		private void OpenButtonHandler()
		{
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
        }
		private void BackButtonHandler()
		{
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
		}

		private void ArrowLeftButtonHandler()
		{
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK);
            MoveCardsPage(-1);
		}

        private void ArrowRightButtonHandler()
		{
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK);
            MoveCardsPage(1);
		}


		#endregion

		public void MoveCardsPage(int direction)
		{
            currentPage += direction;

			if (currentPage < 0)
			{
                currentSet += direction;

                if (currentSet < 0)
                {
                    currentSet = numSets - 1;
                    currentPage = numPages - 1;
                }
                else
                {
                    currentPage = numPages - 1;
                    currentPage = currentPage < 0 ? 0 : currentPage;
                }                   
            }
            else if (currentPage >= numPages)
			{
                currentSet += direction;

                if (currentSet >= numSets)
                {
                    currentSet = 0;
                    currentPage = 0;
                }
                else
                {
                    currentPage = 0;
                }
            }

            numPages = Mathf.CeilToInt(_dataManager.CachedCardsLibraryData.sets[currentSet].cards.Count / (float)cardPositions.Count);
            _cardSetsSlider.value = currentSet;

			LoadCards(currentPage, currentSet);
		}

		public void OnNextPageButtonPressed()
		{
			
		}

		public void LoadCards(int page, int setIndex)
		{
            CorrectSetIndex(ref setIndex);

            var set = _dataManager.CachedCardsLibraryData.sets[setIndex];
            var cards = set.cards;

			var startIndex = page * cardPositions.Count;

			var endIndex = Mathf.Min(startIndex + cardPositions.Count, cards.Count);

			foreach (var card in MonoBehaviour.FindObjectsOfType<CardView>())
			{
				MonoBehaviour.Destroy(card.gameObject);
			}

			for (var i = startIndex; i < endIndex; i++)
			{
                if (i >= cards.Count)
                    break;

				var card = cards[i];

                GameObject go = null;
				if ((Enumerators.CardKind)card.cardKind == Enumerators.CardKind.CREATURE)
				{
					go = MonoBehaviour.Instantiate(_cardCreaturePrefab as GameObject);
				}
				else if ((Enumerators.CardKind)card.cardKind == Enumerators.CardKind.SPELL)
				{
					go = MonoBehaviour.Instantiate(_cardSpellPrefab as GameObject);
				}
              
                var cardView = go.GetComponent<CardView>();
                var amount = _dataManager.CachedCollectionData.GetCardData(card.id).amount;
                cardView.PopulateWithLibraryInfo(card, set.name, amount);
				cardView.SetHighlightingEnabled(false);
				cardView.transform.position = cardPositions[i % cardPositions.Count].position;
                cardView.transform.localScale = Vector3.one * 0.35f;
                cardView.GetComponent<SortingGroup>().sortingLayerName = "Default";
				cardView.GetComponent<SortingGroup>().sortingOrder = 1;
			}
		}

		private void OpenAlertDialog(string msg)
		{
			_uiManager.DrawPopup<WarningPopup>(msg);
		}

		public void SetActive(int id, bool active)
		{

		}

        private void CorrectSetIndex(ref int index)
        {
            switch (index)
            {
                case 0:
                    index = 3;
                    break;
                case 1:
                    index = 4;
                    break;
                case 2:
                    index = 1;
                    break;
                case 3:
                    index = 5;
                    break;
                case 4:
                    index = 0;
                    break;
                case 5:
                    index = 2;
                    break;
                case 6:
                    break;
                default:
                    break;
            }
        }
    }
}
