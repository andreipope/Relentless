using System;
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
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        public GameObject Self { get; private set; }

        private TextMeshProUGUI _textCardName,
                                _textCardDescription;

        private Button _buttonAdd,
                       _buttonRemove,
                       _buttonBack,
                       _buttonLeftArrow,
                       _buttonRightArrow;

        private TMP_InputField _inputFieldSearch;

        private Transform _groupCreatureCard;
        
        private IReadOnlyCard _currentCard;
        
        private GameObject _cardCreaturePrefab, 
                           _cardItemPrefab;

        private GameObject _createdBoardCard;

        private const float BoardCardScale = 0.5858f;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
        }
        
        public void Dispose()
        {
            if (_createdBoardCard != null)
                Object.Destroy(_createdBoardCard);
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
            _buttonLeftArrow.onClick.AddListener(PlayClickSound);
            
            _buttonRightArrow = Self.transform.Find("Button_RightArrow").GetComponent<Button>();
            _buttonRightArrow.onClick.AddListener(ButtonRightArrowHandler);
            _buttonRightArrow.onClick.AddListener(PlayClickSound);
            
            _buttonAdd = Self.transform.Find("Button_AddToDeck").GetComponent<Button>();
            _buttonAdd.onClick.AddListener(ButtonAddCardHandler);
            _buttonAdd.onClick.AddListener(PlayClickSound);
            
            _buttonRemove = Self.transform.Find("Button_Remove").GetComponent<Button>();
            _buttonRemove.onClick.AddListener(ButtonRemoveCardHandler);
            _buttonRemove.onClick.AddListener(PlayClickSound);
            
            _buttonBack = Self.transform.Find("Background/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);
            _buttonBack.onClick.AddListener(PlayClickSound);
            
            _inputFieldSearch = Self.transform.Find("InputText_SearchDeckName").GetComponent<TMP_InputField>();
            _inputFieldSearch.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            _inputFieldSearch.text = "";
            
            _textCardName = Self.transform.Find("Text_CardName").GetComponent<TextMeshProUGUI>();            
            _textCardDescription = Self.transform.Find("Text_CardDesc").GetComponent<TextMeshProUGUI>();

            _groupCreatureCard = Self.transform.Find("Group_CreatureCard");

            _buttonAdd.gameObject.SetActive(false);
            _buttonRemove.gameObject.SetActive(false);
            _buttonLeftArrow.gameObject.SetActive(false);
            _buttonRightArrow.gameObject.SetActive(false);

            UpdateBoardCard();
        }

        public void Show(object data)
        {
            _currentCard = (IReadOnlyCard) data;
            Show();
        }

        public void Update()
        {
        }

        #region UI Handlers
        
        private void ButtonBackHandler()
        {
            Hide();
        }
        
        private void ButtonAddCardHandler()
        {

        }
        
        private void ButtonRemoveCardHandler()
        {

        }
        
        private void ButtonLeftArrowHandler()
        {

        }
        
        private void ButtonRightArrowHandler()
        {

        }
        
        public void OnInputFieldSearchEndedEdit(string value)
        {
        
        }

        #endregion
        
        private void UpdateBoardCard()
        {
            if (_createdBoardCard != null)
                Object.Destroy(_createdBoardCard);

            if (_currentCard == null)
            {
                Debug.Log($"Current card in {nameof(CardInfoWithSearchPopup)} is null");
                return;
            }
                
            RectTransform rectContainer = _groupCreatureCard.GetComponent<RectTransform>();
            BoardCard boardCard = CreateBoardCard
            (
                _currentCard, 
                rectContainer,
                _groupCreatureCard.position, 
                BoardCardScale
            );

            boardCard.Transform.SetParent(_groupCreatureCard);
            _createdBoardCard = boardCard.GameObject;

            _textCardName.text = _currentCard.Name;
            _textCardDescription.text = _currentCard.Description;
        }

        private BoardCard CreateBoardCard(IReadOnlyCard card, RectTransform root, Vector3 position, float scale)
        {
            GameObject go;
            BoardCard boardCard;
            
            switch (card.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(_cardCreaturePrefab);
                    boardCard = new UnitBoardCard(go);
                    break;
                case Enumerators.CardKind.SPELL:
                    go = Object.Instantiate(_cardItemPrefab);
                    boardCard = new SpellBoardCard(go);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(card.CardKind), card.CardKind, null);
            }
            
            boardCard.Init(card);
            boardCard.SetHighlightingEnabled(false);
            boardCard.Transform.position = position;
            boardCard.Transform.localScale = Vector3.one * scale;
            boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI2;
            boardCard.Transform.Find("Amount").gameObject.SetActive(false);
            
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