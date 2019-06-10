using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Card = Loom.ZombieBattleground.Data.Card;
using Deck = Loom.ZombieBattleground.Data.Deck;
using OverlordUserInstance = Loom.ZombieBattleground.Data.OverlordUserInstance;

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

        private Deck _selectedDeck;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _deckCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/DeckCard_UI");
        }

        public void Reset()
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

        public void Load(GameObject obj)
        {
            GameObject selfObject = obj;

            _deckNameText = selfObject.transform.Find("Top_Panel/Panel_Image/Deck_Name").GetComponent<TextMeshProUGUI>();
            _cardsCountText = selfObject.transform.Find("Bottom_Panel/Image_CardCounter/Text_CardsAmount").GetComponent<TextMeshProUGUI>();

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
        }

        public void ShowDeck(Deck deck)
        {
            if (deck == null)
                return;

            Reset();

            _selectedDeck = deck;

            _deckNameText.text = _selectedDeck.Name;

            OverlordUserInstance overlord = _dataManager.CachedOverlordData.GetOverlordById(_selectedDeck.OverlordId);
            _overlordImage.sprite = GetOverlordThumbnailSprite(overlord.Prototype.Faction);

            SetSkills();

            SetCards();

            UpdateCardsInDeckCountDisplay();
        }

        public void ShowNewDeck(OverlordUserInstance currentEditOverlord)
        {
            Reset();

            _selectedDeck = new Deck(
                new DeckId(-1),
                currentEditOverlord.Prototype.Id,
                GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().GenerateDeckName(),
                new List<DeckCardData>(),
                0,
                0);

            // TODO : set name of deck entered by user... if not , use same
            _deckNameText.text = _selectedDeck.Name;

            OverlordUserInstance overlord = _dataManager.CachedOverlordData.GetOverlordById(_selectedDeck.OverlordId);
            _overlordImage.sprite = GetOverlordThumbnailSprite(overlord.Prototype.Faction);

            SetSkills();

            SetCards();

            UpdateCardsInDeckCountDisplay();
        }

        private void SetCards()
        {
            for (int i = 0; i < _selectedDeck.Cards.Count; i++)
            {
                DeckCardData deckCardData = _selectedDeck.Cards[i];

                int cardIndex = _dataManager.CachedCardsLibraryData.Cards.FindIndex(cachedCard => cachedCard.MouldId == deckCardData.MouldId);
                if (cardIndex == -1)
                {
                    Log.Error($"Card with MouldId {deckCardData.MouldId} not found.");
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
            multiPointerClickHandler.SingleClickReceived += () => { OnSingleClickDeckCard(card); };
            multiPointerClickHandler.DoubleClickReceived += () => { OnMultiClickDeckCard(card); };

            DeckCardUI deckCardUi = new DeckCardUI();
            deckCardUi.Init(deckCard);
            deckCardUi.FillCard(card, cardAmount);
            _deckCards.Add(deckCardUi);
        }

        public void RemoveCard(Card card)
        {
            DeckCardUI cardUi = _deckCards.Find(deckCard => deckCard.GetCardInterface().MouldId == card.MouldId);
            Object.Destroy(cardUi.GetGameObject());

            _deckCards.Remove(cardUi);
        }

        public void UpdateCard(Card card, int cardAmount)
        {
            DeckCardUI deckCard = _deckCards.Find(cardInDeck => cardInDeck.GetCardInterface().MouldId == card.MouldId);
            if (deckCard == null)
            {
                Log.Error($"Card with MouldId {card.MouldId} not found.");
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
            if (_uiManager.GetPopup<CardInfoWithSearchPopup>().Self != null)
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
            OverlordUserInstance overlord = _dataManager.CachedOverlordData.GetOverlordById(_selectedDeck.OverlordId);

            // primary skill
            if (_selectedDeck.PrimarySkill == Enumerators.Skill.NONE)
            {
                _overlordPrimarySkillImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
            }
            else
            {
                string iconPath = overlord.GetSkill(_selectedDeck.PrimarySkill).Prototype.IconPath;
                _overlordPrimarySkillImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
            }


            // secondary skill
            if (_selectedDeck.SecondarySkill == Enumerators.Skill.NONE)
            {
                _overlordSecondarySkillImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
            }
            else
            {
                string iconPath = overlord.GetSkill(_selectedDeck.SecondarySkill).Prototype.IconPath;
                _overlordSecondarySkillImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
            }
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

        }

        private void ButtonRenameHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonRename.name))
                return;

            PlayClickSound();
            HordeSelectionWithNavigationPage deckPage = _uiManager.GetPage<HordeSelectionWithNavigationPage>();
            deckPage.IsRenameWhileEditing = true;
            deckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.Rename);
        }

        private Sprite GetOverlordThumbnailSprite(Enumerators.Faction overlordFaction)
        {
            string path = "Images/UI/MyDecks/OverlordPortrait";
            switch(overlordFaction)
            {
                case Enumerators.Faction.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_air");
                case Enumerators.Faction.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_fire");
                case Enumerators.Faction.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_earth");
                case Enumerators.Faction.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_toxic");
                case Enumerators.Faction.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_water");
                case Enumerators.Faction.LIFE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_life");
                default:
                    return null;
            }
        }

        private void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK,
                Constants.SfxSoundVolume, false, false, true);
        }
    }
}



