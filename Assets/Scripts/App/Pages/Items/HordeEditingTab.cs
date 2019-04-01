using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using DG.Tweening;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using System.IO;
using Loom.ZombieBattleground.Protobuf;
using Card = Loom.ZombieBattleground.Data.Card;
using Deck = Loom.ZombieBattleground.Data.Deck;
using Faction = Loom.ZombieBattleground.Data.Faction;

namespace Loom.ZombieBattleground
{
    public class HordeEditingTab
    {
        private static readonly ILog Log = Logging.GetLog(nameof(HordeEditingTab));

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private IAnalyticsManager _analyticsManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private HordeSelectionWithNavigationPage _myDeckPage;

        private CardFilterPopup _cardFilterPopup;

        public List<Transform> CollectionsCardPositions,
                               DeckCardPositions;

        public GameObject CardCreaturePrefab,
                          CardItemPrefab,
                          CollectionsCardPlaceholdersPrefab,
                          DeckCardPlaceholdersPrefab;

        public GameObject CollectionsCardPlaceholders,
                          DeckCardPlaceholders;

        private List<BoardCardView> _createdDeckBoardCards,
                                _createdCollectionsBoardCards;

        private GameObject _selfPage;

        private CollectionData _collectionData;

        private GameObject _draggingObject;

        private Button _buttonFilter,
                       _buttonUpperLeftArrow,
                       _buttonUpperRightArrow,
                       _buttonLowerLeftArrow,
                       _buttonLowerRightArrow,
                       _buttonSaveDeck,
                       _buttonAbilities,
                       _buttonAuto,
                       _buttonRename;

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

        private const float BoardCardScale = 0.2756f;

        private Dictionary<string, int> _cacheDeckPageIndexDictionary;
        private Dictionary<string, int> _cacheCollectionPageIndexDictionary;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            InitBoardCardPrefabsAndLists();

            _myDeckPage = _uiManager.GetPage<HordeSelectionWithNavigationPage>();
            _myDeckPage.EventChangeTab += (HordeSelectionWithNavigationPage.Tab tab) =>
            {
                if (tab != HordeSelectionWithNavigationPage.Tab.Editing)
                    return;

                _inputFieldSearchName.text = "";
                UpdateDeckPageIndexDictionary();
                FillCollectionData();
                SubtractInitialDeckCardsAmountFromCollections(_myDeckPage.CurrentEditDeck);
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
            };           

            _cacheCollectionCardsList = new List<Card>();
            _cacheDeckPageIndexDictionary = new Dictionary<string, int>();
            _cacheCollectionPageIndexDictionary = new Dictionary<string, int>();
        }

