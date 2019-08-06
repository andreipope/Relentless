using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Card = Loom.ZombieBattleground.Data.Card;
using Deck = Loom.ZombieBattleground.Data.Deck;

namespace Loom.ZombieBattleground
{
    public class CustomDeckUI
    {
        private static readonly ILog Log = Logging.GetLog(nameof(CustomDeckUI));

        private TextMeshProUGUI _deckNameText;
        private TextMeshProUGUI _cardsCountText;

        private Button _buttonRename;
        private Button _buttonViewDeck;
        private Button _buttonSave;

        private Image _overlordImage;
        private Image _overlordPrimarySkillImage;
        private Image _overlordSecondarySkillImage;

        private GameObject _deckCardPrefab;

        private IUIManager _uiManager;
        private IDataManager _dataManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ITutorialManager _tutorialManager;

        private List<DeckCardUI> _deckCards = new List<DeckCardUI>();

        private RectTransform _allCardsContent;

        private Camera _mainCamera;

        private Deck _selectedDeck;

        private ViewDeckPage _viewDeckPage;

        private DeckCardUI _selectedDeckCard;

        private GameObject _cardListScrollRect;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _deckCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/DeckCard_UI");

            _viewDeckPage = new ViewDeckPage();
            _viewDeckPage.Init();

            _mainCamera = Camera.main;

            _selectedDeckCard = null;
        }

        private void Reset()
        {
            _selectedDeck = null;

            if (_deckCards.Count > 0)
            {
                for (int i = _deckCards.Count - 1; i >= 0; i--)
                {
                    Object.Destroy(_deckCards[i].GetGameObject());
                }
            }

            _deckCards = new List<DeckCardUI>();

            UpdateCardsInDeckCountDisplay();
        }

        public void Update()
        {
            _viewDeckPage.Update();
        }

        public void Load(GameObject obj)
        {
            GameObject selfObject = obj;

            _deckNameText = selfObject.transform.Find("Top_Panel/Panel_Image/Deck_Name").GetComponent<TextMeshProUGUI>();
            _cardsCountText = selfObject.transform.Find("Bottom_Panel/Image_CardCounter/Text_CardsAmount").GetComponent<TextMeshProUGUI>();

            _cardListScrollRect = selfObject.transform.parent.Find("Panel_Frame/Panel_Content/Army/Element/Scroll View").gameObject;

            _buttonRename = selfObject.transform.Find("Top_Panel/Panel_Image/Button_Rename").GetComponent<Button>();
            _buttonRename.onClick.AddListener(ButtonRenameHandler);

            _buttonViewDeck = selfObject.transform.Find("Top_Panel/Panel_Image/Button_ViewDeck").GetComponent<Button>();
            _buttonViewDeck.onClick.AddListener(ButtonViewDeckHandler);

            _buttonSave = selfObject.transform.Find("Bottom_Panel/Button_SaveDeck").GetComponent<Button>();
            _buttonSave.onClick.AddListener(ButtonSaveHandler);

            _allCardsContent = selfObject.transform.Find("Cards/Scroll View").GetComponent<ScrollRect>().content;

            _overlordImage = selfObject.transform.Find("Top_Panel/Panel_Image/Overlord_Frame/Overlord_Image/Image").GetComponent<Image>();
            _overlordPrimarySkillImage = selfObject.transform.Find("Top_Panel/Panel_Image/Overlord_Frame/Overlord_Skill_Primary/Image").GetComponent<Image>();
            _overlordSecondarySkillImage = selfObject.transform.Find("Top_Panel/Panel_Image/Overlord_Frame/Overlord_Skill_Secondary/Image").GetComponent<Image>();

            _selectedDeckCard = null;
        }

        public void ShowDeck(Deck deck)
        {
            if (deck == null)
                return;

            Reset();

            _selectedDeck = deck;

            _deckNameText.text = _tutorialManager.IsTutorial ? Constants.TutorialDefaultDeckName : _selectedDeck.Name;

            Enumerators.Faction faction = DataUtilities.GetFaction(deck.OverlordId);
            _overlordImage.sprite = DataUtilities.GetOverlordImage(deck.OverlordId);
            RectTransform rectTransform = _overlordImage.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = DataUtilities.GetOverlordImagePositionInViewDeck(faction);
            rectTransform.localScale = DataUtilities.GetOverlordImageScaleInViewDeck(faction);

            SetSkills();

            SetCards();

            UpdateCardsInDeckCountDisplay();
        }

