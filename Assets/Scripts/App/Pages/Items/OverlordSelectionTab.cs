using System;
using System.Collections.Generic;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class OverlordSelectionTab
    {
        private static readonly ILog Log = Logging.GetLog(nameof(OverlordSelectionTab));

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;
        private ITutorialManager _tutorialManager;

        private HordeSelectionWithNavigationPage _myDeckPage;

        private TextMeshProUGUI _textSelectOverlordName;
        private TextMeshProUGUI _textSelectOverlordDescription;

        private Image _overlordImage;
        private Image _overlordFactionImage;
        private Image _overlordAgainstFactionImage;

        private Button _buttonSelectOverlordLeftArrow,
                       _buttonSelectOverlordRightArrow,
                       _buttonSelectOverlordContinue,
                       _buttonBack;

        private int _selectOverlordIndex;

        public static Action<OverlordId> OnSelectOverlord;

        private GameObject _selectOverlordCardPrefab;
        private List<OverlordCard> _overlordCards;

        private RectTransform _allCardsContent;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _myDeckPage = GameClient.Get<IUIManager>().GetPage<HordeSelectionWithNavigationPage>();
            _myDeckPage.EventChangeTab += (HordeSelectionWithNavigationPage.Tab tab) =>
            {
                if (tab != HordeSelectionWithNavigationPage.Tab.SelectOverlord)
                    return;


                int index = 0;

                if (_tutorialManager.IsTutorial)
                {
                    index = _dataManager.CachedOverlordData.Overlords.FindIndex(overlord => overlord.Prototype.Faction == _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MainSet);
                }

                ChangeOverlordIndex(index);
            };

            _selectOverlordCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/SelectOverlord_UI");
        }

        public void Show(GameObject overlordTabObj)
        {
            _buttonSelectOverlordLeftArrow = overlordTabObj.transform.Find("Panel_Content/Panel/Right_Panel/Button_LeftArrow").GetComponent<Button>();
            _buttonSelectOverlordLeftArrow.onClick.AddListener(ButtonSelectOverlordLeftArrowHandler);

            _buttonSelectOverlordRightArrow = overlordTabObj.transform.Find("Panel_Content/Panel/Right_Panel/Button_RightArrow").GetComponent<Button>();
            _buttonSelectOverlordRightArrow.onClick.AddListener(ButtonSelectOverlordRightArrowHandler);

            _buttonSelectOverlordContinue = overlordTabObj.transform.Find("Panel_Content/Panel/Right_Panel/Button_Continue").GetComponent<Button>();
            _buttonSelectOverlordContinue.onClick.AddListener(ButtonSelectOverlordContinueHandler);

            _buttonBack = overlordTabObj.transform.Find("Panel_Content/Image_ButtonBackTray/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);

            _textSelectOverlordName = overlordTabObj.transform.Find("Panel_Content/Panel/Left_Panel/Champion_Data/Text_Champion_Name").GetComponent<TextMeshProUGUI>();
            _textSelectOverlordDescription = overlordTabObj.transform.Find("Panel_Content/Panel/Left_Panel/Champion_Data/Champion_description").GetComponent<TextMeshProUGUI>();

            _overlordImage = overlordTabObj.transform.Find("Panel_Content/Viewport/Image_Champion").GetComponent<Image>();
            _overlordFactionImage = overlordTabObj.transform.Find("Panel_Content/Panel/Left_Panel/Champion_Data/Image_Champion_Element").GetComponent<Image>();
            _overlordAgainstFactionImage = overlordTabObj.transform.Find("Panel_Content/Panel/Left_Panel/Champion_Data/Image_Champion_Weak").GetComponent<Image>();

            _allCardsContent = overlordTabObj.transform.Find("Panel_Content/Panel/Right_Panel/Overlords/Scroll View").GetComponent<ScrollRect>().content;

            OnSelectOverlord += SelectOverlord;

            int prefabCount = _dataManager.CachedOverlordData.Overlords.Count / 2;
            _overlordCards = new List<OverlordCard>();
            for (int i = 0; i < prefabCount; i++)
            {
                GameObject overlordCardUi = UnityEngine.Object.Instantiate(_selectOverlordCardPrefab, _allCardsContent, true);
                overlordCardUi.transform.localScale = Vector3.one;

                GameObject topOverlord = overlordCardUi.transform.Find("Image_Overlord_Top").gameObject;
                OverlordCard overlordCardTop = new OverlordCard();
                overlordCardTop.Init(topOverlord);

                GameObject bottomOverlord = overlordCardUi.transform.Find("Image_Overlord_Bottom").gameObject;
                OverlordCard overlordCardBottom = new OverlordCard();
                overlordCardBottom.Init(bottomOverlord);

                _overlordCards.Add(overlordCardTop);
                _overlordCards.Add(overlordCardBottom);
            }


            for (int i = 0; i < _dataManager.CachedOverlordData.Overlords.Count; i++)
            {
                OverlordUserInstance overlordUserInstance = _dataManager.CachedOverlordData.Overlords[i];
                _overlordCards[i].SetOverlordId(overlordUserInstance.Prototype.Id);
                _overlordCards[i].SetOverlordImage(overlordUserInstance.Prototype.Faction);
            }

            SelectOverlord(new OverlordId(0));
        }

        private void SelectOverlord(OverlordId overlordId)
        {
            for (int i = 0; i < _overlordCards.Count; i++)
            {
                _overlordCards[i].SelectOverlord(_overlordCards[i].GetOverlordId != overlordId);
            }
            ChangeOverlordIndex((int) overlordId.Id);
        }

        public void Update()
        {

        }

        public void Dispose()
        {
            OnSelectOverlord -= SelectOverlord;
        }

        #region Button Handlers

        private void ButtonBackHandler()
        {
            _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SelectDeck);
        }

        private void ButtonSelectOverlordLeftArrowHandler()
        {

        }

        private void ButtonSelectOverlordRightArrowHandler()
        {

        }

        private void ButtonSelectOverlordContinueHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonSelectOverlordContinue.name))
                return;

            PlayClickSound();

            _myDeckPage.CurrentEditOverlord = _dataManager.CachedOverlordData.Overlords[_selectOverlordIndex];
            _myDeckPage.AssignNewDeck();
            _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SelectOverlordSkill);
        }

        #endregion

        private void ChangeOverlordIndex(int newIndex)
        {
            _selectOverlordIndex = newIndex;
            UpdateSelectedOverlordDisplay(_selectOverlordIndex);
        }

        private void UpdateSelectedOverlordDisplay(int selectedOverlordIndex)
        {
            OverlordUserInstance overlord = _dataManager.CachedOverlordData.Overlords[selectedOverlordIndex];

            _overlordImage.sprite = GetOverlordPortraitSprite(overlord.Prototype.Faction);
            _textSelectOverlordName.text = overlord.Prototype.Name;
            _textSelectOverlordDescription.text = overlord.Prototype.ShortDescription;

            Enumerators.Faction againstFaction = _myDeckPage.HordeEditTab.FactionAgainstDictionary
            [
                overlord.Prototype.Faction
            ];

            _overlordFactionImage.sprite = GetElementIcon(overlord.Prototype.Faction);
            _overlordAgainstFactionImage.sprite = GetElementIcon(againstFaction);
        }

        public Sprite GetOverlordPortraitSprite(Enumerators.Faction overlordFaction)
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
                    Log.Info($"No Overlord portrait found for faction {overlordFaction}");
                    return null;
            }
        }

        public Sprite GetElementIcon(Enumerators.Faction faction)
        {
            string path = "Images/UI/ChooseOverlord";
            switch(faction)
            {
                case Enumerators.Faction.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/icon_element_air");
                case Enumerators.Faction.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/icon_element_fire");
                case Enumerators.Faction.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/icon_element_earth");
                case Enumerators.Faction.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/icon_element_toxic");
                case Enumerators.Faction.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/icon_element_water");
                case Enumerators.Faction.LIFE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/icon_element_life");
                default:
                    Log.Info($"No Overlord portrait found for faction {faction}");
                    return null;
            }
        }

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
    }

    public class OverlordCard
    {
        private OverlordId _overlordId;
        private Image _overlordImage;
        private Button _overlordButton;

        private ILoadObjectsManager _loadObjectsManager;

        public OverlordId GetOverlordId => _overlordId;

        public void Init(GameObject obj)
        {
            _overlordImage = obj.GetComponent<Image>();
            _overlordButton = obj.transform.Find("Frame").GetComponent<Button>();
            _overlordButton.onClick.AddListener(ButtonSelectOverlordHandler);

            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        }

        private void ButtonSelectOverlordHandler()
        {
            OverlordSelectionTab.OnSelectOverlord?.Invoke(_overlordId);
        }

        public void SetOverlordId(OverlordId overlordId)
        {
            _overlordId = overlordId;
        }

        public void SetOverlordImage(Enumerators.Faction faction)
        {
            _overlordImage.sprite = GetOverlordMiniPortraitSprite(faction);
        }

        public void SelectOverlord(bool selected)
        {
            _overlordButton.interactable = selected;
        }

        private Sprite GetOverlordMiniPortraitSprite(Enumerators.Faction overlordFaction)
        {
            string path = "Images/UI/ChooseOverlord";
            switch(overlordFaction)
            {
                case Enumerators.Faction.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/portrait_air_hero");
                case Enumerators.Faction.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/portrait_fire_hero");
                case Enumerators.Faction.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/portrait_earth_hero");
                case Enumerators.Faction.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/portrait_toxic_hero");
                case Enumerators.Faction.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/portrait_water_hero");
                case Enumerators.Faction.LIFE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/portrait_life_hero");
            }

            return null;
        }
    }
}
