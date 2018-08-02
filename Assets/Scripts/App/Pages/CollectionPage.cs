// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using System.Linq;
using LoomNetwork.CZB.Common;
using TMPro;
using DG.Tweening;
using LoomNetwork.Internal;

namespace LoomNetwork.CZB
{
	public class CollectionPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;
		private IDataManager _dataManager;

        private GameObject _selfPage;

        private Button _buttonBuy,
                                _buttonOpen,
                                _buttonBack;
        private Button  _buttonArrowLeft,
                        _buttonArrowRight;

		private TextMeshProUGUI _cardCounter;

		public List<Transform> cardPositions;

        public GameObject _cardCreaturePrefab,
                          _cardSpellPrefab,
                          _cardPlaceholdersPrefab,
                          _cardPlaceholders;

        private TextMeshProUGUI gooValueText;

		private GameObject _cardSetsIcons;

		private int numPages;
		private int currentPage;

        private int numSets;
        private int currentSet;


        private CardInfoPopupHandler _cardInfoPopupHandler;

        private List<BoardCard> _createdBoardCards;

        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();
			_dataManager = GameClient.Get<IDataManager>();

            _cardInfoPopupHandler = new CardInfoPopupHandler();
            _cardInfoPopupHandler.Init();
            _cardInfoPopupHandler.StateChanging += () => ChangeStatePopup(_cardInfoPopupHandler.IsStateChanging);
            _cardInfoPopupHandler.StateChanged += () => ChangeStatePopup(_cardInfoPopupHandler.IsStateChanging);
            _cardInfoPopupHandler.Closing += UpdateGooValue;

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/CollectionPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            gooValueText = _selfPage.transform.Find("GooValue/Value").GetComponent<TextMeshProUGUI>();

			_buttonBuy = _selfPage.transform.Find("BuyButton").GetComponent<Button>();
			_buttonOpen = _selfPage.transform.Find("OpenButton").GetComponent<Button>();
			_buttonBack = _selfPage.transform.Find("Panel_Header/BackButton").GetComponent<Button>();
            _buttonArrowLeft = _selfPage.transform.Find("ArrowLeftButton").GetComponent<Button>();
            _buttonArrowRight = _selfPage.transform.Find("ArrowRightButton").GetComponent<Button>();

			_cardCounter = _selfPage.transform.Find ("CardsCounter").GetChild (0).GetComponent<TextMeshProUGUI> ();

			_cardSetsIcons = _selfPage.transform.Find ("Panel_Header/Icons").gameObject;

            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonOpen.onClick.AddListener(OpenButtonHandler);
            _buttonBack.onClick.AddListener(BackButtonHandler);
            _buttonArrowLeft.onClick.AddListener(ArrowLeftButtonHandler);
            _buttonArrowRight.onClick.AddListener(ArrowRightButtonHandler);

			Button[] iconButtons = _cardSetsIcons.GetComponentsInChildren<Button> ();
			foreach (Button item in iconButtons) {
				item.onClick.AddListener (delegate {
					iconSetButtonClick (item);
				});
			}

            //_cardSetsSlider.onValueChanged.AddListener(CardSetsSliderOnValueChangedHandler);

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
			_cardSpellPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/SpellCard");
			_cardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholders");



            _createdBoardCards = new List<BoardCard>();

