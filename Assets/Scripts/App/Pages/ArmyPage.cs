using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ArmyPage : IUIElement
    {
        public List<Transform> CardPositions;

        public GameObject CardCreaturePrefab, CardItemPrefab, CardPlaceholdersPrefab, CardPlaceholders;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private GameObject _selfPage;

        private ButtonShiftingContent _buttonBuy, _buttonOpen;

        private Button _buttonArrowLeft, _buttonArrowRight, _buttonBack;

        private TextMeshProUGUI _cardCounter;

        private TextMeshProUGUI _gooValueText;

        private GameObject _cardSetsIcons;

        private int _currentElementPage, _numElementPages;

        private Enumerators.Faction _currentSet = Enumerators.Faction.FIRE;

        private Toggle _airToggle, _earthToggle, _fireToggle, _waterToggle, _toxicTogggle, _lifeToggle, _itemsToggle;

        private ToggleGroup _toggleGroup;

        private CardInfoPopupHandler _cardInfoPopupHandler;

        private List<BoardCardView> _createdBoardCards;

        private CardHighlightingVFXItem _highlightingVFXItem;


        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _cardInfoPopupHandler = new CardInfoPopupHandler();
            _cardInfoPopupHandler.Init();
            _cardInfoPopupHandler.StateChanging += () => ChangeStatePopup(_cardInfoPopupHandler.IsStateChanging);
            _cardInfoPopupHandler.StateChanged += () => ChangeStatePopup(_cardInfoPopupHandler.IsStateChanging);
            _cardInfoPopupHandler.Closing += UpdateGooValue;

            CardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            CardItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard");
            CardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholders");

            _createdBoardCards = new List<BoardCardView>();
        }

        public void Update()
        {
            if (_selfPage != null && _selfPage.activeInHierarchy)
            {
                _cardInfoPopupHandler.Update();
                if (_cardInfoPopupHandler.IsInteractable)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                        RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, SRLayerMask.Default);
                        if (hits.Length > 0)
                        {
                            foreach (RaycastHit2D hit in hits)
                            {
                                if (hit.collider != null)
                                {
                                    for (int i = 0; i < _createdBoardCards.Count; i++)
                                    {
                                        if (hit.collider.gameObject == _createdBoardCards[i].GameObject)
                                        {
                                            _highlightingVFXItem.SetActiveCard(_createdBoardCards[i]);
                                            _cardInfoPopupHandler.SelectCard(_createdBoardCards[i]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/ArmyPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _gooValueText = _selfPage.transform.Find("GooValue/Value").GetComponent<TextMeshProUGUI>();

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

            _highlightingVFXItem = new CardHighlightingVFXItem(Object.Instantiate(
            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/UI/ArmyCardSelection"), _selfPage.transform, true));

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
                        ToggleChooseOnValueChangedHandler(Enumerators.Faction.AIR);
                    }
                });
            _lifeToggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.Faction.LIFE);
                    }
                });
            _waterToggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.Faction.WATER);
                    }
                });
            _toxicTogggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.Faction.TOXIC);
                    }
                });
            _fireToggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.Faction.FIRE);
                    }
                });
            _earthToggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.Faction.EARTH);
                    }
                });
            _itemsToggle.onValueChanged.AddListener(
                state =>
                {
                    if (state)
                    {
                        ToggleChooseOnValueChangedHandler(Enumerators.Faction.ITEM);
                    }
                });

            _gooValueText.text = GameClient.Get<IPlayerManager>().GetGoo().ToString();

            _selfPage.SetActive(true);
            InitObjects();
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
            Object.Destroy(CardPlaceholders);
            ResetBoardCards();
            _cardInfoPopupHandler.Dispose();
        }

        public void UpdateGooValue()
        {
            _gooValueText.text = GameClient.Get<IPlayerManager>().GetGoo().ToString();
        }

        public void MoveCardsPage(int direction)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            CalculateNumberOfPages();

            _currentElementPage += direction;

            if (_currentElementPage < 0)
            {
                _currentSet += direction;

                if (_currentSet < Enumerators.Faction.FIRE)
                {
                    _currentSet = Enumerators.Faction.ITEM;
                    CalculateNumberOfPages();
                    _currentElementPage = _numElementPages - 1;
                }
                else
                {
                    CalculateNumberOfPages();

                    _currentElementPage = _numElementPages - 1;

                    _currentElementPage = _currentElementPage < 0 ? 0 : _currentElementPage;
                }
            }
            else if (_currentElementPage >= _numElementPages)
            {
                _currentSet += direction;

                if (_currentSet > Enumerators.Faction.ITEM)
                {
                    _currentSet = Enumerators.Faction.FIRE;
                    _currentElementPage = 0;
                }
                else
                {
                    _currentElementPage = 0;
                }
            }

            LoadCards(_currentElementPage, _currentSet);
        }

        public void LoadCards(int page, Enumerators.Faction setType)
        {
            _toggleGroup.transform.GetChild(setType - Enumerators.Faction.FIRE).GetComponent<Toggle>().isOn = true;

            CardSet set = SetTypeUtility.GetCardSet(_dataManager, setType);

            List<Card> cards = set.Cards;

            int startIndex = page * CardPositions.Count;

            int endIndex = Mathf.Min(startIndex + CardPositions.Count, cards.Count);

            ResetBoardCards();
            _highlightingVFXItem.ChangeState(false);

            for (int i = startIndex; i < endIndex; i++)
            {
                if (i >= cards.Count)
                    break;

                Card card = cards[i];
                CollectionCardData cardData = _dataManager.CachedCollectionData.GetCardData(card.Name);

                // hack !!!! CHECK IT!!!
                if (cardData == null)
                    continue;

                GameObject go;
                BoardCardView boardCardView;
                BoardUnitModel boardUnitModel = new BoardUnitModel(new WorkingCard(card, card, null));
                switch (card.CardKind)
                {
                    case Enumerators.CardKind.CREATURE:
                        go = Object.Instantiate(CardCreaturePrefab);
                        boardCardView = new UnitBoardCard(go, boardUnitModel);
                        break;
                    case Enumerators.CardKind.ITEM:
                        go = Object.Instantiate(CardItemPrefab);
                        boardCardView = new SpellBoardCard(go, boardUnitModel);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(card.CardKind), card.CardKind, null);
                }

                boardCardView.SetAmount(cardData.Amount);
                boardCardView.SetShowAmountEnabled(true);
                boardCardView.SetHighlightingEnabled(false);
                boardCardView.Transform.position = CardPositions[i % CardPositions.Count].position;
                boardCardView.Transform.localScale = Vector3.one * 0.32f;
                boardCardView.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;

                _createdBoardCards.Add(boardCardView);

                if (boardCardView.BoardUnitModel.Card.Prototype.MouldId == _highlightingVFXItem.MouldId)
                {
                    _highlightingVFXItem.ChangeState(true);
                }
            }

            HighlightCorrectIcon();
        }

        private void ResetBoardCards()
        {
            foreach (BoardCardView item in _createdBoardCards)
            {
                item.Dispose();
            }

            _createdBoardCards.Clear();
        }

        private void InitObjects()
        {
            CardPlaceholders = Object.Instantiate(CardPlaceholdersPrefab);
            CardPositions = new List<Transform>();

            foreach (Transform placeholder in CardPlaceholders.transform)
            {
                CardPositions.Add(placeholder);
            }

            CalculateNumberOfPages();
            LoadCards(0, _currentSet);

            //TODO first number should be cards in collection. Collection for now equals ALL cards, once it won't,
            //we'll have to change this.
            _cardCounter.text = _dataManager.CachedCardsLibraryData.CardsInActiveSetsCount + "/" +
                _dataManager.CachedCardsLibraryData.CardsInActiveSetsCount;
        }

        private void HighlightCorrectIcon()
        {
            for (int i = 0; i < _cardSetsIcons.transform.childCount; i++)
            {
                GameObject c = _cardSetsIcons.transform.GetChild(i).GetChild(0).gameObject;
                c.SetActive(i == _currentSet - Enumerators.Faction.FIRE);
            }
        }

        private void CalculateNumberOfPages()
        {
            _numElementPages = Mathf.CeilToInt(SetTypeUtility.GetCardSet(_dataManager, _currentSet).Cards.Count /
                (float) CardPositions.Count);
        }

        #region Buttons Handlers

        private void ToggleChooseOnValueChangedHandler(Enumerators.Faction type)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _currentSet = type;
            _currentElementPage = 0;
            LoadCards(0, type);
        }

        private void ChangeStatePopup(bool isStart)
        {
            _buttonBuy.interactable = !isStart;
            _buttonOpen.interactable = !isStart;
            _buttonArrowLeft.interactable = !isStart;
            _buttonArrowRight.interactable = !isStart;
            _buttonBack.interactable = !isStart;
        }

        private void BuyButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.SHOP);
        }

        private void OpenButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.PACK_OPENER);
        }

        private void BackButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }

        private void ArrowLeftButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            MoveCardsPage(-1);
        }

        private void ArrowRightButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            MoveCardsPage(1);
        }

        #endregion
    }
}
