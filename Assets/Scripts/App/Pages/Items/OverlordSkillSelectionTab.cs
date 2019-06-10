using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using DG.Tweening;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class OverlordSkillSelectionTab
    {
        private static readonly ILog Log = Logging.GetLog(nameof(OverlordSkillSelectionTab));

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private HordeSelectionWithNavigationPage _myDeckPage;

        private Image _imageSelectOverlordSkillPortrait;

        private Image[] _imageSkillIcons;

        private TextMeshProUGUI[] _textSkillDescriptions;

        private TextMeshProUGUI _textSelectedAmount;

        private Button _continueButton;
        private Button _buttonBack;

        private GameObject _abilitiesGroup;

        private List<OverlordSkillItem> _overlordSkillItems;

        private Canvas _backLayerCanvas;

        public Enumerators.Skill SelectedPrimarySkill { get; private set; }

        public Enumerators.Skill SelectedSecondarySkill { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _imageSkillIcons = new Image[2];
            _textSkillDescriptions = new TextMeshProUGUI[2];
            _overlordSkillItems = new List<OverlordSkillItem>();

            _myDeckPage = GameClient.Get<IUIManager>().GetPage<HordeSelectionWithNavigationPage>();
            _myDeckPage.EventChangeTab += (HordeSelectionWithNavigationPage.Tab tab) =>
            {
                if (tab == HordeSelectionWithNavigationPage.Tab.SelectOverlordSkill)
                {
                    UpdateTabShow();
                    UpdateSkillIconAndDescriptionDisplay();
                    UpdateOverlordPortrait();
                }
                else
                {
                    Dispose();
                }
            };
        }

        public void Show(GameObject skillTabObj)
        {
           _imageSelectOverlordSkillPortrait = skillTabObj.transform.Find("Panel_Content/Image_OverlordPortrait").GetComponent<Image>();

            _imageSkillIcons[0] = skillTabObj.transform.Find("Panel_Content/Image_SkillSlots/Image_SkillIcon_1").GetComponent<Image>();
            _imageSkillIcons[1] = skillTabObj.transform.Find("Panel_Content/Image_SkillSlots/Image_SkillIcon_2").GetComponent<Image>();

            _textSkillDescriptions[0] = skillTabObj.transform.Find("Panel_Content/Image_SkillSlots/Text_Desc_1").GetComponent<TextMeshProUGUI>();
            _textSkillDescriptions[1] = skillTabObj.transform.Find("Panel_Content/Image_SkillSlots/Text_Desc_2").GetComponent<TextMeshProUGUI>();

            _textSelectedAmount = skillTabObj.transform.Find("Panel_Content/Image_SelectAmount/Text_SelectedAmount").GetComponent<TextMeshProUGUI>();

            _backLayerCanvas = skillTabObj.transform.Find("Canvas_BackLayer").GetComponent<Canvas>();

            _continueButton = _backLayerCanvas.transform.Find("Button_Continue").GetComponent<Button>();
            _continueButton.onClick.AddListener(ContinueButtonOnClickHandler);

            _buttonBack = skillTabObj.transform.Find("Image_ButtonBackTray/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);

            _abilitiesGroup = _backLayerCanvas.transform.Find("Abilities").gameObject;
        }

        public void Update()
        {

        }

        public void Dispose()
        {
            ResetItems();
        }

        private void UpdateSkillIconAndDescriptionDisplay()
        {
            List<OverlordSkillItem> items = _overlordSkillItems.FindAll(x => x.IsSelected);
            for (int i = 0; i < 2; i++)
            {
                if(i < items.Count)
                {
                    _imageSkillIcons[i].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + items[i].Skill.Prototype.IconPath);
                    _textSkillDescriptions[i].text = items[i].Skill.Prototype.Title + ":"+ items[i].Skill.Prototype.Description;
               }
                else
                {
                     _imageSkillIcons[i].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_empty");
                    _textSkillDescriptions[i].text = "No selected skill";
                }
            }
            _textSelectedAmount.text = "" + items.Count + "/2";
        }

        private void UpdateOverlordPortrait()
        {
            _imageSelectOverlordSkillPortrait.sprite = _myDeckPage.SelectOverlordTab.GetOverlordPortraitSprite
            (
                _myDeckPage.CurrentEditOverlord.Prototype.Faction
            );
        }

        private void UpdateTabShow()
        {
            FillAvailableAbilities();
            UpdateSelectedSkills();
        }

        private void UpdateSelectedSkills()
        {
            List<OverlordSkillUserInstance> selectedSkills = new List<OverlordSkillUserInstance>();
            if(_myDeckPage.CurrentEditDeck.PrimarySkill != Enumerators.Skill.NONE)
            {
                OverlordSkillUserInstance overlordSkill = _myDeckPage.CurrentEditOverlord.GetSkill(_myDeckPage.CurrentEditDeck.PrimarySkill);
                selectedSkills.Add(overlordSkill);
            }

            if(_myDeckPage.CurrentEditDeck.SecondarySkill != Enumerators.Skill.NONE)
            {
                OverlordSkillUserInstance overlordSkill = _myDeckPage.CurrentEditOverlord.GetSkill(_myDeckPage.CurrentEditDeck.SecondarySkill);
                selectedSkills.Add(overlordSkill);
            }

            foreach (OverlordSkillUserInstance skill in selectedSkills)
            {
                OverlordSkillItem item = _overlordSkillItems.Find(x => x.Skill.Prototype.Skill == skill.Prototype.Skill);
                OverlordSkillSelectedHandler(item);
            }
        }

        #region button handlers

        private void ButtonBackHandler()
        {
            _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SelectDeck);
        }

        private void ContinueButtonOnClickHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_continueButton.name))
                return;

            PlayClickSound();
            List<OverlordSkillItem> items = _overlordSkillItems.FindAll(x => x.IsSelected);

            if (items.Count > 1)
            {
                SelectedPrimarySkill = items[0].Skill.Prototype.Skill;
                SelectedSecondarySkill = items[1].Skill.Prototype.Skill;
            }
            else if(items.Count == 1)
            {
                 SelectedPrimarySkill = items[0].Skill.Prototype.Skill;
                 SelectedSecondarySkill = Enumerators.Skill.NONE;
            }
            else
            {
                 SelectedPrimarySkill = Enumerators.Skill.NONE;
                 SelectedSecondarySkill = Enumerators.Skill.NONE;
            }


            if (_myDeckPage.CurrentEditDeck != null)
            {
                _myDeckPage.CurrentEditDeck.PrimarySkill = SelectedPrimarySkill;
                _myDeckPage.CurrentEditDeck.SecondarySkill = SelectedSecondarySkill;
            }

            if (_myDeckPage.IsEditingNewDeck)
            {
                if (GameClient.Get<ITutorialManager>().IsTutorial)
                {
                    _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.Editing);
                }
                else
                {
                    _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.Rename);
                }
            }
            else
            {
                DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
                deckGeneratorController.FinishEditDeck += FinishEditDeck;
                _continueButton.enabled = false;
                deckGeneratorController.ProcessEditDeck(_myDeckPage.CurrentEditDeck);
            }
        }

        private void FinishEditDeck(bool success, Deck deck)
        {
            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().FinishEditDeck -= FinishEditDeck;

            if (GameClient.Get<IAppStateManager>().AppState != Enumerators.AppState.HordeSelection)
                return;

            _continueButton.enabled = true;

            if(success)
            {
                _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.Editing);
            }
        }

        #endregion

        private void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        private void FillAvailableAbilities()
        {
            ResetItems();

            // All overlords are supposed to have 5 unlockable abilities, but not all exist yet.
            // Until then, just show them as empty locked abilities.
            const int finalAbilityCount = 5;
            for (int i = 0; i < finalAbilityCount; i++)
            {
                OverlordSkillUserInstance overlordSkill = null;

                if (i < _myDeckPage.CurrentEditOverlord.Skills.Count)
                {
                    overlordSkill = _myDeckPage.CurrentEditOverlord.Skills[i];
                }

                OverlordSkillItem itemInstance = new OverlordSkillItem(_abilitiesGroup.transform, overlordSkill);
                itemInstance.OverlordAbilitySelected += OverlordSkillSelectedHandler;

                _overlordSkillItems.Add(itemInstance);
            }
        }

        private void ResetItems()
        {
            foreach (OverlordSkillItem itemInstance in _overlordSkillItems)
            {
                itemInstance.Dispose();
            }

            _overlordSkillItems.Clear();
        }

        private void OverlordSkillSelectedHandler(OverlordSkillItem item)
        {
            if (item == null)
                return;

            if (item.IsSelected)
            {
                item.Deselect();
            }
            else
            {
                if (_overlordSkillItems.FindAll(x => x.IsSelected).Count < 2)
                {
                    item.Select();
                }
            }

            UpdateSkillIconAndDescriptionDisplay();
        }

        private class OverlordSkillItem : IDisposable
        {
            public event Action<OverlordSkillItem> OverlordAbilitySelected;

            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly GameObject _selfObject;

            private readonly Button _selectButton;

            private readonly GameObject _glowObj,
                                        _frameObj,
                                        _lockObj;

            private readonly Image _abilityIconImage;

            public readonly OverlordSkillUserInstance Skill;

            public bool IsSelected { get; private set; }

            public bool IsUnlocked { get; }

            public OverlordSkillItem(Transform root, OverlordSkillUserInstance skill)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                Skill = skill;

                _selfObject =
                    Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>(
                            "Prefabs/UI/Elements/DeckSelection/OverlordAbilityItem"), root, false);

                _selfObject.SetActive(true);
                _glowObj = _selfObject.transform.Find("Glow").gameObject;
                _frameObj = _selfObject.transform.Find("Frame").gameObject;
                _lockObj = _selfObject.transform.Find("Image_Lock").gameObject;
                _abilityIconImage = _selfObject.transform.Find("AbilityIcon").GetComponent<Image>();
                _selectButton = _selfObject.GetComponent<Button>();

                _selectButton.onClick.AddListener(SelectButtonOnClickHandler);

                IsUnlocked = Skill != null ? Skill.UserData.IsUnlocked : false;

                if(IsUnlocked)
                {
                    _abilityIconImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + Skill.Prototype.IconPath);
                }

                _frameObj.SetActive(IsUnlocked);
                _lockObj.SetActive(!IsUnlocked);

                _selectButton.interactable = IsUnlocked;

                _glowObj.SetActive(false);
            }

            public void Dispose()
            {
                Object.Destroy(_selfObject);
            }

            public void Select()
            {
                IsSelected = true;

                _glowObj.SetActive(IsUnlocked && IsSelected);
            }

            public void Deselect()
            {
                IsSelected = false;

                _glowObj.SetActive(IsSelected);
            }

            private void SelectButtonOnClickHandler()
            {
                if (GameClient.Get<ITutorialManager>().BlockAndReport(_selectButton.name))
                    return;

                GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
                OverlordAbilitySelected?.Invoke(this);
            }
        }
    }
}
