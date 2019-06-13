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

        private IDataManager _dataManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private HordeSelectionWithNavigationPage _myDeckPage;

        private GameObject CardCreaturePrefab;

        private CollectionData _collectionData;

        private GameObject _draggingObject;

        private Button _buttonAutoComplete;
        private Button _buttonBack;

        private Button _buttonLeftArrowScroll;
        private Button _buttonRightArrowScroll;

        private Scrollbar _cardCollectionScrollBar;

        private TextMeshProUGUI _textEditDeckName,
                                _textEditDeckCardsAmount;

        private TMP_InputField _inputFieldSearchName;

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

        private HordeSelectionWithNavigationPage.Tab _nextTab;

        private Enumerators.Faction _againstFaction;

        private const int MaxCollectionCardPoolAmount = 30;

        private UICardCollections _uiCardCollections;
        private CustomDeckUI _customDeckUi;

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _collectionData = new CollectionData();

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

            _uiCardCollections = new UICardCollections();
            _uiCardCollections.Init();

            _customDeckUi = new CustomDeckUI();
            _customDeckUi.Init();

            Log.Info("Editing init called");
        }

        public void Load(GameObject editingTabObj)
        {
            _buttonAutoComplete = editingTabObj.transform.Find("Panel_Frame/Upper_Items/Button_AutoComplete").GetComponent<Button>();
            _buttonAutoComplete.onClick.AddListener(ButtonAutoCompleteHandler);

            _buttonBack = editingTabObj.transform.Find("Panel_Frame/Upper_Items/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);

            _uiCardCollections.Show(editingTabObj, PageType.DeckEditing);
            _customDeckUi.Load(editingTabObj.transform.Find("Deck_Content").gameObject);

            _buttonLeftArrowScroll = editingTabObj.transform.Find("Panel_Content/Army/Element/LeftArrow").GetComponent<Button>();
            _buttonLeftArrowScroll.onClick.AddListener(ButtonLeftArrowScrollHandler);

            _buttonRightArrowScroll = editingTabObj.transform.Find("Panel_Content/Army/Element/RightArrow").GetComponent<Button>();
            _buttonRightArrowScroll.onClick.AddListener(ButtonRightArrowScrollHandler);

            _cardCollectionScrollBar = editingTabObj.transform.Find("Panel_Content/Army/Element/Scroll View").GetComponent<ScrollRect>().horizontalScrollbar;
        }

        public void Show(int deckId)
        {
            FillCollectionData();

            if (deckId == -1)
            {
                _customDeckUi.ShowDeck(_myDeckPage.CurrentEditDeck);
                _uiCardCollections.UpdateCardsAmountDisplay();

            }
            else
            {
                _customDeckUi.ShowDeck(_myDeckPage.CurrentEditDeck);
                _uiCardCollections.UpdateCardsAmountDisplay(deckId);
                UpdateCollectionCards(deckId);
            }
        }

        private void UpdateCollectionCards(int deckId)
        {
            Deck selectedDeck = _dataManager.CachedDecksData.Decks.Find(deck => deck.Id.Id == deckId);
            if (selectedDeck == null)
                return;

            for (int i = 0; i < _collectionData.Cards.Count; i++)
            {
                CollectionCardData cardData = _collectionData.Cards[i];

                // get amount of card in collection data
                CollectionCardData cardInCollection = _dataManager.CachedCollectionData.Cards.Find(card => card.MouldId == cardData.MouldId);
                int totalCardAmount = cardInCollection.Amount;

                DeckCardData deckCardData = selectedDeck.Cards.Find(card => card.MouldId == cardData.MouldId);
                if (deckCardData != null)
                    cardData.Amount = totalCardAmount - deckCardData.Amount;
            }
        }

        public void Update()
        {
            _uiCardCollections.Update();
        }

        public void Dispose()
        {
            _uiCardCollections.Hide();

            if (_draggingObject != null)
            {
                Object.Destroy(_draggingObject);
                _draggingObject = null;
                _isDragging = false;
            }

            //Object.Destroy(_materialNormal);
            //Object.Destroy(_materialGrayscale);
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

            DataUtilities.PlayClickSound();
            _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SelectOverlordSkill);
        }

        private void ButtonAutoCompleteHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonAutoComplete.name))
                return;

            DataUtilities.PlayClickSound();
            FillCollectionData();
            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().GenerateCardsToDeck
            (
                _myDeckPage.CurrentEditDeck,
                _collectionData
            );
            SubtractInitialDeckCardsAmountFromCollections(_myDeckPage.CurrentEditDeck);

            _customDeckUi.ShowDeck(_myDeckPage.CurrentEditDeck);
            _uiCardCollections.UpdateCardsAmountDisplay(_myDeckPage.CurrentEditDeck);

            //_customDeckUi.ShowDeck((int)_myDeckPage.CurrentEditDeck.Id.Id);

            //UpdateDeckPageIndexDictionary();

            //ResetCollectionPageState();
            //UpdateEditDeckCardsAmount();
        }

        public void UpdateEditingTab(Deck deck, CollectionData collectionData)
        {
            _myDeckPage.CurrentEditDeck = deck;
            _collectionData = collectionData;

            _customDeckUi.ShowDeck(_myDeckPage.CurrentEditDeck);
            _uiCardCollections.UpdateCardsAmountDisplay(_myDeckPage.CurrentEditDeck);
        }

        private void ButtonBackHandler()
        {
            DataUtilities.PlayClickSound();
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
                CollectionCardData cardData = new CollectionCardData(card.MouldId, card.Amount);
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

        private void SubtractInitialDeckCardsAmountFromCollections(Deck deck)
        {
            foreach(DeckCardData card in deck.Cards)
            {
                Card fetchedCard = _dataManager.CachedCardsLibraryData.GetCardFromMouldId(card.MouldId);
                _collectionData.GetCardData(fetchedCard.MouldId).Amount -= card.Amount;
            }
        }

        /*private void UpdateDeckPageIndexDictionary()
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
        }*/

        private void AddDragging()
        {
            /*OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();
            eventHandler.DragBegan += BoardCardDragBeganHandler;
            eventHandler.DragEnded += BoardCardDeckDragEndedHandler;
            eventHandler.DragUpdated += BoardCardDragUpdatedHandler;*/
        }

        public void AddCardToDeck(IReadOnlyCard card, bool animate = false)
        {
            if (_myDeckPage.CurrentEditDeck == null)
            {
                Debug.LogError("current edit deck is nul");
                return;
            }

            OverlordUserInstance overlordData = _dataManager.CachedOverlordData.GetOverlordById(_myDeckPage.CurrentEditDeck.OverlordId);
            if (FactionAgainstDictionary[overlordData.Prototype.Faction] == card.Faction)
            {
                OpenAlertDialog(
                    "Cannot add from the faction your Overlord is weak against.");
                return;
            }

            CollectionCardData collectionCardData = _collectionData.GetCardData(card.MouldId);
            if (collectionCardData.Amount <= 0)
            {
                OpenAlertDialog(
                    "You don't have enough of this card.\nBuy or earn packs to get more cards.");
                return;
            }

            DeckCardData existingCard = _myDeckPage.CurrentEditDeck.Cards.Find(x => x.MouldId == card.MouldId);
            int existingCardAmount = existingCard?.Amount ?? 0;


            uint maxCopies = GetMaxCopiesValue(card);
            if (existingCard != null && existingCard.Amount == maxCopies)
            {
                OpenAlertDialog("Cannot have more than " + maxCopies + " copies of an " +
                    card.Rank.ToString().ToLowerInvariant() + " card in one deck.");
                return;
            }

            if (_myDeckPage.CurrentEditDeck.GetNumCards() == Constants.DeckMaxSize)
            {
                OpenAlertDialog("Cannot have more than " + Constants.DeckMaxSize + " cards in one deck.");
                return;
            }

            bool isCardAlreadyExist = _myDeckPage.CurrentEditDeck.Cards.Exists(x => x.MouldId == card.MouldId);
            _myDeckPage.CurrentEditDeck.AddCard(card.MouldId);
            existingCardAmount++;

            // update count in card collection list left panel
            collectionCardData.Amount--;
            _uiCardCollections.UpdateCardAmountDisplay(card, collectionCardData.Amount);

            // Update card in deck - right panel
            if (isCardAlreadyExist)
            {
                _customDeckUi.UpdateCard((Card) card, existingCardAmount);
            }
            else
            {
                _customDeckUi.AddCard((Card)card, existingCardAmount);
            }

            _customDeckUi.UpdateCardsInDeckCountDisplay();

            if (_tutorialManager.IsTutorial && _myDeckPage.CurrentEditDeck.GetNumCards() >= _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.HordeFilled);
            }
        }

        public void RemoveCardFromDeck(IReadOnlyCard card, bool animate)
        {
            CollectionCardData collectionCardData = _collectionData.GetCardData(card.MouldId);
            collectionCardData.Amount++;

            DeckCardData existingCard = _myDeckPage.CurrentEditDeck.Cards.Find(x => x.MouldId == card.MouldId);
            int existingCardAmount = existingCard?.Amount ?? 0;

            _myDeckPage.CurrentEditDeck.RemoveCard(card.MouldId);

            // update right panel
                // if more than one card, only indicator changes
                // if no more card left, remove the card from ui
            bool isCardAlreadyExist = _myDeckPage.CurrentEditDeck.Cards.Exists(x => x.MouldId == card.MouldId);
            if (isCardAlreadyExist)
            {
                _customDeckUi.UpdateCard((Card) card, existingCardAmount - 1);
            }
            else
            {
                _customDeckUi.RemoveCard((Card)card);
            }

            // update left panel.. change the card amount in card
            _uiCardCollections.UpdateCardAmountDisplay(card, collectionCardData.Amount);

            // update card count
            _customDeckUi.UpdateCardsInDeckCountDisplay();
        }


        /*public void RemoveCardFromDeck(IReadOnlyCard card, bool animate = false)
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
        }*/

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

        /*private void UpdateEditDeckCardsAmount()
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
        }*/

        private void ResetCollectionPageState()
        {
            UpdateAvailableCollectionCards();
            LoadCollectionsCards();
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

            //UpdateCacheFilteredCardList(resultList);
        }

        private void ResetSearchAndFilterResult()
        {
            //_cardFilterPopup.FilterData.Reset();
            _inputFieldSearchName.text = "";
            ResetCollectionPageState();
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

        public void SaveDeck(HordeSelectionWithNavigationPage.Tab nextTab)
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

        private void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }
    }
}
