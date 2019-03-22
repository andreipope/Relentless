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

        private int _deckPageIndex;

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

        private int _currentCollectionPage,
                    _currentCollectionPagesAmount,
                    _currentCollectionFactionIndex;

        private const float BoardCardScale = 0.2756f;

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
                FillCollectionData();
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
            
            _myDeckPage.AssignCurrentDeck(false, true);
            if (status)
            {                
                ProcessEditDeck(_myDeckPage.CurrentEditDeck, HordeSelectionWithNavigationPage.Tab.Rename);
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
            ProcessEditDeck(_myDeckPage.CurrentEditDeck, HordeSelectionWithNavigationPage.Tab.SelectDeck);
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

            int startIndex = _currentCollectionPage * CollectionsCardPositions.Count;
            int endIndex = Mathf.Min(startIndex + CollectionsCardPositions.Count, cards.Count);

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

        public void LoadDeckCards(Deck deck)
        {
            ResetDeckBoardCards();

            RectTransform rectContainer = _myDeckPage.LocatorDeckCards.GetComponent<RectTransform>();
            foreach (DeckCardData card in deck.Cards)
            {
                Card prototype = _dataManager.CachedCardsLibraryData.GetCardFromName(card.CardName);

                bool itemFound = false;
                foreach (BoardCardView item in _createdDeckBoardCards)
                {
                    if (item.Model.Card.Prototype.Name == card.CardName)
                    {
                        itemFound = true;
                        break;
                    }
                }

                if (!itemFound)
                {
                    BoardCardView boardCard = CreateBoardCard
                    (
                        prototype,
                        rectContainer,
                        Vector3.zero,
                        BoardCardScale
                    );

                    _createdDeckBoardCards.Add(boardCard);

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

                    _collectionData.GetCardData(card.CardName).Amount -= card.Amount;

                    UpdateEditDeckCardsAmount();
                }
            }

            UpdateDeckCardPage();
        }

        public void AddCardToDeck(IReadOnlyCard card)
        {
            if (_myDeckPage.CurrentEditDeck == null)
                return;


            if (FactionAgainstDictionary[_myDeckPage.CurrentEditHero.HeroElement] == card.Faction)
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
                    card.CardRank.ToString().ToLowerInvariant() + " card in your deck.");
                return;
            }

            if (_myDeckPage.CurrentEditDeck.GetNumCards() == Constants.DeckMaxSize)
            {
                _myDeckPage.OpenAlertDialog("You can not add more than " + Constants.DeckMaxSize + " Cards in a single Horde.");
                return;
            }

            bool itemFound = false;
            BoardCardView foundItem = null;
            foreach (BoardCardView item in _createdDeckBoardCards)
            {
                if (item.Model.Card.Prototype.MouldId == card.MouldId)
                {
                    foundItem = item;
                    itemFound = true;

                    break;
                }
            }

            collectionCardData.Amount--;
            UpdateBoardCardAmount(false, card.Name, collectionCardData.Amount);


            if (!itemFound)
            {
                RectTransform rectContainer = _myDeckPage.LocatorDeckCards.GetComponent<RectTransform>();
                BoardCardView boardCard = CreateBoardCard
                (
                    card,
                    rectContainer,
                    Vector3.zero,
                    BoardCardScale
                );
                foundItem = boardCard;

                OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();

                eventHandler.DragBegan += BoardCardDragBeganHandler;
                eventHandler.DragEnded += BoardCardDeckDragEndedHandler;
                eventHandler.DragUpdated += BoardCardDragUpdatedHandler;

                DeckBuilderCard deckBuilderCard = boardCard.GameObject.AddComponent<DeckBuilderCard>();
                deckBuilderCard.Page = this;
                deckBuilderCard.Card = boardCard.Model.Prototype;
                deckBuilderCard.IsHordeItem = true;

                _createdDeckBoardCards.Add(boardCard);
            }

            _myDeckPage.CurrentEditDeck.AddCard(card.Name);

            foundItem.SetAmountOfCardsInEditingPage
            (
                false, 
                GetMaxCopiesValue(card),
                _myDeckPage.CurrentEditDeck.Cards.Find(x => x.CardName == foundItem.Model.Card.Prototype.Name).Amount,
                BoardCardView.AmountTrayType.Radio
            );

            UpdateDeckCardPage();
            UpdateEditDeckCardsAmount();

            if(_tutorialManager.IsTutorial && _myDeckPage.CurrentEditDeck.GetNumCards() >= _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount)
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

        public void RemoveCardFromDeck(IReadOnlyCard card)
        {
            CollectionCardData collectionCardData = _collectionData.GetCardData(card.Name);
            collectionCardData.Amount++;
            UpdateBoardCardAmount
            (
                false,
                card.Name,
                collectionCardData.Amount
            );

            BoardCardView boardCard = _createdDeckBoardCards.Find(item => item.Model.Card.Prototype.MouldId == card.MouldId);
            boardCard.CardsAmountDeckEditing--;
            _myDeckPage.CurrentEditDeck.RemoveCard(card.Name);

            if (boardCard.CardsAmountDeckEditing == 0)
            {
                _createdDeckBoardCards.Remove(boardCard);

                Object.DestroyImmediate(boardCard.GameObject);

                int currentDeckPage = _deckPageIndex;
                int deckPagesAmount = GetDeckPageAmount();
                if (currentDeckPage >= deckPagesAmount)
                {
                    _deckPageIndex = Mathf.Clamp(deckPagesAmount - 1, 0, deckPagesAmount);
                }
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

            UpdateDeckCardPage();
            UpdateEditDeckCardsAmount();
        }

        private BoardCardView CreateBoardCard(IReadOnlyCard card, RectTransform root, Vector3 position, float scale)
        {
            GameObject go;
            BoardCardView boardCard;
            BoardUnitModel boardUnitModel = new BoardUnitModel(new WorkingCard(card, card, null));
            int amount = _collectionData.GetCardData(card.Name).Amount;

            switch (card.CardKind)
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
                    throw new ArgumentOutOfRangeException(nameof(card.CardKind), card.CardKind, null);
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
            Hero hero = _dataManager.CachedHeroesData.Heroes[_myDeckPage.CurrentEditDeck.HeroId];
            if(deck.PrimarySkill == Enumerators.OverlordSkill.NONE)
            {
                _imageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
            }
            else
            {
                string iconPath = hero.GetSkill(deck.PrimarySkill).IconPath;
                _imageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
            }
            if(deck.SecondarySkill == Enumerators.OverlordSkill.NONE)
            {
                _imageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
            }
            else
            {
                string iconPath = hero.GetSkill(deck.SecondarySkill).IconPath;
                _imageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
            }

            _imageAbilitiesPanel.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/OverlordAbilitiesPanel/abilities_button_"+hero.HeroElement.ToString().ToLower());
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

        private void UpdateDeckCardPage()
        {
            int startIndex = _deckPageIndex * GetDeckCardAmountPerPage();
            int endIndex = (_deckPageIndex + 1) * GetDeckCardAmountPerPage();
            List<BoardCardView> displayCardList = new List<BoardCardView>();

            for ( int i = 0; i < _createdDeckBoardCards.Count; ++i)
            {
                if(i >= startIndex && i < endIndex)
                {
                    _createdDeckBoardCards[i].GameObject.SetActive(true);
                    displayCardList.Add(_createdDeckBoardCards[i]);
                }
                else
                {
                    _createdDeckBoardCards[i].GameObject.SetActive(false);
                }
            }
            for (int i = 0; i < displayCardList.Count; ++i)
            {
                displayCardList[i].Transform.position = DeckCardPositions[i].position;
            }
        }

        private void MoveDeckPageIndex(int direction)
        {
            _deckPageIndex = Mathf.Clamp(_deckPageIndex + direction, 0, GetDeckPageAmount() - 1);
            UpdateDeckCardPage();
        }

        private void ResetDeckPageState()
        {
            _deckPageIndex = 0;
            LoadDeckCards(_myDeckPage.CurrentEditDeck);
        }

        public void MoveCollectionPageIndex(int direction)
        {
            _currentCollectionPage += direction;

            if (_currentCollectionPage < 0)
            {
                _currentCollectionFactionIndex += direction;
                if(_currentCollectionFactionIndex < 0)
                {
                    _currentCollectionFactionIndex = _availableFaction.Count-1;
                }
                UpdateAvailableCollectionCards();
                _currentCollectionPage = Mathf.Max(_currentCollectionPagesAmount - 1, 0);

            }
            else if (_currentCollectionPage >= _currentCollectionPagesAmount)
            {
                 _currentCollectionFactionIndex += direction;
                if(_currentCollectionFactionIndex >= _availableFaction.Count)
                    _currentCollectionFactionIndex = 0;
                UpdateAvailableCollectionCards();
                _currentCollectionPage = 0;
            }

            LoadCollectionsCards();
        }

        private void ResetCollectionPageState()
        {
            ExcludeFilterDataWithAgainstFaction();
            _availableFaction = _cardFilterPopup.FilterData.GetFilterFactionList();
            if (_tutorialManager.IsTutorial)
            {
                _currentCollectionFactionIndex = _availableFaction.FindIndex(set => set == _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MainSet);
            }
            else
            {
                _currentCollectionFactionIndex = 0;
            }
            _currentCollectionPage = 0;
            UpdateAvailableCollectionCards();
            LoadCollectionsCards();
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
            Enumerators.Faction againstFaction = FactionAgainstDictionary[_myDeckPage.CurrentEditHero.HeroElement];
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
            if (_availableFaction.Count > _currentCollectionFactionIndex)
            {
                Enumerators.Faction faction = _availableFaction[_currentCollectionFactionIndex];
                
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
                        resultList.Add(card);
                }
            }
            else
            {
                _myDeckPage.OpenAlertDialog("Sorry, no matches faction found.");
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
            return _cardFilterPopup.FilterData.RankDictionary[card.CardRank];
        }

        private bool CheckIfSatisfyTypeFilter(Card card)
        {
            return _cardFilterPopup.FilterData.TypeDictionary[card.CardType];
        }

        private bool CheckIfAnyCacheCollectionCardsExist()
        {
            return _cacheCollectionCardsList.Count > 0;
        }

        private void ExcludeFilterDataWithAgainstFaction()
        {
            Enumerators.Faction againstFaction = FactionAgainstDictionary[_myDeckPage.CurrentEditHero.HeroElement];
            _cardFilterPopup.FilterData.FactionDictionary[againstFaction] = false;
        }

        private void ResetSearchAndFilterResult()
        {
            _cardFilterPopup.FilterData.Reset();
            _inputFieldSearchName.text = "";
            ResetCollectionPageState();
        }

        private void UpdateCacheFilteredCardList(List<Card> cardList)
        {
            _cacheCollectionCardsList = cardList.ToList();
            _currentCollectionPagesAmount = Mathf.CeilToInt
            (
                _cacheCollectionCardsList.Count / (float)CollectionsCardPositions.Count
            );
        }

        private void ResetCollectionsBoardCards()
        {
            foreach (BoardCardView item in _createdCollectionsBoardCards)
            {
                item.Dispose();
            }

            _createdCollectionsBoardCards.Clear();
        }

        private int GetDeckPageAmount()
        {
            return Mathf.CeilToInt((float) _createdDeckBoardCards.Count / GetDeckCardAmountPerPage());
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
            Enumerators.CardRank rank = card.CardRank;
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

        public async void ProcessEditDeck(Deck deckToSave, HordeSelectionWithNavigationPage.Tab nextTab)
        {
            _myDeckPage.ButtonSaveRenameDeck.interactable = false;
            _buttonSaveDeck.interactable = false;

            if (!VerifyDeckName(deckToSave.Name))
            {
                _myDeckPage.ButtonSaveRenameDeck.interactable = true;
                _buttonSaveDeck.interactable = true;
                return;
            }

            List<Deck> deckList = _myDeckPage.GetDeckList();
            foreach (Deck deck in deckList)
            {
                if (deckToSave.Id != deck.Id &&
                    deck.Name.Trim().Equals(deckToSave.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    _myDeckPage.ButtonSaveRenameDeck.interactable = true;
                    _buttonSaveDeck.interactable = true;
                    _myDeckPage.OpenAlertDialog("Not able to Edit Deck: \n Deck Name already exists.");
                    return;
                }
            }

            bool success = true;
            try
            {
                await _backendFacade.EditDeck(_backendDataControlMediator.UserDataModel.UserId, deckToSave);

                for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count; i++)
                {
                    if (_dataManager.CachedDecksData.Decks[i].Id == deckToSave.Id)
                    {
                        _dataManager.CachedDecksData.Decks[i] = deckToSave;
                        break;
                    }
                }

                _analyticsManager.SetEvent(AnalyticsManager.EventDeckEdited);
                Log.Info(" ====== Edit Deck Successfully ==== ");
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogExceptionAsWarning(Log, e);

                success = false;

                if (e is Client.RpcClientException || e is TimeoutException)
                {
                    GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true);
                }
                else
                {
                    string message = e.Message;

                    string[] description = e.Message.Split('=');
                    if (description.Length > 0)
                    {
                        message = description[description.Length - 1].TrimStart(' ');
                        message = char.ToUpper(message[0]) + message.Substring(1);
                    }
                    if (_tutorialManager.IsTutorial)
                    {
                        message = Constants.ErrorMessageForConnectionFailed;
                    }
                    _myDeckPage.OpenAlertDialog("Not able to Edit Deck: \n" + message);
                }
            }

            if (success)
            {
                _dataManager.CachedUserLocalData.LastSelectedDeckId = (int)deckToSave.Id;
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                _myDeckPage.ChangeTab(nextTab);
            }

            _myDeckPage.ButtonSaveRenameDeck.interactable = true;
            _buttonSaveDeck.interactable = true;
        }

        private bool VerifyDeckName(string deckName)
        {
            if (string.IsNullOrWhiteSpace(deckName))
            {
                _myDeckPage.OpenAlertDialog("Saving Deck with an empty name is not allowed.");
                return false;
            }
            return true;
        }

        public void ProcessRenameDeck(Deck deckToSave, string newName)
        {
            _myDeckPage.ButtonSaveRenameDeck.interactable = false;

            if (!VerifyDeckName(newName))
            {
                _myDeckPage.ButtonSaveRenameDeck.interactable = true;
                return;
            }

            deckToSave.Name = newName;
            HordeSelectionWithNavigationPage.Tab tab = _myDeckPage.IsDisplayRenameDeck ?
                HordeSelectionWithNavigationPage.Tab.Editing :
                HordeSelectionWithNavigationPage.Tab.SelectDeck;

            ProcessEditDeck(deckToSave,tab);
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
