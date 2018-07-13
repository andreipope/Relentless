// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;

namespace LoomNetwork.CZB
{
    public class DeckEditingPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IDataManager _dataManager;

        private GameObject _selfPage;

        private TMP_InputField _deckNameInputField;

        private Button _buttonBack,
                                //_buttonBuy,
                                //_buttonOpen,
                                _buttonSave,
                                _buttonArmy;
        private Button _buttonArmyArrowLeft,
                       _buttonArmyArrowRight,
                        _buttonHordeArrowLeft,
                       _buttonHordeArrowRight;

        //private ScrollRect _cardsListScrollRect;

        private TMP_Text _cardAmountText;

        private Deck _currentDeck;

        // private Slider _cardSetsSlider;

        private int _numElementPages,
                    _currentElementPage,
                    _numHordePages,
                    _currentHordePage;

        private int numSets;
        private int currentSet;

        private Toggle _airToggle,
                        _earthToggle,
                        _fireToggle,
                        _waterToggle,
                        _toxicTogggle,
                        _lifeToggle,
                        _itemsToggle;

        //private List<Transform> cardPositions;

        private GameObject _cardCreaturePrefab;
        private GameObject _cardSpellPrefab;
        //private GameObject _cardPlaceholdersPrefab;
        //private GameObject _cardListContent;

        private GameObject _backgroundCanvasPrefab;

        private CollectionData _collectionData;

        private int _currentDeckId;
        private int _currentHeroId;

        private const int _cardAmount = 5;
        private Dictionary<Enumerators.SetType, Enumerators.SetType> _against = new Dictionary<Enumerators.SetType, Enumerators.SetType>()
        {
                { Enumerators.SetType.FIRE, Enumerators.SetType.WATER},
                { Enumerators.SetType.TOXIC, Enumerators.SetType.FIRE},
                { Enumerators.SetType.LIFE, Enumerators.SetType.TOXIC},
                { Enumerators.SetType.EARTH, Enumerators.SetType.LIFE},
                { Enumerators.SetType.AIR, Enumerators.SetType.EARTH},
                { Enumerators.SetType.WATER, Enumerators.SetType.AIR},
        };

        public int CurrentDeckId
        {
            set { _currentDeckId = value; }
        }
        public int CurrentHeroId
        {
            set { _currentHeroId = value; }
        }

        private string _currentSetName;

        private List<BoardCard> _createdArmyCards,
                                _createdHordeCards;