        public void ChangeDeckName(string newName)
        {
            _deckNameText.text = newName;
        }

        public void ChangeAbilities(Enumerators.Skill primarySkill, Enumerators.Skill secondarySkill)
        {
            _selectedDeck.PrimarySkill = primarySkill;
            _selectedDeck.SecondarySkill = secondarySkill;

            SetSkills();
        }

        private void SetCards()
        {
            for (int i = 0; i < _selectedDeck.Cards.Count; i++)
            {
                DeckCardData deckCardData = _selectedDeck.Cards[i];

                int cardIndex = _dataManager.CachedCardsLibraryData.Cards.FindIndex(cachedCard => cachedCard.CardKey == deckCardData.CardKey);
                if (cardIndex == -1)
                {
                    Log.Error($"Card with MouldId {deckCardData.CardKey} not found.");
                    return;
                }

                Card card = _dataManager.CachedCardsLibraryData.Cards[cardIndex];
                AddCard(card, deckCardData.Amount);
            }
        }

        public void AddCard(Card card, int cardAmount)
        {
            GameObject deckCard = Object.Instantiate(_deckCardPrefab, _allCardsContent);

            MultiPointerClickHandler multiPointerClickHandler = deckCard.AddComponent<MultiPointerClickHandler>();
            if(!_tutorialManager.IsTutorial)
                multiPointerClickHandler.SingleClickReceived += () => { OnSingleClickDeckCard(card); };
            multiPointerClickHandler.DoubleClickReceived += () => { OnMultiClickDeckCard(card); };

            // add drag / drop
            OnBehaviourHandler onBehaviourHandler = deckCard.AddComponent<OnBehaviourHandler>();
            onBehaviourHandler.DragBegan += DragBeganEventHandler;
            onBehaviourHandler.DragUpdated += DragUpdatedEventHandler;
            onBehaviourHandler.DragEnded += DragEndedEventHandler;

            DeckCardUI deckCardUi = new DeckCardUI();
            deckCardUi.Init(deckCard);
            deckCardUi.FillCard(card, cardAmount);
            _deckCards.Add(deckCardUi);
        }

        private void DragBeganEventHandler(PointerEventData pointerEventData, GameObject obj)
        {
            if (_tutorialManager.IsTutorial &&
                !_tutorialManager.CurrentTutorial.IsGameplayTutorial() &&
                (_tutorialManager.CurrentTutorialStep.ToMenuStep().CardsInteractingLocked))
                return;

            if (_selectedDeckCard != null || pointerEventData.delta.normalized.y >= 0.5f || pointerEventData.delta.normalized.y <= -0.5f)
            {
                obj.GetComponentInParent<ScrollRect>().OnBeginDrag(pointerEventData);
                return;
            }

            GameObject cardObj = Object.Instantiate(_deckCardPrefab, obj.transform, false);
            cardObj.transform.localScale = Vector3.one;

            Card selectedCard = _deckCards.Find(card => card.GetGameObject() == obj).GetCard();
            DeckCardUI deckCardUi = new DeckCardUI();
            deckCardUi.Init(cardObj);
            deckCardUi.FillCard(selectedCard, 0);

            _selectedDeckCard = deckCardUi;
        }

        private void DragUpdatedEventHandler(PointerEventData pointerEventData, GameObject arg2)
        {
            if (_selectedDeckCard == null)
            {
                arg2.GetComponentInParent<ScrollRect>().OnDrag(pointerEventData);
                return;
            }

            _selectedDeckCard.GetGameObject().transform.SetParent(_uiManager.Canvas.transform);
            Vector3 position = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            _selectedDeckCard.GetGameObject().transform.position = new Vector3(position.x, position.y, _selectedDeckCard.GetGameObject().transform.position.z);
        }

