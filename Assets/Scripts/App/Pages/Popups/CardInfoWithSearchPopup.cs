using System;
using System.Linq;
using System.Collections.Generic;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class CardInfoWithSearchPopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(CardInfoWithSearchPopup));
        
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private ITutorialManager _tutorialManager;

        public GameObject Self { get; private set; }

        private TextMeshProUGUI _textHeader,
                                _textDescription;

        private Button _buttonAdd,
                       _buttonRemove,
                       _buttonBack,
                       _buttonLeftArrow,
                       _buttonRightArrow;

        private TMP_InputField _inputFieldSearch;

        private Transform _groupCreatureCard;
        
        private GameObject _cardCreaturePrefab, 
                           _cardItemPrefab;

        private GameObject _createdBoardCard;

        private const float BoardCardScale = 0.42f;

        private List<IReadOnlyCard> _cardList,
                                    _filteredCardList;

        private int _currentCardIndex;
        
        public enum PopupType
        {
            NONE,
            ADD_CARD,
            REMOVE_CARD
        }

        private PopupType _currentPopupType;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _cardList = new List<IReadOnlyCard>();
            _filteredCardList = new List<IReadOnlyCard>();
            _currentCardIndex = -1;
        }
        
        public void Dispose()
        {
            if (_createdBoardCard != null)
                Object.Destroy(_createdBoardCard);

            if (_cardList != null)
                _cardList.Clear();

            if (_filteredCardList != null)
                _filteredCardList.Clear();
        }

        public void Hide()
        {
            Dispose();
            
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
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/CardInfoWithSearchPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);
            
            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>
            (
                "Prefabs/Gameplay/Cards/CreatureCard"
            );
            _cardItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>
            (
                "Prefabs/Gameplay/Cards/ItemCard"
            );
            
            _buttonLeftArrow = Self.transform.Find("Button_LeftArrow").GetComponent<Button>();
            _buttonLeftArrow.onClick.AddListener(ButtonLeftArrowHandler);
            
            _buttonRightArrow = Self.transform.Find("Button_RightArrow").GetComponent<Button>();
            _buttonRightArrow.onClick.AddListener(ButtonRightArrowHandler);
            
            _buttonAdd = Self.transform.Find("Button_AddToDeck").GetComponent<Button>();
            _buttonAdd.onClick.AddListener(ButtonAddCardHandler);
            
            _buttonRemove = Self.transform.Find("Button_Remove").GetComponent<Button>();
            _buttonRemove.onClick.AddListener(ButtonRemoveCardHandler);
            
            _buttonBack = Self.transform.Find("Background/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);
            
            _inputFieldSearch = Self.transform.Find("InputText_SearchDeckName").GetComponent<TMP_InputField>();
            _inputFieldSearch.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            _inputFieldSearch.text = "";
            
            _textHeader = Self.transform.Find("Text_CardTitle").GetComponent<TextMeshProUGUI>();            
            _textDescription = Self.transform.Find("Text_CardDesc").GetComponent<TextMeshProUGUI>();

            _groupCreatureCard = Self.transform.Find("Group_CreatureCard");

            UpdatePopupType();
            UpdateFilteredCardList();
            UpdateBoardCard();
        }

        public void Show(object data)
        {
            if (data is object[] param)
            {
                _cardList = (List<IReadOnlyCard>)param[0];
                IReadOnlyCard card = (IReadOnlyCard)param[1];
                _currentCardIndex = _cardList.IndexOf(card);
                _currentPopupType = (PopupType)param[2];
                            
            }
            Show();
        }

        public void Update()
        {
        }

        #region UI Handlers
        
        private void ButtonBackHandler()
        {
            PlayClickSound();
            Hide();
        }
        
        private void ButtonAddCardHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonAdd.name))
                return;

            PlayClickSound();
            _uiManager.GetPage<HordeSelectionWithNavigationPage>().HordeEditTab.AddCardToDeck
            (
                _filteredCardList[_currentCardIndex],
                true
            );
            Hide();
        }
        
        private void ButtonRemoveCardHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonRemove.name))
                return;

            PlayClickSound();
            _uiManager.GetPage<HordeSelectionWithNavigationPage>().HordeEditTab.RemoveCardFromDeck
            (
                _filteredCardList[_currentCardIndex],
                true
            ); 
            Hide();
        }
        
        private void ButtonLeftArrowHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonLeftArrow.name))
                return;

            PlayClickSound();
            MoveCardIndex(-1);
        }
        
        private void ButtonRightArrowHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonRightArrow.name))
                return;

            PlayClickSound();
            MoveCardIndex(1);
        }
        
        public void OnInputFieldSearchEndedEdit(string value)
        {
            UpdateFilteredCardList();
            _currentCardIndex = 0;
            MoveCardIndex(0);
        }

        #endregion
        
        private void UpdatePopupType()
        {
            switch(_currentPopupType)
            {
                case PopupType.NONE:
                    _buttonAdd.gameObject.SetActive(false);
                    _buttonRemove.gameObject.SetActive(false);
                    break;
                case PopupType.ADD_CARD:
                    _buttonAdd.gameObject.SetActive(true);
                    _buttonRemove.gameObject.SetActive(false);
                    break;
                case PopupType.REMOVE_CARD:
                    _buttonAdd.gameObject.SetActive(false);
                    _buttonRemove.gameObject.SetActive(true);
                    break;
                default:
                    return;
            }
        }

        private void UpdateBoardCard()
        {
            if (_createdBoardCard != null)
                Object.Destroy(_createdBoardCard);

            if (_cardList == null)
            {
                Log.Info($"Current _cardList in {nameof(CardInfoWithSearchPopup)} is null");
                return;
            }

            if (_currentCardIndex < 0 || _currentCardIndex >= _filteredCardList.Count)
            {                
                Log.Info($"No matching card index for {nameof(CardInfoWithSearchPopup)}");
                return;
            }

            IReadOnlyCard card = _filteredCardList[_currentCardIndex];
                
            RectTransform rectContainer = _groupCreatureCard.GetComponent<RectTransform>();
            BoardCardView boardCard = CreateBoardCard
            (
                card, 
                rectContainer,
                _groupCreatureCard.position, 
                BoardCardScale
            );
            boardCard.Transform.SetParent(_groupCreatureCard);
            
            _createdBoardCard = boardCard.GameObject;

            _textHeader.text = "";
            _textDescription.text = card.FlavorText;
        }
        
        private void UpdateFilteredCardList()
        {
            string keyword = _inputFieldSearch.text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                _filteredCardList = _cardList.ToList();
            }
            else
            {
                _filteredCardList.Clear();
                keyword = keyword.ToLower();
                foreach (IReadOnlyCard card in _cardList)
                {
                    if (card.Name.ToLower().Contains(keyword))
                        _filteredCardList.Add(card);
                }
                if(_filteredCardList.Count <= 0)
                {
                    OpenAlertDialog($"No card found for keyword {_inputFieldSearch.text.Trim()}");
                    _filteredCardList = _cardList.ToList();
                }
            }
        }

        private void MoveCardIndex(int direction)
        {
            if(_filteredCardList == null)
                Log.Info($"Current _filteredCardList in {nameof(CardInfoWithSearchPopup)} is null");

            if (_filteredCardList.Count <= 1)
            {
                _currentCardIndex = _filteredCardList.Count-1;
            }
            else
            {
                int newIndex = _currentCardIndex + direction;

                if (newIndex < 0)
                    newIndex = _filteredCardList.Count - 1;
                else if (newIndex >= _filteredCardList.Count)
                    newIndex = 0;

                _currentCardIndex = newIndex;
            }
            UpdateBoardCard();
        }

        private BoardCardView CreateBoardCard(IReadOnlyCard card, RectTransform root, Vector3 position, float scale)
        {
            GameObject go;
            BoardCardView boardCard;
            CardModel cardModel = new CardModel(new WorkingCard(card, card, null));

            switch (card.Kind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(_cardCreaturePrefab);
                    boardCard = new UnitBoardCardView(go, cardModel);
                    break;
                case Enumerators.CardKind.ITEM:
                    go = Object.Instantiate(_cardItemPrefab);
                    boardCard = new ItemBoardCardView(go, cardModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(card.Kind), card.Kind, null);
            }

            boardCard.SetHighlightingEnabled(false);
            boardCard.Transform.position = position;
            boardCard.Transform.localScale = Vector3.one * scale;
            boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameplayInfo;
            
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