        private ToggleGroup _toggleGroup;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _collectionData = new CollectionData();
            _collectionData.cards = new List<CollectionCardData>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/DeckEditingPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);


            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            _cardSpellPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/SpellCard");
            //_cardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersEditingDeck");
            _backgroundCanvasPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/BackgroundEditingCanvas");

            _toggleGroup = _selfPage.transform.Find("ElementsToggles").GetComponent<ToggleGroup>();
            _airToggle = _selfPage.transform.Find("ElementsToggles/Air").GetComponent<Toggle>();
            _lifeToggle = _selfPage.transform.Find("ElementsToggles/Life").GetComponent<Toggle>();
            _waterToggle = _selfPage.transform.Find("ElementsToggles/Water").GetComponent<Toggle>();
            _toxicTogggle = _selfPage.transform.Find("ElementsToggles/Toxic").GetComponent<Toggle>();
            _fireToggle = _selfPage.transform.Find("ElementsToggles/Fire").GetComponent<Toggle>();
            _earthToggle = _selfPage.transform.Find("ElementsToggles/Earth").GetComponent<Toggle>();
            _itemsToggle = _selfPage.transform.Find("ElementsToggles/Items").GetComponent<Toggle>();

            _cardAmountText = _selfPage.transform.Find("CardsAmount/CardsAmountText").GetComponent<TMP_Text>();

            _deckNameInputField = _selfPage.transform.Find("DeckTitleInputText").GetComponent<TMP_InputField>();

            _buttonBack = _selfPage.transform.Find("BackButton").GetComponent<Button>();
            //_buttonBuy = _selfPage.transform.Find("Button_Buy").GetComponent<Button>();
            //_buttonOpen = _selfPage.transform.Find("Button_Open").GetComponent<MenuButtonNoGlow>();
            _buttonSave = _selfPage.transform.Find("Button_Save").GetComponent<Button>();
            _buttonArmy = _selfPage.transform.Find("Button_Collection").GetComponent<Button>();
            _buttonArmyArrowLeft = _selfPage.transform.Find("Army/ArrowLeftButton").GetComponent<Button>();
            _buttonArmyArrowRight = _selfPage.transform.Find("Army/ArrowRightButton").GetComponent<Button>();
            _buttonHordeArrowLeft = _selfPage.transform.Find("Horde/ArrowLeftButton").GetComponent<Button>();
            _buttonHordeArrowRight = _selfPage.transform.Find("Horde/ArrowRightButton").GetComponent<Button>();

            //_cardSetsSlider.onValueChanged.AddListener(CardSetsSliderOnValueChangedHandler);

            _buttonBack.onClick.AddListener(BackButtonHandler);
            //_buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonSave.onClick.AddListener(SaveButtonHandler);
            _buttonArmy.onClick.AddListener(ArmyButtonHandler);
            //_buttonOpen.onClickEvent.AddListener(OpenButtonHandler);

            _airToggle.onValueChanged.AddListener((state) => { if(state)ToggleChooseOnValueChangedHandler(Enumerators.SetType.AIR); });
            _lifeToggle.onValueChanged.AddListener((state) => { if(state)ToggleChooseOnValueChangedHandler(Enumerators.SetType.LIFE); });
            _waterToggle.onValueChanged.AddListener((state) => { if(state)ToggleChooseOnValueChangedHandler(Enumerators.SetType.WATER); });
            _toxicTogggle.onValueChanged.AddListener((state) => { if(state)ToggleChooseOnValueChangedHandler(Enumerators.SetType.TOXIC); });
            _fireToggle.onValueChanged.AddListener((state) => { if(state)ToggleChooseOnValueChangedHandler(Enumerators.SetType.FIRE); });
            _earthToggle.onValueChanged.AddListener((state) => { if(state)ToggleChooseOnValueChangedHandler(Enumerators.SetType.EARTH); });
            _itemsToggle.onValueChanged.AddListener((state) => { if(state)ToggleChooseOnValueChangedHandler(Enumerators.SetType.ITEM); });

            _buttonArmyArrowLeft.onClick.AddListener(ArmyArrowLeftButtonHandler);
            _buttonArmyArrowRight.onClick.AddListener(ArmyArrowRightButtonHandler);
            _buttonHordeArrowLeft.onClick.AddListener(HordeArrowLeftButtonHandler);
            _buttonHordeArrowRight.onClick.AddListener(HordeArrowRightButtonHandler);

            _deckNameInputField.onEndEdit.AddListener(OnDeckNameInputFieldEndedEdit);

            _createdArmyCards = new List<BoardCard>();
            _createdHordeCards = new List<BoardCard>();

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
                DeckCardData cardDat = null;
                foreach (var item in _dataManager.CachedDecksData.decks[_currentDeckId].cards)
                {
                    cardDat = new DeckCardData();
                    cardDat.cardId = item.cardId;
                    cardDat.amount = item.amount;
                    _currentDeck.cards.Add(cardDat);
                }
            }
            LoadDeckInfo(_currentDeck);
            InitObjects();
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
            Dispose();
        }

        public void Dispose()
        {
            ResetArmyCards();
            ResetHordeItems();
            WarningPopup.OnHidePopupEvent -= OnCloseAlertDialogEventHandler;
        }


        private void ResetArmyCards()
        {
            if (_createdArmyCards != null)
            {
                foreach (var item in _createdArmyCards)
                    item.Dispose();
                _createdArmyCards.Clear();
            }
        }

        private void ResetHordeItems()
        {
            if (_createdHordeCards != null)
            {
                foreach (var item in _createdHordeCards)
                    item.Dispose();
                _createdHordeCards.Clear();
            }
        }

        private void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);
            foreach (var card in _createdArmyCards)
            {
                card.gameObject.GetComponent<DeckBuilderCard>().isActive = false;
            }
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        private void OnCloseAlertDialogEventHandler()
        {
            foreach (var card in _createdArmyCards)
            {
                card.gameObject.GetComponent<DeckBuilderCard>().isActive = true;
            }
        }

        private void InitObjects()
        {
            numSets = _dataManager.CachedCardsLibraryData.sets.Count - 1;
            _numElementPages = Mathf.CeilToInt(_dataManager.CachedCardsLibraryData.sets[currentSet].cards.Count / _cardAmount);

            LoadCards(0, 0);
        }

        

        #region button handlers
        private void ToggleChooseOnValueChangedHandler(Enumerators.SetType type)
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);
            _currentElementPage = 0;
            currentSet = (int)type;
            Debug.Log(currentSet);
            LoadCards(0, (int)type);
        }

        private void BackButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            if (Constants.DEV_MODE)
                OnDoneButtonPressed();
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
        }

        //private void BuyButtonHandler()
        //{
        //    GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        //    GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.SHOP);
        //}

        private void ArmyButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.COLLECTION);
        }

        //private void OpenButtonHandler()
        //{
        //    GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        //    GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
        //}
        private void SaveButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            OnDoneButtonPressed();
        }

        private void ArmyArrowLeftButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            MoveCardsPage(-1);
        }

        private void ArmyArrowRightButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            MoveCardsPage(1);
        }

        private void HordeArrowLeftButtonHandler()
        {
            _currentHordePage--;
            if (_currentHordePage < 0)
                _currentHordePage = _numHordePages;
            CalculateVisibility();

            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        }

        private void HordeArrowRightButtonHandler()
        {
            _currentHordePage++;

            if (_currentHordePage > _numHordePages)
                _currentHordePage = 0;
            CalculateVisibility();
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        }

        #endregion

        public void MoveCardsPage(int direction)
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);
            _currentElementPage += direction;
            Debug.Log(direction);
            if (_currentElementPage < 0)
            {
                currentSet += direction;

                if (currentSet < 0)
                {
                    currentSet = numSets - 1;
                    _currentElementPage = _numElementPages - 1;
                }
                else
                {
                    _currentElementPage = _numElementPages - 1;
                    _currentElementPage = _currentElementPage < 0 ? 0 : _currentElementPage;
                }
            }
            else if (_currentElementPage >= _numElementPages)
            {
                currentSet += direction;

                if (currentSet >= numSets)
                {
                    currentSet = 0;
                    _currentElementPage = 0;
                }
                else
                {
                    _currentElementPage = 0;
                }
            }

            _numElementPages = Mathf.CeilToInt(_dataManager.CachedCardsLibraryData.sets[currentSet].cards.Count / _cardAmount);

            LoadCards(_currentElementPage, currentSet, true );
        }

        public void LoadCards(int page, int setIndex, bool needCast = false)
        {
            //if(needCast)
            //    CorrectSetIndex(ref setIndex);
            var set = _dataManager.CachedCardsLibraryData.sets[setIndex];

            var cards = set.cards;
            //_currentSetName = set.name;

            var startIndex = page * _cardAmount;
            var endIndex = Mathf.Min(startIndex + _cardAmount, cards.Count);

            ResetArmyCards();

            Vector3 _startPos = new Vector3(-7f, -2.4f, 0);
            float stepX = 3.4f;

            for (var i = startIndex; i < endIndex; i++)
            {
                if (i >= cards.Count)
                    break;

                var card = cards[i];

                BoardCard boardCard = CreateCard(card, _startPos + Vector3.right * stepX * i);

                var deckBuilderCard = boardCard.gameObject.AddComponent<DeckBuilderCard>();
                deckBuilderCard.scene = this;
                deckBuilderCard.card = card;

                _createdArmyCards.Add(boardCard);
            }
        }

        public BoardCard CreateCard(Card card, Vector3 pos)
        {
            BoardCard boardCard = null;
            GameObject go = null;
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
            var amount = _collectionData.GetCardData(card.id).amount;

            boardCard.Init(card, amount);
            boardCard.SetHighlightingEnabled(false);
            boardCard.transform.position = pos;
            boardCard.transform.localScale = Vector3.one * 0.3f;
            boardCard.gameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LAYER_DEFAULT;
            boardCard.gameObject.GetComponent<SortingGroup>().sortingOrder = 1;
            return boardCard;
        }

        public void OnDeckNameInputFieldEndedEdit(string value)
        {
            _currentDeck.name = value;
        }

        public void LoadDeckInfo(Deck deck)
        {
            _deckNameInputField.text = deck.name;

            ResetHordeItems();

            foreach (var card in deck.cards)
            {
                var libraryCard = _dataManager.CachedCardsLibraryData.GetCard(card.cardId);
                UpdateCardAmount(card.cardId, card.amount);

                var itemFound = false;
                foreach (var item in _createdHordeCards)
                {
                    if (item.libraryCard.id == card.cardId)
                    {
                        itemFound = true;
                        //item.AddCard();
                        break;
                    }
                }
                if (!itemFound)
                {
                    BoardCard boardCard = CreateCard(libraryCard, new Vector3(-7.9f + 3.85f * (_createdHordeCards.Count % _cardAmount), 3, 0));
                    boardCard.transform.Find("Amount").gameObject.SetActive(false);

                    var deckBuilderCard = boardCard.gameObject.AddComponent<DeckBuilderCard>();
                    deckBuilderCard.scene = this;
                    deckBuilderCard.card = libraryCard;
                    deckBuilderCard.isHordeItem = true;

                    _createdHordeCards.Add(boardCard);

                    _currentDeck.AddCard(libraryCard.id);

                    _collectionData.GetCardData(card.cardId).amount -= card.amount;
                    UpdateNumCardsText();
                }
            }
            _numHordePages = Mathf.CeilToInt((_createdHordeCards.Count - 1) / _cardAmount);
            _currentHordePage = 0;
            CalculateVisibility();
        }

        private void CalculateVisibility()
        {
            for(int i = 0; i < _createdHordeCards.Count; i++)
            {
                if (((i + 1) > _currentHordePage * _cardAmount) && ((i + 1) < (_currentHordePage + 1) * _cardAmount + 1))
                    _createdHordeCards[i].gameObject.SetActive(true);
                else
                    _createdHordeCards[i].gameObject.SetActive(false);
            }
        }

        private void RepositionHordeCards()
        {
            for (int i = 0; i < _createdHordeCards.Count; i++)
            {
                _createdHordeCards[i].transform.position = new Vector3( 7.9f + 3.85f * (_createdHordeCards.Count % _cardAmount), 3, 0);
                CalculateVisibility();
            }
        }

        public void RemoveCardFromDeck(Card card)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_REMOVE_CARD, Constants.SFX_SOUND_VOLUME, false, false, true);
            var collectionCardData = _collectionData.GetCardData(card.id);
            collectionCardData.amount++;
            UpdateCardAmount(card.id, collectionCardData.amount);
            BoardCard boardCard = _createdHordeCards.Where((item) => item.libraryCard.id == card.id) as BoardCard;
            Debug.Log(boardCard);
            _createdHordeCards.Remove(boardCard);
            Debug.Log(_createdHordeCards.Count);
            MonoBehaviour.Destroy(boardCard.gameObject);
            _numHordePages = Mathf.CeilToInt((_createdHordeCards.Count - 1) / _cardAmount);
            RepositionHordeCards();
        }

        public void AddCardToDeck(Card card)
        {
            if (_currentDeck == null)
            {
                return;
            }

            if (_against[_dataManager.CachedHeroesData.Heroes[_currentHeroId].heroElement] == card.cardSetType)
            {
                OpenAlertDialog("It's not possible to add cards to the deck \n from the faction from which the hero is weak against");
                return;
            }
            var collectionCardData = _collectionData.GetCardData(card.id);
            if (collectionCardData.amount == 0)
            {
                OpenAlertDialog("You don't have enough cards of this type. \n Buy or earn new packs to get more cards!");
                return;
            }
            var existingCards = _currentDeck.cards.Find(x => x.cardId == card.id);

            uint maxCopies = GetMaxCopiesValue(card.cardRank);
            var cardRarity = "You cannot have more than ";

            if (existingCards != null && existingCards.amount == maxCopies)
            {
                OpenAlertDialog("You cannot have more than " + maxCopies + " copies of the " + card.cardRank.ToString().ToLower() + " card in your deck.");
                return;
            }
            var maxDeckSize = Constants.DECK_MAX_SIZE;
            if (_currentDeck.GetNumCards() == maxDeckSize)
            {
                OpenAlertDialog("Your '" + _currentDeck.name + "' deck has more than " + maxDeckSize + " cards.");
                return;
            }
            var itemFound = false;
            foreach (var item in _createdHordeCards)
            {
                if (item.libraryCard.id == card.id)
                {
                    itemFound = true;
                    //item.AddCard();
                    break;
                }
            }
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_ADD_CARD, Constants.SFX_SOUND_VOLUME, false, false, true);
            collectionCardData.amount--;
            UpdateCardAmount(card.id, collectionCardData.amount);

            if (!itemFound)
            {
                BoardCard boardCard = CreateCard(card, new Vector3(-7.9f + 3.85f * (_createdHordeCards.Count % _cardAmount), 3, 0));
                boardCard.transform.Find("Amount").gameObject.SetActive(false);

                var deckBuilderCard = boardCard.gameObject.AddComponent<DeckBuilderCard>();
                deckBuilderCard.scene = this;
                deckBuilderCard.card = card;
                deckBuilderCard.isHordeItem = true;

                _createdHordeCards.Add(boardCard);

                _numHordePages = Mathf.CeilToInt((_createdHordeCards.Count - 1) / _cardAmount);
                Debug.Log("_numHordePages " + _numHordePages);
                CalculateVisibility();
            }

            _currentDeck.AddCard(card.id);
        }

        public uint GetMaxCopiesValue(Enumerators.CardRank rarity)
        {
            uint maxCopies = 0;
            switch (rarity)
            {
                case Enumerators.CardRank.MINION:
                    maxCopies = Constants.CARD_MINION_MAX_COPIES;
                    break;
                case Enumerators.CardRank.OFFICER:
                    maxCopies = Constants.CARD_OFFICER_MAX_COPIES;
                    break;
                case Enumerators.CardRank.COMMANDER:
                    maxCopies = Constants.CARD_COMMANDER_MAX_COPIES;
                    break;
                case Enumerators.CardRank.GENERAL:
                    maxCopies = Constants.CARD_GENERAL_MAX_COPIES;
                    break;
            }
            return maxCopies;
        }

        public void UpdateCardAmount(int cardId, int amount)
        {
            foreach (var card in _createdArmyCards)
            {
                if (card.libraryCard.id == cardId)
                {
                    card.UpdateAmount(amount);
                    break;
                }
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

        private void CorrectSetIndex(ref int id)
        {
            switch (id)
            {
                case 0:
                    id =  3;
                    break;
                case 1:
                    id = 4;
                    break;
                case 2:
                    id = 1;
                    break;
                case 3:
                    id = 5;
                    break;
                case 4:
                    id = 0;
                    break;
                case 5:
                    id = 2;
                    break;
                default:
                    break;
            }
        }
    }
}