        private void DragEndedEventHandler(PointerEventData arg1, GameObject arg2)
        {
            if (_selectedDeckCard == null)
            {
                arg2.GetComponentInParent<ScrollRect>().OnEndDrag(arg1);
                return;
            }

            Vector3 point = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(point, Vector3.forward, Mathf.Infinity, SRLayerMask.Default);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == _cardListScrollRect)
                {
                    RemoveCardFromDeck(_selectedDeckCard.GetCard());
                }
            }

            Object.Destroy(_selectedDeckCard.GetGameObject());
            _selectedDeckCard = null;
        }



        public void RemoveCard(Card card)
        {
            DeckCardUI cardUi = _deckCards.Find(deckCard => deckCard.GetCardInterface().CardKey == card.CardKey);
            Object.Destroy(cardUi.GetGameObject());

            _deckCards.Remove(cardUi);
        }

        public void UpdateCard(Card card, int cardAmount)
        {
            DeckCardUI deckCard = _deckCards.Find(cardInDeck => cardInDeck.GetCardInterface().CardKey == card.CardKey);
            if (deckCard == null)
            {
                Log.Error($"Card with MouldId {card.CardKey} not found.");
                return;
            }

            deckCard.UpdateCard(cardAmount);
        }



        public List<IReadOnlyCard> GetAllCardsInDeck()
        {
            List<IReadOnlyCard> cardList = _deckCards.Select(card => card.GetCardInterface()).ToList();
            return cardList;
        }

        private void OnSingleClickDeckCard(Card selectedCard)
        {
            if (_uiManager.GetPopup<CardInfoWithSearchPopup>().Self != null || _selectedDeckCard != null)
                return;

            if (_tutorialManager.IsTutorial &&
                !_tutorialManager.CurrentTutorial.IsGameplayTutorial() &&
                (_tutorialManager.CurrentTutorialStep.ToMenuStep().CardsInteractingLocked))
                return;

            List<IReadOnlyCard> cardList = _deckCards.Select(card => card.GetCardInterface()).ToList();
            _uiManager.DrawPopup<CardInfoWithSearchPopup>(new object[]
            {
                cardList,
                selectedCard,
                CardInfoWithSearchPopup.PopupType.REMOVE_CARD
            });
        }

        private void OnMultiClickDeckCard(Card selectedCard)
        {
            if (_tutorialManager.IsTutorial &&
                !_tutorialManager.CurrentTutorial.IsGameplayTutorial() &&
                (_tutorialManager.CurrentTutorialStep.ToMenuStep().CardsInteractingLocked))
                return;

            RemoveCardFromDeck(selectedCard);
        }

        private void RemoveCardFromDeck(Card selectedCard)
        {
            _uiManager.GetPage<HordeSelectionWithNavigationPage>().HordeEditTab.RemoveCardFromDeck
            (
                selectedCard,
                true
            );
        }

        public void UpdateCardsInDeckCountDisplay()
        {
            int cardsInDeckCount = 0;
            if (_selectedDeck != null)
            {
                for (int i = 0; i < _selectedDeck.Cards.Count; i++)
                {
                    DeckCardData deckCardData = _selectedDeck.Cards[i];
                    cardsInDeckCount += deckCardData.Amount;
                }
            }

            _cardsCountText.text = cardsInDeckCount + "/" + Constants.MaxDeckSize;
        }

        private void SetSkills()
        {
            _overlordPrimarySkillImage.sprite =
                DataUtilities.GetAbilityIcon(_selectedDeck.OverlordId, _selectedDeck.PrimarySkill);

            _overlordSecondarySkillImage.sprite =
                DataUtilities.GetAbilityIcon(_selectedDeck.OverlordId, _selectedDeck.SecondarySkill);
        }


        private void ButtonSaveHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonSave.name))
                return;

            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.HordeSaveButtonPressed);

            PlayClickSound();

            HordeSelectionWithNavigationPage deckPage = _uiManager.GetPage<HordeSelectionWithNavigationPage>();
            deckPage.HordeEditTab.SaveDeck(HordeSelectionWithNavigationPage.Tab.SelectDeck);
        }

        private void ButtonViewDeckHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonViewDeck.name))
                return;

            _viewDeckPage.Show(_selectedDeck);
        }

        private void ButtonRenameHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonRename.name))
                return;

            PlayClickSound();

            _uiManager.DrawPopup<RenamePopup>(new object[] {_selectedDeck, false});
        }

        private void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK,
                Constants.SfxSoundVolume, false, false, true);
        }
    }
}



