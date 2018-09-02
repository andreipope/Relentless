// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using LoomNetwork.CZB.BackendCommunication;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class DeckEditingPage : IUIElement
    {
        private const int CARDS_PER_PAGE = 5;

        private readonly Dictionary<Enumerators.SetType, Enumerators.SetType> _against = new Dictionary<Enumerators.SetType, Enumerators.SetType>
                                                                                         {
                                                                                             { Enumerators.SetType.FIRE, Enumerators.SetType.WATER },
                                                                                             { Enumerators.SetType.TOXIC, Enumerators.SetType.FIRE },
                                                                                             { Enumerators.SetType.LIFE, Enumerators.SetType.TOXIC },
                                                                                             { Enumerators.SetType.EARTH, Enumerators.SetType.LIFE },
                                                                                             { Enumerators.SetType.AIR, Enumerators.SetType.EARTH },
                                                                                             { Enumerators.SetType.WATER, Enumerators.SetType.AIR }
                                                                                         };

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private ILocalizationManager _localizationManager;

        private IDataManager _dataManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private GameObject _selfPage;

        private TMP_InputField _deckNameInputField;

        private ButtonShiftingContent _buttonBuy, _buttonSave, _buttonArmy;

        private Button _buttonArmyArrowLeft, _buttonArmyArrowRight, _buttonHordeArrowLeft, _buttonHordeArrowRight, _buttonBack;

        private TMP_Text _cardAmountText;

        private Deck _currentDeck;

        private int _numSets, _currentElementPage, _numElementPages, _numHordePages, _currentHordePage;

        private Enumerators.SetType _currentSet;

        private Toggle _airToggle, _earthToggle, _fireToggle, _waterToggle, _toxicTogggle, _lifeToggle, _itemsToggle;

        private GameObject _cardCreaturePrefab, _cardSpellPrefab, _backgroundCanvasPrefab;

        private CollectionData _collectionData;

        private int _currentDeckId, _currentHeroId;

        private string _currentSetName;

        private List<BoardCard> _createdArmyCards, _createdHordeCards;

        private ToggleGroup _toggleGroup;

        private RectTransform _armyCardsContainer;

        private RectTransform _hordeCardsContainer;

        private SimpleScrollNotifier _armyScrollNotifier;

        private SimpleScrollNotifier _hordeScrollNotifier;

        private CardInfoPopupHandler _cardInfoPopupHandler;

        private GameObject _draggingObject;

        private Vector3 _initialCardPosition;

        private GameObject _hordeAreaObject, _armyAreaObject;

        private bool _isDragging;

        public int CurrentDeckId
        {
            set => _currentDeckId = value;
        }

        public int CurrentHeroId
        {
            set => _currentHeroId = value;
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _cardInfoPopupHandler = new CardInfoPopupHandler();
            _cardInfoPopupHandler.Init();
            _cardInfoPopupHandler.PreviewCardInstantiated += boardCard =>
            {
                boardCard.transform.Find("Amount").gameObject.SetActive(false);
                boardCard.SetAmountOfCardsInEditingPage(true, 0, 0);
            };

            _collectionData = new CollectionData();
            _collectionData.cards = new List<CollectionCardData>();

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            _cardSpellPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/SpellCard");

            // _cardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersEditingDeck");
            _backgroundCanvasPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/BackgroundEditingCanvas");

            _createdArmyCards = new List<BoardCard>();
            _createdHordeCards = new List<BoardCard>();
        }

        public void Update()
        {
            if ((_selfPage != null) && _selfPage.activeInHierarchy)
            {
                UpdateNumCardsText();

                _cardInfoPopupHandler.Update();
            }
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/DeckEditingPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

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

            _buttonSave = _selfPage.transform.Find("Button_Save").GetComponent<ButtonShiftingContent>();
            _buttonArmy = _selfPage.transform.Find("Button_Army").GetComponent<ButtonShiftingContent>();
            _buttonBuy = _selfPage.transform.Find("Button_Buy").GetComponent<ButtonShiftingContent>();
            _buttonBack = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _buttonArmyArrowLeft = _selfPage.transform.Find("Army/ArrowLeftButton").GetComponent<Button>();
            _buttonArmyArrowRight = _selfPage.transform.Find("Army/ArrowRightButton").GetComponent<Button>();
            _armyCardsContainer = _selfPage.transform.Find("Army/Cards").GetComponent<RectTransform>();
            _armyScrollNotifier = _selfPage.transform.Find("Army/ScrollArea").GetComponent<SimpleScrollNotifier>();

            _buttonHordeArrowLeft = _selfPage.transform.Find("Horde/ArrowLeftButton").GetComponent<Button>();
            _buttonHordeArrowRight = _selfPage.transform.Find("Horde/ArrowRightButton").GetComponent<Button>();
            _hordeCardsContainer = _selfPage.transform.Find("Horde/Cards").GetComponent<RectTransform>();
            _hordeScrollNotifier = _selfPage.transform.Find("Horde/ScrollArea").GetComponent<SimpleScrollNotifier>();

            _buttonBack.onClick.AddListener(BackButtonHandler);
            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonSave.onClick.AddListener(SaveButtonHandler);
            _buttonArmy.onClick.AddListener(ArmyButtonHandler);

            _airToggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.SetType.AIR);
                    }
                });
            _lifeToggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.SetType.LIFE);
                    }
                });
            _waterToggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.SetType.WATER);
                    }
                });
            _toxicTogggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.SetType.TOXIC);
                    }
                });
            _fireToggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.SetType.FIRE);
                    }
                });
            _earthToggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.SetType.EARTH);
                    }
                });
            _itemsToggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.SetType.ITEM);
                    }
                });

            _buttonArmyArrowLeft.onClick.AddListener(ArmyArrowLeftButtonHandler);
            _buttonArmyArrowRight.onClick.AddListener(ArmyArrowRightButtonHandler);
            _buttonHordeArrowLeft.onClick.AddListener(HordeArrowLeftButtonHandler);
            _buttonHordeArrowRight.onClick.AddListener(HordeArrowRightButtonHandler);

            _armyScrollNotifier.Scrolled += v =>
            {
                ScrollCardList(false, v);
            };

            _hordeScrollNotifier.Scrolled += v =>
            {
                ScrollCardList(true, v);
            };

            _deckNameInputField.onEndEdit.AddListener(OnDeckNameInputFieldEndedEdit);

            WarningPopup.OnHidePopupEvent += OnCloseAlertDialogEventHandler;
            FillCollectionData();

            _selfPage.SetActive(true);
            if (_currentDeckId == -1)
            {
                _currentDeck = new Deck();
                _currentDeck.id = -1;
                _currentDeck.name = "HORDE " + _dataManager.CachedDecksData.decks.Count;
                _currentDeck.cards = new List<DeckCardData>();
                _currentDeck.heroId = _currentHeroId;
            } else
            {
                _currentDeck = _dataManager.CachedDecksData.decks.First(d => d.id == _currentDeckId).Clone();
            }

            LoadDeckInfo(_currentDeck);
            InitObjects();

            _hordeAreaObject = _selfPage.transform.Find("Horde/ScrollArea").gameObject;
            _armyAreaObject = _selfPage.transform.Find("Army/ScrollArea").gameObject;
        }

        public void Hide()
        {
            Dispose();

            if (_selfPage == null)
            
return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
        }

        public void Dispose()
        {
            ResetArmyCards();
            ResetHordeItems();
            WarningPopup.OnHidePopupEvent -= OnCloseAlertDialogEventHandler;

            _cardInfoPopupHandler.Dispose();
        }

        public void MoveCardsPage(int direction)
        {
            // GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);
            _currentElementPage += direction;

            if (_currentElementPage < 0)
            {
                _currentSet += direction;

                if (_currentSet < 0)
                {
                    _currentSet = (Enumerators.SetType)(_numSets - 1);
                    CalculateNumberOfPages();
                    _currentElementPage = _numElementPages - 1;
                } else
                {
                    CalculateNumberOfPages();

                    _currentElementPage = _numElementPages - 1;

                    _currentElementPage = _currentElementPage < 0?0:_currentElementPage;
                }
            } else if (_currentElementPage >= _numElementPages)
            {
                _currentSet += direction;

                if ((int)_currentSet >= _numSets)
                {
                    _currentSet = 0;
                    _currentElementPage = 0;
                } else
                {
                    _currentElementPage = 0;
                }
            }

            UpdateCardsPage();
        }

        public void LoadCards(int page, Enumerators.SetType setType)
        {
            _toggleGroup.transform.GetChild((int)setType).GetComponent<Toggle>().isOn = true;

            CardSet set = SetTypeUtility.GetCardSet(_dataManager, setType);

            List<Card> cards = set.cards;

            // _currentSetName = set.name;
            int startIndex = page * CARDS_PER_PAGE;
            int endIndex = Mathf.Min(startIndex + CARDS_PER_PAGE, cards.Count);

            ResetArmyCards();

            for (int i = startIndex; i < endIndex; i++)
            {
                if (i >= cards.Count)
                {
                    break;
                }

                Card card = cards[i];
                CollectionCardData cardData = _dataManager.CachedCollectionData.GetCardData(card.name);

                // hack !!!! CHECK IT!!!
                if (cardData == null)
                {
                    continue;
                }

                BoardCard boardCard = CreateCard(card, Vector3.zero, _armyCardsContainer);

                DeckBuilderCard deckBuilderCard = boardCard.gameObject.AddComponent<DeckBuilderCard>();
                deckBuilderCard.scene = this;
                deckBuilderCard.card = card;

                OnBehaviourHandler eventHandler = boardCard.gameObject.GetComponent<OnBehaviourHandler>();

                eventHandler.OnBeginDragEvent += BoardCardOnBeginDragEventHandler;
                eventHandler.OnEndDragEvent += BoardCardOnEndDragEventHandler;
                eventHandler.OnDragEvent += BoardCardOnDragEventHandler;

                _createdArmyCards.Add(boardCard);
            }
        }

        public BoardCard CreateCard(Card card, Vector3 worldPos, RectTransform root)
        {
            BoardCard boardCard = null;
            GameObject go = null;
            if (card.cardKind == Enumerators.CardKind.CREATURE)
            {
                go = Object.Instantiate(_cardCreaturePrefab);
                boardCard = new UnitBoardCard(go);
            } else if (card.cardKind == Enumerators.CardKind.SPELL)
            {
                go = Object.Instantiate(_cardSpellPrefab);
                boardCard = new SpellBoardCard(go);
            }

            int amount = _collectionData.GetCardData(card.name).amount;

            boardCard.Init(card, amount);
            boardCard.SetHighlightingEnabled(false);
            boardCard.transform.position = worldPos;
            boardCard.transform.localScale = Vector3.one * 0.3f;
            boardCard.gameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LAYER_GAME_UI1;

            boardCard.transform.SetParent(_uiManager.Canvas.transform, true);
            RectTransform cardRectTransform = boardCard.gameObject.AddComponent<RectTransform>();

            if (root != null)
            {
                cardRectTransform.SetParent(root);
            }

            Vector3 anchoredPos = boardCard.transform.localPosition;
            anchoredPos.z = 0;
            boardCard.transform.localPosition = anchoredPos;

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

            foreach (DeckCardData card in deck.cards)
            {
                Card libraryCard = _dataManager.CachedCardsLibraryData.GetCardFromName(card.cardName);
                UpdateCardAmount(card.cardName, card.amount);

                bool itemFound = false;
                foreach (BoardCard item in _createdHordeCards)
                {
                    if (item.libraryCard.name == card.cardName)
                    {
                        itemFound = true;

                        // item.AddCard();
                        break;
                    }
                }

                if (!itemFound)
                {
                    BoardCard boardCard = CreateCard(libraryCard, Vector3.zero, _hordeCardsContainer);
                    boardCard.transform.Find("Amount").gameObject.SetActive(false);

                    DeckBuilderCard deckBuilderCard = boardCard.gameObject.AddComponent<DeckBuilderCard>();
                    deckBuilderCard.scene = this;
                    deckBuilderCard.card = libraryCard;
                    deckBuilderCard.isHordeItem = true;

                    _createdHordeCards.Add(boardCard);

                    boardCard.SetAmountOfCardsInEditingPage(true, GetMaxCopiesValue(libraryCard), card.amount);

                    _collectionData.GetCardData(card.cardName).amount -= card.amount;
                    UpdateNumCardsText();
                }
            }

            UpdateTopDeck();
        }

        public void RemoveCardFromDeck(DeckBuilderCard sender, Card card)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_REMOVE_CARD, Constants.SFX_SOUND_VOLUME, false, false, true);
            CollectionCardData collectionCardData = _collectionData.GetCardData(card.name);
            collectionCardData.amount++;
            UpdateCardAmount(card.name, collectionCardData.amount);
            BoardCard boardCard = _createdHordeCards.Find(item => item.libraryCard.id == card.id);
            boardCard.cardsAmountDeckEditing--;
            _currentDeck.RemoveCard(card.name);

            // Animated moving card
            if (sender != null)
            {
                int setIndex, cardIndex;
                GetSetAndIndexForCard(boardCard.libraryCard, out setIndex, out cardIndex);
                _currentSet = SetTypeUtility.GetCardSetType(_dataManager, setIndex);
                _currentElementPage = cardIndex / CARDS_PER_PAGE;
                UpdateCardsPage();

                Vector3 senderPosition = sender.transform.position;

                // Wait for 1 frame for UI to rebuild itself
                Sequence waitSequence = DOTween.Sequence();
                waitSequence.AppendInterval(Time.fixedDeltaTime);
                waitSequence.AppendCallback(
                    () =>
                    {
                        CreateExchangeAnimationCard(
                            senderPosition,
                            boardCard.libraryCard,
                            true,
                            _createdArmyCards,
                            pageIndex =>
                            {
                            });
                    });
            }

            if (boardCard.cardsAmountDeckEditing == 0)
            {
                _createdHordeCards.Remove(boardCard);

                Object.DestroyImmediate(boardCard.gameObject);

                int currentHordePage = _currentHordePage;
                UpdateHordePagesCount();
                if (currentHordePage >= _numHordePages)
                {
                    _currentHordePage = _numHordePages - 1;
                }

                RepositionHordeCards();
                UpdateNumCardsText();
            } else
            {
                boardCard.SetAmountOfCardsInEditingPage(false, GetMaxCopiesValue(boardCard.libraryCard), boardCard.cardsAmountDeckEditing);
            }
        }

        public void AddCardToDeck(DeckBuilderCard sender, Card card)
        {
            if (_currentDeck == null)
            
return;

            if (_against[_dataManager.CachedHeroesData.Heroes[_currentHeroId].heroElement] == card.cardSetType)
            {
                OpenAlertDialog("It's not possible to add cards to the deck \n from the faction from which the hero is weak against");
                return;
            }

            CollectionCardData collectionCardData = _collectionData.GetCardData(card.name);
            if (collectionCardData.amount == 0)
            {
                OpenAlertDialog("You don't have enough cards of this type. \n Buy or earn new packs to get more cards!");
                return;
            }

            DeckCardData existingCards = _currentDeck.cards.Find(x => x.cardName == card.name);

            uint maxCopies = GetMaxCopiesValue(card);
            string cardRarity = "You cannot have more than ";

            if ((existingCards != null) && (existingCards.amount == maxCopies))
            {
                OpenAlertDialog("You cannot have more than " + maxCopies + " copies of the " + card.cardRank.ToString().ToLower() + " card in your deck.");
                return;
            }

            uint maxDeckSize = Constants.DECK_MAX_SIZE;
            if (_currentDeck.GetNumCards() == maxDeckSize)
            {
                OpenAlertDialog("Your '" + _currentDeck.name + "' deck has more than " + maxDeckSize + " cards.");
                return;
            }

            bool itemFound = false;
            BoardCard foundItem = null;
            foreach (BoardCard item in _createdHordeCards)
            {
                if (item.libraryCard.id == card.id)
                {
                    foundItem = item;
                    itemFound = true;

                    // item.AddCard();
                    break;
                }
            }

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_ADD_CARD, Constants.SFX_SOUND_VOLUME, false, false, true);
            collectionCardData.amount--;
            UpdateCardAmount(card.name, collectionCardData.amount);

            if (!itemFound)
            {
                BoardCard boardCard = CreateCard(card, Vector3.zero, _hordeCardsContainer);
                boardCard.transform.Find("Amount").gameObject.SetActive(false);
                foundItem = boardCard;

                DeckBuilderCard deckBuilderCard = boardCard.gameObject.AddComponent<DeckBuilderCard>();
                deckBuilderCard.scene = this;
                deckBuilderCard.card = card;
                deckBuilderCard.isHordeItem = true;

                _createdHordeCards.Add(boardCard);

                UpdateHordePagesCount();
                CalculateVisibility();
            }

            _currentDeck.AddCard(card.name);

            foundItem.SetAmountOfCardsInEditingPage(false, GetMaxCopiesValue(card), _currentDeck.cards.Find(x => x.cardName == foundItem.libraryCard.name).amount);

            // Animated moving card
            if (sender != null)
            {
                CreateExchangeAnimationCard(
                    sender.transform.position,
                    foundItem.libraryCard,
                    itemFound,
                    _createdHordeCards,
                    pageIndex =>
                    {
                        _currentHordePage = pageIndex;
                        CalculateVisibility();
                        Canvas.ForceUpdateCanvases();
                    });
            }

            if (_currentHordePage + 1 < _numHordePages)
            {
                MoveHordeToRight();
            }
        }

        public uint GetMaxCopiesValue(Card card)
        {
            Enumerators.CardRank rank = card.cardRank;
            uint maxCopies = 0;

            string setName = GameClient.Get<IGameplayManager>().GetController<CardsController>().GetSetOfCard(card);

            if (setName.ToLower().Equals("item"))
            {
                maxCopies = Constants.CARD_ITEM_MAX_COPIES;
                return maxCopies;
            }

            switch (rank)
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

        public void UpdateCardAmount(string cardId, int amount)
        {
            foreach (BoardCard card in _createdArmyCards)
            {
                if (card.libraryCard.name == cardId)
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
                _cardAmountText.text = _currentDeck.GetNumCards() + " / " + Constants.DECK_MAX_SIZE;
            }
        }

        public async void OnDoneButtonPressed()
        {
            if (string.IsNullOrWhiteSpace(_currentDeck.name))
            {
                OpenAlertDialog("Saving Horde with an empty name is not allowed.");
                return;
            }

            _dataManager.CachedDecksLastModificationTimestamp = Utilites.GetCurrentUnixTimestampMillis();

            foreach (Deck deck in _dataManager.CachedDecksData.decks)
            {
                if ((_currentDeckId != deck.id) && deck.name.Trim().Equals(_currentDeck.name.Trim(), StringComparison.CurrentCultureIgnoreCase))
                {
                    OpenAlertDialog("Not able to Edit Deck: \n Deck Name already exists.");
                    return;
                }
            }

            bool success = true;
            if (_currentDeckId == -1)
            {
                // HACK for offline mode: in online mode, local data should only be saved after
                // backend operation has succeeded
                // Quick Fix for : if there are no decks, error
                if (_dataManager.CachedDecksData.decks.Count > 0)
                {
                    _currentDeck.id = _dataManager.CachedDecksData.decks.Max(d => d.id) + 1;
                } else
                {
                    _currentDeck.id = 0;
                }

                // Add new deck
                _currentDeck.heroId = _currentHeroId;
                _dataManager.CachedDecksData.decks.Add(_currentDeck);

                try
                {
                    long newDeckId = await _backendFacade.AddDeck(_backendDataControlMediator.UserDataModel.UserId, _currentDeck, _dataManager.CachedDecksLastModificationTimestamp);
                    _currentDeck.id = newDeckId;
                    Debug.Log(" ====== Add Deck " + newDeckId + " Successfully ==== ");
                } catch (Exception e)
                {
                    Debug.Log("Result === " + e);

                    // HACK: for offline mode
                    if (false)
                    {
                        success = false;
                        OpenAlertDialog("Not able to Add Deck: \n" + e.Message);
                    }
                }
            } else
            {
                // Update existing deck
                for (int i = 0; i < _dataManager.CachedDecksData.decks.Count; i++)
                {
                    if (_dataManager.CachedDecksData.decks[i].id == _currentDeckId)
                    {
                        _dataManager.CachedDecksData.decks[i] = _currentDeck;
                        break;
                    }
                }

                try
                {
                    await _backendFacade.EditDeck(_backendDataControlMediator.UserDataModel.UserId, _currentDeck, _dataManager.CachedDecksLastModificationTimestamp);
                    Debug.Log(" ====== Edit Deck Successfully ==== ");
                } catch (Exception e)
                {
                    Debug.Log("Result === " + e);

                    // HACK: for offline mode
                    if (false)
                    {
                        success = false;
                        OpenAlertDialog("Not able to Edit Deck: \n" + e.Message);
                    }
                }
            }

            if (success)
            {
                _dataManager.CachedUserLocalData.lastSelectedDeckId = (int)_currentDeck.id;
                await _dataManager.SaveCache(Enumerators.CacheDataType.DECKS_DATA);
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.DECK_SELECTION);
            }
        }

        public void ScrollCardList(bool isHordeItem, Vector2 scrollDelta)
        {
            if (isHordeItem)
            {
                if (scrollDelta.y > 0.5f)
                {
                    MoveHordeToRight();
                } else if (scrollDelta.y < -0.5f)
                {
                    MoveHordeToLeft();
                }
            } else
            {
                MoveCardsPage(Mathf.RoundToInt(scrollDelta.y));
            }
        }

        public void SelectCard(DeckBuilderCard deckBuilderCard, Card card)
        {
            BoardCard boardCard = null;
            if (deckBuilderCard.isHordeItem)
            {
                boardCard = _createdHordeCards.First(c => c.libraryCard.id == card.id);
            } else
            {
                boardCard = _createdArmyCards.First(c => c.libraryCard.id == card.id);
            }

            _cardInfoPopupHandler.SelectCard(boardCard);
        }

        private void FillCollectionData()
        {
            _collectionData.cards.Clear();
            CollectionCardData cardData;
            foreach (CollectionCardData card in _dataManager.CachedCollectionData.cards)
            {
                cardData = new CollectionCardData();
                cardData.amount = card.amount;
                cardData.cardName = card.cardName;

                _collectionData.cards.Add(cardData);
            }
        }

        private void ResetArmyCards()
        {
            if (_createdArmyCards != null)
            {
                foreach (BoardCard item in _createdArmyCards)
                {
                    item.Dispose();
                }

                _createdArmyCards.Clear();
            }
        }

        private void ResetHordeItems()
        {
            if (_createdHordeCards != null)
            {
                foreach (BoardCard item in _createdHordeCards)
                {
                    item.Dispose();
                }

                _createdHordeCards.Clear();
            }
        }

        private void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);
            foreach (BoardCard card in _createdArmyCards)
            {
                card.gameObject.GetComponent<DeckBuilderCard>().isActive = false;
            }

            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        private void OnCloseAlertDialogEventHandler()
        {
            foreach (BoardCard card in _createdArmyCards)
            {
                card.gameObject.GetComponent<DeckBuilderCard>().isActive = true;
            }
        }

        private void InitObjects()
        {
            _numSets = _dataManager.CachedCardsLibraryData.sets.Count - 1;
            CalculateNumberOfPages();

            Enumerators.SetType heroSetType = _dataManager.CachedHeroesData.Heroes.Find(x => x.heroId == _currentDeck.heroId).heroElement;
            LoadCards(0, heroSetType);
        }

        private bool GetSetAndIndexForCard(Card card, out int setIndex, out int cardIndex)
        {
            setIndex = -1;
            cardIndex = -1;
            for (int i = 0; i < _dataManager.CachedCardsLibraryData.sets.Count; i++)
            {
                CardSet cardSet = _dataManager.CachedCardsLibraryData.sets[i];
                cardIndex = cardSet.cards.FindIndex(c => c.id == card.id);

                if (cardIndex != -1)
                {
                    setIndex = i;
                    break;
                }
            }

            return false;
        }

        private void UpdateCardsPage()
        {
            CalculateNumberOfPages();
            LoadCards(_currentElementPage, _currentSet);
        }

        private void CalculateNumberOfPages()
        {
            _numElementPages = Mathf.CeilToInt(SetTypeUtility.GetCardSet(_dataManager, _currentSet).cards.Count / (float)CARDS_PER_PAGE);
        }

        private void UpdateTopDeck()
        {
            UpdateHordePagesCount();
            _currentHordePage = 0;
            RepositionHordeCards();
        }

        private void UpdateHordePagesCount()
        {
            _numHordePages = Mathf.CeilToInt((float)_createdHordeCards.Count / CARDS_PER_PAGE);
        }

        private void CalculateVisibility()
        {
            for (int i = 0; i < _createdHordeCards.Count; i++)
            {
                if ((i + 1 > _currentHordePage * CARDS_PER_PAGE) && (i + 1 < ((_currentHordePage + 1) * CARDS_PER_PAGE) + 1))
                {
                    _createdHordeCards[i].gameObject.SetActive(true);
                } else
                {
                    _createdHordeCards[i].gameObject.SetActive(false);
                }
            }
        }

        private void RepositionHordeCards()
        {
            CalculateVisibility();
        }

        private void CreateExchangeAnimationCard(Vector3 sourceCardPosition, Card targetLibraryCard, bool targetCardWasAlreadyPresent, List<BoardCard> targetRowCards, Action<int> setPageIndexAction)
        {
            BoardCard animatedCard = CreateCard(targetLibraryCard, sourceCardPosition, null);
            animatedCard.transform.Find("Amount").gameObject.SetActive(false);
            animatedCard.gameObject.GetComponent<SortingGroup>().sortingOrder++;

            int foundItemIndex = targetRowCards.FindIndex(c => c.libraryCard.id == targetLibraryCard.id);
            setPageIndexAction(foundItemIndex / CARDS_PER_PAGE);

            BoardCard targetCard = targetRowCards.First(card => card.libraryCard.id == targetLibraryCard.id);
            Vector3 animatedCardDestination = targetCard.transform.position;

            if (!targetCardWasAlreadyPresent)
            {
                targetCard.gameObject.SetActive(false);
            }

            Sequence animatedCardSequence = DOTween.Sequence();
            animatedCardSequence.Append(animatedCard.transform.DOMove(animatedCardDestination, .3f));
            animatedCardSequence.AppendCallback(() => Object.Destroy(animatedCard.gameObject));
            animatedCardSequence.AppendCallback(
                () =>
                {
                    if (!targetCardWasAlreadyPresent)
                    {
                        targetCard.gameObject.SetActive(true);
                    }
                });
        }

        private void BoardCardOnBeginDragEventHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (_isDragging)
            
