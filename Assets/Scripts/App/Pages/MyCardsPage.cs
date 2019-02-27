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
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class MyCardsPage : IUIElement
    {
        private IUIManager _uiManager;

        private IDataManager _dataManager;
        
        private ILoadObjectsManager _loadObjectsManager;
        
        private GameObject _selfPage;

        private Button _buttonLeftArrow,
                       _buttonRightArrow,
                       _buttonFilter,
                       _buttonBuyPacks,
                       _buttonMarketplace;
                       
        private TMP_InputField _inputFieldSearchName; 
        
        #region IUIElement
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            
            CardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            CardItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard");
            CardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersMyCards");

            _createdBoardCards = new List<BoardCard>();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MyCardsPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);
            
            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_CARDS);
            _uiManager.DrawPopup<AreaBarPopup>();
            
            _highlightingVFXItem = new CardHighlightingVFXItem(Object.Instantiate(
            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/UI/ArmyCardSelection"), _selfPage.transform, true));
            
            _cardCounter = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Frame/Upper_Items/Image_CardfCounter/Text_CardCounter").GetComponent<TextMeshProUGUI>();
            
            _buttonFilter = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Frame/Upper_Items/Button_Filter").GetComponent<Button>();
            _buttonFilter.onClick.AddListener(ButtonFilterHandler);
            _buttonFilter.onClick.AddListener(PlayClickSound);
            
            _buttonMarketplace = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Frame/Upper_Items/Button_MarketPlace").GetComponent<Button>();
            _buttonMarketplace.onClick.AddListener(ButtonMarketplace);
            _buttonMarketplace.onClick.AddListener(PlayClickSound);
            
            _buttonBuyPacks = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Frame/Lower_Items/Button_BuyMorePacks").GetComponent<Button>();
            _buttonBuyPacks.onClick.AddListener(ButtonBuyPacksHandler);
            _buttonBuyPacks.onClick.AddListener(PlayClickSound);
            
            _buttonLeftArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Content/Button_LeftArrow").GetComponent<Button>();
            _buttonLeftArrow.onClick.AddListener(ButtonLeftArrowHandler);
            _buttonLeftArrow.onClick.AddListener(PlayClickSound);
            
            _buttonRightArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Content/Button_RightArrow").GetComponent<Button>();
            _buttonRightArrow.onClick.AddListener(ButtonRightArrowHandler);
            _buttonRightArrow.onClick.AddListener(PlayClickSound);
            
            _inputFieldSearchName = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Frame/Upper_Items/InputText_Search").GetComponent<TMP_InputField>();
            _inputFieldSearchName.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            _inputFieldSearchName.text = "";
            
            LoadObjects();

        }
        
        public void Hide()
        {
            Dispose();
        
            if (_selfPage == null)
                return;
        
            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }
        
        public void Dispose()
        {          
            ResetBoardCards();
            Object.Destroy(CardPlaceholders);
        }

        #endregion

        #region UI Handlers

        private void ButtonFilterHandler()
        {
            GameClient.Get<IUIManager>().DrawPopup<CardFilterPopup>();
            CardFilterPopup popup = GameClient.Get<IUIManager>().GetPopup<CardFilterPopup>();
            popup.ActionPopupHiding += FilterPopupHidingHandler;
        }
        
        private void FilterPopupHidingHandler(CardFilterPopup.CardFilterData cardFilterData)
        {
            CardFilterPopup popup = GameClient.Get<IUIManager>().GetPopup<CardFilterPopup>();
            popup.ActionPopupHiding -= FilterPopupHidingHandler;
        }
        
        private void ButtonBuyPacksHandler()
        {
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.SHOP);
        }
        
        private void ButtonLeftArrowHandler()
        {
            MoveCardsPage(-1);
        }
        
        private void ButtonRightArrowHandler()
        {
            MoveCardsPage(1);
        }
        
        private void ButtonMarketplace()
        {
            Application.OpenURL(Constants.MarketPlaceLink);
        }

        public void OnInputFieldSearchEndedEdit(string value)
        {
        
        }

        #endregion

        #region Board Cards

        private TextMeshProUGUI _cardCounter;
        
        public List<Transform> CardPositions;

        public GameObject CardCreaturePrefab,
                          CardItemPrefab,
                          CardPlaceholdersPrefab;
        
        public GameObject CardPlaceholders;
        
        private List<BoardCard> _createdBoardCards;

        private CardHighlightingVFXItem _highlightingVFXItem;
        
        private Enumerators.SetType _currentSet = Enumerators.SetType.FIRE;
        
        private int _currentElementPage, _numElementPages;
        
        private void LoadObjects()
        {
            CardPlaceholders = Object.Instantiate(CardPlaceholdersPrefab);
            Vector3 cardPlaceholdersPos = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Content/Locator_CardPosition").position;
            cardPlaceholdersPos.z = 0f;
            CardPlaceholders.transform.position = cardPlaceholdersPos;
            
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

        public void LoadCards(int page, Enumerators.SetType setType)
        {
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
                BoardCard boardCard;
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

                int amount = cardData.Amount;
                boardCard.Init(card, amount);
                boardCard.SetHighlightingEnabled(false);
                boardCard.Transform.position = CardPositions[i % CardPositions.Count].position;
                boardCard.Transform.localScale = Vector3.one * 0.3f;
                boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;
                boardCard.Transform.Find("Amount").gameObject.SetActive(false);

                _createdBoardCards.Add(boardCard);

                if (boardCard.LibraryCard.MouldId == _highlightingVFXItem.MouldId)
                {
                    _highlightingVFXItem.ChangeState(true);
                }
            }
        }

        private void ResetBoardCards()
        {
            foreach (BoardCard item in _createdBoardCards)
            {
                item.Dispose();
            }

            _createdBoardCards.Clear();
        }

        private void CalculateNumberOfPages()
        {
            _numElementPages = Mathf.CeilToInt(
                SetTypeUtility.GetCardSet
                (
                    _dataManager, _currentSet).Cards.Count / (float) CardPositions.Count
                );
        }
        
        public void MoveCardsPage(int direction)
        {
            CalculateNumberOfPages();

            _currentElementPage += direction;

            if (_currentElementPage < 0)
            {
                _currentSet += direction;

                if (_currentSet < Enumerators.SetType.FIRE)
                {
                    _currentSet = Enumerators.SetType.ITEM;
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

                if (_currentSet > Enumerators.SetType.ITEM)
                {
                    _currentSet = Enumerators.SetType.FIRE;
                    _currentElementPage = 0;
                }
                else
                {
                    _currentElementPage = 0;
                }
            }

            LoadCards(_currentElementPage, _currentSet);
        }

        #endregion
        
        #region Util

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
        
        public void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        #endregion
    }
}
