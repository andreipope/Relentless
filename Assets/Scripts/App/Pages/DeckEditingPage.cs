using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using System.IO;
using FullSerializer;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class DeckEditingPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IDataManager _dataManager;

        private GameObject _selfPage;

        private TMP_InputField _deckNameInputField;

        private MenuButtonNoGlow _buttonBack,
                                _buttonBuy,
                                _buttonOpen,
                                _buttonSave,
                                _buttonArrowLeft,
                                _buttonArrowRight;

        private ScrollRect _cardsListScrollRect;

        private TMP_Text _cardAmountText;

        private Deck _currentDeck;

        private fsSerializer serializer = new fsSerializer();

        private Slider _cardSetsSlider;

        private int numPages;
        private int currentPage;

        private int numSets;
        private int currentSet;

        private List<Transform> cardPositions;

        private GameObject _cardCreaturePrefab;
        private GameObject _cardSpellPrefab;
        private GameObject _cardPlaceholdersPrefab;
        private GameObject _cardPlaceholders;
        private GameObject _cardListItemPrefab;
        private GameObject _cardListContent;

        private GameObject _backgroundCanvasPrefab,
                           _backgroundCanvas;

        private int _currentDeckId;
        private int _currentHeroId;

        public int CurrentDeckId
        {
            set { _currentDeckId = value; }
        }
        public int CurrentHeroId
        {
            set { _currentHeroId = value; }
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/DeckEditingPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CreatureCard");
            _cardSpellPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/SpellCard");
            _cardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersEditingDeck");
            _cardListItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/CardListItem");
            _backgroundCanvasPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/BackgroundEditingCanvas");

            _cardAmountText = _selfPage.transform.Find("CardsAmount/CardsAmountText").GetComponent<TMP_Text>();

            _deckNameInputField = _selfPage.transform.Find("DeckTitleInputText").GetComponent<TMP_InputField>();

            _cardSetsSlider = _selfPage.transform.Find("Elements").GetComponent<Slider>();

            _buttonBack = _selfPage.transform.Find("Button_Back").GetComponent<MenuButtonNoGlow>();
            _buttonBuy = _selfPage.transform.Find("Button_Buy").GetComponent<MenuButtonNoGlow>();
            _buttonOpen = _selfPage.transform.Find("Button_Open").GetComponent<MenuButtonNoGlow>();
            _buttonSave = _selfPage.transform.Find("Button_Save").GetComponent<MenuButtonNoGlow>();
            _buttonArrowLeft = _selfPage.transform.Find("ArrowLeftButton").GetComponent<MenuButtonNoGlow>();
            _buttonArrowRight = _selfPage.transform.Find("ArrowRightButton").GetComponent<MenuButtonNoGlow>();

            _cardSetsSlider.onValueChanged.AddListener(CardSetsSliderOnValueChangedHandler);

            _cardListContent = _selfPage.transform.Find("Panel_CardsList/Group").gameObject;

            _cardsListScrollRect = _selfPage.transform.Find("Panel_CardsList").GetComponent<ScrollRect>();

            _buttonBack.onClickEvent.AddListener(BackButtonHandler);
            _buttonBuy.onClickEvent.AddListener(BuyButtonHandler);
            _buttonOpen.onClickEvent.AddListener(OpenButtonHandler);
            _buttonSave.onClickEvent.AddListener(SaveButtonHandler);
            _buttonArrowLeft.onClickEvent.AddListener(ArrowLeftButtonHandler);
            _buttonArrowRight.onClickEvent.AddListener(ArrowRightButtonHandler);

            _deckNameInputField.onEndEdit.AddListener(OnDeckNameInputFieldEndedEdit);

            Hide();
        }


        public void Update()
        {
            if (_selfPage.activeInHierarchy)
            {
                UpdateNumCardsText();
            }
        }

        public void Show()
        {
            _selfPage.SetActive(true);
            if (_currentDeckId == -1)
            {
				_currentDeck = new Deck();
                _currentDeck.name = "Deck " + _dataManager.CachedDecksData.decks.Count;
                _currentDeck.cards = new List<DeckCardData>();

            }
            else
                _currentDeck = _dataManager.CachedDecksData.decks[_currentDeckId];
            InitObjects();
            LoadDeckInfo(_currentDeck);

            _cardsListScrollRect.verticalNormalizedPosition = 1f;
            _cardsListScrollRect.CalculateLayoutInputVertical();
            _uiManager.Canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
            Dispose();
        }

        public void Dispose()
        {
            foreach (var card in MonoBehaviour.FindObjectsOfType<CardView>())
            {
                MonoBehaviour.Destroy(card.gameObject);
            }
            MonoBehaviour.Destroy(_cardPlaceholders);
            MonoBehaviour.Destroy(_backgroundCanvas);
            _uiManager.Canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        private void InitObjects()
        {
            _cardPlaceholders = MonoBehaviour.Instantiate(_cardPlaceholdersPrefab);
            _backgroundCanvas = MonoBehaviour.Instantiate(_backgroundCanvasPrefab);
            _backgroundCanvas.GetComponent<Canvas>().worldCamera = Camera.allCameras[0];
            cardPositions = new List<Transform>();
            foreach (Transform placeholder in _cardPlaceholders.transform)
            {
                cardPositions.Add(placeholder);
            }

            numSets = _dataManager.CachedCardsLibraryData.sets.Count;
            numPages = Mathf.CeilToInt(_dataManager.CachedCardsLibraryData.sets[currentSet].cards.Count / (float)cardPositions.Count);

            _cardSetsSlider.value = 0;
            LoadCards(0, 0);
        }

        #region button handlers

        private void CardSetsSliderOnValueChangedHandler(float value)
        {
            currentPage = 0;
            currentSet = (int)value;
            LoadCards(0, (int)value);
        }

        private void BackButtonHandler()
        {
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
        }

        private void BuyButtonHandler()
        {
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.SHOP);
        }
        private void OpenButtonHandler()
        {
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
        }
        private void SaveButtonHandler()
        {
            OnDoneButtonPressed();
        }

        private void ArrowLeftButtonHandler()
        {
            MoveCardsPage(-1);
        }

        private void ArrowRightButtonHandler()
        {
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
                    currentSet = 0;
                    currentPage = 0;
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
                    currentSet = numSets - 1;
                    currentPage = numPages - 1;

                    currentPage = currentPage < 0 ? 0 : currentPage;
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

        public void LoadCards(int page, int setIndex)
        {
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
                if ((Enumerators.CardKind)card.cardTypeId == Enumerators.CardKind.CREATURE)
                {
                    go = MonoBehaviour.Instantiate(_cardCreaturePrefab as GameObject);
                }
                else if ((Enumerators.CardKind)card.cardTypeId == Enumerators.CardKind.SPELL)
                {
                    go = MonoBehaviour.Instantiate(_cardSpellPrefab as GameObject);
                }

                var amount = _dataManager.CachedCollectionData.GetCardData(card.id).amount;

                var cardView = go.GetComponent<CardView>();
                cardView.PopulateWithLibraryInfo(card, set.name, amount);
                cardView.SetHighlightingEnabled(false);
                cardView.transform.position = cardPositions[i % cardPositions.Count].position;
                cardView.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                cardView.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
                cardView.GetComponent<SpriteRenderer>().sortingOrder = 1;
                cardView.GetComponent<SortingGroup>().sortingLayerName = "Default";
                cardView.GetComponent<SortingGroup>().sortingOrder = 1;

                var deckBuilderCard = go.AddComponent<DeckBuilderCard>();
                deckBuilderCard.scene = this;
                deckBuilderCard.card = card;
            }
        }

        public void OnDeckNameInputFieldEndedEdit(string value)
        {
            _currentDeck.name = value;
        }

        public void LoadDeckInfo(Deck deck)
        {
            _deckNameInputField.text = deck.name;

            foreach (var item in _cardListContent.GetComponentsInChildren<CardListItem>())
            {
                MonoBehaviour.Destroy(item.gameObject);
            }

            foreach (var card in deck.cards)
            {
                var libraryCard = _dataManager.CachedCardsLibraryData.GetCard(card.cardId);
                var go = MonoBehaviour.Instantiate(_cardListItemPrefab) as GameObject;
                go.transform.SetParent(_cardListContent.transform, false);
                go.GetComponent<CardListItem>().deckButton = deck;
                go.GetComponent<CardListItem>().card = libraryCard;
                go.GetComponent<CardListItem>().cardNameText.text = libraryCard.name;
                go.GetComponent<CardListItem>().cardAmountText.text = "x" + card.amount.ToString();
                go.GetComponent<CardListItem>().count = card.amount;
            }       

            UpdateNumCardsText();
        }

        public void AddCardToDeck(Card card)
        {
            if (_currentDeck == null)
            {
                return;
            }

            var existingCards = _currentDeck.cards.Find(x => x.cardId == card.id);
            var maxCopies = Constants.CARD_MAX_COPIES;
            if (existingCards != null && existingCards.amount == maxCopies)
            {
                OpenAlertDialog("You cannot have more than " + maxCopies + " copies of this card in your deck.");
                return;
            }

			var maxDeckSize = Constants.DECK_MAX_SIZE;
			if (_currentDeck.GetNumCards() == maxDeckSize)
			{
				OpenAlertDialog("Your '" + _currentDeck.name + "' deck has more than " + maxDeckSize + " cards.");
				return;
			}

            var itemFound = false;
            foreach (var item in _cardListContent.GetComponentsInChildren<CardListItem>())
            {
                if (item.card == card)
                {
                    itemFound = true;
                    item.AddCard();
                    break;
                }
            }

            if (!itemFound)
            {
                var go = MonoBehaviour.Instantiate(_cardListItemPrefab) as GameObject;
                go.transform.SetParent(_cardListContent.transform, false);
                go.GetComponent<CardListItem>().deckButton = _currentDeck;
                go.GetComponent<CardListItem>().card = card;
                go.GetComponent<CardListItem>().cardNameText.text = card.name;
            }

            _currentDeck.AddCard(card.id); 
        }

        public void OnClearAllButtonPressed()
        {
            _currentDeck.cards.Clear();
            foreach (var item in _cardListContent.GetComponentsInChildren<CardListItem>())
            {
               MonoBehaviour.Destroy(item.gameObject);
            }
        }

        public void UpdateNumCardsText()
        {
            if (_currentDeck != null)
            {
                _cardAmountText.text = _currentDeck.GetNumCards().ToString() + "/" + Constants.DECK_MAX_SIZE;
            }
        }

        public void OnDoneButtonPressed()
        {
            if (_currentDeckId == -1)
            {
                _currentDeck.heroId = _currentHeroId;
                _dataManager.CachedDecksData.decks.Add(_currentDeck);
            }

            _dataManager.SaveCache(Enumerators.CacheDataType.DECKS_DATA);

            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
        }
    }
}
