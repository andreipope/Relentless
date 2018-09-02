// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class CollectionPage : IUIElement
    {
        public List<Transform> cardPositions;

        public GameObject _cardCreaturePrefab, _cardSpellPrefab, _cardPlaceholdersPrefab, _cardPlaceholders;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private ILocalizationManager _localizationManager;

        private IDataManager _dataManager;

        private GameObject _selfPage;

        private ButtonShiftingContent _buttonBuy, _buttonOpen;

        private Button _buttonArrowLeft, _buttonArrowRight, _buttonBack;

        private TextMeshProUGUI _cardCounter;

        private TextMeshProUGUI gooValueText;

        private GameObject _cardSetsIcons;

        private int _numSets, _currentElementPage, _numElementPages;

        private Enumerators.SetType _currentSet;

        private Toggle _airToggle, _earthToggle, _fireToggle, _waterToggle, _toxicTogggle, _lifeToggle, _itemsToggle;

        private ToggleGroup _toggleGroup;

        private CardInfoPopupHandler _cardInfoPopupHandler;

        private List<BoardCard> _createdBoardCards;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _cardInfoPopupHandler = new CardInfoPopupHandler();
            _cardInfoPopupHandler.Init();
            _cardInfoPopupHandler.StateChanging += () => ChangeStatePopup(_cardInfoPopupHandler.IsStateChanging);
            _cardInfoPopupHandler.StateChanged += () => ChangeStatePopup(_cardInfoPopupHandler.IsStateChanging);
            _cardInfoPopupHandler.Closing += UpdateGooValue;

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            _cardSpellPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/SpellCard");
            _cardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholders");

            _createdBoardCards = new List<BoardCard>();
        }

        public void Update()
        {
            if ((_selfPage != null) && _selfPage.activeInHierarchy)
            {
                _cardInfoPopupHandler.Update();
                if (_cardInfoPopupHandler.IsInteractable)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
                        if (hit.collider != null)
                        {
                            for (int i = 0; i < _createdBoardCards.Count; i++)
                            {
                                if (hit.collider.gameObject == _createdBoardCards[i].gameObject)
                                {
                                    _cardInfoPopupHandler.SelectCard(_createdBoardCards[i]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/CollectionPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            gooValueText = _selfPage.transform.Find("GooValue/Value").GetComponent<TextMeshProUGUI>();

            _buttonBuy = _selfPage.transform.Find("Button_Buy").GetComponent<ButtonShiftingContent>();
            _buttonOpen = _selfPage.transform.Find("Button_Open").GetComponent<ButtonShiftingContent>();
            _buttonBack = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _buttonArrowLeft = _selfPage.transform.Find("Button_ArrowLeft").GetComponent<Button>();
            _buttonArrowRight = _selfPage.transform.Find("Button_ArrowRight").GetComponent<Button>();

            _toggleGroup = _selfPage.transform.Find("ElementsToggles").GetComponent<ToggleGroup>();
            _airToggle = _selfPage.transform.Find("ElementsToggles/Air").GetComponent<Toggle>();
            _lifeToggle = _selfPage.transform.Find("ElementsToggles/Life").GetComponent<Toggle>();
            _waterToggle = _selfPage.transform.Find("ElementsToggles/Water").GetComponent<Toggle>();
            _toxicTogggle = _selfPage.transform.Find("ElementsToggles/Toxic").GetComponent<Toggle>();
            _fireToggle = _selfPage.transform.Find("ElementsToggles/Fire").GetComponent<Toggle>();
            _earthToggle = _selfPage.transform.Find("ElementsToggles/Earth").GetComponent<Toggle>();
            _itemsToggle = _selfPage.transform.Find("ElementsToggles/Items").GetComponent<Toggle>();

            _cardCounter = _selfPage.transform.Find("CardsCounter").GetChild(0).GetComponent<TextMeshProUGUI>();

            _cardSetsIcons = _selfPage.transform.Find("ElementsToggles").gameObject;

            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonOpen.onClick.AddListener(OpenButtonHandler);
            _buttonBack.onClick.AddListener(BackButtonHandler);
            _buttonArrowLeft.onClick.AddListener(ArrowLeftButtonHandler);
            _buttonArrowRight.onClick.AddListener(ArrowRightButtonHandler);

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

            // _uiManager.Canvas.GetComponent<Canvas>().worldCamera = GameObject.Find("Camera2").GetComponent<Camera>();
            gooValueText.text = GameClient.Get<IPlayerManager>().GetGoo().ToString();

            _selfPage.SetActive(true);
            InitObjects();
        }

        public void Hide()
        {
            Object.Destroy(_cardPlaceholders);
            ResetBoardCards();

            if (_selfPage == null)
            
return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
        }

        public void Dispose()
        {
            Object.Destroy(_cardPlaceholders);
            ResetBoardCards();
            _cardInfoPopupHandler.Dispose();
        }

        public void UpdateGooValue()
        {
            gooValueText.text = GameClient.Get<IPlayerManager>().GetGoo().ToString();
        }

        public void MoveCardsPage(int direction)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);
            CalculateNumberOfPages();

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

            LoadCards(_currentElementPage, _currentSet);
        }

        public void OnNextPageButtonPressed()
        {
        }

        public void LoadCards(int page, Enumerators.SetType setType)
        {
            // CorrectSetIndex(ref setIndex);
            _toggleGroup.transform.GetChild((int)setType).GetComponent<Toggle>().isOn = true;

            CardSet set = SetTypeUtility.GetCardSet(_dataManager, setType);

            List<Card> cards = set.cards;

            int startIndex = page * cardPositions.Count;

            int endIndex = Mathf.Min(startIndex + cardPositions.Count, cards.Count);

            ResetBoardCards();

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

                GameObject go = null;
                BoardCard boardCard = null;
                if (card.cardKind == Enumerators.CardKind.CREATURE)
                {
                    go = Object.Instantiate(_cardCreaturePrefab);
                    boardCard = new UnitBoardCard(go);
                } else if (card.cardKind == Enumerators.CardKind.SPELL)
                {
                    go = Object.Instantiate(_cardSpellPrefab);
                    boardCard = new SpellBoardCard(go);
                }

                int amount = cardData.amount;
                boardCard.Init(card, amount);
                boardCard.SetHighlightingEnabled(false);
                boardCard.transform.position = cardPositions[i % cardPositions.Count].position;
                boardCard.transform.localScale = Vector3.one * 0.32f;
                boardCard.gameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LAYER_GAME_UI1;

                _createdBoardCards.Add(boardCard);
            }

            highlightCorrectIcon();
        }

        private void ResetBoardCards()
        {
            foreach (BoardCard item in _createdBoardCards)
            {
                item.Dispose();
            }

            _createdBoardCards.Clear();
        }

        private void iconSetButtonClick(Button toggleObj)
        {
            _currentSet = (Enumerators.SetType)toggleObj.transform.GetSiblingIndex();
            _currentElementPage = 0;
            LoadCards(_currentElementPage, _currentSet);
        }

        private void InitObjects()
        {
            _cardPlaceholders = Object.Instantiate(_cardPlaceholdersPrefab);
            cardPositions = new List<Transform>();

            foreach (Transform placeholder in _cardPlaceholders.transform)
            {
                cardPositions.Add(placeholder);
            }

            // pageText.text = "Page " + (currentPage + 1) + "/" + numPages;
            _numSets = _dataManager.CachedCardsLibraryData.sets.Count - 1;
            CalculateNumberOfPages();

            // _cardSetsSlider.value = 0;
            LoadCards(0, 0);

            _cardCounter.text = _dataManager.CachedCollectionData.cards.Count + "/" + _dataManager.CachedCardsLibraryData.Cards.Count;
        }

        private void highlightCorrectIcon()
        {
            for (int i = 0; i < _cardSetsIcons.transform.childCount; i++)
            {
                GameObject c = _cardSetsIcons.transform.GetChild(i).GetChild(0).gameObject;
                if (i == (int)_currentSet)
                {
                    c.SetActive(true);
                } else
                {
                    c.SetActive(false);
                }
            }
        }

        private void CalculateNumberOfPages()
        {
            _numElementPages = Mathf.CeilToInt(SetTypeUtility.GetCardSet(_dataManager, _currentSet).cards.Count / (float)cardPositions.Count);
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        #region Buttons Handlers

        private void ToggleChooseOnValueChangedHandler(Enumerators.SetType type)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);
            _currentSet = type;
            LoadCards(0, type);
        }

        private void ChangeStatePopup(bool isStart)
        {
            // _cardSetsSlider.interactable = !isStart;
            _buttonBuy.interactable = !isStart;
            _buttonOpen.interactable = !isStart;
            _buttonArrowLeft.interactable = !isStart;
            _buttonArrowRight.interactable = !isStart;
            _buttonBack.interactable = !isStart;
        }

        private void BuyButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.SHOP);
        }

        private void OpenButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.PACK_OPENER);
        }

        private void BackButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }

        private void ArrowLeftButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            MoveCardsPage(-1);
        }

        private void ArrowRightButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            MoveCardsPage(1);
        }

        #endregion
    }
}
