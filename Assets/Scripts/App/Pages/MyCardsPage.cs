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
            
            ResetBoardCards();
            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }
        
        public void Dispose()
        {          
            ResetBoardCards();
        }

        #endregion

        #region Board Cards
        
        private TextMeshProUGUI _cardCounter;
        
        public List<Transform> CardPositions;

        public GameObject CardCreaturePrefab, CardItemPrefab, CardPlaceholdersPrefab, CardPlaceholders;
        
        private List<BoardCard> _createdBoardCards;

        private CardHighlightingVFXItem _highlightingVFXItem;
        
        private Enumerators.SetType _currentSet = Enumerators.SetType.FIRE;
        
        private int _currentElementPage, _numElementPages;
        
        private void InitObjects()
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
            _numElementPages = Mathf.CeilToInt(SetTypeUtility.GetCardSet(_dataManager, _currentSet).Cards.Count /
                (float) CardPositions.Count);
        }

        #endregion
    }
}
