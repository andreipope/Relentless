using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Card = Loom.ZombieBattleground.Data.Card;
using Deck = Loom.ZombieBattleground.Data.Deck;
using OverlordUserInstance = Loom.ZombieBattleground.Data.OverlordUserInstance;

namespace Loom.ZombieBattleground
{
    public class HordeEditingTab
    {
        private static readonly ILog Log = Logging.GetLog(nameof(HordeEditingTab));

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private HordeSelectionWithNavigationPage _myDeckPage;

        private GameObject CardCreaturePrefab;


        private List<BoardCardView> _displayDeckBoardCards,
                                    _deckBoardCardsPool,
                                    _displayCollectionsBoardCards,
                                    _collectionBoardCardsPool;

        private GameObject _selfPage;

        private CollectionData _collectionData;

        private GameObject _draggingObject;

        private Button _buttonAutoComplete,
            _buttonBack;

        private Button _buttonLeftArrowScroll;
        private Button _buttonRightArrowScroll;

        private Scrollbar _cardCollectionScrollBar;

        private TextMeshProUGUI _textEditDeckName,
                                _textEditDeckCardsAmount;

        private TMP_InputField _inputFieldSearchName;

        private Image[] _imageAbilityIcons;

        private Image _imageAbilitiesPanel;

        private int _deckPageIndex,
                    _collectionPageIndex;

        private bool _isDragging;

        public readonly Dictionary<Enumerators.Faction, Enumerators.Faction> FactionAgainstDictionary =
            new Dictionary<Enumerators.Faction, Enumerators.Faction>
            {
                {
                    Enumerators.Faction.FIRE, Enumerators.Faction.WATER
                },
                {
                    Enumerators.Faction.TOXIC, Enumerators.Faction.FIRE
                },
                {
                    Enumerators.Faction.LIFE, Enumerators.Faction.TOXIC
                },
                {
                    Enumerators.Faction.EARTH, Enumerators.Faction.LIFE
                },
                {
                    Enumerators.Faction.AIR, Enumerators.Faction.EARTH
                },
                {
                    Enumerators.Faction.WATER, Enumerators.Faction.AIR
                }
            };

        private List<Enumerators.Faction> _availableFaction;

        private List<Card> _cacheCollectionCardsList;

        private HordeSelectionWithNavigationPage.Tab _nextTab;

        private const float BoardCardScale = 0.20f;

        private Dictionary<string, int> _cacheDeckPageIndexDictionary;
        private Dictionary<string, int> _cacheCollectionPageIndexDictionary;

        private Enumerators.Faction _againstFaction;

        private const int MaxCollectionCardPoolAmount = 30;

        private UICardCollections _uiCardCollections;
        private CustomDeckUI _customDeckUi;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            InitBoardCardPrefabsAndLists();

            _myDeckPage = _uiManager.GetPage<HordeSelectionWithNavigationPage>();
            /*_myDeckPage.EventChangeTab += (HordeSelectionWithNavigationPage.Tab tab) =>
            {
                if (tab != HordeSelectionWithNavigationPage.Tab.Editing)
                    return;

                _inputFieldSearchName.text = "";
                ResetAvailableFactions();
                UpdateDeckPageIndexDictionary();
                FillCollectionData();
                SubtractInitialDeckCardsAmountFromCollections(_myDeckPage.CurrentEditDeck);

                LoadDeckCardsPool(_myDeckPage.CurrentEditDeck);
                ResetCollectionPageState();
                ResetDeckPageState();

                UpdateOverlordAbilitiesButton();

                _textEditDeckName.text = _myDeckPage.CurrentEditDeck.Name;

                if (_tutorialManager.IsTutorial)
                {
                    _textEditDeckCardsAmount.text = $"{_myDeckPage.CurrentEditDeck.GetNumCards()}/{_tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount}";
                }
                else
                {
                    _textEditDeckCardsAmount.text = $"{_myDeckPage.CurrentEditDeck.GetNumCards()}/{Constants.MaxDeckSize}";
                }
            };*/

            _cacheCollectionCardsList = new List<Card>();
            _cacheDeckPageIndexDictionary = new Dictionary<string, int>();
            _cacheCollectionPageIndexDictionary = new Dictionary<string, int>();

            _uiCardCollections = new UICardCollections();
            _uiCardCollections.Init();

            _customDeckUi = new CustomDeckUI();
            _customDeckUi.Init();

            Log.Info("Editing init called");
        }

