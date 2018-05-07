using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using System.IO;
using FullSerializer;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;
using GrandDevs.Internal;

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
                                _buttonCollection;
        private Button _buttonArrowLeft,
                       _buttonArrowRight;

        private ScrollRect _cardsListScrollRect;

        private TMP_Text _cardAmountText;

        private TextMeshProUGUI _currentSetPageCountText;

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

        private CollectionData _collectionData;

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

        private List<DeckBuilderCard> _currentCards;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _collectionData = new CollectionData();
            _collectionData.cards = new List<CollectionCardData>();
            _currentCards = new List<DeckBuilderCard>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/DeckEditingPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CreatureCard");
            _cardSpellPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/SpellCard");
            _cardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersEditingDeck");
            _cardListItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/CardListItem");
            //_backgroundCanvasPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/BackgroundEditingCanvas");

            _cardAmountText = _selfPage.transform.Find("CardsAmount/CardsAmountText").GetComponent<TMP_Text>();

            _deckNameInputField = _selfPage.transform.Find("DeckTitleInputText").GetComponent<TMP_InputField>();

            _cardSetsSlider = _selfPage.transform.Find("Panel_Header/Elements").GetComponent<Slider>();

            _buttonBack = _selfPage.transform.Find("Panel_Header/BackButton").GetComponent<MenuButtonNoGlow>();
            _buttonBuy = _selfPage.transform.Find("Button_Buy").GetComponent<MenuButtonNoGlow>();
            //_buttonOpen = _selfPage.transform.Find("Button_Open").GetComponent<MenuButtonNoGlow>();
            _buttonSave = _selfPage.transform.Find("Button_Save").GetComponent<MenuButtonNoGlow>();
            _buttonCollection = _selfPage.transform.Find("Button_Collection").GetComponent<MenuButtonNoGlow>();
            _buttonArrowLeft = _selfPage.transform.Find("ArrowLeftButton").GetComponent<Button>();
            _buttonArrowRight = _selfPage.transform.Find("ArrowRightButton").GetComponent<Button>();

            _cardSetsSlider.onValueChanged.AddListener(CardSetsSliderOnValueChangedHandler);

            _cardListContent = _selfPage.transform.Find("Panel_CardsList/Group").gameObject;

            _cardsListScrollRect = _selfPage.transform.Find("Panel_CardsList").GetComponent<ScrollRect>();

            _currentSetPageCountText = _selfPage.transform.Find("Text_Count").GetComponent<TextMeshProUGUI>();

            _buttonBack.onClickEvent.AddListener(BackButtonHandler);
            _buttonBuy.onClickEvent.AddListener(BuyButtonHandler);
            _buttonSave.onClickEvent.AddListener(SaveButtonHandler);
            _buttonCollection.onClickEvent.AddListener(SaveButtonHandler);
            //_buttonOpen.onClickEvent.AddListener(OpenButtonHandler);
            
            _buttonArrowLeft.onClick.AddListener(ArrowLeftButtonHandler);
            _buttonArrowRight.onClick.AddListener(ArrowRightButtonHandler);

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
            WarningPopup.OnHidePopupEvent += OnCloseAlertDialogEventHandler;
            _collectionData.cards.Clear();
            CollectionCardData cardData;
            foreach (var card in _dataManager.CachedCollectionData.cards)
            {
                cardData = new CollectionCardData();
                cardData.amount = card.amount;
                cardData.cardId = card.cardId;
                _collectionData.cards.Add(cardData);
            }

            _selfPage.SetActive(true);
            if (_currentDeckId == -1)
            {
                _currentDeck = new Deck();
                _currentDeck.name = "Deck " + _dataManager.CachedDecksData.decks.Count;
                _currentDeck.cards = new List<DeckCardData>();

            }
            else
            {
                _currentDeck = new Deck();
                _currentDeck.name = _dataManager.CachedDecksData.decks[_currentDeckId].name;
                _currentDeck.heroId = _dataManager.CachedDecksData.decks[_currentDeckId].heroId;
                _currentDeck.cards = new List<DeckCardData>();
                //_currentDeck.cards = _dataManager.CachedDecksData.decks[_currentDeckId].cards;
                DeckCardData cardDat = null;
                foreach (var item in _dataManager.CachedDecksData.decks[_currentDeckId].cards)
                {
                    cardDat = new DeckCardData();
                    cardDat.cardId = item.cardId;
                    cardDat.amount = item.amount;
                    _currentDeck.cards.Add(cardDat);
                }
            }
            //InitObjects();
            LoadDeckInfo(_currentDeck);
            InitObjects();

            _cardsListScrollRect.verticalNormalizedPosition = 1f;
            _cardsListScrollRect.CalculateLayoutInputVertical();
            //_uiManager.Canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
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
            //_uiManager.Canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            WarningPopup.OnHidePopupEvent -= OnCloseAlertDialogEventHandler;
        }

        private void OpenAlertDialog(string msg)
        {
            foreach (var card in _currentCards)
            {
                card.isActive = false;
            }
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        private void OnCloseAlertDialogEventHandler()
        {
            foreach (var card in _currentCards)
            {
                card.isActive = true;
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

            numSets = _dataManager.CachedCardsLibraryData.sets.Count - 1; //1 - tutorial
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
            if (Constants.DEV_MODE)
                OnDoneButtonPressed();
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
        }

        private void BuyButtonHandler()
        {
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.SHOP);
        }

        private void CollectionButtonHandler()
        {
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.COLLECTION);
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

        public void LoadCards(int page, int setIndex)
        {
            CorrectSetIndex(ref setIndex);
            _currentCards.Clear();
			var set = _dataManager.CachedCardsLibraryData.sets[setIndex];
			var cards = set.cards;

            _currentSetPageCountText.text = string.Format("{0} Elements Cards {1}/{2}", Utilites.FirstCharToUpper(set.name), (currentPage + 1).ToString(), numPages.ToString()); 

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

                var amount = _collectionData.GetCardData(card.id).amount;

                var cardView = go.GetComponent<CardView>();
                cardView.PopulateWithLibraryInfo(card, set.name, amount);
                cardView.SetHighlightingEnabled(false);
                cardView.transform.position = cardPositions[i % cardPositions.Count].position;
                cardView.transform.localScale = Vector3.one * 0.3f;
                cardView.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
                cardView.GetComponent<SpriteRenderer>().sortingOrder = 1;
                cardView.GetComponent<SortingGroup>().sortingLayerName = "Default";
                cardView.GetComponent<SortingGroup>().sortingOrder = 1;

                var deckBuilderCard = go.AddComponent<DeckBuilderCard>();
                deckBuilderCard.scene = this;
                deckBuilderCard.card = card;
                _currentCards.Add(deckBuilderCard);
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
                var cardListItem = go.GetComponent<CardListItem>();
                //cardListItem.deckButton = deck;
                //cardListItem.card = libraryCard;
                //cardListItem.cardNameText.text = libraryCard.name;
                //cardListItem.cardAmountText.text = "x" + card.amount.ToString();
                //cardListItem.count = card.amount;
                int maxCount = _collectionData.GetCardData(libraryCard.id).amount;// + card.amount;
                cardListItem.Init(deck, libraryCard, card.amount, maxCount);
                cardListItem.OnDeleteCard += DeleteCardHandler;
               


                _collectionData.GetCardData(card.cardId).amount -= card.amount;
            }
            UpdateNumCardsText();
        }

        private void DeleteCardHandler(int cardId)
        {
            var collectionCardData = _collectionData.GetCardData(cardId);
            collectionCardData.amount++;
            UpdateCardAmount(cardId, collectionCardData.amount);
        }

        public void AddCardToDeck(Card card)
        {
            if (_currentDeck == null)
            {
                return;
            }

            var collectionCardData = _collectionData.GetCardData(card.id);
            if(collectionCardData.amount == 0)
            {
                OpenAlertDialog("You don't have enough cards of this type. \n Buy or earn new packs to get more cards!");
                return;
            }
            collectionCardData.amount--;
            UpdateCardAmount(card.id, collectionCardData.amount);

            var existingCards = _currentDeck.cards.Find(x => x.cardId == card.id);

            uint maxCopies = 0;
            var cardRarity = "You cannot have more than ";

            switch (card.cardRarity)
            {
                case Enumerators.CardRarity.COMMON:
                    maxCopies = Constants.CARD_COMMON_MAX_COPIES;
                    break;
                case Enumerators.CardRarity.RARE:
                    maxCopies = Constants.CARD_RARE_MAX_COPIES;
                    break;
                case Enumerators.CardRarity.LEGENDARY:
                    maxCopies = Constants.CARD_LEGENDARY_MAX_COPIES;
                    break;
                case Enumerators.CardRarity.EPIC:
                    maxCopies = Constants.CARD_EPIC_MAX_COPIES;
                    break;
            }

            if (existingCards != null && existingCards.amount == maxCopies)
            {
                OpenAlertDialog("You cannot have more than " + maxCopies + " copies of the " + card.cardRarity.ToString().ToLower() + " card in your deck.");
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
                var cardListItem = go.GetComponent<CardListItem>();
                //cardListItem.deckButton = _currentDeck;
                //cardListItem.card = card;
                //cardListItem.cardNameText.text = card.name;
                
                int maxCount = _collectionData.GetCardData(card.id).amount + 1;
                cardListItem.Init(_currentDeck, card, 1, maxCount);
                cardListItem.OnDeleteCard += DeleteCardHandler;

            }

            _currentDeck.AddCard(card.id); 
        }

        public void UpdateCardAmount(int cardId, int amount)
        {
            foreach (var card in MonoBehaviour.FindObjectsOfType<CardView>())
            {
                if (card.libraryCard.id == cardId)
                {
                    card.UpdateAmount(amount);
                    break;
                }
            }
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
                _cardAmountText.text = _currentDeck.GetNumCards().ToString() + " / " + Constants.DECK_MAX_SIZE;
            }
        }

        public void OnDoneButtonPressed()
        {
            if (_currentDeckId == -1)
            {
                _currentDeck.heroId = _currentHeroId;
                _dataManager.CachedDecksData.decks.Add(_currentDeck);
            }
            else
            {
                _dataManager.CachedDecksData.decks[_currentDeckId] = _currentDeck;
            }

            _dataManager.SaveCache(Enumerators.CacheDataType.DECKS_DATA);

            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
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
