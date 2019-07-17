using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Card = Loom.ZombieBattleground.Data.Card;
using Deck = Loom.ZombieBattleground.Data.Deck;
using Faction = Loom.ZombieBattleground.Data.Faction;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class UICardCollections
    {
        private IUIManager _uiManager;
        private IDataManager _dataManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ITutorialManager _tutorialManager;

        private GameObject _selfPage;

        private RectTransform _allCardsContent;

        private TMP_InputField _inputFieldSearchName;


        private const float BoardCardScale = 0.33f;

        private CardFilter _cardFilter;

        private List<UnitCardUI> _cardUIList = new List<UnitCardUI>();
        private Camera _mainCamera;

        private int _allCardsCount;

        private TextMeshProUGUI _cardCounter;

        private GameObject _cardCreaturePrefab;

        private PageType _pageType;

        private static UnitCardUI _selectedUnitCard;

        private GameObject _deckScrollRect;

        #region IUIElement

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/CreatureCard_UI");

            _cardFilter = new CardFilter();
            _cardFilter.Init();
            _cardFilter.UpdateElementFilterEvent += UpdateElementFilterHandler;
            _cardFilter.UpdateRankFilterEvent += UpdateRankFilterHandler;
            _cardFilter.UpdateGooCostFilterEvent += UpdateGooCostFilterHandler;

            _mainCamera = Camera.main;

            _selectedUnitCard = null;
        }

        public void Show(GameObject obj, PageType pageType)
        {
            _selfPage = obj;

            _pageType = pageType;

            _cardCounter = _selfPage.transform.Find("Panel_Frame/Upper_Items/Image_CardCounter/Text_CardCounter").GetComponent<TextMeshProUGUI>();

            _inputFieldSearchName = _selfPage.transform.Find("Panel_Frame/Upper_Items/Image_SearchBar/InputText_Search").GetComponent<TMP_InputField>();
            _inputFieldSearchName.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            _inputFieldSearchName.text = "";

            ScrollRect scrollRect = _selfPage.transform.Find("Panel_Frame/Panel_Content/Army/Element/Scroll View")
                .GetComponent<ScrollRect>();
            _allCardsContent = scrollRect.content;
            Scrollbar cardCollectionScrollbar = scrollRect.horizontalScrollbar;

            switch (pageType)
            {
                case PageType.Army:
                    LoadAllCards();
                    break;
                case PageType.DeckEditing:
                    _deckScrollRect = _selfPage.transform.Find("Deck_Content/Cards/Scroll View").gameObject;
                    if(_tutorialManager.IsTutorial)
                        LoadTutorialCards();
                    else
                        LoadUserOwnedCards();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pageType), pageType, null);
            }

            _allCardsCount = _cardUIList.Count;

            UpdateCardsUiList();
            _cardFilter.Show(_selfPage.transform.Find("Panel_Frame/Lower_Items/Filters").gameObject);

            _selectedUnitCard = null;

            // interactive in tutorial
            if (GameClient.Get<ITutorialManager>().IsTutorial)
            {
                _inputFieldSearchName.interactable = false;
                cardCollectionScrollbar.interactable = false;
            }
            else
            {
                _inputFieldSearchName.interactable = true;
                cardCollectionScrollbar.interactable = true;
            }
        }

        private void UpdateCardsUiList()
        {
            List<Enumerators.Faction> availableSetType = _cardFilter.FilterData.GetFilteredFactionList();
            List<Card> resultList = new List<Card>();
            foreach (Enumerators.Faction item in availableSetType)
            {
                Faction  set = SetTypeUtility.GetCardFaction(_dataManager, item);
                List<Card> cards = set.Cards.ToList();
                foreach(Card card in cards)
                {
                    if
                    (
                        CheckIfSatisfyFactionFilter(card) &&
                        CheckIfSatisfyGooCostFilter(card) &&
                        CheckIfSatisfyRankFilter(card)
                    )
                    {
                        resultList.Add(card);
                    }
                }
            }

            DisplayCards(resultList);
            UpdateCardCounterText(resultList.Count);
        }

        private void UpdateElementFilterHandler(Enumerators.Faction faction)
        {
            _inputFieldSearchName.text = string.Empty;
            OnInputFieldSearchEndedEdit(string.Empty);

            List<Enumerators.Faction> factionList = _cardFilter.FilterData.GetFilteredFactionList();
            List<UnitCardUI> cardUiList = _cardUIList.FindAll(card => card.GetCard().Faction == faction);
            bool isFilter = factionList.Contains(faction);
            for (int i = 0; i < cardUiList.Count; i++)
            {
                Card card = cardUiList[i].GetCard();
                if(CheckIfSatisfyRankFilter(card) && CheckIfSatisfyGooCostFilter(card))
                    cardUiList[i].EnableObject(isFilter);
            }

            UpdateCardCounterText();
        }

        private void UpdateRankFilterHandler(Enumerators.CardRank rank)
        {
            _inputFieldSearchName.text = string.Empty;
            OnInputFieldSearchEndedEdit(string.Empty);

            List<Enumerators.CardRank> rankList = _cardFilter.FilterData.GetFilteredRankList();
            List<UnitCardUI> cardUiList = _cardUIList.FindAll(card => card.GetCard().Rank == rank);
            bool isFilter = rankList.Contains(rank);
            for (int i = 0; i < cardUiList.Count; i++)
            {
                Card card = cardUiList[i].GetCard();
                if(CheckIfSatisfyFactionFilter(card) && CheckIfSatisfyGooCostFilter(card))
                    cardUiList[i].EnableObject(isFilter);
            }

            UpdateCardCounterText();
        }

        private void UpdateGooCostFilterHandler(int gooCost)
        {
            _inputFieldSearchName.text = string.Empty;
            OnInputFieldSearchEndedEdit(string.Empty);

            List<int> gooCostList = _cardFilter.FilterData.GetGooCostList();
            List<UnitCardUI> cardUiList = _cardUIList.FindAll(card => card.GetCard().Cost == gooCost);
            bool isFilter = gooCostList.Contains(gooCost);
            for (int i = 0; i < cardUiList.Count; i++)
            {
                Card card = cardUiList[i].GetCard();
                if(CheckIfSatisfyFactionFilter(card) && CheckIfSatisfyRankFilter(card))
                    cardUiList[i].EnableObject(isFilter);
            }

            UpdateCardCounterText();
        }

        private void LoadAllCards()
        {
            _cardUIList = new List<UnitCardUI>();

            for (int i = 0; i < _dataManager.CachedCardsLibraryData.Factions.Count; i++)
            {
                for (int j = 0; j < _dataManager.CachedCardsLibraryData.Factions[i].Cards.Count; j++)
                {
                    Card card = _dataManager.CachedCardsLibraryData.Factions[i].Cards[j];
                    InstantiateCard(card);
                }
            }
        }

        private void LoadUserOwnedCards()
        {
            _cardUIList = new List<UnitCardUI>();
            for (int i = 0; i < _dataManager.CachedCollectionData.Cards.Count; i++)
            {
                CollectionCardData cardData = _dataManager.CachedCollectionData.Cards[i];

                int index = _dataManager.CachedCardsLibraryData.Cards.FindIndex(libraryCard => libraryCard.CardKey == cardData.CardKey);
                if (index == -1)
                    return;

                Card card = _dataManager.CachedCardsLibraryData.Cards[index];
                InstantiateCard(card);
            }
        }

        private void LoadTutorialCards()
        {
            _cardUIList = new List<UnitCardUI>();

            // get cards from open packer
            List<CollectionCardData> tutorialCardCollectionData = _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.CardsForArmy
                .Select(card => card.ToCollectionCardData(_dataManager))
                .ToList();

            for (int i = 0; i < tutorialCardCollectionData.Count; i++)
            {
                CollectionCardData cardData = tutorialCardCollectionData[i];
                int index = _dataManager.CachedCardsLibraryData.Cards.FindIndex(libraryCard => libraryCard.CardKey == cardData.CardKey);
                if (index == -1)
                    return;

                Card card = _dataManager.CachedCardsLibraryData.Cards[index];
                InstantiateCard(card);
            }
        }

        private void InstantiateCard(Card card)
        {
            GameObject go = Object.Instantiate(_cardCreaturePrefab, _allCardsContent, false);
            go.transform.localScale = Vector3.one * BoardCardScale;

            UnitCardUI unitCard = CreateUnitCardUi(card, go);

            _cardUIList.Add(unitCard);

            MultiPointerClickHandler multiPointerClickHandler = go.AddComponent<MultiPointerClickHandler>();

            if (_pageType == PageType.Army)
            {
                multiPointerClickHandler.SingleClickReceived += () => { BoardCardSingleClickHandler(unitCard.GetCard()); };
                multiPointerClickHandler.DoubleClickReceived += () => { BoardCardSingleClickHandler(unitCard.GetCard()); };
            }
            else if (_pageType == PageType.DeckEditing)
            {
                if(!_tutorialManager.IsTutorial)
                    multiPointerClickHandler.SingleClickReceived += () => { BoardCardSingleClickHandler(unitCard.GetCard()); };

                multiPointerClickHandler.DoubleClickReceived += () => { BoardCardDoubleClickHandler(unitCard.GetCard()); };

                // add drag / drop
                OnBehaviourHandler onBehaviourHandler = go.AddComponent<OnBehaviourHandler>();
                onBehaviourHandler.DragBegan += DragBeganEventHandler;
                onBehaviourHandler.DragUpdated += DragUpdatedEventHandler;
                onBehaviourHandler.DragEnded += DragEndedEventHandler;
            }
        }

        private UnitCardUI CreateUnitCardUi(Card card, GameObject cardObj)
        {
            UnitCardUI unitCard = new UnitCardUI();
            unitCard.Init(cardObj);
            unitCard.FillCardData(card, 0);

            return unitCard;
        }

        private void DragBeganEventHandler(PointerEventData arg1, GameObject obj)
        {
            if (_selectedUnitCard != null)
                return;

            if (_tutorialManager.IsTutorial &&
                !_tutorialManager.CurrentTutorial.IsGameplayTutorial() &&
                (_tutorialManager.CurrentTutorialStep.ToMenuStep().CardsInteractingLocked))
                return;

            GameObject cardObj = Object.Instantiate(_cardCreaturePrefab, obj.transform, false);
            cardObj.transform.localScale = Vector3.one;

            Card selectedCard = _cardUIList.Find(card => card.GetGameObject() == obj).GetCard();
            UnitCardUI unitCard = CreateUnitCardUi(selectedCard, cardObj);

            _selectedUnitCard = unitCard;
        }

        private void DragUpdatedEventHandler(PointerEventData arg1, GameObject arg2)
        {
            if (_selectedUnitCard == null)
                return;

            _selectedUnitCard.GetGameObject().transform.SetParent(_uiManager.Canvas.transform);
            Vector3 position = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            _selectedUnitCard.GetGameObject().transform.position = new Vector3(position.x, position.y, _selectedUnitCard.GetGameObject().transform.position.z);
        }

        private void DragEndedEventHandler(PointerEventData arg1, GameObject arg2)
        {
            if(_selectedUnitCard == null)
                return;

            Vector3 point = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(point, Vector3.forward, Mathf.Infinity, SRLayerMask.Default);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == _deckScrollRect)
                {
                    AddCardToDeck(_selectedUnitCard.GetCard());

                    if (_tutorialManager.IsTutorial)
                    {
                        _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.CardDragged);
                        if (_deckScrollRect != null)
                            _deckScrollRect.GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }

            Object.Destroy(_selectedUnitCard.GetGameObject());
            _selectedUnitCard = null;
        }


        public void UpdateCardsAmountDisplay(int deckId)
        {
            Deck selectedDeck = _dataManager.CachedDecksData.Decks.Find(deck => deck.Id.Id == deckId);
            if (selectedDeck == null)
                return;

            for (int i = 0; i < _cardUIList.Count; i++)
            {
                Card cardInUi = _cardUIList[i].GetCard();

                // get amount of card in collection data
                CollectionCardData cardInCollection = _dataManager.CachedCollectionData.Cards.Find(card => card.CardKey == cardInUi.CardKey);
                int totalCardAmount = cardInCollection.Amount;

                DeckCardData deckCardData = selectedDeck.Cards.Find(card => card.CardKey == cardInUi.CardKey);
                if (deckCardData == null)
                {
                    _cardUIList[i].UpdateCardAmount(totalCardAmount);
                }
                else
                {
                    _cardUIList[i].UpdateCardAmount(totalCardAmount - deckCardData.Amount);
                }
            }
        }

        public void UpdateCardsAmountDisplay(Deck deck)
        {
            if (deck == null)
                return;

            for (int i = 0; i < _cardUIList.Count; i++)
            {
                Card cardInUi = _cardUIList[i].GetCard();

                // get amount of card in collection data
                CollectionCardData cardInCollection = _dataManager.CachedCollectionData.Cards.Find(card => card.CardKey == cardInUi.CardKey);
                int totalCardAmount = cardInCollection.Amount;

                DeckCardData deckCardData = deck.Cards.Find(card => card.CardKey == cardInUi.CardKey);
                if (deckCardData == null)
                {
                    _cardUIList[i].UpdateCardAmount(totalCardAmount);
                }
                else
                {
                    _cardUIList[i].UpdateCardAmount(totalCardAmount - deckCardData.Amount);
                }
            }
        }



        public void UpdateCardsAmountDisplay()
        {
            for (int i = 0; i < _cardUIList.Count; i++)
            {
                Card cardInUi = _cardUIList[i].GetCard();

                // get amount of card in collection data
                CollectionCardData cardInCollection = _dataManager.CachedCollectionData.Cards.Find(card => card.CardKey == cardInUi.CardKey);
                int totalCardAmount = cardInCollection.Amount;
               _cardUIList[i].UpdateCardAmount(totalCardAmount);
            }
        }

        public void UpdateCardsAmountDisplayTutorial()
        {
            for (int i = 0; i < _cardUIList.Count; i++)
            {
                Card cardInUi = _cardUIList[i].GetCard();

                List<CollectionCardData> tutorialCardCollectionData = _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.CardsForArmy
                    .Select(card => card.ToCollectionCardData(_dataManager))
                    .ToList();

                // get amount of card in collection data
                CollectionCardData cardInCollection = tutorialCardCollectionData.Find(card => card.CardKey == cardInUi.CardKey);
                int totalCardAmount = cardInCollection.Amount;
                _cardUIList[i].UpdateCardAmount(totalCardAmount);
            }
        }

        public void UpdateCardAmountDisplay(IReadOnlyCard card, int amount)
        {
            UnitCardUI unitCardUi = _cardUIList.Find(cardUi => cardUi.GetCard().CardKey == card.CardKey);
            if (unitCardUi == null)
                return;

            unitCardUi.UpdateCardAmount(amount);
        }

        public void Hide()
        {
            if (_selfPage == null)
                return;

            if (_selectedUnitCard != null)
            {
                Object.Destroy(_selectedUnitCard.GetGameObject());
                _selectedUnitCard = null;
            }

            _cardFilter.Hide();
        }

        public void Update()
        {
            UpdateRenderForCards();
        }

        private void UpdateRenderForCards()
        {
            if (_selfPage == null)
                return;

            for (int i = 0; i < _cardUIList.Count; i++)
            {
                _cardUIList[i]
                    .EnableRenderer(RendererExtensions.IsVisibleFrom(_cardUIList[i].GetFrameRectTransform(),
                        _mainCamera));
            }
        }

        #endregion

        #region UI Handlers
        public void OnInputFieldSearchEndedEdit(string value)
        {
            string keyword = _inputFieldSearchName.text.Trim().ToLower();
            Faction set;
            List<Card> cards;
            List<Card> resultList = new List<Card>();
            List<Enumerators.Faction> allAvailableSetTypeList = _cardFilter.AllAvailableFactionList;

            foreach (Enumerators.Faction item in allAvailableSetTypeList)
            {
                set = SetTypeUtility.GetCardFaction(_dataManager, item);
                cards = set.Cards.ToList();

                foreach (Card card in cards)
                {
                    if
                    (
                        card.Name.ToLower().Contains(keyword) &&
                        CheckIfSatisfyFactionFilter(card) &&
                        CheckIfSatisfyGooCostFilter(card) &&
                        CheckIfSatisfyRankFilter(card)
                    )
                    {
                        resultList.Add(card);
                    }
                }
            }

            DisplayCards(resultList);
            UpdateCardCounterText(resultList.Count);
        }

        private void DisplayCards(List<Card> cards)
        {
            List<UnitCardUI> cardUiList = _cardUIList.Where(uiCard =>
                cards.Any(resultCard => uiCard.GetCard().CardKey == resultCard.CardKey)).ToList();
            for (int i = 0; i < cardUiList.Count; i++)
            {
                cardUiList[i].EnableObject(true);
            }

            cardUiList.Clear();
            cardUiList = _cardUIList.Where(uiCard =>
                cards.All(resultCard => uiCard.GetCard().CardKey != resultCard.CardKey)).ToList();
            for (int i = 0; i < cardUiList.Count; i++)
            {
                cardUiList[i].EnableObject(false);
            }
        }

        #endregion

        #region Board Cards
        private void BoardCardSingleClickHandler(Card selectedCard)
        {
            if (_uiManager.GetPopup<CardInfoWithSearchPopup>().Self != null)
                return;

            List<IReadOnlyCard> cardList = _cardUIList.Select(card => card.GetCardInterface()).ToList();

            CardInfoWithSearchPopup.PopupType popupType= CardInfoWithSearchPopup.PopupType.NONE;
            switch (_pageType)
            {
                case PageType.Army:
                    popupType = CardInfoWithSearchPopup.PopupType.NONE;
                    break;
                case PageType.DeckEditing:
                    popupType = CardInfoWithSearchPopup.PopupType.ADD_CARD;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _uiManager.DrawPopup<CardInfoWithSearchPopup>(new object[]
            {
                cardList,
                selectedCard,
                popupType
            });
        }

        private void BoardCardDoubleClickHandler(Card selectedCard)
        {
            if (_tutorialManager.IsTutorial &&
                !_tutorialManager.CurrentTutorial.IsGameplayTutorial() &&
                (_tutorialManager.CurrentTutorialStep.ToMenuStep().CardsInteractingLocked))
                return;

            AddCardToDeck(selectedCard);

            if (_tutorialManager.IsTutorial)
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.CardAdded);
        }

        private void AddCardToDeck(Card selectedCard)
        {
            _uiManager.GetPage<HordeSelectionWithNavigationPage>().HordeEditTab.AddCardToDeck
            (
                selectedCard,
                true
            );
        }

        #endregion

        private void UpdateCardCounterText(int cardCount)
        {
            _cardCounter.text = cardCount + "/" + _allCardsCount;
        }

        private void UpdateCardCounterText()
        {
            var activeCards = _cardUIList.FindAll(card => card.IsActive());
           _cardCounter.text = activeCards.Count + "/" + _allCardsCount;
        }

        private bool CheckIfSatisfyGooCostFilter(Card card)
        {
            if (card.Cost < 0)
                return false;

            if(card.Cost >= 10)
            {
                if (_cardFilter.FilterData.GooCostList[10])
                    return true;
            }
            else
            {
                if(_cardFilter.FilterData.GooCostList[card.Cost])
                    return true;
            }

            return false;
        }

        private bool CheckIfSatisfyRankFilter(Card card)
        {
            return _cardFilter.FilterData.RankDictionary[card.Rank];
        }

        private bool CheckIfSatisfyFactionFilter(Card card)
        {
            return _cardFilter.FilterData.FactionDictionary[card.Faction];
        }
    }
}