        public void Show(GameObject selfPage)
        {
            _selfPage = selfPage;

            _cardFilterPopup = _uiManager.GetPopup<CardFilterPopup>();

            _textEditDeckName = _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();
            _textEditDeckCardsAmount = _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Lower_Items/Image_CardCounter/Text_CardsAmount").GetComponent<TextMeshProUGUI>();
            
            _buttonRename = _textEditDeckName.GetComponent<Button>();
            _buttonRename.onClick.AddListener(ButtonRenameHandler);

            _buttonFilter = _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Upper_Items/Button_Filter").GetComponent<Button>();
            _buttonFilter.onClick.AddListener(ButtonEditDeckFilterHandler);

            _buttonUpperLeftArrow = _selfPage.transform.Find("Tab_Editing/Panel_Content/Button_UpperLeftArrow").GetComponent<Button>();
            _buttonUpperLeftArrow.onClick.AddListener(ButtonEditDeckUpperLeftArrowHandler);

            _buttonUpperRightArrow = _selfPage.transform.Find("Tab_Editing/Panel_Content/Button_UpperRightArrow").GetComponent<Button>();
            _buttonUpperRightArrow.onClick.AddListener(ButtonEditDeckUpperRightArrowHandler);

            _buttonLowerLeftArrow = _selfPage.transform.Find("Tab_Editing/Panel_Content/Button_LowerLeftArrow").GetComponent<Button>();
            _buttonLowerLeftArrow.onClick.AddListener(ButtonEditDeckLowerLeftArrowHandler);

            _buttonLowerRightArrow = _selfPage.transform.Find("Tab_Editing/Panel_Content/Button_LowerRightArrow").GetComponent<Button>();
            _buttonLowerRightArrow.onClick.AddListener(ButtonEditDeckLowerRightArrowHandler);

            _buttonSaveDeck = _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Lower_Items/Button_SaveDeck").GetComponent<Button>();
            _buttonSaveDeck.onClick.AddListener(ButtonSaveEditDeckHandler);

            _buttonAbilities = _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Upper_Items/Button_OverlordAbilities").GetComponent<Button>();
            _buttonAbilities.onClick.AddListener(ButtonOverlordAbilitiesHandler);

            _buttonAuto = _selfPage.transform.Find("Panel_Frame/Image_ButtonAutoTray/Button_Auto").GetComponent<Button>();
            _buttonAuto.onClick.AddListener(ButtonAutoHandler);

            _inputFieldSearchName = _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Upper_Items/InputText_Search").GetComponent<TMP_InputField>();
            _inputFieldSearchName.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            _inputFieldSearchName.text = "";

            _imageAbilityIcons = new Image[]
            {
                _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Upper_Items/Button_OverlordAbilities/Image_SkillIcon_1").GetComponent<Image>(),
                _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Upper_Items/Button_OverlordAbilities/Image_SkillIcon_2").GetComponent<Image>()
            };

            _imageAbilitiesPanel = _selfPage.transform.Find("Tab_Editing/Panel_FrameComponents/Upper_Items/Button_OverlordAbilities").GetComponent<Image>();

            LoadBoardCardComponents();

            _myDeckPage.DragAreaCollections.Scrolled += ScrollCollectionAreaHandler;
            _myDeckPage.DragAreaDeck.Scrolled += ScrollDeckAreaHandler;
        }

        public void Update()
        {

        }

        public void Dispose()
        {
            DisposeBoardCards();

            if (_draggingObject != null)
            {
                Object.Destroy(_draggingObject);
                _draggingObject = null;
                _isDragging = false;
            }

            _cacheCollectionCardsList.Clear();
            _imageAbilityIcons = null;
        }

        private void InitBoardCardPrefabsAndLists()
        {
            CardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            CardItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard");
            CollectionsCardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersMyDecksLower");
            DeckCardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersMyDecksUpper");

            _createdDeckBoardCards = new List<BoardCardView>();
            _createdCollectionsBoardCards = new List<BoardCardView>();

            _collectionData = new CollectionData();
            _collectionData.Cards = new List<CollectionCardData>();
        }

        #region Button Handlers

        private void ScrollDeckAreaHandler(Vector2 scrollDelta)
        {
            if (_tutorialManager.BlockAndReport(_buttonUpperLeftArrow.name) ||
                _tutorialManager.BlockAndReport(_buttonUpperRightArrow.name))
                return;

            ScrollCardList(true, scrollDelta);
        }

        private void ScrollCollectionAreaHandler(Vector2 scrollDelta)
        {
            if (_tutorialManager.BlockAndReport(_buttonLowerRightArrow.name) ||
                _tutorialManager.BlockAndReport(_buttonLowerLeftArrow.name))
                return;

            ScrollCardList(false, scrollDelta);
        }