            Hide();
        }

        public void Update()
        {
            if (_selfPage.activeInHierarchy)
            {
                _cardInfoPopupHandler.Update();
                if (_cardInfoPopupHandler.IsInteractable)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        var hit = Physics2D.Raycast(mousePos, Vector2.zero);
                        if (hit.collider != null)
                        {
                            for(int i =0;i < _createdBoardCards.Count; i++)
                            {
                                if (hit.collider.gameObject == _createdBoardCards[i].gameObject)
                                {
                                    _cardInfoPopupHandler.SelectCard(_createdBoardCards[i]);
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
            gooValueText.text = GameClient.Get<IPlayerManager>().GetGoo().ToString();

            _selfPage.SetActive(true);
            InitObjects();
		}

        public void Hide()
        {
            _selfPage.SetActive(false);
            MonoBehaviour.Destroy(_cardPlaceholders);
            ResetBoardCards();
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(_cardPlaceholders);
            ResetBoardCards();
            _cardInfoPopupHandler.Dispose();
        }

        private void ResetBoardCards()
        {
            foreach (var item in _createdBoardCards)
                item.Dispose();
            _createdBoardCards.Clear();
        }

		private void iconSetButtonClick (Button toggleObj) {
			currentSet = toggleObj.transform.GetSiblingIndex ();
			currentPage = 0;
			LoadCards (currentPage, currentSet);
		}

        private void InitObjects()
        {
			_cardPlaceholders = MonoBehaviour.Instantiate(_cardPlaceholdersPrefab);
			cardPositions = new List<Transform>();

			foreach (Transform placeholder in _cardPlaceholders.transform)
				cardPositions.Add(placeholder);
            //pageText.text = "Page " + (currentPage + 1) + "/" + numPages;

            numSets = _dataManager.CachedCardsLibraryData.sets.Count - 1;
            CalculateNumberOfPages();

            //_cardSetsSlider.value = 0;
            LoadCards(0, 0);

			_cardCounter.text = _dataManager.CachedCollectionData.cards.Count.ToString () + "/" + _dataManager.CachedCardsLibraryData.Cards.Count.ToString ();
        }



        public void UpdateGooValue()
        {
            gooValueText.text = GameClient.Get<IPlayerManager>().GetGoo().ToString();
        }

#region Buttons Handlers

        private void ChangeStatePopup(bool isStart)
        {
            //_cardSetsSlider.interactable = !isStart;
            _buttonBuy.interactable = !isStart;
            _buttonOpen.interactable = !isStart;
            _buttonArrowLeft.interactable = !isStart;
            _buttonArrowRight.interactable = !isStart;
            _buttonBack.interactable = !isStart;
        }

        private void CardSetsSliderOnValueChangedHandler(float value)
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);
            currentPage = 0;
            currentSet = (int)value;
            LoadCards(0, (int)value);
        }

        private void BuyButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.SHOP);
        }
		private void OpenButtonHandler()
		{
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
        }
		private void BackButtonHandler()
		{
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
		}

		private void ArrowLeftButtonHandler()
		{
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            MoveCardsPage(-1);
		}

        private void ArrowRightButtonHandler()
		{
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            MoveCardsPage(1);
		}


		#endregion

        public void MoveCardsPage(int direction)
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);

            currentPage += direction;

            if (currentPage < 0)
            {
                currentSet += direction;

                if (currentSet < 0)
                {
                    currentSet = numSets - 1;
                    CalculateNumberOfPages();
                    currentPage = numPages - 1;
                }
                else
                {
                    CalculateNumberOfPages();

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

            CalculateNumberOfPages();

            //_cardSetsSlider.value = currentSet;

            LoadCards(currentPage, currentSet);
        }

		private void highlightCorrectIcon () {
			for (int i = 0; i < _cardSetsIcons.transform.childCount; i++) {
				GameObject c = _cardSetsIcons.transform.GetChild (i).GetChild (0).gameObject;
				if (i == currentSet) {
					c.SetActive (true);
				} else {
					c.SetActive (false);
				}
			}
		}


        private void CalculateNumberOfPages()
        {
            numPages = Mathf.CeilToInt((float)_dataManager.CachedCardsLibraryData.sets[currentSet].cards.Count / (float)cardPositions.Count);
        }

        public void OnNextPageButtonPressed()
		{

		}

		public void LoadCards(int page, int setIndex)
		{
           // CorrectSetIndex(ref setIndex);

            var set = _dataManager.CachedCardsLibraryData.sets[setIndex];
            var cards = set.cards;

			var startIndex = page * cardPositions.Count;

			var endIndex = Mathf.Min(startIndex + cardPositions.Count, cards.Count);

            ResetBoardCards();

            for (var i = startIndex; i < endIndex; i++)
			{
                if (i >= cards.Count)
                    break;

				var card = cards[i];
                var cardData = _dataManager.CachedCollectionData.GetCardData(card.name);

                // hack !!!! CHECK IT!!!
                if (cardData == null)
                    continue;

                GameObject go = null;
                BoardCard boardCard = null;
                if ((Enumerators.CardKind)card.cardKind == Enumerators.CardKind.CREATURE)
                {
                    go = MonoBehaviour.Instantiate(_cardCreaturePrefab as GameObject);
                    boardCard = new UnitBoardCard(go);
                }
                else if ((Enumerators.CardKind)card.cardKind == Enumerators.CardKind.SPELL)
                {
                    go = MonoBehaviour.Instantiate(_cardSpellPrefab as GameObject);
                    boardCard = new SpellBoardCard(go);
                }



                var amount = cardData.amount;
                boardCard.Init(card, amount);
				boardCard.SetHighlightingEnabled(false);
				boardCard.transform.position = cardPositions[i % cardPositions.Count].position;
                boardCard.transform.localScale = Vector3.one * 0.32f;
                boardCard.gameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LAYER_DEFAULT;
                boardCard.gameObject.GetComponent<SortingGroup>().sortingOrder = 1;

                _createdBoardCards.Add(boardCard);
            }

			highlightCorrectIcon ();
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