        public void Load(GameObject selfPage)
        {
            _selfPage = selfPage;
            Log.Info("editing show");

            //_textEditDeckName = _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();
            //_textEditDeckCardsAmount = _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Lower_Items/Image_CardCounter/Text_CardsAmount").GetComponent<TextMeshProUGUI>();

            //_buttonSaveDeck = _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Lower_Items/Button_SaveDeck").GetComponent<Button>();
            //_buttonSaveDeck.onClick.AddListener(ButtonSaveEditDeckHandler);

            _buttonAutoComplete = _selfPage.transform.Find("Tab_Editing/Panel_Frame/Upper_Items/Button_AutoComplete").GetComponent<Button>();
            _buttonAutoComplete.onClick.AddListener(ButtonAutoCompleteHandler);

            _buttonBack = _selfPage.transform.Find("Tab_Editing/Panel_Frame/Upper_Items/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);

            _uiCardCollections.Show(_selfPage.transform.Find("Tab_Editing").gameObject, PageType.DeckEditing);
            _customDeckUi.Load(_selfPage.transform.Find("Tab_Editing/Deck_Content").gameObject);

            _buttonLeftArrowScroll = _selfPage.transform.Find("Tab_Editing/Panel_Content/Army/Element/LeftArrow").GetComponent<Button>();
            _buttonLeftArrowScroll.onClick.AddListener(ButtonLeftArrowScrollHandler);

            _buttonRightArrowScroll = _selfPage.transform.Find("Tab_Editing/Panel_Content/Army/Element/RightArrow").GetComponent<Button>();
            _buttonRightArrowScroll.onClick.AddListener(ButtonRightArrowScrollHandler);

            _cardCollectionScrollBar = _selfPage.transform.Find("Tab_Editing/Panel_Content/Army/Element/Scroll View").GetComponent<ScrollRect>().horizontalScrollbar;

            /*_imageAbilityIcons = new Image[]
            {
                _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Upper_Items/Button_OverlordAbilities/Image_SkillIcon_1").GetComponent<Image>(),
                _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Upper_Items/Button_OverlordAbilities/Image_SkillIcon_2").GetComponent<Image>()
            };*/

            //_imageAbilitiesPanel = _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Upper_Items/Button_OverlordAbilities").GetComponent<Image>();

            //_materialNormal = new Material(Shader.Find("Sprites/Default"));
            //_materialGrayscale = new Material(Shader.Find("Sprites/Grayscale"));

            //LoadBoardCardComponents();

            //_myDeckPage.DragAreaCollections.Scrolled += ScrollCollectionAreaHandler;
            //_myDeckPage.DragAreaDeck.Scrolled += ScrollDeckAreaHandler;
        }

        public void Show(int deckId)
        {
            _customDeckUi.ShowDeck(deckId);
        }

        public void Update()
        {
            _uiCardCollections.Update();
        }

        public void Dispose()
        {
            DisposeBoardCards();

            _uiCardCollections.Hide();

            if (_draggingObject != null)
            {
                Object.Destroy(_draggingObject);
                _draggingObject = null;
                _isDragging = false;
            }

            //Object.Destroy(_materialNormal);
            //Object.Destroy(_materialGrayscale);

            _cacheCollectionCardsList.Clear();
            _imageAbilityIcons = null;
        }

        private void InitBoardCardPrefabsAndLists()
        {
            CardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");

            _displayDeckBoardCards = new List<BoardCardView>();
            _deckBoardCardsPool = new List<BoardCardView>();
            _displayCollectionsBoardCards = new List<BoardCardView>();
            _collectionBoardCardsPool = new List<BoardCardView>();

            _collectionData = new CollectionData();
            _collectionData.Cards = new List<CollectionCardData>();
        }

        #region Button Handlers

        /*private void ScrollDeckAreaHandler(Vector2 scrollDelta)
        {
            if (_tutorialManager.BlockAndReport(_buttonUpperLeftArrow.name) ||
                _tutorialManager.BlockAndReport(_buttonUpperRightArrow.name))
                return;

            ScrollCardList(true, scrollDelta);
        }*/

        /*private void ScrollCollectionAreaHandler(Vector2 scrollDelta)
        {
            if (_tutorialManager.BlockAndReport(_buttonLowerRightArrow.name) ||
                _tutorialManager.BlockAndReport(_buttonLowerLeftArrow.name))
                return;

            ScrollCardList(false, scrollDelta);
        }*/

        private void ButtonLeftArrowScrollHandler()
        {
            if (_cardCollectionScrollBar.value <= 0)
                return;

            _cardCollectionScrollBar.value -= _cardCollectionScrollBar.size;
            if (_cardCollectionScrollBar.value <= 0)
                _cardCollectionScrollBar.value = 0;

        }

        private void ButtonRightArrowScrollHandler()
        {
            if (_cardCollectionScrollBar.value >= 1)
                return;

            _cardCollectionScrollBar.value += _cardCollectionScrollBar.size;
            if (_cardCollectionScrollBar.value >= 1)
                _cardCollectionScrollBar.value = 1;
        }


        /*private void FilterPopupHidingHandler(CardFilterPopup.CardFilterData cardFilterData)
        {
            ResetCollectionPageState();
            CardFilterPopup popup = _uiManager.GetPopup<CardFilterPopup>();
            popup.ActionPopupHiding -= FilterPopupHidingHandler;
        }*/

        /*private void ButtonEditDeckUpperLeftArrowHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonUpperLeftArrow.name))
                return;

            PlayClickSound();
            MoveDeckPageIndex(-1);
        }

        private void ButtonEditDeckUpperRightArrowHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonUpperRightArrow.name))
                return;

            PlayClickSound();
            MoveDeckPageIndex(1);
        }

        private void ButtonEditDeckLowerLeftArrowHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonLowerLeftArrow.name))
                return;

            PlayClickSound();
            MoveCollectionPageIndex(-1);
        }

        private void ButtonEditDeckLowerRightArrowHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonLowerRightArrow.name))
                return;

            PlayClickSound();
            MoveCollectionPageIndex(1);
        }*/

        /*private void ButtonSaveEditDeckHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonSaveDeck.name))
                return;

            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.HordeSaveButtonPressed);

            _buttonSaveDeck.enabled = false;

            PlayClickSound();

            SaveDeck(HordeSelectionWithNavigationPage.Tab.SelectDeck);
        }
*/

        private void FinishAddDeck(bool success, Deck deck)
        {
            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().FinishAddDeck -= FinishAddDeck;
            _myDeckPage.IsEditingNewDeck = false;

            if (GameClient.Get<IAppStateManager>().AppState != Enumerators.AppState.HordeSelection)
                return;

            //_buttonSaveDeck.enabled = true;

            if (_myDeckPage.CurrentEditDeck.Id.Id < 0)
                return;

            List<Deck> cacheDeckList = _myDeckPage.GetDeckList();
            _myDeckPage.SelectDeckIndex = cacheDeckList.IndexOf(_myDeckPage.CurrentEditDeck);
            _myDeckPage.SelectDeckIndex = Mathf.Min(_myDeckPage.SelectDeckIndex, cacheDeckList.Count-1);

            _myDeckPage.AssignCurrentDeck(_myDeckPage.SelectDeckIndex);
            _myDeckPage.ChangeTab(_nextTab);
        }

        private void FinishEditDeck(bool success, Deck deck)
        {
            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().FinishEditDeck -= FinishEditDeck;

            if (GameClient.Get<IAppStateManager>().AppState != Enumerators.AppState.HordeSelection)
                return;

            //_buttonSaveDeck.enabled = true;

            _myDeckPage.ChangeTab(_nextTab);
        }

        private void ButtonOverlordAbilitiesHandler()
        {
            //if (_tutorialManager.BlockAndReport(_buttonAbilities.name))
              //  return;

            PlayClickSound();
            _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SelectOverlordSkill);
        }