return;

            _draggingObject = Object.Instantiate(onOnject);
            _draggingObject.transform.localScale = Vector3.one * 0.3f;
            _draggingObject.transform.Find("Amount").gameObject.SetActive(false);
            _draggingObject.name = onOnject.GetInstanceID().ToString();

            _isDragging = true;

            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.z = _draggingObject.transform.position.z;
            _draggingObject.transform.position = position;
        }

        private void BoardCardOnEndDragEventHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (!_isDragging)
            
return;

            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, 1 << 0);

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject == _hordeAreaObject)
                    {
                        BoardCard armyCard = _createdArmyCards.Find(x => x.gameObject.GetInstanceID().ToString() == _draggingObject.name);

                        AddCardToDeck(null, armyCard.libraryCard);
                    }
                }
            }

            Object.Destroy(_draggingObject);
            _draggingObject = null;
            _isDragging = false;
        }

        private void BoardCardOnDragEventHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (!_isDragging)
            
return;

            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.z = _draggingObject.transform.position.z;
            _draggingObject.transform.position = position;
        }

        #region button handlers

        private void ToggleChooseOnValueChangedHandler(Enumerators.SetType type)
        {
            if (type == _currentSet)
            
return;

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);

            _currentSet = type;
            _currentElementPage = 0;
            LoadCards(_currentElementPage, _currentSet);
        }

        private void BackButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _uiManager.GetPopup<QuestionPopup>().ConfirmationEvent += ConfirmQuitEventHandler;
            _uiManager.DrawPopup<QuestionPopup>(new object[] { "Would you like to save the current horde?", true });
        }

        private void ConfirmQuitEventHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationEvent -= ConfirmQuitEventHandler;
            if (status)
            {
                OnDoneButtonPressed();
            }

            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.DECK_SELECTION);
        }

        private void BuyButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.SHOP);
        }

        private void ArmyButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.COLLECTION);
        }

        // private void OpenButtonHandler()
        // {
        // GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        // GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
        // }
        private void SaveButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            OnDoneButtonPressed();
        }

        private void ArmyArrowLeftButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            MoveCardsPage(-1);
        }

        private void ArmyArrowRightButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            MoveCardsPage(1);
        }

        private void HordeArrowLeftButtonHandler()
        {
            MoveHordeToLeft();
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        }

        private void MoveHordeToLeft()
        {
            _currentHordePage--;
            if (_currentHordePage < 0)
            {
                _currentHordePage = _numHordePages - 1;
            }

            CalculateVisibility();
        }

        private void HordeArrowRightButtonHandler()
        {
            MoveHordeToRight();
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        }

        private void MoveHordeToRight()
        {
            _currentHordePage++;

            if (_currentHordePage >= _numHordePages)
            {
                _currentHordePage = 0;
            }

            CalculateVisibility();
        }

        #endregion
    }
}
