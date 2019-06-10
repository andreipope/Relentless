using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using TMPro;
using UnityEngine;
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

        #region IUIElement

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/CreatureCard_UI");

            _cardFilter = new CardFilter();
            _cardFilter.Init();
            _cardFilter.UpdateElementFilterEvent += UpdateElementFilterHandler;
            _cardFilter.UpdateRankFilterEvent += UpdateRankFilterHandler;
            _cardFilter.UpdateGooCostFilterEvent += UpdateGooCostFilterHandler;

            _mainCamera = Camera.main;
        }

        public void Show(GameObject obj, PageType pageType)
        {
            _selfPage = obj;

            _pageType = pageType;

            _cardCounter = _selfPage.transform.Find("Panel_Frame/Upper_Items/Image_CardCounter/Text_CardCounter").GetComponent<TextMeshProUGUI>();

            _inputFieldSearchName = _selfPage.transform.Find("Panel_Frame/Upper_Items/Image_SearchBar/InputText_Search").GetComponent<TMP_InputField>();
            _inputFieldSearchName.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            _inputFieldSearchName.text = "";

            _allCardsContent = _selfPage.transform.Find("Panel_Content/Army/Element/Scroll View")
                .GetComponent<ScrollRect>().content;

            switch (pageType)
            {
                case PageType.Army:
                    LoadAllCards();
                    break;
                case PageType.DeckEditing:
                    LoadUserOwnedCards();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pageType), pageType, null);
            }

            _allCardsCount = _cardUIList.Count;

            UpdateCardsUiList();
            _cardFilter.Show(_selfPage.transform.Find("Panel_Frame/Lower_Items/Filters").gameObject);
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

                int index = _dataManager.CachedCardsLibraryData.Cards.FindIndex(libraryCard => libraryCard.MouldId == cardData.MouldId);
                if (index == -1)
                    return;

                Card card = _dataManager.CachedCardsLibraryData.Cards[index];
                InstantiateCard(card);
            }
        }

        private void InstantiateCard(Card card)
        {
            GameObject go = Object.Instantiate(_cardCreaturePrefab);
            go.transform.SetParent(_allCardsContent);
            go.transform.localScale = Vector3.one * BoardCardScale;

            UnitCardUI unitCard = new UnitCardUI();
            unitCard.Init(go);
            int index = _dataManager.CachedCollectionData.Cards.FindIndex(cardData => cardData.MouldId == card.MouldId);
            int cardAmount = index != -1 ?_dataManager.CachedCollectionData.Cards[index].Amount : 0;
            unitCard.FillCardData(card, cardAmount);

            _cardUIList.Add(unitCard);

            MultiPointerClickHandler multiPointerClickHandler = go.AddComponent<MultiPointerClickHandler>();
            multiPointerClickHandler.SingleClickReceived += () => { BoardCardSingleClickHandler(unitCard.GetCard()); };

            if (_pageType == PageType.Army)
            {
                multiPointerClickHandler.DoubleClickReceived += () => { BoardCardSingleClickHandler(unitCard.GetCard()); };
            }
            else if (_pageType == PageType.DeckEditing)
            {
                multiPointerClickHandler.DoubleClickReceived += () => { BoardCardDoubleClickHandler(unitCard.GetCard()); };
            }
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
                CollectionCardData cardInCollection = _dataManager.CachedCollectionData.Cards.Find(card => card.MouldId == cardInUi.MouldId);
                int totalCardAmount = cardInCollection.Amount;

                DeckCardData deckCardData = selectedDeck.Cards.Find(card => card.MouldId == cardInUi.MouldId);
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
                CollectionCardData cardInCollection = _dataManager.CachedCollectionData.Cards.Find(card => card.MouldId == cardInUi.MouldId);
                int totalCardAmount = cardInCollection.Amount;

                DeckCardData deckCardData = deck.Cards.Find(card => card.MouldId == cardInUi.MouldId);
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
                CollectionCardData cardInCollection = _dataManager.CachedCollectionData.Cards.Find(card => card.MouldId == cardInUi.MouldId);
                int totalCardAmount = cardInCollection.Amount;
               _cardUIList[i].UpdateCardAmount(totalCardAmount);
            }
        }

        public void UpdateCardAmountDisplay(IReadOnlyCard card, int amount)
        {
            UnitCardUI unitCardUi = _cardUIList.Find(cardUi => cardUi.GetCard().MouldId == card.MouldId);
            if (unitCardUi == null)
                return;

            unitCardUi.UpdateCardAmount(amount);
        }

        public void Hide()
        {
            if (_selfPage == null)
                return;

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
                cards.Any(resultCard => uiCard.GetCard().MouldId == resultCard.MouldId)).ToList();
            for (int i = 0; i < cardUiList.Count; i++)
            {
                cardUiList[i].EnableObject(true);
            }

            cardUiList.Clear();
            cardUiList = _cardUIList.Where(uiCard =>
                cards.All(resultCard => uiCard.GetCard().MouldId != resultCard.MouldId)).ToList();
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
