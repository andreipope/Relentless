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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ArmyWithNavigationPage : IUIElement
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

        #region Cache Data

        private List<Enumerators.Faction> _availableSetType;

        private int _currentPage,
                    _currentPagesAmount;

        private List<Card> _cacheFilteredSetTypeCardsList;

        private int _countAllCardsAmount;

        private bool _isAllCardsCounted;

        #endregion

        #region IUIElement

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            
            CardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            CardItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard");
            CardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersMyCards");

            _createdBoardCards = new List<BoardCardView>();
            _cacheFilteredSetTypeCardsList = new List<Card>();

            _isAllCardsCounted = false;          
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
            
            _cardCounter = _selfPage.transform.Find("Panel_Frame/Upper_Items/Image_CardfCounter/Text_CardCounter").GetComponent<TextMeshProUGUI>();
            
            _buttonFilter = _selfPage.transform.Find("Panel_Frame/Upper_Items/Button_Filter").GetComponent<Button>();
            _buttonFilter.onClick.AddListener(ButtonFilterHandler);
            
            _buttonMarketplace = _selfPage.transform.Find("Panel_Frame/Upper_Items/Button_MarketPlace").GetComponent<Button>();
            _buttonMarketplace.onClick.AddListener(ButtonMarketplace);
            
            _buttonBuyPacks = _selfPage.transform.Find("Panel_Frame/Lower_Items/Button_BuyMorePacks").GetComponent<Button>();
            _buttonBuyPacks.onClick.AddListener(ButtonBuyPacksHandler);
            
            _buttonLeftArrow = _selfPage.transform.Find("Panel_Content/Button_LeftArrow").GetComponent<Button>();
            _buttonLeftArrow.onClick.AddListener(ButtonLeftArrowHandler);
            
            _buttonRightArrow = _selfPage.transform.Find("Panel_Content/Button_RightArrow").GetComponent<Button>();
            _buttonRightArrow.onClick.AddListener(ButtonRightArrowHandler);
            
            _inputFieldSearchName = _selfPage.transform.Find("Panel_Frame/Upper_Items/InputText_Search").GetComponent<TMP_InputField>();
            _inputFieldSearchName.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            _inputFieldSearchName.text = "";

            _uiManager.GetPopup<CardFilterPopup>().FilterData.Reset();   
            
            UpdateAllCardsCount();         
            
            LoadObjects();

            UpdatePageScaleToMatchResolution();            
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
            if (_cacheFilteredSetTypeCardsList != null)
            {
                _cacheFilteredSetTypeCardsList.Clear();
            }
            Object.Destroy(_createdBoardCardContainer);
        }

        #endregion
        
        private void UpdatePageScaleToMatchResolution()
        {
            float screenRatio = (float)Screen.width/Screen.height;
            if(screenRatio < 1.76f)
            {
                _selfPage.transform.localScale = Vector3.one * 0.93f;
            }
        }

        #region UI Handlers

        private void ButtonFilterHandler()
        {
            PlayClickSound();
            GameClient.Get<IUIManager>().DrawPopup<CardFilterPopup>();
            CardFilterPopup popup = GameClient.Get<IUIManager>().GetPopup<CardFilterPopup>();
            popup.ActionPopupHiding += FilterPopupHidingHandler;
        }
        
        private void FilterPopupHidingHandler(CardFilterPopup.CardFilterData cardFilterData)
        {
            ResetPageState();
            CardFilterPopup popup = GameClient.Get<IUIManager>().GetPopup<CardFilterPopup>();
            popup.ActionPopupHiding -= FilterPopupHidingHandler;
        }
        
        private void ButtonBuyPacksHandler()
        {
            PlayClickSound();
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.SHOP);
        }
        
        private void ButtonLeftArrowHandler()
        {
            PlayClickSound();
            MoveCardsPage(-1);
        }
        
        private void ButtonRightArrowHandler()
        {
            PlayClickSound();
            MoveCardsPage(1);
        }
        
        private void ButtonMarketplace()
        {
            PlayClickSound();
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmRedirectMarketplaceLink;
            _uiManager.DrawPopup<QuestionPopup>("Do you want to redirect to marketplace webpage?"); 
        }
        
        private void ConfirmRedirectMarketplaceLink(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmRedirectMarketplaceLink;
            if(status)
            {
                Application.OpenURL(Constants.MarketPlaceLink);
            }
        }

        public void OnInputFieldSearchEndedEdit(string value)
        {
            ResetPageState();
        }

        #endregion

        #region Board Cards

        private TextMeshProUGUI _cardCounter;
        
        public List<Transform> CardPositions;

        public GameObject CardCreaturePrefab,
                          CardItemPrefab,
                          CardPlaceholdersPrefab;
        
        public GameObject CardPlaceholders;

        private GameObject _createdBoardCardContainer;
        
        private List<BoardCardView> _createdBoardCards;

        private CardHighlightingVFXItem _highlightingVFXItem;
        
        private void LoadObjects()
        {
            CardPlaceholders = Object.Instantiate(CardPlaceholdersPrefab);
            _createdBoardCardContainer = new GameObject("BoardCardContainer");
            
            CardPositions = new List<Transform>();

            foreach (Transform placeholder in CardPlaceholders.transform)
            {
                CardPositions.Add(placeholder);
            }

            ResetPageState();            
        }

        public void LoadCards()
        {
            ResetBoardCards();
            List<Card> cards = _cacheFilteredSetTypeCardsList.ToList();

            int startIndex = _currentPage * CardPositions.Count;
            int endIndex = Mathf.Min(startIndex + CardPositions.Count, cards.Count);

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
                Vector3 position = CardPositions[i % CardPositions.Count].position;
                BoardCardView boardCard = CreateBoardCard
                (
                    card,
                    cardData,
                    position                    
                );
                _createdBoardCards.Add(boardCard);
                
                MultiPointerClickHandler multiPointerClickHandler = boardCard.GameObject.AddComponent<MultiPointerClickHandler>();
                multiPointerClickHandler.SingleClickReceived += ()=>
                {
                    BoardCardSingleClickHandler(boardCard);
                };
                multiPointerClickHandler.DoubleClickReceived += ()=>
                {
                    BoardCardSingleClickHandler(boardCard);
                };
            }
        }
        
        private void BoardCardSingleClickHandler(BoardCardView boardCard)
        {
            if (_uiManager.GetPopup<CardInfoWithSearchPopup>().Self != null)
                return;    
                
            List<IReadOnlyCard> cardList = _createdBoardCards.Select(i => i.Model.Card.Prototype).ToList();
            _uiManager.DrawPopup<CardInfoWithSearchPopup>(new object[]
            {
                cardList,
                boardCard.Model.Card.Prototype,
                CardInfoWithSearchPopup.PopupType.NONE
            });
        }
        
        private BoardCardView CreateBoardCard(Card card, CollectionCardData cardData, Vector3 position)
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
                    go = Object.Instantiate(CardItemPrefab);
                    boardCard = new ItemBoardCardView(go, cardModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(card.Kind), card.Kind, null);
            }

            int amount = cardData.Amount;
            boardCard.SetAmount(BoardCardView.AmountTrayType.None, amount);
            boardCard.SetHighlightingEnabled(false);
            boardCard.Transform.position = position;
            boardCard.Transform.localScale = Vector3.one * 0.3f;
            boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;
            
            boardCard.Transform.SetParent(_createdBoardCardContainer.transform);
            
            if (boardCard.Model.Card.Prototype.MouldId == _highlightingVFXItem.MouldId)
            {
                _highlightingVFXItem.ChangeState(true);
            }

            return boardCard;
        }

        private void ResetBoardCards()
        {
            foreach (BoardCardView item in _createdBoardCards)
            {
                item.Dispose();
            }

            _createdBoardCards.Clear();
        }
        
        public void MoveCardsPage(int direction)
        {
            _currentPage += direction;

            if (_currentPage < 0)
            {                
                _currentPage = Mathf.Max(_currentPagesAmount - 1, 0);               
            }
            else if (_currentPage >= _currentPagesAmount)
            {
                _currentPage = 0;
            }

            LoadCards();
        }

        #endregion
        
        private void ResetPageState()
        {
            _availableSetType = _uiManager.GetPopup<CardFilterPopup>().FilterData.GetFilteredFactionList();
            _currentPage = 0;
            UpdateAvailableSetTypeCards();
            LoadCards();
            UpdateCardCounterText();
        }

        private void UpdateAvailableSetTypeCards()
        {
            string keyword = _inputFieldSearchName.text.Trim().ToLower();
            Faction set;
            List<Card> cards;
            List<Card> resultList = new List<Card>();
            List<Enumerators.Faction> allAvailableSetTypeList = _uiManager.GetPopup<CardFilterPopup>().AllAvailableFactionList;

            foreach (Enumerators.Faction item in allAvailableSetTypeList)
            {
                set = SetTypeUtility.GetCardFaction(_dataManager, item);
                cards = set.Cards.ToList();

                foreach (Card card in cards)
                {
                    if (string.IsNullOrEmpty(keyword))
                    {
                        if
                        (
                            CheckIfSatisfyGooCostFilter(card) &&
                            CheckIfSatisfyRankFilter(card) &&
                            CheckIfSatisfyTypeFilter(card)
                        )
                        {
                            resultList.Add(card);
                        }
                    }
                    else
                    {
                        if (card.Name.ToLower().Contains(keyword))
                        {
                            resultList.Add(card);
                        }
                    }
                }
            }
            
            UpdateCacheFilteredCardList(resultList);
        }
        
        private void UpdateCardCounterText()
        {
            int amount = 0;
            string keyword = _inputFieldSearchName.text.Trim();
            Faction set;
            List<Card> cards;
            if (string.IsNullOrEmpty(keyword))
            {
                foreach (Enumerators.Faction item in _availableSetType)
                {
                    set = SetTypeUtility.GetCardFaction(_dataManager, item);
                    cards = set.Cards.ToList();
                    foreach(Card card in cards)
                    {
                        if
                        (
                            CheckIfSatisfyGooCostFilter(card) &&
                            CheckIfSatisfyRankFilter(card) &&
                            CheckIfSatisfyTypeFilter(card)
                        )
                        {
                            ++amount;
                        }
                    }
                }
            }
            else
            {
                keyword = keyword.ToLower();
                List<Enumerators.Faction> allAvailableSetTypeList = _uiManager.GetPopup<CardFilterPopup>().AllAvailableFactionList;
                foreach (Enumerators.Faction item in allAvailableSetTypeList)
                {
                    set = SetTypeUtility.GetCardFaction(_dataManager, item);
                    cards = set.Cards.ToList();
                    foreach (Card card in cards)
                    {
                        if (card.Name.ToLower().Contains(keyword))
                        {
                            ++amount;
                        }
                    }
                }
            }
            
            _cardCounter.text = amount + "/" +
                _countAllCardsAmount;
        }
        
        private void UpdateAllCardsCount()
        {
            if (_isAllCardsCounted)
                return;
                
            int amount = 0;
            Faction set;
            List<Enumerators.Faction> allFactions = _uiManager.GetPopup<CardFilterPopup>().AllAvailableFactionList;
            foreach (Enumerators.Faction item in allFactions)
            {
                set = SetTypeUtility.GetCardFaction(_dataManager, item);
                amount += set.Cards.ToList().Count;
            }
            _countAllCardsAmount = amount;
            _isAllCardsCounted = true;       
        }

        private bool CheckIfSatisfyGooCostFilter(Card card)
        {
            if (card.Cost < 0)
                return false;
                
            if(card.Cost >= 10)
            {
                if (_uiManager.GetPopup<CardFilterPopup>().FilterData.GooCostList[10])
                    return true;
            }
            else
            {
                if(_uiManager.GetPopup<CardFilterPopup>().FilterData.GooCostList[card.Cost])
                    return true;
            }

            return false;
        }
        
        private bool CheckIfSatisfyRankFilter(Card card)
        {
            return _uiManager.GetPopup<CardFilterPopup>().FilterData.RankDictionary[card.Rank];
        }
        
        private bool CheckIfSatisfyTypeFilter(Card card)
        {
            return _uiManager.GetPopup<CardFilterPopup>().FilterData.TypeDictionary[card.Type];
        }
        
        private void UpdateCacheFilteredCardList(List<Card> cardList)
        {
            _cacheFilteredSetTypeCardsList = cardList.FindAll(card => !card.Hidden).ToList();
            _currentPagesAmount = Mathf.CeilToInt
            (
                _cacheFilteredSetTypeCardsList.Count / (float) CardPositions.Count
            );
        }
        
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
