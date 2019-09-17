using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class SelectSkinPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        private ILoadObjectsManager _loadObjectsManager;
        private ISoundManager _soundManager;
        private IUIManager _uiManager;
        private IDataManager _dataManager;
        private IGameplayManager _gameplayManager;

        private GameObject _unitCardPrefab;
        private Transform _cardsParentTransform;

        private Button _arrowLeft;
        private Button _arrowRight;

        private Toggle _popupToggle;

        private IReadOnlyCard _card;
        private List<CollectionCardData> _variantCardsList;

        private CollectionData _collectionData;

        private List<SelectSkinCard> _cardsToShow;

        private int currentPage;

        private int maxPages;

        private const float UnitCardSize = 0.55f;

        public void Dispose()
        {
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();

            _unitCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/CreatureCard_UI");
        }

        public void Hide()
        {
            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(
              _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/SelectSkinPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _arrowLeft = Self.transform.Find("ArrowLeft").GetComponent<Button>();
            _arrowLeft.onClick.AddListener(LeftArrowButtonHandler);
            _arrowRight = Self.transform.Find("ArrowRight").GetComponent<Button>();
            _arrowRight.onClick.AddListener(RightArrowButtonHandler);
            _popupToggle = Self.transform.Find("AutomatedPopupToggle").GetComponent<Toggle>();

            _popupToggle.isOn = !_uiManager.GetPage<HordeSelectionWithNavigationPage>().HordeEditTab.VariantPopupIsActive;

            _popupToggle.onValueChanged.AddListener(PopupToggleHandler);
            
            if (_variantCardsList.Count <= 3)
            {
                _arrowLeft.gameObject.SetActive(false);
                _arrowRight.gameObject.SetActive(false);
            }

            _cardsParentTransform = Self.transform.Find("Cards").transform;

            currentPage = 0;

            FillCards();
            RightArrowButtonHandler();
        }

        public void Show(object data)
        {
            _card = (IReadOnlyCard) data;
            _collectionData = _uiManager.GetPage<HordeSelectionWithNavigationPage>().HordeEditTab.GetCollectionData();
            _variantCardsList = _collectionData.Cards.FindAll(x => _card.CardKey.MouldId == x.CardKey.MouldId && x.Amount > 0).ToList();
            Show();
        }

        public void Update()
        {
        }

        private void CloseFromPopupCallback()
        {   
            WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
            popup.ConfirmationReceived -= CloseFromPopupCallback;

            Hide();
        }

        private void PopupToggleHandler(bool change)
        {
            _uiManager.GetPage<HordeSelectionWithNavigationPage>().HordeEditTab.VariantPopupIsActive = !change;

            if (change)
            {
                WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
                popup.ConfirmationReceived += CloseFromPopupCallback;

                _uiManager.DrawPopup<WarningPopup>("Double-Tap a card to manually select card edition.");
            }
        }

        private void LeftArrowButtonHandler()
        {
            for (int i = 0; i < _cardsToShow.Count; i++)
            {
                _cardsToShow[i].SetActive(false);
            }
            currentPage--;
            if (currentPage <= 0)
            {
                currentPage = (int)Mathf.Ceil((float)_cardsToShow.Count/3f);
            }

            for (int i = currentPage*3-1; i > currentPage*3-4; i--)
            {
                if (i < _cardsToShow.Count && _cardsToShow[i] != null)
                {
                    _cardsToShow[i].SetActive(true);
                }
            }
        }

        private void RightArrowButtonHandler()
        {
            for (int i = 0; i < _cardsToShow.Count; i++)
            {
                _cardsToShow[i].SetActive(false);
            }
            currentPage++;
            if (currentPage > (int)Mathf.Ceil((float)_cardsToShow.Count/3f))
            {
                currentPage = 1;
            }

            for (int i = currentPage*3-1; i > currentPage*3-4; i--)
            {
                if (i < _cardsToShow.Count && _cardsToShow[i] != null)
                {
                    _cardsToShow[i].SetActive(true);
                }
            }
        }

        private void FillCards()
        {
            _cardsToShow = new List<SelectSkinCard>();

            for (int i = 0; i < _variantCardsList.Count; i++)
            {
                GameObject cardObj = Object.Instantiate(_unitCardPrefab, _cardsParentTransform, false);
                cardObj.transform.localScale = Vector3.one * UnitCardSize;

                UnitCardUI unitCardUi = new UnitCardUI();
                unitCardUi.Init(cardObj);
                unitCardUi.FillCardData((Card)_dataManager.CachedCardsLibraryData.Cards.FirstOrDefault(x => x.CardKey == _variantCardsList[i].CardKey), 0);

                SelectSkinCard skinCard = new SelectSkinCard(unitCardUi);
                _cardsToShow.Add(skinCard);

                MultiPointerClickHandler multiPointerClickHandler = cardObj.AddComponent<MultiPointerClickHandler>();
                multiPointerClickHandler.SingleClickReceived += () => { BoardCardSingleClickHandler(skinCard); };
            }
        }

        private void BoardCardSingleClickHandler(SelectSkinCard skinCard)
        {
            _uiManager.GetPage<HordeSelectionWithNavigationPage>().HordeEditTab.AddCardToDeck(_card, skipPopup:true, cardKey:skinCard.GetCardKey());
            Hide();
        }
    }

    public class SelectSkinCard
    {
        private readonly UnitCardUI _unitCardUi;

        public SelectSkinCard(UnitCardUI unitCardUi)
        {
            _unitCardUi = unitCardUi;
        }

        public CardKey GetCardKey()
        {
            return _unitCardUi.GetCard().CardKey;
        }

        public void SetActive(bool active)
        {
            _unitCardUi.SetActive(active);
        }
    }
}