        private void ButtonRenameHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonRename.name))
                return;

            PlayClickSound();
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmSaveDeckHandler;
            _uiManager.DrawPopup<QuestionPopup>("Do you want to save the current deck editing progress?");
        }
        
        private void ConfirmSaveDeckHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmSaveDeckHandler;
            
            if (status)
            {                
                SaveDeck(HordeSelectionWithNavigationPage.Tab.Rename);
            }
            else
            {                
                _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.Rename);        
            }  
        }

        private void ButtonEditDeckFilterHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonFilter.name))
                return;

            PlayClickSound();
            _uiManager.DrawPopup<CardFilterPopup>();
            CardFilterPopup popup = _uiManager.GetPopup<CardFilterPopup>();
            popup.ActionPopupHiding += FilterPopupHidingHandler;
        }

        private void FilterPopupHidingHandler(CardFilterPopup.CardFilterData cardFilterData)
        {
            ResetCollectionPageState();
            CardFilterPopup popup = _uiManager.GetPopup<CardFilterPopup>();
            popup.ActionPopupHiding -= FilterPopupHidingHandler;
        }

        private void ButtonEditDeckUpperLeftArrowHandler()
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
        }

        private void ButtonSaveEditDeckHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonSaveDeck.name))
                return;

            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.HordeSaveButtonPressed);

            PlayClickSound();

            SaveDeck(HordeSelectionWithNavigationPage.Tab.SelectDeck);
        }
        
        private void FinishAddDeck(bool success, Deck deck)
        {
            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().FinishAddDeck -= FinishAddDeck;
            _myDeckPage.IsEditingNewDeck = false;
            _myDeckPage.SelectDeckIndex = _myDeckPage.GetDeckList().IndexOf(_myDeckPage.CurrentEditDeck);
            _myDeckPage.AssignCurrentDeck();
            _myDeckPage.ChangeTab(_nextTab);
        }
        
        private void FinishEditDeck(bool success, Deck deck)
        {
            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().FinishEditDeck -= FinishEditDeck; 
            _myDeckPage.ChangeTab(_nextTab);
        }

        private void ButtonOverlordAbilitiesHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonAbilities.name))
                return;

            PlayClickSound();
            _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SelecOverlordSkill);
        }

        private void ButtonAutoHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonAuto.name))
                return;

            PlayClickSound();
            FillCollectionData();
            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().GenerateCardsToDeck
            (
                _myDeckPage.CurrentEditDeck,
                _collectionData
            );
            UpdateDeckPageIndexDictionary();

            ResetCollectionPageState();
            ResetDeckPageState();
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
            DeckCardPlaceholders = Object.Instantiate(DeckCardPlaceholdersPrefab);
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
            }
        }

        private void FillCollectionData()
        {
            _collectionData.Cards.Clear();
            CollectionCardData cardData;

            List<CollectionCardData> data;
            if (_tutorialManager.IsTutorial)
            {
                data = _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.CardsForArmy;
            }
            else
            {
                data = _dataManager.CachedCollectionData.Cards;
            }

            foreach (CollectionCardData card in data)
            {
                cardData = new CollectionCardData();
                cardData.Amount = card.Amount;
                cardData.CardName = card.CardName;

                _collectionData.Cards.Add(cardData);
            }
        }

        public void LoadCollectionsCards()
        {
            List<Card> cards = _cacheCollectionCardsList.ToList();
            ResetCollectionsBoardCards();

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
                    cardData = _tutorialManager.GetCardData(card.Name);
                }
                else
                {
                    cardData = _dataManager.CachedCollectionData.GetCardData(card.Name);
                }

                BoardCardView boardCard = CreateBoardCard
                (
                    card,
                    rectContainer,
                    CollectionsCardPositions[i % CollectionsCardPositions.Count].position,
                    BoardCardScale
                );
                _createdCollectionsBoardCards.Add(boardCard);

                OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();

                eventHandler.DragBegan += BoardCardDragBeganHandler;
                eventHandler.DragEnded += BoardCardCollectionDragEndedHandler;
                eventHandler.DragUpdated += BoardCardDragUpdatedHandler;

                DeckBuilderCard deckBuilderCard = boardCard.GameObject.AddComponent<DeckBuilderCard>();
                deckBuilderCard.Page = this;
                deckBuilderCard.Card = boardCard.Model.Card.Prototype;
                deckBuilderCard.IsHordeItem = false;

                collectionCardData = _collectionData.GetCardData(card.Name);
                UpdateBoardCardAmount
                (
                    true,
                    card.Name,
                    collectionCardData.Amount
                );
            }
        }

        public void UpdateBoardCardAmount(bool init, string cardId, int amount)
        {
            foreach (BoardCardView card in _createdCollectionsBoardCards)
            {
                if (card.Model.Card.Prototype.Name == cardId)
                {
                    card.SetAmountOfCardsInEditingPage
                    (
                        init,
                        GetMaxCopiesValue(card.Model.Card.Prototype),
                        amount,
                        BoardCardView.AmountTrayType.Counter                        
                    );
                    break;
                }
            }
        }
        
        private void SubtractInitialDeckCardsAmountFromCollections(Deck deck)
        {
            foreach(DeckCardData card in deck.Cards)
            {
                _collectionData.GetCardData(card.CardName).Amount -= card.Amount;
            }
        }
        
        private void UpdateDeckPageIndexDictionary()
        {
            _cacheDeckPageIndexDictionary.Clear();
            int page = 0;
            int count = 0;
            foreach(DeckCardData card in _myDeckPage.CurrentEditDeck.Cards)
            {
                _cacheDeckPageIndexDictionary.Add(card.CardName, page);

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
                _cacheCollectionPageIndexDictionary.Add(card.Name, page);

                ++count;
                if(count >= GetCollectionCardAmountPerPage())
                {
                    count = 0;
                    ++page;
                }
            }
        }

        public void LoadDeckCards(Deck deck)
        {
            ResetDeckBoardCards();
            

            int startIndex = _deckPageIndex * GetDeckCardAmountPerPage();
            int endIndex = Mathf.Min
            (
                (_deckPageIndex + 1) * GetDeckCardAmountPerPage(),
                deck.Cards.Count
            );

            RectTransform rectContainer = _myDeckPage.LocatorDeckCards.GetComponent<RectTransform>();
            for (int i = startIndex; i < endIndex; ++i)
            {
                DeckCardData card = deck.Cards[i];
                Card prototype = _dataManager.CachedCardsLibraryData.GetCardFromName(card.CardName);

                BoardCardView boardCard = CreateBoardCard
                (
                    prototype,
                    rectContainer,
                    Vector3.zero,
                    BoardCardScale
                );

                _createdDeckBoardCards.Add(boardCard);

                boardCard.Transform.position = DeckCardPositions[_createdDeckBoardCards.Count - 1].position;

                boardCard.SetAmountOfCardsInEditingPage
                (
                    true,
                    GetMaxCopiesValue(prototype),
                    card.Amount, 
                    BoardCardView.AmountTrayType.Radio
                );
                    
                OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();

                eventHandler.DragBegan += BoardCardDragBeganHandler;
                eventHandler.DragEnded += BoardCardDeckDragEndedHandler;
                eventHandler.DragUpdated += BoardCardDragUpdatedHandler;

                DeckBuilderCard deckBuilderCard = boardCard.GameObject.AddComponent<DeckBuilderCard>();
                deckBuilderCard.Page = this;
                deckBuilderCard.Card = boardCard.Model.Card.Prototype;
                deckBuilderCard.IsHordeItem = true;                
            }
            
            UpdateEditDeckCardsAmount();
        }

        public void AddCardToDeck(IReadOnlyCard card, bool animate = false)
        {
            if (_myDeckPage.CurrentEditDeck == null)
                return;

            OverlordModel overlordData = _dataManager.CachedOverlordData.Overlords[_myDeckPage.CurrentEditDeck.OverlordId];
            if (FactionAgainstDictionary[overlordData.Faction] == card.Faction)
            {
                _myDeckPage.OpenAlertDialog(
                    "It's not possible to add cards to the deck \n from the faction from which the hero is weak against");
                return;
            }

            CollectionCardData collectionCardData = _collectionData.GetCardData(card.Name);
            if (collectionCardData.Amount == 0)
            {
                _myDeckPage.OpenAlertDialog(
                    "You don't have enough cards of this type. \n Buy or earn new packs to get more cards!");
                return;
            }

            DeckCardData existingCards = _myDeckPage.CurrentEditDeck.Cards.Find(x => x.CardName == card.Name);

            uint maxCopies = GetMaxCopiesValue(card);

            if (existingCards != null && existingCards.Amount == maxCopies)
            {
                _myDeckPage.OpenAlertDialog("You cannot have more than " + maxCopies + " copies of the " +
                    card.Rank.ToString().ToLowerInvariant() + " card in your deck.");
                return;
            }

            if (_myDeckPage.CurrentEditDeck.GetNumCards() == Constants.DeckMaxSize)
            {
                _myDeckPage.OpenAlertDialog("You can not add more than " + Constants.DeckMaxSize + " Cards in a single Horde.");
                return;
            }

            collectionCardData.Amount--;
            UpdateBoardCardAmount(false, card.Name, collectionCardData.Amount);
            bool isCardAlreadyExist = _myDeckPage.CurrentEditDeck.Cards.Exists(x => x.CardName == card.Name);
            _myDeckPage.CurrentEditDeck.AddCard(card.Name);
            UpdateDeckPageIndexDictionary();
            
            if (_createdDeckBoardCards.Exists(item => item.Model.Card.Prototype.MouldId == card.MouldId))
            {
                BoardCardView foundItem = _createdDeckBoardCards.Find(item => item.Model.Card.Prototype.MouldId == card.MouldId);
                foundItem.SetAmountOfCardsInEditingPage
                (
                    false,
                    GetMaxCopiesValue(card),
                    _myDeckPage.CurrentEditDeck.Cards.Find(x => x.CardName == foundItem.Model.Card.Prototype.Name).Amount,
                    BoardCardView.AmountTrayType.Radio
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
                        _createdCollectionsBoardCards.Find
                        (
                            x => x.Model.Card.Prototype.MouldId == card.MouldId
                        ).GameObject.transform.position,
                        BoardCardScale
                    ),
                    _createdDeckBoardCards.Find(x => x.Model.Prototype.MouldId == card.MouldId),
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
            CollectionCardData collectionCardData = _collectionData.GetCardData(card.Name);
            collectionCardData.Amount++;
            UpdateBoardCardAmount
            (
                false,
                card.Name,
                collectionCardData.Amount
            );
            
            _myDeckPage.CurrentEditDeck.RemoveCard(card.Name);
            UpdateDeckPageIndexDictionary();
            
            if(_createdDeckBoardCards.Exists(item => item.Model.Card.Prototype.MouldId == card.MouldId))
            {
                BoardCardView boardCard = _createdDeckBoardCards.Find(item => item.Model.Card.Prototype.MouldId == card.MouldId);
                boardCard.CardsAmountDeckEditing--;
                
                if (boardCard.CardsAmountDeckEditing == 0)
                {
                    int deckPagesAmount = GetDeckPageAmount(_myDeckPage.CurrentEditDeck) - 1;
                    _deckPageIndex = deckPagesAmount < 0 ? 0 : Mathf.Min(_deckPageIndex, deckPagesAmount);
        
                    UpdateDeckCardPage();
                }
                else
                {
                    boardCard.SetAmountOfCardsInEditingPage
                    (
                        false, 
                        GetMaxCopiesValue(boardCard.Model.Card.Prototype), 
                        boardCard.CardsAmountDeckEditing, 
                        BoardCardView.AmountTrayType.Radio
                    );
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
                        _createdCollectionsBoardCards.Find(x => x.Model.Prototype.MouldId == card.MouldId),
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
            BoardUnitModel boardUnitModel = new BoardUnitModel(new WorkingCard(card, card, null));
            int amount = _collectionData.GetCardData(card.Name).Amount;

            switch (card.Kind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(CardCreaturePrefab);
                    boardCard = new UnitBoardCard(go, boardUnitModel);
                    break;
                case Enumerators.CardKind.ITEM:
                    go = Object.Instantiate(CardItemPrefab);
                    boardCard = new ItemBoardCard(go, boardUnitModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(card.Kind), card.Kind, null);
            }

            boardCard.SetAmount(BoardCardView.AmountTrayType.None,amount);
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
                cardList = _createdDeckBoardCards.Select(i => i.Model.Card.Prototype).ToList();
                popupType = CardInfoWithSearchPopup.PopupType.REMOVE_CARD;
            }
            else
            {
                cardList = _createdCollectionsBoardCards.Select(i => i.Model.Card.Prototype).ToList();
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
            _draggingObject.transform.Find("DeckEditingGroupUI").gameObject.SetActive(false);
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

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject == _myDeckPage.DragAreaDeck.gameObject)
                    {
                        BoardCardView boardCard = _createdCollectionsBoardCards.Find(x =>
                            x.GameObject.GetInstanceID().ToString() == _draggingObject.name);

                        PlayAddCardSound();
                        AddCardToDeck(boardCard.Model.Card.Prototype);

                        GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.CardDragged);
                    }
                }
            }

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

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject == _myDeckPage.DragAreaCollections.gameObject)
                    {
                        BoardCardView boardCard = _createdDeckBoardCards.Find(x =>
                            x.GameObject.GetInstanceID().ToString() == _draggingObject.name);

                        PlayRemoveCardSound();
                        RemoveCardFromDeck(boardCard.Model.Card.Prototype);
                    }
                }
            }

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
            OverlordModel overlord = _dataManager.CachedOverlordData.Overlords[_myDeckPage.CurrentEditDeck.OverlordId];
            if(deck.PrimarySkill == Enumerators.Skill.NONE)
            {
                _imageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
            }
            else
            {
                string iconPath = overlord.GetSkill(deck.PrimarySkill).IconPath;
                _imageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
            }
            if(deck.SecondarySkill == Enumerators.Skill.NONE)
            {
                _imageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
            }
            else
            {
                string iconPath = overlord.GetSkill(deck.SecondarySkill).IconPath;
                _imageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
            }

            _imageAbilitiesPanel.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/OverlordAbilitiesPanel/abilities_button_"+overlord.Faction.ToString().ToLower());
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
            LoadDeckCards(_myDeckPage.CurrentEditDeck);
        }

        private void UpdateDeckCardPage()
        {
            LoadDeckCards(_myDeckPage.CurrentEditDeck);
        }

        private void MoveDeckPageIndex(int direction)
        {
            int newIndex = Mathf.Clamp(_deckPageIndex + direction, 0, GetDeckPageAmount(_myDeckPage.CurrentEditDeck) - 1);
            if (newIndex == _deckPageIndex)
                return;
                
            _deckPageIndex = newIndex;
            UpdateDeckCardPage();
        }

        private void ResetDeckPageState()
        {
            _deckPageIndex = 0;
            UpdateDeckCardPage();
        }

        public void MoveCollectionPageIndex(int direction)
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
            OverlordModel overlordModel = _dataManager.CachedOverlordData.Overlords[_myDeckPage.CurrentEditDeck.OverlordId];
            Enumerators.Faction againstFaction = FactionAgainstDictionary[overlordModel.Faction];

            _availableFaction = _cardFilterPopup.FilterData.GetFilteredFactionList();
            _availableFaction = ExcludeFactionFromList
            (
                _availableFaction,
                againstFaction
            );
            
            Enumerators.Faction firstFaction = _tutorialManager.IsTutorial ?
                _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MainSet :
                overlordModel.Faction;
                
            _availableFaction = SortFactionList
            (
                _availableFaction,
                firstFaction
            );
            
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
            string keyword = _inputFieldSearchName.text.Trim();

            if (string.IsNullOrEmpty(keyword))
                UpdateCollectionCardsByFilter();
            else
                UpdateCollectionCardsByKeyword();

            if (!CheckIfAnyCacheCollectionCardsExist() && !_tutorialManager.IsTutorial)
            {
                _myDeckPage.OpenAlertDialog("Sorry, you can't add zombies from that faction to this deck");
                ResetSearchAndFilterResult();
            }
        }

        private void UpdateCollectionCardsByKeyword()
        {
            string keyword = _inputFieldSearchName.text.Trim().ToLower();
            List<Card> resultList = new List<Card>();
            List<Enumerators.Faction> allAvailableFactionList = _cardFilterPopup.AllAvailableFactionList;
            OverlordModel overlordModel = _dataManager.CachedOverlordData.Overlords[_myDeckPage.CurrentEditDeck.OverlordId];
            Enumerators.Faction againstFaction = FactionAgainstDictionary[overlordModel.Faction];
            allAvailableFactionList.Remove(againstFaction);
            foreach (Enumerators.Faction item in allAvailableFactionList)
            {
                List<Card> cards;

                if (_tutorialManager.IsTutorial)
                {
                    cards = _tutorialManager.GetSpecificCardsBySet(item);
                }
                else
                {
                    Faction set = SetTypeUtility.GetCardFaction(_dataManager, item);
                    cards = cards = set.Cards.ToList();
                }

                foreach (Card card in cards)
                {
                    if (card.Name.ToLower().Contains(keyword))
                    {
                        resultList.Add(card);
                    }
                }
            }

            UpdateCacheFilteredCardList(resultList);
        }

        private void UpdateCollectionCardsByFilter()
        {
            List<Card> resultList = new List<Card>();
            foreach (Enumerators.Faction faction in _availableFaction)
            {
                List<Card> cards;
                if (_tutorialManager.IsTutorial)
                {
                    cards = _tutorialManager.GetSpecificCardsBySet(faction);
                }
                else
                {
                    Faction set = SetTypeUtility.GetCardFaction(_dataManager, faction);
                    cards = set.Cards.ToList();
                }

                foreach (Card card in cards)
                {
                    if
                    (
                        CheckIfSatisfyGooCostFilter(card) &&
                        CheckIfSatisfyRankFilter(card) &&
                        CheckIfSatisfyTypeFilter(card)
                    )
                    {
                        resultList.Add(card);
                    }
                }
            }
            UpdateCacheFilteredCardList(resultList);
        }

        private bool CheckIfSatisfyGooCostFilter(Card card)
        {
            if (card.Cost < 0)
                return false;

            if(card.Cost >= 10)
            {
                if (_cardFilterPopup.FilterData.GooCostList[10])
                    return true;
            }
            else
            {
                if(_cardFilterPopup.FilterData.GooCostList[card.Cost])
                    return true;
            }

            return false;
        }

        private bool CheckIfSatisfyRankFilter(Card card)
        {
            return _cardFilterPopup.FilterData.RankDictionary[card.Rank];
        }

        private bool CheckIfSatisfyTypeFilter(Card card)
        {
            return _cardFilterPopup.FilterData.TypeDictionary[card.Type];
        }

        private bool CheckIfAnyCacheCollectionCardsExist()
        {
            return _cacheCollectionCardsList.Count > 0;
        }

        private void ResetSearchAndFilterResult()
        {
            _cardFilterPopup.FilterData.Reset();
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
            foreach (BoardCardView item in _createdCollectionsBoardCards)
            {
                item.Dispose();
            }

            _createdCollectionsBoardCards.Clear();
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
            return DeckCardPositions.Count;
        }

        private int GetCollectionCardAmountPerPage()
        {
            return CollectionsCardPositions.Count;
        }

        private void ResetDeckBoardCards()
        {
            foreach (BoardCardView item in _createdDeckBoardCards)
            {
                item.Dispose();
            }

            _createdDeckBoardCards.Clear();
        }

        private void DisposeBoardCards()
        {
            ResetCollectionsBoardCards();
            ResetDeckBoardCards();
            Object.Destroy(CollectionsCardPlaceholders);
            Object.Destroy(DeckCardPlaceholders);
        }

        public uint GetMaxCopiesValue(IReadOnlyCard card)
        {
            Enumerators.CardRank rank = card.Rank;
            uint maxCopies;

            Enumerators.Faction faction = GameClient.Get<IGameplayManager>().GetController<CardsController>().GetSetOfCard(card);

            if (faction == Enumerators.Faction.ITEM)
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
        
        public void SaveDeck(HordeSelectionWithNavigationPage.Tab nextTab)
        {
            _nextTab = nextTab;
            
            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
            if(_myDeckPage.IsEditingNewDeck)
            {
                deckGeneratorController.FinishAddDeck += FinishAddDeck;
                deckGeneratorController.ProcessAddDeck
                (
                    _myDeckPage.CurrentEditDeck,
                    _myDeckPage.CurrentEditOverlord
                );
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
                SaveDeck(HordeSelectionWithNavigationPage.Tab.SelectDeck);            
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