        private void ButtonAutoCompleteHandler()
        {
            Debug.LogError("auto Complete button called");
            if (_tutorialManager.BlockAndReport(_buttonAutoComplete.name))
                return;

            PlayClickSound();
            /*FillCollectionData();
            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().GenerateCardsToDeck
            (
                _myDeckPage.CurrentEditDeck,
                _collectionData
            );
            SubtractInitialDeckCardsAmountFromCollections(_myDeckPage.CurrentEditDeck);
            UpdateDeckPageIndexDictionary();

            ResetCollectionPageState();
            ResetDeckPageState();
            UpdateEditDeckCardsAmount();*/
        }

        private void ButtonBackHandler()
        {
            PlayClickSound();
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmSaveDeckHandler;
            _uiManager.DrawPopup<QuestionPopup>("Would you like to save your progress?");
        }

        private void ConfirmSaveDeckHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmSaveDeckHandler;

            if (status)
            {
                SaveDeck(HordeSelectionWithNavigationPage.Tab.SelectDeck);
            }
            else
            {
                _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SelectDeck);
            }
        }

        public void OnInputFieldSearchEndedEdit(string value)
        {
            if (_tutorialManager.BlockAndReport(_inputFieldSearchName.name))
                return;

            ResetCollectionPageState();
        }

        #endregion

        private void LoadBoardCardComponents()
        {
            /*DeckCardPlaceholders = Object.Instantiate(DeckCardPlaceholdersPrefab);
            Vector3 deckCardPlaceholdersPos = _myDeckPage.LocatorDeckCards.position;
            deckCardPlaceholdersPos.z = 0f;
            DeckCardPlaceholders.transform.position = deckCardPlaceholdersPos;

            DeckCardPositions = new List<Transform>();

            foreach (Transform placeholder in DeckCardPlaceholders.transform)
            {
                DeckCardPositions.Add(placeholder);
            }

            CollectionsCardPlaceholders = Object.Instantiate(CollectionsCardPlaceholdersPrefab);
            Vector3 collectionsCardPlaceholdersPos = _myDeckPage.LocatorCollectionCards.position;
            collectionsCardPlaceholdersPos.z = 0f;
            CollectionsCardPlaceholders.transform.position = collectionsCardPlaceholdersPos;

            CollectionsCardPositions = new List<Transform>();

            foreach (Transform placeholder in CollectionsCardPlaceholders.transform)
            {
                CollectionsCardPositions.Add(placeholder);
            }*/
        }

        private void FillCollectionData()
        {
            _collectionData.Cards.Clear();
            CollectionCardData cardData;

            List<CollectionCardData> data;
            if (_tutorialManager.IsTutorial)
            {
                data =
                    _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.CardsForArmy
                        .Select(card => card.ToCollectionCardData(_dataManager))
                        .ToList();
            }
            else
            {
                data = _dataManager.CachedCollectionData.Cards;
            }

            foreach (CollectionCardData card in data)
            {
                cardData = new CollectionCardData(card.MouldId, card.Amount);

                _collectionData.Cards.Add(cardData);
            }
        }

        public void LoadCollectionsCards()
        {
            /*List<Card> cards = _cacheCollectionCardsList.ToList();

            foreach(BoardCardView item in _displayCollectionsBoardCards)
            {
                item.GameObject.SetActive(false);
                _collectionBoardCardsPool.Add(item);
                if(_collectionBoardCardsPool.Count > MaxCollectionCardPoolAmount)
                {
                    int amountToDelete = _collectionBoardCardsPool.Count - MaxCollectionCardPoolAmount;
                    for(int i = 0; i < amountToDelete && _collectionBoardCardsPool.Count > 0; ++i)
                    {
                        Object.Destroy(_collectionBoardCardsPool[0].GameObject);
                        _collectionBoardCardsPool.RemoveAt(0);
                    }
                }
            }
            _displayCollectionsBoardCards.Clear();

            int startIndex = _collectionPageIndex * GetCollectionCardAmountPerPage();
            int endIndex = Mathf.Min
            (
                (_collectionPageIndex + 1) * GetCollectionCardAmountPerPage(),
                cards.Count
            );

            CollectionCardData collectionCardData = null;
            RectTransform rectContainer = _myDeckPage.LocatorCollectionCards.GetComponent<RectTransform>();

            for (int i = startIndex; i < endIndex; i++)
            {
                if (i >= cards.Count)
                    break;

                Card card = cards[i];
                CollectionCardData cardData;

                if (_tutorialManager.IsTutorial)
                {
                    cardData =
                        _tutorialManager
                            .GetCardData(_dataManager.CachedCardsLibraryData.GetCardNameFromMouldId(card.MouldId))
                            .ToCollectionCardData(_dataManager);
                }
                else
                {
                    cardData = _dataManager.CachedCollectionData.GetCardData(card.MouldId);
                }

                BoardCardView boardCard;
                Vector3 position = CollectionsCardPositions[i % CollectionsCardPositions.Count].position;

                if(_collectionBoardCardsPool.Exists(x => x.Model.Name == card.Name))
                {
                    boardCard = _collectionBoardCardsPool.Find(x => x.Model.Name == card.Name);
                    _collectionBoardCardsPool.Remove(boardCard);
                    boardCard.Transform.position = position;
                }
                else
                {
                    boardCard = CreateBoardCard
                    (
                        card,
                        rectContainer,
                        position,
                        BoardCardScale
                    );

                    if (card.Faction != _againstFaction)
                    {
                        OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();

                        eventHandler.DragBegan += BoardCardDragBeganHandler;
                        eventHandler.DragEnded += BoardCardCollectionDragEndedHandler;
                        eventHandler.DragUpdated += BoardCardDragUpdatedHandler;
                    }

                    DeckBuilderCard deckBuilderCard = boardCard.GameObject.AddComponent<DeckBuilderCard>();
                    deckBuilderCard.Page = this;
                    deckBuilderCard.Card = boardCard.Model.Card.Prototype;
                    deckBuilderCard.IsHordeItem = false;
                }

                boardCard.GameObject.SetActive(true);
                _displayCollectionsBoardCards.Add(boardCard);

                collectionCardData = _collectionData.GetCardData(card.MouldId);
                UpdateCollectionCardsDisplay
                (
                    card.Name,
                    collectionCardData.Amount
                );
            }*/
        }

        public void UpdateCollectionCardsDisplay(string cardId, int amount)
        {
            /*foreach (BoardCardView card in _displayCollectionsBoardCards)
            {
                if (card.Model.Card.Prototype.Name == cardId)
                {
                    card.SetAmount
                    (
                        BoardCardView.AmountTrayType.Counter,
                        amount,
                        (int)GetMaxCopiesValue(card.Model.Card.Prototype)
                    );
                    SetCardFrameMaterial
                    (
                        card,
                        (amount > 0 && card.Model.Faction != _againstFaction) ?
                            _materialNormal :
                            _materialGrayscale
                    );
                    break;
                }
            }*/
        }

        private void UpdateDeckCardsDisplay()
        {
            foreach (BoardCardView card in _displayDeckBoardCards)
            {
                card.SetAmount
                (
                    BoardCardView.AmountTrayType.Radio,
                    _myDeckPage.CurrentEditDeck.Cards.Find(x => x.MouldId == card.Model.Card.Prototype.MouldId).Amount,
                    (int)GetMaxCopiesValue(card.Model.Card.Prototype)
                );
            }
        }

        private void SetCardFrameMaterial(BoardCardView card, Material material)
        {
            card.GameObject.transform.Find("Frame").GetComponent<SpriteRenderer>().material = material;
            card.GameObject.transform.Find("Picture").GetComponent<SpriteRenderer>().material = material;
            card.GameObject.transform.Find("RankIcon").GetComponent<SpriteRenderer>().material = material;
        }

        private void SubtractInitialDeckCardsAmountFromCollections(Deck deck)
        {
            foreach(DeckCardData card in deck.Cards)
            {
                Card fetchedCard = _dataManager.CachedCardsLibraryData.GetCardFromMouldId(card.MouldId);
                _collectionData.GetCardData(fetchedCard.MouldId).Amount -= card.Amount;
            }
        }

        private void UpdateDeckPageIndexDictionary()
        {
            _cacheDeckPageIndexDictionary.Clear();
            int page = 0;
            int count = 0;
            foreach(DeckCardData card in _myDeckPage.CurrentEditDeck.Cards)
            {
                string cardName = _dataManager.CachedCardsLibraryData.GetCardNameFromMouldId(card.MouldId);
                _cacheDeckPageIndexDictionary.Add(cardName, page);

                ++count;
                if(count >= GetDeckCardAmountPerPage())
                {
                    count = 0;
                    ++page;
                }
            }
        }

        private void UpdateCollectionPageIndexDictionary()
        {
            _cacheCollectionPageIndexDictionary.Clear();
            int page = 0;
            int count = 0;
            foreach(Card card in _cacheCollectionCardsList)
            {
                if (_cacheCollectionPageIndexDictionary.ContainsKey(card.Name))
                    continue;

                _cacheCollectionPageIndexDictionary.Add(card.Name, page);

                ++count;
                if(count >= GetCollectionCardAmountPerPage())
                {
                    count = 0;
                    ++page;
                }
            }
        }

        public void LoadDeckCardsPool(Deck deck)
        {
            ResetDeckBoardCards();

            RectTransform rectContainer = _myDeckPage.LocatorDeckCards.GetComponent<RectTransform>();
            for (int i = 0; i < deck.Cards.Count; ++i)
            {
                DeckCardData card = deck.Cards[i];
                CreateDeckCardToPool
                (
                    deck.Cards[i],
                    rectContainer
                );
            }

            UpdateEditDeckCardsAmount();
            UpdateDeckCardPage();
        }

        private BoardCardView CreateDeckCardToPool(DeckCardData card, RectTransform rectContainer)
        {
            Card prototype = _dataManager.CachedCardsLibraryData.GetCardFromMouldId(card.MouldId);

            BoardCardView boardCard = CreateBoardCard
            (
                prototype,
                rectContainer,
                Vector3.zero,
                BoardCardScale
            );

            _deckBoardCardsPool.Add(boardCard);
            boardCard.GameObject.SetActive(false);

            boardCard.SetAmount
            (
                BoardCardView.AmountTrayType.Radio,
                card.Amount,
                (int)GetMaxCopiesValue(prototype)
            );

            OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();

            eventHandler.DragBegan += BoardCardDragBeganHandler;
            eventHandler.DragEnded += BoardCardDeckDragEndedHandler;
            eventHandler.DragUpdated += BoardCardDragUpdatedHandler;

            DeckBuilderCard deckBuilderCard = boardCard.GameObject.AddComponent<DeckBuilderCard>();
            deckBuilderCard.Page = this;
            deckBuilderCard.Card = boardCard.Model.Card.Prototype;
            deckBuilderCard.IsHordeItem = true;

            return boardCard;
        }

        public void AddCardToDeck(IReadOnlyCard card, bool animate = false)
        {
            if (_myDeckPage.CurrentEditDeck == null)
                return;

            OverlordUserInstance overlordData = _dataManager.CachedOverlordData.GetOverlordById(_myDeckPage.CurrentEditDeck.OverlordId);
            if (FactionAgainstDictionary[overlordData.Prototype.Faction] == card.Faction)
            {
                _myDeckPage.OpenAlertDialog(
                    "Cannot add from the faction your Overlord is weak against.");
                return;
            }

            CollectionCardData collectionCardData = _collectionData.GetCardData(card.MouldId);
            if (collectionCardData.Amount == 0)
            {
                _myDeckPage.OpenAlertDialog(
                    "You don't have enough of this card.\nBuy or earn packs to get more cards.");
                return;
            }

            DeckCardData existingCards = _myDeckPage.CurrentEditDeck.Cards.Find(x => x.MouldId == card.MouldId);

            uint maxCopies = GetMaxCopiesValue(card);

            if (existingCards != null && existingCards.Amount == maxCopies)
            {
                _myDeckPage.OpenAlertDialog("Cannot have more than " + maxCopies + " copies of an " +
                    card.Rank.ToString().ToLowerInvariant() + " card in one deck.");
                return;
            }

            if (_myDeckPage.CurrentEditDeck.GetNumCards() == Constants.DeckMaxSize)
            {
                _myDeckPage.OpenAlertDialog("Cannot have more than " + Constants.DeckMaxSize + " cards in one deck.");
                return;
            }

            collectionCardData.Amount--;
            UpdateCollectionCardsDisplay(card.Name, collectionCardData.Amount);
            bool isCardAlreadyExist = _myDeckPage.CurrentEditDeck.Cards.Exists(x => x.MouldId == card.MouldId);
            _myDeckPage.CurrentEditDeck.AddCard(card.MouldId);
            UpdateDeckPageIndexDictionary();

            if (_displayDeckBoardCards.Exists(item => item.Model.Card.Prototype.MouldId == card.MouldId))
            {
                BoardCardView foundItem = _displayDeckBoardCards.Find(item => item.Model.Card.Prototype.MouldId == card.MouldId);
                foundItem.SetAmount
                (
                    BoardCardView.AmountTrayType.Radio,
                    _myDeckPage.CurrentEditDeck.Cards.Find(x => x.MouldId == foundItem.Model.Card.Prototype.MouldId).Amount,
                    (int)GetMaxCopiesValue(card)
                );
            }
            else
            {
                int deckPagesAmount = GetDeckPageAmount(_myDeckPage.CurrentEditDeck) - 1;
                _deckPageIndex = Mathf.Max(_deckPageIndex, deckPagesAmount);
                UpdateDeckCardPage();
            }

            UpdateDeckCardPage
            (
                _cacheDeckPageIndexDictionary[card.Name]
            );
            if (animate)
            {
                CreateExchangeAnimationCard
                (
                    CreateBoardCard
                    (
                        card,
                        _myDeckPage.LocatorCollectionCards.GetComponent<RectTransform>(),
                        _displayCollectionsBoardCards.Find
                        (
                            x => x.Model.Card.Prototype.MouldId == card.MouldId
                        ).GameObject.transform.position,
                        BoardCardScale
                    ),
                    _displayDeckBoardCards.Find(x => x.Model.Prototype.MouldId == card.MouldId),
                    isCardAlreadyExist
                );
            }

            UpdateEditDeckCardsAmount();

            if (_tutorialManager.IsTutorial && _myDeckPage.CurrentEditDeck.GetNumCards() >= _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.HordeFilled);
            }
        }

        public void ScrollCardList(bool isHordeItem, Vector2 scrollDelta)
        {
            int index = 0;
            if (scrollDelta.y > 0.5f)
            {
                index = 1;
            }
            else if (scrollDelta.y < -0.5f)
            {
                index = - 1;
            }
            if (index != 0)
            {
                if (isHordeItem)
                {
                    MoveDeckPageIndex(index);
                }
                else
                {
                    MoveCollectionPageIndex(index);
                }
            }
        }

        public void RemoveCardFromDeck(IReadOnlyCard card, bool animate = false)
        {
            CollectionCardData collectionCardData = _collectionData.GetCardData(card.MouldId);
            collectionCardData.Amount++;
            UpdateCollectionCardsDisplay
            (
                card.Name,
                collectionCardData.Amount
            );

            _myDeckPage.CurrentEditDeck.RemoveCard(card.MouldId);
            UpdateDeckPageIndexDictionary();

            if(_displayDeckBoardCards.Exists(item => item.Model.Card.Prototype.MouldId == card.MouldId))
            {
                BoardCardView boardCard = _displayDeckBoardCards.Find(item => item.Model.Card.Prototype.MouldId == card.MouldId);
                boardCard.CardsAmountDeckEditing--;

                if (boardCard.CardsAmountDeckEditing <= 0)
                {
                    int deckPagesAmount = GetDeckPageAmount(_myDeckPage.CurrentEditDeck) - 1;
                    _deckPageIndex = deckPagesAmount < 0 ? 0 : Mathf.Min(_deckPageIndex, deckPagesAmount);

                    UpdateDeckCardPage();
                }
                else
                {
                    boardCard.SetAmount
                    (
                        BoardCardView.AmountTrayType.Radio,
                        boardCard.CardsAmountDeckEditing,
                        (int)GetMaxCopiesValue(boardCard.Model.Card.Prototype)
                    );
                }

                if(!_cacheCollectionPageIndexDictionary.ContainsKey(card.Name))
                {
                    _cacheCollectionPageIndexDictionary.Add(card.Name, 0);
                }

                UpdateCollectionPageIndex
                (
                    _cacheCollectionPageIndexDictionary[card.Name]
                );
                if (animate)
                {
                    CreateExchangeAnimationCard
                    (
                        CreateBoardCard
                        (
                            card,
                            _myDeckPage.LocatorDeckCards.GetComponent<RectTransform>(),
                            boardCard.GameObject.transform.position,
                            BoardCardScale
                        ),
                        _displayCollectionsBoardCards.Find(x => x.Model.Prototype.MouldId == card.MouldId),
                        true
                    );
                }
            }

            UpdateEditDeckCardsAmount();
        }

        private BoardCardView CreateBoardCard(IReadOnlyCard card, RectTransform root, Vector3 position, float scale)
        {
            GameObject go;
            BoardCardView boardCard;
            CardModel cardModel = new CardModel(new WorkingCard(card, card, null));

            switch (card.Kind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(CardCreaturePrefab);
                    boardCard = new UnitBoardCardView(go, cardModel);
                    break;
                case Enumerators.CardKind.ITEM:
                    go = Object.Instantiate(CardCreaturePrefab);
                    boardCard = new ItemBoardCardView(go, cardModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(card.Kind), card.Kind, null);
            }

            boardCard.SetAmount(BoardCardView.AmountTrayType.None);
            boardCard.SetHighlightingEnabled(false);
            boardCard.Transform.position = position;
            boardCard.Transform.localScale = Vector3.one * scale;
            boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;

            boardCard.Transform.SetParent(GameClient.Get<IUIManager>().Canvas.transform, true);
            RectTransform cardRectTransform = boardCard.GameObject.AddComponent<RectTransform>();

            if (root != null)
            {
                cardRectTransform.SetParent(root);
            }

            Vector3 anchoredPos = boardCard.Transform.localPosition;
            anchoredPos.z = 0;
            boardCard.Transform.localPosition = anchoredPos;

            return boardCard;
        }

        private void CreateExchangeAnimationCard
        (
            BoardCardView animatedCard,
            BoardCardView targetCard,
            bool targetCardWasAlreadyPresent
        )
        {
            if(targetCard == null)
            {
                Object.Destroy(animatedCard.GameObject);
                return;
            }

            animatedCard.GameObject.GetComponent<SortingGroup>().sortingOrder++;

            Vector3 endPosition = targetCard.GameObject.transform.position;

            if (!targetCardWasAlreadyPresent)
            {
                targetCard.GameObject.SetActive(false);
            }

            Sequence animatedSequence = DOTween.Sequence();
            animatedSequence.Append(animatedCard.Transform.DOMove(endPosition, .3f));
            animatedSequence.AppendCallback
            (() =>
            {
                Object.Destroy(animatedCard.GameObject);
                if (!targetCardWasAlreadyPresent)
                {
                    targetCard.GameObject.SetActive(true);
                }
            });
        }

        #region Boardcard Handler

        public void SelectCard(DeckBuilderCard deckBuilderCard, IReadOnlyCard card)
        {
            if (_isDragging ||
                _uiManager.GetPopup<CardInfoWithSearchPopup>().Self != null)
                return;

            List<IReadOnlyCard> cardList;
            CardInfoWithSearchPopup.PopupType popupType;

            if (deckBuilderCard.IsHordeItem)
            {
                cardList = _displayDeckBoardCards.Select(i => i.Model.Card.Prototype).ToList();
                popupType = CardInfoWithSearchPopup.PopupType.REMOVE_CARD;
            }
            else
            {
                cardList = _displayCollectionsBoardCards.Select(i => i.Model.Card.Prototype).ToList();
                popupType = CardInfoWithSearchPopup.PopupType.ADD_CARD;
            }
            _uiManager.DrawPopup<CardInfoWithSearchPopup>(new object[]
            {
                cardList,
                card,
                popupType
            });
        }

        private void BoardCardDragBeganHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (_isDragging || (GameClient.Get<ITutorialManager>().IsTutorial &&
                !GameClient.Get<ITutorialManager>().CurrentTutorial.IsGameplayTutorial() &&
                (GameClient.Get<ITutorialManager>().CurrentTutorialStep.ToMenuStep().CardsInteractingLocked ||
                !GameClient.Get<ITutorialManager>().CurrentTutorialStep.ToMenuStep().CanDragCards)))
                return;

            _draggingObject = Object.Instantiate(onOnject);
            _draggingObject.transform.localScale = Vector3.one * 0.3f;
            _draggingObject.name = onOnject.GetInstanceID().ToString();
            _draggingObject.GetComponent<SortingGroup>().sortingOrder = 2;

            _isDragging = true;

            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.z = 0f;
            _draggingObject.transform.position = position;
        }

        private void BoardCardCollectionDragEndedHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (!_isDragging)
                return;

            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, SRLayerMask.Default);

            /*if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject == _myDeckPage.DragAreaDeck.gameObject)
                    {
                        BoardCardView boardCard = _displayCollectionsBoardCards.Find(x =>
                            x.GameObject.GetInstanceID().ToString() == _draggingObject.name);

                        PlayAddCardSound();
                        AddCardToDeck(boardCard.Model.Card.Prototype);

                        GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.CardDragged);
                    }
                }
            }*/

            Object.Destroy(_draggingObject);
            _draggingObject = null;
            _isDragging = false;
        }

        private void BoardCardDeckDragEndedHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (!_isDragging)
                return;

            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, SRLayerMask.Default);

            /*if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject == _myDeckPage.DragAreaCollections.gameObject)
                    {
                        BoardCardView boardCard = _displayDeckBoardCards.Find(x =>
                            x.GameObject.GetInstanceID().ToString() == _draggingObject.name);

                        PlayRemoveCardSound();
                        RemoveCardFromDeck(boardCard.Model.Card.Prototype);
                    }
                }
            }*/

            Object.Destroy(_draggingObject);
            _draggingObject = null;
            _isDragging = false;
        }

        private void BoardCardDragUpdatedHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (!_isDragging)
                return;


            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.z = _draggingObject.transform.position.z;
            _draggingObject.transform.position = position;
        }

        #endregion

        private void UpdateOverlordAbilitiesButton()
        {
            Deck deck = _myDeckPage.CurrentEditDeck;
            OverlordUserInstance overlord = _dataManager.CachedOverlordData.GetOverlordById(_myDeckPage.CurrentEditDeck.OverlordId);
            if(deck.PrimarySkill == Enumerators.Skill.NONE)
            {
                _imageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
            }
            else
            {
                string iconPath = overlord.GetSkill(deck.PrimarySkill).Prototype.IconPath;
                _imageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
            }
            if(deck.SecondarySkill == Enumerators.Skill.NONE)
            {
                _imageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
            }
            else
            {
                string iconPath = overlord.GetSkill(deck.SecondarySkill).Prototype.IconPath;
                _imageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
            }

            _imageAbilitiesPanel.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/OverlordAbilitiesPanel/abilities_button_"+overlord.Prototype.Faction.ToString().ToLower());
        }

        private void UpdateEditDeckCardsAmount()
        {
            Deck currentDeck = _myDeckPage.CurrentEditDeck;
            if (currentDeck != null)
            {
                if (_tutorialManager.IsTutorial)
                {
                    _textEditDeckCardsAmount.text = currentDeck.GetNumCards() + " / " + _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount;
                }
                else
                {
                    _textEditDeckCardsAmount.text = currentDeck.GetNumCards() + " / " + Constants.DeckMaxSize;
                }
            }
        }

        private void UpdateDeckCardPage(int newPageIndex)
        {
            newPageIndex = Mathf.Clamp(newPageIndex, 0, GetDeckPageAmount(_myDeckPage.CurrentEditDeck) - 1);
            if (newPageIndex == _deckPageIndex)
                return;

            _deckPageIndex = newPageIndex;
            UpdateDeckCardPage();
        }

        private void UpdateDeckCardPage()
        {
            /*

            int startIndex = _deckPageIndex * GetDeckCardAmountPerPage();
            int endIndex = Mathf.Min
            (
                (_deckPageIndex + 1) * GetDeckCardAmountPerPage(),
                _myDeckPage.CurrentEditDeck.Cards.Count
            );
            List<MouldId> cardMouldIds = new List<MouldId>();
            for (int i = startIndex; i < endIndex; ++i)
            {
                cardMouldIds.Add
                (
                    _myDeckPage.CurrentEditDeck.Cards[i].MouldId
                );
            }
            UpdateDeckBoardCardDisplayList(cardMouldIds);

            for (int i = 0; i < _displayDeckBoardCards.Count; ++i)
            {
                _displayDeckBoardCards[i].Transform.position = DeckCardPositions[i].position;
            }*/
        }

        private void UpdateDeckBoardCardDisplayList(List<MouldId> cardMouldIds)
        {
            if(_displayDeckBoardCards == null)
            {
                _displayDeckBoardCards = new List<BoardCardView>();
            }
            else
            {
                foreach(BoardCardView item in _displayDeckBoardCards)
                {
                    item.GameObject.SetActive(false);
                }
                _displayDeckBoardCards.Clear();
            }

            BoardCardView boardCard;
            foreach(MouldId cardMouldId in cardMouldIds)
            {
                boardCard = _deckBoardCardsPool.Find(x => x.Model.Card.Prototype.MouldId == cardMouldId);
                if(boardCard == null)
                {
                    boardCard = CreateDeckCardToPool
                    (
                        _myDeckPage.CurrentEditDeck.Cards.Find(x => x.MouldId == cardMouldId),
                        _myDeckPage.LocatorDeckCards.GetComponent<RectTransform>()
                    );
                }
                boardCard.GameObject.SetActive(true);
                _displayDeckBoardCards.Add(boardCard);
            }
        }

        private void MoveDeckPageIndex(int direction)
        {
            int newIndex = Mathf.Clamp(_deckPageIndex + direction, 0, Mathf.Max(0, GetDeckPageAmount(_myDeckPage.CurrentEditDeck) - 1));

            if (newIndex == _deckPageIndex)
                return;

            _deckPageIndex = newIndex;
            UpdateDeckCardPage();
        }

        private void ResetDeckPageState()
        {
            _deckPageIndex = 0;
            UpdateDeckCardPage();
            UpdateDeckCardsDisplay();
        }

        private void MoveCollectionPageIndex(int direction)
        {
            int newIndex = Mathf.Clamp(_collectionPageIndex + direction, 0, GetCollectionPageAmount() - 1);
            if (newIndex == _collectionPageIndex)
                return;

            _collectionPageIndex = newIndex;
            LoadCollectionsCards();
        }

        private void UpdateCollectionPageIndex(int newCollectionIndex)
        {
            newCollectionIndex = Mathf.Clamp(newCollectionIndex, 0, GetCollectionPageAmount() - 1);
            if (newCollectionIndex == _collectionPageIndex)
                return;

            _collectionPageIndex = newCollectionIndex;
            LoadCollectionsCards();
        }

        private void ResetCollectionPageState()
        {
            _collectionPageIndex = 0;

            UpdateAvailableCollectionCards();
            LoadCollectionsCards();
        }

        private List<Enumerators.Faction> SortFactionList(List<Enumerators.Faction> factions, Enumerators.Faction firstFaction)
        {
            if (!factions.Contains(firstFaction))
            {
                return factions.ToList();
            }

            int index = factions.IndexOf(firstFaction);
            if(index == 0)
            {
                return factions.ToList();
            }

            List<Enumerators.Faction> resultList = factions.ToList();
            Enumerators.Faction tmpFaction = resultList[0];
            resultList[0] = firstFaction;
            resultList[index] = tmpFaction;

            return resultList;
        }

        private List<Enumerators.Faction> ExcludeFactionFromList(List<Enumerators.Faction> factions, Enumerators.Faction excludeFaction)
        {
            if (!factions.Contains(excludeFaction))
            {
                return factions.ToList();
            }

            List<Enumerators.Faction> resultList = factions.ToList();
            resultList.Remove(excludeFaction);
            return resultList;
        }

        private void UpdateAvailableCollectionCards()
        {
            /*string keyword = _inputFieldSearchName.text.Trim();

            if (string.IsNullOrEmpty(keyword))
                UpdateCollectionCardsByFilter();
            else
                UpdateCollectionCardsByKeyword();

            if (!CheckIfAnyCacheCollectionCardsExist() && !_tutorialManager.IsTutorial)
            {
                _myDeckPage.OpenAlertDialog("No cards found with that search.");
                ResetSearchAndFilterResult();
            }*/
        }

        private void UpdateCollectionCardsByKeyword()
        {
            List<Card> resultList = new List<Card>();
            List<Card> cards;

            string keyword = _inputFieldSearchName.text.Trim().ToLower();

            foreach (Enumerators.Faction faction in _availableFaction)
            {
                cards = _tutorialManager.IsTutorial ?
                    _tutorialManager.GetSpecificCardsBySet(faction) :
                    SetTypeUtility.GetCardFaction(_dataManager, faction).Cards.ToList();

                foreach (Card card in cards)
                {
                    if (card.Name.Trim().ToLower().Contains(keyword))
                    {
                        resultList.Add(card);
                    }
                }
            }

            UpdateCacheFilteredCardList(resultList);
        }


        private bool CheckIfAnyCacheCollectionCardsExist()
        {
            return _cacheCollectionCardsList.Count > 0;
        }

        private void ResetSearchAndFilterResult()
        {
            //_cardFilterPopup.FilterData.Reset();
            _inputFieldSearchName.text = "";
            ResetCollectionPageState();
        }

        private void UpdateCacheFilteredCardList(List<Card> cardList)
        {
            _cacheCollectionCardsList = cardList.FindAll(card => !card.Hidden).ToList();
            UpdateCollectionPageIndexDictionary();
        }

        private void ResetCollectionsBoardCards()
        {
            foreach (BoardCardView item in _collectionBoardCardsPool)
            {
                item.Dispose();
            }

            _displayCollectionsBoardCards.Clear();
            _collectionBoardCardsPool.Clear();
        }

        private int GetDeckPageAmount(Deck deck)
        {
            return Mathf.CeilToInt((float) deck.Cards.Count / GetDeckCardAmountPerPage());
        }

        private int GetCollectionPageAmount()
        {
            return Mathf.CeilToInt((float) _cacheCollectionCardsList.Count / GetCollectionCardAmountPerPage());
        }

        private int GetDeckCardAmountPerPage()
        {
            return 0; //DeckCardPositions.Count;
        }

        private int GetCollectionCardAmountPerPage()
        {
            return 0; //CollectionsCardPositions.Count;
        }

        private void ResetDeckBoardCards()
        {
            foreach (BoardCardView item in _deckBoardCardsPool)
            {
                item.Dispose();
            }

            _displayDeckBoardCards.Clear();
            _deckBoardCardsPool.Clear();
        }

        private void DisposeBoardCards()
        {
            ResetCollectionsBoardCards();
            ResetDeckBoardCards();
        }

        private uint GetMaxCopiesValue(IReadOnlyCard card)
        {
            Enumerators.CardRank rank = card.Rank;
            uint maxCopies;

            if (card.Faction == Enumerators.Faction.ITEM)
            {
                maxCopies = Constants.CardItemMaxCopies;
                return maxCopies;
            }

            switch (rank)
            {
                case Enumerators.CardRank.MINION:
                    maxCopies = Constants.CardMinionMaxCopies;
                    break;
                case Enumerators.CardRank.OFFICER:
                    maxCopies = Constants.CardOfficerMaxCopies;
                    break;
                case Enumerators.CardRank.COMMANDER:
                    maxCopies = Constants.CardCommanderMaxCopies;
                    break;
                case Enumerators.CardRank.GENERAL:
                    maxCopies = Constants.CardGeneralMaxCopies;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            return maxCopies;
        }

        private void SaveDeck(HordeSelectionWithNavigationPage.Tab nextTab)
        {
            _nextTab = nextTab;

            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
            if(_myDeckPage.IsEditingNewDeck)
            {
                deckGeneratorController.FinishAddDeck += FinishAddDeck;
                _myDeckPage.CurrentEditDeck.OverlordId = _myDeckPage.CurrentEditOverlord.Prototype.Id;
                _myDeckPage.CurrentEditDeck.PrimarySkill = _myDeckPage.SelectOverlordSkillTab.SelectedPrimarySkill;
                _myDeckPage.CurrentEditDeck.SecondarySkill = _myDeckPage.SelectOverlordSkillTab.SelectedSecondarySkill;

                deckGeneratorController.ProcessAddDeck(_myDeckPage.CurrentEditDeck);
            }
            else
            {
                deckGeneratorController.FinishEditDeck += FinishEditDeck;
                deckGeneratorController.ProcessEditDeck(_myDeckPage.CurrentEditDeck);
            }
        }

        public void RenameDeck(string newName)
        {
            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();

            string previousDeckName = _myDeckPage.IsEditingNewDeck ? "" : _myDeckPage.CurrentEditDeck.Name;

            if (!deckGeneratorController.VerifyDeckName(newName,previousDeckName))
                return;

            _myDeckPage.CurrentEditDeck.Name = newName;

            if(_myDeckPage.IsEditingNewDeck)
            {
                _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.Editing);
            }
            else
            {
                HordeSelectionWithNavigationPage.Tab tab = _myDeckPage.IsRenameWhileEditing ?
                    HordeSelectionWithNavigationPage.Tab.Editing :
                    HordeSelectionWithNavigationPage.Tab.SelectDeck;
                SaveDeck(tab);
                _myDeckPage.IsRenameWhileEditing = false;
            }
        }

        private void PlayAddCardSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_ADD_CARD,
                Constants.SfxSoundVolume, false, false, true);
        }

        private void PlayRemoveCardSound()
        {
             GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_REMOVE_CARD,
                Constants.SfxSoundVolume, false, false, true);
        }

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
    }
}
