using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace Loom.ZombieBattleground
{
    public class MyDecksEditTab
    {
        private ILoadObjectsManager _loadObjectsManager;
        
        private IDataManager _dataManager;
        
        private ITutorialManager _tutorialManager;
        
        private IAnalyticsManager _analyticsManager;
        
        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;
    
        private MyDecksPage _myDeckPage;
        
        public List<Transform> CollectionsCardPositions,
                               DeckCardPositions;

        public GameObject CardCreaturePrefab,
                          CardItemPrefab,
                          CollectionsCardPlaceholdersPrefab,
                          DeckCardPlaceholdersPrefab;

        public GameObject CollectionsCardPlaceholders,
                          DeckCardPlaceholders;

        private List<BoardCard> _createdDeckBoardCards,
                                _createdCollectionsBoardCards;
                               
        private GameObject _selfPage;
        
        private CollectionData _collectionData;
        
        private GameObject _draggingObject;

        private Button _buttonFilter,
                       _buttonSearch,
                       _buttonUpperLeftArrow,
                       _buttonUpperRightArrow,
                       _buttonLowerLeftArrow,
                       _buttonLowerRightArrow,
                       _buttonSaveDeck;

        private TextMeshProUGUI _textEditDeckName,
                                _textEditDeckCardsAmount;
                                
        private TMP_InputField _inputFieldSearchName;                   
       
        private int _deckPageIndex;

        private int _collectionPageIndex;
        
        private bool _isDragging;

        private List<Card> _cacheCollectionCards;
        
        public enum CollectionFilterType
        {
            NONE,
            SEARCH_KEYWORD,
        }

        private CollectionFilterType _collectionFilterType;

        private Enumerators.SetType _filterSetType;

        private readonly Dictionary<Enumerators.SetType, Enumerators.SetType> _setTypeAgainstDictionary =
            new Dictionary<Enumerators.SetType, Enumerators.SetType>
            {
                {
                    Enumerators.SetType.FIRE, Enumerators.SetType.WATER
                },
                {
                    Enumerators.SetType.TOXIC, Enumerators.SetType.FIRE
                },
                {
                    Enumerators.SetType.LIFE, Enumerators.SetType.TOXIC
                },
                {
                    Enumerators.SetType.EARTH, Enumerators.SetType.LIFE
                },
                {
                    Enumerators.SetType.AIR, Enumerators.SetType.EARTH
                },
                {
                    Enumerators.SetType.WATER, Enumerators.SetType.AIR
                }
            };

        private readonly List<Enumerators.SetType> _availableCollectionSetType =
            new List<Enumerators.SetType>
            {
                Enumerators.SetType.FIRE,
                Enumerators.SetType.WATER,
                Enumerators.SetType.EARTH,
                Enumerators.SetType.AIR,
                Enumerators.SetType.LIFE,
                Enumerators.SetType.TOXIC,
                Enumerators.SetType.OTHERS
            };

        private int _collectionSetTypeIndex;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            
            _myDeckPage = GameClient.Get<IUIManager>().GetPage<MyDecksPage>();
            InitBoardCardPrefabsAndLists();

            _myDeckPage.EventChangeTab += (MyDecksPage.TAB tab) =>
            {
                if (tab != MyDecksPage.TAB.EDITING)
                    return;

                _deckPageIndex = 0;
                _collectionPageIndex = 0;
                _collectionSetTypeIndex = 0;
                _collectionFilterType = CollectionFilterType.NONE;
                _inputFieldSearchName.text = "";

                _cacheCollectionCards.Clear();                    
                ResetDeckBoardCards();
                ResetCollectionsBoardCards();
                _textEditDeckName.text = _myDeckPage.CurrentEditDeck.Name;
                _textEditDeckCardsAmount.text =  $"{_myDeckPage.CurrentEditDeck.GetNumCards()}/{Constants.MaxDeckSize}";  
                LoadCollectionsCards(GetCollectionCardList());
                LoadDeckCards(_myDeckPage.CurrentEditDeck);
                UpdateDeckCardPage();
            };

            _cacheCollectionCards = new List<Card>();
        }
        
        public void Show(GameObject selfPage)
        {   
            _selfPage = selfPage;
            
            _textEditDeckName = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();
            _textEditDeckCardsAmount = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Lower_Items/Image_CardCounter/Text_CardsAmount").GetComponent<TextMeshProUGUI>();
            
            _buttonFilter = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Upper_Items/Button_Filter").GetComponent<Button>();
            _buttonFilter.onClick.AddListener(ButtonEditDeckFilterHandler);
            _buttonFilter.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _buttonSearch = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Upper_Items/Button_SearchBar").GetComponent<Button>();
            _buttonSearch.onClick.AddListener(ButtonEditDeckSearchHandler);
            _buttonSearch.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _buttonUpperLeftArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Button_UpperLeftArrow").GetComponent<Button>();
            _buttonUpperLeftArrow.onClick.AddListener(ButtonEditDeckUpperLeftArrowHandler);
            _buttonUpperLeftArrow.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _buttonUpperRightArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Button_UpperRightArrow").GetComponent<Button>();
            _buttonUpperRightArrow.onClick.AddListener(ButtonEditDeckUpperRightArrowHandler);
            _buttonUpperRightArrow.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _buttonLowerLeftArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Button_LowerLeftArrow").GetComponent<Button>();
            _buttonLowerLeftArrow.onClick.AddListener(ButtonEditDeckLowerLeftArrowHandler);
            _buttonLowerLeftArrow.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _buttonLowerRightArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Button_LowerRightArrow").GetComponent<Button>();
            _buttonLowerRightArrow.onClick.AddListener(ButtonEditDeckLowerRightArrowHandler);
            _buttonLowerRightArrow.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _buttonSaveDeck = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Lower_Items/Button_SaveDeck").GetComponent<Button>();
            _buttonSaveDeck.onClick.AddListener(ButtonSaveEditDeckHandler);
            _buttonSaveDeck.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _inputFieldSearchName = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Upper_Items/InputText_Search").GetComponent<TMP_InputField>();
            _inputFieldSearchName.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            _inputFieldSearchName.text = "";
            
            LoadBoardCardComponents();
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
            
            _cacheCollectionCards.Clear();
        }
        
        private void InitBoardCardPrefabsAndLists()
        {
            CardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            CardItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard");
            CollectionsCardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersMyDecksLower");
            DeckCardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersMyDecksUpper");            
            
            _createdDeckBoardCards = new List<BoardCard>();
            _createdCollectionsBoardCards = new List<BoardCard>();
            
            _collectionData = new CollectionData();
            _collectionData.Cards = new List<CollectionCardData>();
        }

        #region Button Handlers
        
        private void ButtonEditDeckFilterHandler()
        {
            GameClient.Get<IUIManager>().DrawPopup<ElementFilterPopup>();
            ElementFilterPopup popup = GameClient.Get<IUIManager>().GetPopup<ElementFilterPopup>();
            popup.ActionPopupHiding += FilterPopupHidingHandler;
        }
        
        private void FilterPopupHidingHandler(Enumerators.SetType selectedSetType)
        {
            ElementFilterPopup popup = GameClient.Get<IUIManager>().GetPopup<ElementFilterPopup>();
            popup.ActionPopupHiding -= FilterPopupHidingHandler;
        }
        
        private void ButtonEditDeckSearchHandler()
        {

        }
        
        private void ButtonEditDeckUpperLeftArrowHandler()
        {
            MoveDeckPageIndex(-1);
            UpdateDeckCardPage();
        }
        
        private void ButtonEditDeckUpperRightArrowHandler()
        {
            MoveDeckPageIndex(1);
            UpdateDeckCardPage();
        }
        
        private void ButtonEditDeckLowerLeftArrowHandler()
        {
            MoveCollectionPageIndex(-1);
            LoadCollectionsCards(GetCollectionCardList());
        }
        
        private void ButtonEditDeckLowerRightArrowHandler()
        {
            MoveCollectionPageIndex(1);
            LoadCollectionsCards(GetCollectionCardList());
        }
        
        private void ButtonSaveEditDeckHandler()
        {
            ProcessEditDeck(_myDeckPage.CurrentEditDeck);            
        }
        
        public void OnInputFieldSearchEndedEdit(string value)
        {
            _collectionFilterType = CollectionFilterType.SEARCH_KEYWORD;
            _collectionPageIndex = 0;         
            LoadCollectionsCards(GetCollectionCardList());
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

            _deckPageIndex = 0;
            _collectionPageIndex = 0;
            _collectionSetTypeIndex = 0;
            FillCollectionData();
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

        public void LoadCollectionsCards(List<Card> collectionCardList)
        {
            _cacheCollectionCards = collectionCardList.ToList();
            ResetCollectionsBoardCards();
            
            int startIndex = _collectionPageIndex * CollectionsCardPositions.Count;
            int endIndex = Mathf.Min(startIndex + CollectionsCardPositions.Count, collectionCardList.Count);
            
            CollectionCardData collectionCardData = null;
            RectTransform rectContainer = _myDeckPage.LocatorCollectionCards.GetComponent<RectTransform>();

            for (int i = startIndex; i < endIndex; i++)
            {
                if (i >= collectionCardList.Count)
                    break;

                Card card = collectionCardList[i];
                CollectionCardData cardData = _dataManager.CachedCollectionData.GetCardData(card.Name);

                // hack !!!! CHECK IT!!!
                if (cardData == null)
                    continue;

                BoardCard boardCard = CreateBoardCard
                (
                    card, 
                    rectContainer,
                    CollectionsCardPositions[i % CollectionsCardPositions.Count].position, 
                    0.265f
                );
                _createdCollectionsBoardCards.Add(boardCard); 
                
                OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();

                eventHandler.DragBegan += BoardCardDragBeganHandler;
                eventHandler.DragEnded += BoardCardCollectionDragEndedHandler;
                eventHandler.DragUpdated += BoardCardDragUpdatedHandler;
                
                collectionCardData = _collectionData.GetCardData(card.Name);
                UpdateBoardCardAmount
                (
                    true, 
                    card.Name, 
                    collectionCardData.Amount
                );
            }
        }
        
        private List<Card> GetCollectionCardList()
        {
            return FilterCollectionCardList();
        }
        
        private List<Card> GetCollectionCardBySetType(List<Enumerators.SetType> setTypeList)
        {
            List<Card> cardList = new List<Card>();
            
            foreach(Enumerators.SetType setType in setTypeList)
            {
                CardSet set = SetTypeUtility.GetCardSet(_dataManager, setType);
                List<Card> cards = set.Cards;
                cardList.AddRange(cards.ToList());
            }
            
            return cardList;
        }

        private List<Card> FilterCollectionCardList()
        {
            List<Card> resultList = new List<Card>();
            switch(_collectionFilterType)
            {
                case CollectionFilterType.SEARCH_KEYWORD:
                    List<Card> cards = GetCollectionCardBySetType(_availableCollectionSetType);
                    string keyword = _inputFieldSearchName.text.Trim().ToLower();

                    if (string.IsNullOrEmpty(keyword))
                    {
                        resultList = cards.ToList();
                        break;
                    }                               
                    
                    foreach(Card card in cards)
                    {
                        string cardName = card.Name.Trim().ToLower();
                        if (cardName.Contains(keyword))
                            resultList.Add(card);
                    }
                    if(resultList.Count <= 0)
                    {
                        _myDeckPage.OpenAlertDialog($"No card found for keyword '{_inputFieldSearchName.text.Trim()}'");
                        resultList = cards.ToList();;
                    }
                    break;
                default:
                    resultList = GetCollectionCardBySetType(new List<Enumerators.SetType>()
                    {
                        _availableCollectionSetType[_collectionSetTypeIndex]
                    });
                    break;
              
            }
            return resultList;
        }

        public void UpdateBoardCardAmount(bool init, string cardId, int amount)
        {
            foreach (BoardCard card in _createdCollectionsBoardCards)
            {
                if (card.LibraryCard.Name == cardId)
                {
                    card.SetAmountOfCardsInEditingPage
                    (
                        init, 
                        GetMaxCopiesValue(card.LibraryCard), 
                        amount, 
                        true
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
                Card libraryCard = _dataManager.CachedCardsLibraryData.GetCardFromName(card.CardName);

                bool itemFound = false;
                foreach (BoardCard item in _createdDeckBoardCards)
                {
                    if (item.LibraryCard.Name == card.CardName)
                    {
                        itemFound = true;
                        break;
                    }
                }

                if (!itemFound)
                {
                    BoardCard boardCard = CreateBoardCard
                    (
                        libraryCard, 
                        rectContainer,
                        Vector3.zero, 
                        0.279f
                    );
                    boardCard.Transform.Find("Amount").gameObject.SetActive(false);

                    _createdDeckBoardCards.Add(boardCard);
                    
                    boardCard.SetAmountOfCardsInEditingPage(true, GetMaxCopiesValue(libraryCard), card.Amount);
                    
                    OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();

                    eventHandler.DragBegan += BoardCardDragBeganHandler;
                    eventHandler.DragEnded += BoardCardDeckDragEndedHandler; 
                    eventHandler.DragUpdated += BoardCardDragUpdatedHandler;

                    _collectionData.GetCardData(card.CardName).Amount -= card.Amount;

                    UpdateEditDeckCardsAmount();
                }
            }
        }
        
        public void AddCardToDeck(IReadOnlyCard card)
        {
            if (_myDeckPage.CurrentEditDeck == null)
                return;
            

            if (_setTypeAgainstDictionary[_myDeckPage.CurrentEditHero.HeroElement] == card.CardSetType)
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
            BoardCard foundItem = null;
            foreach (BoardCard item in _createdDeckBoardCards)
            {
                if (item.LibraryCard.MouldId == card.MouldId)
                {
                    foundItem = item;
                    itemFound = true;

                    break;
                }
            }

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_ADD_CARD,
                Constants.SfxSoundVolume, false, false, true);
                
            collectionCardData.Amount--;
            UpdateBoardCardAmount(false, card.Name, collectionCardData.Amount);

            
            if (!itemFound)
            {
                RectTransform rectContainer = _myDeckPage.LocatorDeckCards.GetComponent<RectTransform>();
                BoardCard boardCard = CreateBoardCard
                (
                    card, 
                    rectContainer,
                    Vector3.zero,
                    0.3f                  
                );
                boardCard.Transform.Find("Amount").gameObject.SetActive(false);
                foundItem = boardCard;
                
                OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();

                eventHandler.DragBegan += BoardCardDragBeganHandler;
                eventHandler.DragEnded += BoardCardDeckDragEndedHandler; 
                eventHandler.DragUpdated += BoardCardDragUpdatedHandler;

                _createdDeckBoardCards.Add(boardCard);

                UpdateEditDeckCardsAmount();
                UpdateDeckCardPage();
            }

            _myDeckPage.CurrentEditDeck.AddCard(card.Name);

            foundItem.SetAmountOfCardsInEditingPage(false, GetMaxCopiesValue(card),
                _myDeckPage.CurrentEditDeck.Cards.Find(x => x.CardName == foundItem.LibraryCard.Name).Amount);

            UpdateDeckCardPage();

            if(_tutorialManager.IsTutorial && _myDeckPage.CurrentEditDeck.GetNumCards() >= _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.HordeFilled);
            }      
        }
        
        public void RemoveCardFromDeck(IReadOnlyCard card)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_REMOVE_CARD,
                Constants.SfxSoundVolume, false, false, true);
            
            CollectionCardData collectionCardData = _collectionData.GetCardData(card.Name);
            collectionCardData.Amount++;
            UpdateBoardCardAmount
            (
                false, 
                card.Name, 
                collectionCardData.Amount
            );
            
            BoardCard boardCard = _createdDeckBoardCards.Find(item => item.LibraryCard.MouldId == card.MouldId);
            boardCard.CardsAmountDeckEditing--;
            _myDeckPage.CurrentEditDeck.RemoveCard(card.Name);

            if (boardCard.CardsAmountDeckEditing == 0)
            {
                _createdDeckBoardCards.Remove(boardCard);

                Object.DestroyImmediate(boardCard.GameObject);

                int currentDeckPage = _deckPageIndex;
                UpdateDeckCardPage();
                int deckPagesAmount = GetDeckPageAmount();
                if (currentDeckPage >= deckPagesAmount)
                {
                    _deckPageIndex = deckPagesAmount - 1;
                }

                UpdateDeckCardPage();
                UpdateEditDeckCardsAmount();
            }
            else
            {
                boardCard.SetAmountOfCardsInEditingPage(false, GetMaxCopiesValue(boardCard.LibraryCard), boardCard.CardsAmountDeckEditing);
            }
        }
        
        private BoardCard CreateBoardCard(IReadOnlyCard card, RectTransform root, Vector3 position, float scale)
        {
            GameObject go;
            BoardCard boardCard;
            int amount = _collectionData.GetCardData(card.Name).Amount;
            
            switch (card.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(CardCreaturePrefab);
                    boardCard = new UnitBoardCard(go);
                    break;
                case Enumerators.CardKind.SPELL:
                    go = Object.Instantiate(CardItemPrefab);
                    boardCard = new SpellBoardCard(go);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(card.CardKind), card.CardKind, null);
            }
            
            boardCard.Init(card, amount);
            boardCard.SetHighlightingEnabled(false);
            boardCard.Transform.position = position;
            boardCard.Transform.localScale = Vector3.one * scale;
            boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;
            boardCard.Transform.Find("Amount").gameObject.SetActive(false);
            
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

        #region Drag Handler

        private void BoardCardDragBeganHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (_isDragging || (GameClient.Get<ITutorialManager>().IsTutorial &&
                !GameClient.Get<ITutorialManager>().CurrentTutorial.IsGameplayTutorial() &&
                (GameClient.Get<ITutorialManager>().CurrentTutorialStep.ToMenuStep().CardsInteractingLocked ||
                !GameClient.Get<ITutorialManager>().CurrentTutorialStep.ToMenuStep().CanDragCards)))
                return;
            
            _draggingObject = Object.Instantiate(onOnject);
            _draggingObject.transform.localScale = Vector3.one * 0.3f;
            _draggingObject.transform.Find("Amount").gameObject.SetActive(false);
            _draggingObject.transform.Find("AmountForArmy").gameObject.SetActive(false);
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
                    if (hit.collider.gameObject == _myDeckPage.DragAreaDeck)
                    {
                        BoardCard boardCard = _createdCollectionsBoardCards.Find(x =>
                            x.GameObject.GetInstanceID().ToString() == _draggingObject.name);

                        AddCardToDeck(boardCard.LibraryCard);

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
                    if (hit.collider.gameObject == _myDeckPage.DragAreaCollections)
                    {
                        BoardCard boardCard = _createdDeckBoardCards.Find(x =>
                            x.GameObject.GetInstanceID().ToString() == _draggingObject.name);

                        RemoveCardFromDeck(boardCard.LibraryCard);
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
            List<BoardCard> displayCardList = new List<BoardCard>();
            for( int i=0; i<_createdDeckBoardCards.Count; ++i)
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
            for(int i=0; i<displayCardList.Count; ++i)
            {
                displayCardList[i].Transform.position = DeckCardPositions[i].position;
            }
        }

        private void MoveDeckPageIndex(int direction)
        {
            _deckPageIndex = Mathf.Clamp(_deckPageIndex + direction, 0, GetDeckPageAmount() - 1);
        }
        
        private void MoveCollectionPageIndex(int direction)
        {
            int newPageIndex = _collectionPageIndex + direction;
            if (newPageIndex < 0)
            {
                _collectionPageIndex = 0;
                int newSetTypeIndex = _collectionSetTypeIndex - 1;
                if (newSetTypeIndex < 0)
                    newSetTypeIndex = _availableCollectionSetType.Count - 1;
                _collectionSetTypeIndex = newSetTypeIndex;                    
            }
            else if (newPageIndex >= GetCollectionPageAmount())
            {
                _collectionPageIndex = 0;
                int newSetTypeIndex = _collectionSetTypeIndex + 1;
                if (newSetTypeIndex >= _availableCollectionSetType.Count)
                    newSetTypeIndex = 0;
                _collectionSetTypeIndex = newSetTypeIndex;
            }
            else
            {
                _collectionPageIndex = newPageIndex;
            }
        }
        
        private void ResetCollectionsBoardCards()
        {
            foreach (BoardCard item in _createdCollectionsBoardCards)
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
            return Mathf.CeilToInt((float) _cacheCollectionCards.Count / GetCollectionCardAmountPerPage());
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
            foreach (BoardCard item in _createdDeckBoardCards)
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

            Enumerators.SetType setType = GameClient.Get<IGameplayManager>().GetController<CardsController>().GetSetOfCard(card);

            if (setType == Enumerators.SetType.ITEM)
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
        
        public async void ProcessEditDeck(Deck deckToSave)
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
                Debug.Log(" ====== Edit Deck Successfully ==== ");
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogException(e);

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
                _myDeckPage.ChangeTab(MyDecksPage.TAB.SELECT_DECK);
                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.HordeSelection);
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
        
        public async void ProcessRenameDeck(Deck deckToSave, string newName)
        {
            _myDeckPage.ButtonSaveRenameDeck.interactable = false;
            
            if (!VerifyDeckName(newName))
            {
                _myDeckPage.ButtonSaveRenameDeck.interactable = true;
                return;
            }

            ProcessEditDeck(deckToSave);
        }
    }
}
