using System;
using System.Collections.Generic;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class SelectOverlordPopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(SelectOverlordPopup));

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;
        private IUIManager _uiManager;
        private ITutorialManager _tutorialManager;

        private TextMeshProUGUI _textSelectOverlordName;
        private TextMeshProUGUI _textSelectOverlordDescription;

        private Image _overlordImage;
        private Image _overlordFactionImage;
        private Image _overlordAgainstFactionImage;

        private Button _buttonSelectOverlordLeftArrow;
        private Button _buttonSelectOverlordRightArrow;
        private Button _buttonSelectOverlordContinue;
        private Button _buttonCancel;

        private ScrollRect _overlordListScrollRect;

        private OverlordId _selectOverlordId;

        public static Action<OverlordId> OnSelectOverlord;

        private GameObject _selectOverlordCardPrefab;
        private List<OverlordCard> _overlordCards;

        private RectTransform _allCardsContent;

        private FadeoutBars _fadeoutBars;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _selectOverlordCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/SelectOverlord_UI");
        }

        public void Show(object data)
        {
            Show();
        }

        public void Hide()
        {
            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/SelectOverlordPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _buttonSelectOverlordLeftArrow = Self.transform.Find("Panel_Content/Panel/Right_Panel/Button_LeftArrow").GetComponent<Button>();
            _buttonSelectOverlordLeftArrow.onClick.AddListener(ButtonSelectOverlordLeftArrowHandler);

            _buttonSelectOverlordRightArrow = Self.transform.Find("Panel_Content/Panel/Right_Panel/Button_RightArrow").GetComponent<Button>();
            _buttonSelectOverlordRightArrow.onClick.AddListener(ButtonSelectOverlordRightArrowHandler);

            _buttonSelectOverlordContinue = Self.transform.Find("Panel_Content/Panel/Right_Panel/Button_Continue").GetComponent<Button>();
            _buttonSelectOverlordContinue.onClick.AddListener(ButtonSelectOverlordContinueHandler);

            _buttonCancel = Self.transform.Find("Panel_Content/Panel/Right_Panel/Button_Cancel").GetComponent<Button>();
            _buttonCancel.onClick.AddListener(ButtonCancelHandler);

            _textSelectOverlordName = Self.transform.Find("Panel_Content/Panel/Left_Panel/Champion_Data/Text_Champion_Name").GetComponent<TextMeshProUGUI>();
            _textSelectOverlordDescription = Self.transform.Find("Panel_Content/Panel/Left_Panel/Champion_Data/Champion_description").GetComponent<TextMeshProUGUI>();

            _overlordImage = Self.transform.Find("Panel_Content/Panel/Left_Panel/Image_Champion").GetComponent<Image>();
            _overlordFactionImage = Self.transform.Find("Panel_Content/Panel/Left_Panel/Champion_Data/Image_Champion_Element").GetComponent<Image>();
            _overlordAgainstFactionImage = Self.transform.Find("Panel_Content/Panel/Left_Panel/Champion_Data/Image_Champion_Weak").GetComponent<Image>();

            _overlordListScrollRect = Self.transform.Find("Panel_Content/Panel/Right_Panel/Overlords/Scroll View")
                .GetComponent<ScrollRect>();
            _allCardsContent = _overlordListScrollRect.content;

            OnSelectOverlord += SelectOverlord;

            _overlordCards = new List<OverlordCard>();
            for (int i = 0; i < _dataManager.CachedOverlordData.Overlords.Count; i++)
            {
                OverlordUserInstance overlordUserInstance = _dataManager.CachedOverlordData.Overlords[i];

                GameObject overlordCardUi = Object.Instantiate(_selectOverlordCardPrefab, _allCardsContent, false);
                overlordCardUi.transform.localScale = Vector3.one;

                OverlordCard overlordCard = new OverlordCard();
                overlordCard.Init(overlordCardUi);
                overlordCard.SetOverlordId(overlordUserInstance.Prototype.Id);
                overlordCard.SetOverlordImage(overlordUserInstance.Prototype.Faction);

                _overlordCards.Add(overlordCard);
            }

            Scrollbar deckCardsScrollBar = Self.transform.Find("Panel_Content/Panel/Right_Panel/Overlords/Scroll View")
                .GetComponent<ScrollRect>().horizontalScrollbar;
            GameObject leftFadeGameObject = Self.transform.Find("Panel_Content/Panel/Right_Panel/Fade_Left").gameObject;
            GameObject rightFadeGameObject = Self.transform.Find("Panel_Content/Panel/Right_Panel/Fade_Right").gameObject;

            _fadeoutBars = new FadeoutBars();
            _fadeoutBars.Init(deckCardsScrollBar, leftFadeGameObject, rightFadeGameObject);


            if (_tutorialManager.IsTutorial)
            {
                OverlordId overlordId = _dataManager.CachedOverlordData.Overlords.
                    Find(overlord => overlord.Prototype.Faction == _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MainSet).Prototype.Id;
                SelectOverlordCard(overlordId);
            }
            else
            {
                SelectOverlord(new OverlordId(0));
            }

        }

        private void SelectOverlord(OverlordId overlordId)
        {
            if (_tutorialManager.IsTutorial)
                return;

            SelectOverlordCard(overlordId);
        }

        private void SelectOverlordCard(OverlordId overlordId)
        {
            for (int i = 0; i < _overlordCards.Count; i++)
            {
                _overlordCards[i].SelectOverlord(_overlordCards[i].GetOverlordId != overlordId);
            }
            _selectOverlordId = overlordId;
            UpdateSelectedOverlordDisplay(overlordId);
        }

        public void Update()
        {
            _fadeoutBars?.Update();
        }

        public void Dispose()
        {
            OnSelectOverlord -= SelectOverlord;
        }

        public void SetMainPriority()
        {

        }

        #region Button Handlers

        private void ButtonCancelHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonCancel.name))
                return;

            Hide();
        }

        private void ButtonSelectOverlordLeftArrowHandler()
        {
            _overlordListScrollRect.horizontalNormalizedPosition = 0f;
        }

        private void ButtonSelectOverlordRightArrowHandler()
        {
            _overlordListScrollRect.horizontalNormalizedPosition = 1f;
        }

        private void ButtonSelectOverlordContinueHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonSelectOverlordContinue.name))
                return;

            DataUtilities.PlayClickSound();

            // TODO : call event based just like in Select overlord ability
            HordeSelectionWithNavigationPage myDeckPage = _uiManager.GetPage<HordeSelectionWithNavigationPage>();
            myDeckPage.AssignNewDeck(_selectOverlordId);

            Hide();
            _uiManager.DrawPopup<SelectOverlordAbilitiesPopup>(new object[] {myDeckPage.CurrentEditDeck, true});
        }

        #endregion



        private void UpdateSelectedOverlordDisplay(OverlordId overlordId)
        {
            OverlordUserInstance overlord = _dataManager.CachedOverlordData.Overlords.Find(overlords => overlords.Prototype.Id == overlordId);

            _overlordImage.sprite = DataUtilities.GetOverlordImage(overlord.Prototype.Faction);
            _textSelectOverlordName.text = overlord.Prototype.ShortName;
            _textSelectOverlordDescription.text = overlord.Prototype.ShortDescription;

            Enumerators.Faction againstFaction = Constants.FactionAgainstDictionary[overlord.Prototype.Faction];

            _overlordFactionImage.sprite = GetElementIcon(overlord.Prototype.Faction);
            _overlordAgainstFactionImage.sprite = GetElementIcon(againstFaction);
        }

        private Sprite GetElementIcon(Enumerators.Faction faction)
        {
            string path = "Images/UI/ChooseOverlord/";
            path = path + "/icon_element_" + faction.ToString().ToLowerInvariant();
            return _loadObjectsManager.GetObjectByPath<Sprite>(path);
        }
    }

    public class OverlordCard
    {
        private OverlordId _overlordId;
        private Image _overlordImage;
        private Button _overlordButton;

        public OverlordId GetOverlordId => _overlordId;

        public void Init(GameObject obj)
        {
            _overlordImage = obj.transform.Find("Viewport/Image_Overlord").GetComponent<Image>();
            _overlordButton = obj.transform.Find("Frame").GetComponent<Button>();
            _overlordButton.onClick.AddListener(ButtonSelectOverlordHandler);
        }

        private void ButtonSelectOverlordHandler()
        {
            SelectOverlordPopup.OnSelectOverlord?.Invoke(_overlordId);
        }

        public void SetOverlordId(OverlordId overlordId)
        {
            _overlordId = overlordId;
        }

        public void SetOverlordImage(Enumerators.Faction faction)
        {
            _overlordImage.sprite = DataUtilities.GetOverlordImage(faction);
            _overlordImage.GetComponent<RectTransform>().anchoredPosition = SetPosition(faction);
        }

        private Vector3 SetPosition(Enumerators.Faction faction)
        {
            switch (faction)
            {
                case Enumerators.Faction.FIRE:
                    return new Vector3(101f, -237f, 0f);
                case Enumerators.Faction.WATER:
                    return new Vector3(15f, -306f, 0f);
                case Enumerators.Faction.EARTH:
                    return new Vector3(-23f, -316f, 0f);
                case Enumerators.Faction.AIR:
                    return new Vector3(-183f, -227f, 0f);
                case Enumerators.Faction.LIFE:
                    return new Vector3(-55f, -264f, 0f);
                case Enumerators.Faction.TOXIC:
                    return new Vector3(130f, -227f, 0f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(faction), faction, null);
            }
        }

        public void SelectOverlord(bool selected)
        {
            _overlordButton.interactable = selected;
        }
    }
}
