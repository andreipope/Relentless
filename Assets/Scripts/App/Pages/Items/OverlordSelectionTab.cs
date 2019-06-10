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

        private TextMeshProUGUI _textSelectOverlordDeckName,
                                _textSelectOverlordName,
                                _textSelectOverlordDescription;

        private Button _buttonSelectOverlordLeftArrow,
                       _buttonSelectOverlordRightArrow,
                       _buttonSelectOverlordContinue,
                       _buttonBack;

        private Image _imageSelectOverlordGlow,
                      _imageSelectOverlordPortrait,
                      _imageCross;

        private List<Transform> _selectOverlordIconList;

        private int _selectOverlordIndex;

        private const int NumberOfOverlord = 6;

        private Dictionary<Enumerators.Faction, Image> _elementImageDictionary;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _selectOverlordIconList = new List<Transform>();

            _myDeckPage = GameClient.Get<IUIManager>().GetPage<HordeSelectionWithNavigationPage>();
            _myDeckPage.EventChangeTab += (HordeSelectionWithNavigationPage.Tab tab) =>
            {
                if (tab != HordeSelectionWithNavigationPage.Tab.SelectOverlord)
                    return;

                _textSelectOverlordDeckName.text = "NEW DECK";

                int index = 0;

                if (_tutorialManager.IsTutorial)
                {
                    index = _dataManager.CachedOverlordData.Overlords.FindIndex(overlord => overlord.Prototype.Faction == _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MainSet);
                }

                ChangeOverlordIndex(index);
            };

            _elementImageDictionary = new Dictionary<Enumerators.Faction, Image>();
        }

        public void Show(GameObject overlordTabObj)
        {
            _textSelectOverlordDeckName = overlordTabObj.transform.Find("Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();

            _buttonSelectOverlordLeftArrow = overlordTabObj.transform.Find("Panel_Content/Button_LeftArrow").GetComponent<Button>();
            _buttonSelectOverlordLeftArrow.onClick.AddListener(ButtonSelectOverlordLeftArrowHandler);

            _buttonSelectOverlordRightArrow = overlordTabObj.transform.Find("Panel_Content/Button_RightArrow").GetComponent<Button>();
            _buttonSelectOverlordRightArrow.onClick.AddListener(ButtonSelectOverlordRightArrowHandler);

            _buttonSelectOverlordContinue = overlordTabObj.transform.Find("Panel_FrameComponents/Lower_Items/Button_Continue").GetComponent<Button>();
            _buttonSelectOverlordContinue.onClick.AddListener(ButtonSelectOverlordContinueHandler);

            _buttonBack = overlordTabObj.transform.Find("Image_ButtonBackTray/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);

            _textSelectOverlordName = overlordTabObj.transform.Find("Panel_Content/Text_SelectOverlord").GetComponent<TextMeshProUGUI>();
            _textSelectOverlordDescription = overlordTabObj.transform.Find("Panel_Content/Text_Desc").GetComponent<TextMeshProUGUI>();

            _imageSelectOverlordGlow = overlordTabObj.transform.Find("Panel_Content/Image_Glow").GetComponent<Image>();
            _imageSelectOverlordPortrait = overlordTabObj.transform.Find("Panel_Content/Image_OverlordPortrait").GetComponent<Image>();
            _imageCross = overlordTabObj.transform.Find("Panel_Content/Image_cross").GetComponent<Image>();

            _elementImageDictionary.Clear();
            for (int i = 0; i < NumberOfOverlord;++i)
            {
                Image overlordIcon = overlordTabObj.transform.Find("Panel_Content/Group_DeckIcon/Button_DeckIcon_" + i).GetComponent<Image>();
                Sprite sprite = GameClient.Get<IUIManager>().GetPopup<DeckSelectionPopup>().GetDeckIconSprite
                (
                    _dataManager.CachedOverlordData.Overlords[i].Prototype.Faction
                );
                overlordIcon.sprite = sprite;

                _selectOverlordIconList.Add
                (
                    overlordIcon.transform
                );

                int index = i;
                Button overlordButton = overlordIcon.GetComponent<Button>();
                overlordButton.onClick.AddListener
                (() =>
                {
                    if (_tutorialManager.BlockAndReport(overlordButton.name))
                        return;

                    ChangeOverlordIndex(index);
                    PlayClickSound();
                });

                string elementName = _dataManager.CachedOverlordData.Overlords[i].Prototype.Faction.ToString().ToLower();
                Image elementImage = overlordTabObj.transform.Find
                (
                    "Panel_Content/Group_Elements/Image_Element_"+elementName
                ).GetComponent<Image>();
                _elementImageDictionary.Add(_dataManager.CachedOverlordData.Overlords[i].Prototype.Faction, elementImage);
            }
        }

        public void Update()
        {

        }

        public void Dispose()
        {
            _selectOverlordIconList.Clear();
        }

        #region Button Handlers

        private void ButtonBackHandler()
        {
            _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SelectDeck);
        }

        private void ButtonSelectOverlordLeftArrowHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonSelectOverlordLeftArrow.name))
                return;

            PlayClickSound();
            int newIndex = _selectOverlordIndex - 1;
            if (newIndex < 0)
            {
                newIndex = _selectOverlordIconList.Count - 1;
            }
            ChangeOverlordIndex(newIndex);
        }

        private void ButtonSelectOverlordRightArrowHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonSelectOverlordRightArrow.name))
                return;

            PlayClickSound();
            int newIndex = _selectOverlordIndex + 1;
            if (newIndex >= _selectOverlordIconList.Count)
            {
                newIndex = 0;
            }
            ChangeOverlordIndex(newIndex);
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
            _imageSelectOverlordGlow.transform.position = _selectOverlordIconList[selectedOverlordIndex].position;
            _imageSelectOverlordPortrait.sprite = GetOverlordPortraitSprite
            (
                overlord.Prototype.Faction
            );
            _textSelectOverlordName.text = overlord.Prototype.FullName;
            _textSelectOverlordDescription.text = overlord.Prototype.ShortDescription;

            Enumerators.Faction againstFaction = _myDeckPage.HordeEditTab.FactionAgainstDictionary
            [
                overlord.Prototype.Faction
            ];
            _imageCross.transform.position = _elementImageDictionary[againstFaction].transform.position;
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

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
    }
}
