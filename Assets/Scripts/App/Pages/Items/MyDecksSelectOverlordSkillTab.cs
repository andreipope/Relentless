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
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class MyDecksSelectOverlordSkillTab
    {
        private ILoadObjectsManager _loadObjectsManager;
        
        private IUIManager _uiManager;
        
        private IDataManager _dataManager;
        
        private ITutorialManager _tutorialManager;
        
        private IAnalyticsManager _analyticsManager;
        
        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;
        
        private MyDecksPage _myDeckPage;
        
        private GameObject _selfPage;
        
        private Image _imageSelectOverlordSkillPortrait;

        private Button _buttonSelectOverlordSkillContinue;

        private Image[] _imageSkillIcons;

        private TextMeshProUGUI[] _textSkillDescriptions;

        private TextMeshProUGUI _textSelectedAmount;
        
        private const int AbilityListSize = 5;

        private const int MaxSelectedAbilities = 2;

        public event Action PopupHiding;

        private ISoundManager _soundManager;

        private Button _continueButton;

        private Button _cancelButton;

        private GameObject _abilitiesGroup;

        private TextMeshProUGUI _skillName;

        private TextMeshProUGUI _skillDescription;

        private List<OverlordAbilityItem> _overlordAbilities;

        private Canvas _backLayerCanvas;

        private bool _singleSelectionMode = false;

        private bool _isPrimarySkillSelected = true;

        private List<HeroSkill> _selectedSkills;
        
        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();            
            _uiManager = GameClient.Get<IUIManager>();
            _soundManager = GameClient.Get<ISoundManager>();

            _imageSkillIcons = new Image[2];
            _textSkillDescriptions = new TextMeshProUGUI[2];
             _overlordAbilities = new List<OverlordAbilityItem>();
            
            _myDeckPage = GameClient.Get<IUIManager>().GetPage<MyDecksPage>();
            _myDeckPage.EventChangeTab += (MyDecksPage.TAB tab) =>
            {
                if (tab == MyDecksPage.TAB.SELECT_OVERLORD_SKILL)
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
           
            PopupHiding += ()=>
            {
                ButtonSelectOverlordSkillContinueHandler();
            };
        }
        
        public void Show(GameObject selfPage)
        {
            _selfPage = selfPage;
            
            _imageSelectOverlordSkillPortrait = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_Content/Image_OverlordPortrait").GetComponent<Image>();            
            
            _buttonSelectOverlordSkillContinue = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_FrameComponents/Lower_Items/Button_Continue").GetComponent<Button>();
            _buttonSelectOverlordSkillContinue.onClick.AddListener(ButtonSelectOverlordSkillContinueHandler);
            _buttonSelectOverlordSkillContinue.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _imageSkillIcons[0] = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_Content/Image_SkillSlots/Image_SkillIcon_1").GetComponent<Image>();  
            _imageSkillIcons[1] = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_Content/Image_SkillSlots/Image_SkillIcon_2").GetComponent<Image>();  
            
            _textSkillDescriptions[0] = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_Content/Image_SkillSlots/Text_Desc_1").GetComponent<TextMeshProUGUI>();  
            _textSkillDescriptions[1] = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_Content/Image_SkillSlots/Text_Desc_2").GetComponent<TextMeshProUGUI>();         
            
            _textSelectedAmount = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_Content/Image_SelectAmount/Text_SelectedAmount").GetComponent<TextMeshProUGUI>();
        }
        
        public void Update()
        {

        }
        
        public void Dispose()
        {
            ResetOverlordAbilities();
        }
        
        private async void ButtonSelectOverlordSkillContinueHandler()
        {
            _buttonSelectOverlordSkillContinue.interactable = false;            
        
            bool success = true;

            Hero hero = _myDeckPage.CurrentEditHero;
            Deck deck = _myDeckPage.CurrentEditDeck;
            hero.PrimarySkill = _myDeckPage.CurrentEditHero.PrimarySkill;
            hero.SecondarySkill = _myDeckPage.CurrentEditHero.SecondarySkill;

            deck.PrimarySkill = hero.PrimarySkill;
            deck.SecondarySkill = hero.SecondarySkill;

            try
            {
                await _backendFacade.EditDeck(_backendDataControlMediator.UserDataModel.UserId, deck);
            }
            catch (Exception e)
            {
                success = false;                
                Helpers.ExceptionReporter.LogException(e);
                Debug.LogWarning($"got exception: {e.Message} ->> {e.StackTrace}");

                OpenAlertDialog("Not able to edit Deck: \n" + e.Message);
            }
            _buttonSelectOverlordSkillContinue.interactable = true;

            if (success)
                _myDeckPage.ChangeTab(MyDecksPage.TAB.EDITING);
        }
        
        private void UpdateSkillIconAndDescriptionDisplay()
        {
            List<OverlordAbilityItem> abilities = _overlordAbilities.FindAll(x => x.IsSelected);
            for(int i=0; i<2;++i)
            {
                if(i < abilities.Count)
                {
                    _imageSkillIcons[i].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + abilities[i].Skill.IconPath);
                    _textSkillDescriptions[i].text = abilities[i].Skill.Title + ":"+ abilities[i].Skill.Description;
               }
                else
                {
                     _imageSkillIcons[i].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
                    _textSkillDescriptions[i].text = "No selected skill";
                }
            }
            _textSelectedAmount.text = "" + abilities.Count + "/2";
        }
        
        private void UpdateOverlordPortrait()
        {
            _imageSelectOverlordSkillPortrait.sprite = _myDeckPage.MyDecksSelectOverlordTab.GetOverlordPortraitSprite
            (
                _myDeckPage.CurrentEditHero.HeroElement
            );
        }

        #region From Old Script

        private void UpdateTabShow()
        {
            _singleSelectionMode = false;
            _isPrimarySkillSelected = false;
        
            GameObject Self = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_OldContent").gameObject;
            _backLayerCanvas = Self.transform.Find("Canvas_BackLayer").GetComponent<Canvas>();

            _continueButton = _backLayerCanvas.transform.Find("Button_Continue").GetComponent<Button>();
            _cancelButton = _backLayerCanvas.transform.Find("Button_Cancel").GetComponent<Button>();

            _continueButton.onClick.AddListener(ContinueButtonOnClickHandler);
            _cancelButton.onClick.AddListener(CancelButtonOnClickHandler);

            _skillName = _backLayerCanvas.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
            _skillDescription = _backLayerCanvas.transform.Find("SkillDescription").GetComponent<TextMeshProUGUI>();

            _abilitiesGroup = Self.transform.Find("Canvas_BackLayer/Abilities").gameObject;

            _skillName.text = "No Skills selected";
            _skillDescription.text = string.Empty;
            
            FillOverlordAbilities();

            if (_singleSelectionMode)
            {
                OverlordAbilityItem ability = _overlordAbilities.Find(x => x.Skill.OverlordSkill == (_isPrimarySkillSelected ?
                 _myDeckPage.CurrentEditDeck.PrimarySkill : _myDeckPage.CurrentEditDeck.SecondarySkill));

                 if(ability == null)
                 {
                    if(_isPrimarySkillSelected && _myDeckPage.CurrentEditDeck.PrimarySkill != Enumerators.OverlordSkill.NONE)
                    {
                        ability = _overlordAbilities.Find(x => x.Skill.OverlordSkill != _myDeckPage.CurrentEditDeck.SecondarySkill);
                    }
                    else if (_myDeckPage.CurrentEditDeck.SecondarySkill != Enumerators.OverlordSkill.NONE)
                    {
                        ability = _overlordAbilities.Find(x => x.Skill.OverlordSkill != _myDeckPage.CurrentEditDeck.PrimarySkill);
                    }
                }

                OverlordAbilitySelectedHandler(ability);
            }
            else
            {
                if (_selectedSkills == null)
                {
                    _selectedSkills = _myDeckPage.CurrentEditHero.Skills.FindAll(x => x.Unlocked);

                    if (_selectedSkills.Count > 1)
                    {
                        _selectedSkills = _selectedSkills.GetRange(0, 2);
                    }
                }

                OverlordAbilityItem ability;
                foreach (HeroSkill skill in _selectedSkills)
                {
                    ability = _overlordAbilities.Find(x => x.Skill.OverlordSkill == skill.OverlordSkill);
                    OverlordAbilitySelectedHandler(ability);
                }
            }
        }
        
        #region button handlers

        public async void ContinueButtonOnClickHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_continueButton.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            if (_singleSelectionMode)
            {
                OverlordAbilityItem ability = _overlordAbilities.Find(x => x.IsSelected);

                if (ability != null)
                {
                    if (_isPrimarySkillSelected)
                    {
                        _myDeckPage.CurrentEditHero.PrimarySkill = ability.Skill.OverlordSkill;
                        _myDeckPage.CurrentEditHero.SecondarySkill = _myDeckPage.CurrentEditDeck.SecondarySkill;
                    }
                    else
                    {
                        _myDeckPage.CurrentEditHero.PrimarySkill = _myDeckPage.CurrentEditDeck.PrimarySkill;
                        _myDeckPage.CurrentEditHero.SecondarySkill = ability.Skill.OverlordSkill;
                    }
                }
            }
            else
            {
                List<OverlordAbilityItem> abilities = _overlordAbilities.FindAll(x => x.IsSelected);

                if (abilities.Count > 1)
                {
                    _myDeckPage.CurrentEditHero.PrimarySkill = abilities[0].Skill.OverlordSkill;
                    _myDeckPage.CurrentEditHero.SecondarySkill = abilities[1].Skill.OverlordSkill;
                }
                else if(abilities.Count == 1)
                {
                    _myDeckPage.CurrentEditHero.PrimarySkill = abilities[0].Skill.OverlordSkill;
                    _myDeckPage.CurrentEditHero.SecondarySkill = Enumerators.OverlordSkill.NONE;
                }
                else
                {
                    _myDeckPage.CurrentEditHero.PrimarySkill = Enumerators.OverlordSkill.NONE;
                    _myDeckPage.CurrentEditHero.SecondarySkill = Enumerators.OverlordSkill.NONE;
                }
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _uiManager.HidePopup<OverlordAbilitySelectionPopup>();

            if (_myDeckPage.CurrentEditDeck != null)
            {
                _myDeckPage.CurrentEditDeck.PrimarySkill = _myDeckPage.CurrentEditHero.PrimarySkill;
                _myDeckPage.CurrentEditDeck.SecondarySkill = _myDeckPage.CurrentEditHero.SecondarySkill;

                try
                {
                    await _backendFacade.EditDeck(_backendDataControlMediator.UserDataModel.UserId, _myDeckPage.CurrentEditDeck);
                }
                catch (Exception e)
                {
                    Helpers.ExceptionReporter.LogException(e);

                    Debug.LogWarning($"got exception: {e.Message} ->> {e.StackTrace}");

                    OpenAlertDialog("Not able to edit Deck: \n" + e.Message);
                }
            }

            PopupHiding?.Invoke();
        }

        public void CancelButtonOnClickHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_cancelButton.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _uiManager.HidePopup<OverlordAbilitySelectionPopup>();
        }

        #endregion

        private void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        private void FillOverlordAbilities()
        {
            ResetOverlordAbilities();

            OverlordAbilityItem abilityInstance;
            HeroSkill ability = null;

            bool overrideLock; 

            for (int i = 0; i < AbilityListSize; i++)
            {
                ability = null;
                overrideLock = false;

                if (i < _myDeckPage.CurrentEditHero.Skills.Count)
                {
                    ability = _myDeckPage.CurrentEditHero.Skills[i];
                }

                if (_singleSelectionMode && ability != null && _myDeckPage.CurrentEditDeck != null)
                {
                    if (_isPrimarySkillSelected)
                    {
                        if (_myDeckPage.CurrentEditDeck.SecondarySkill == ability.OverlordSkill)
                        {
                            overrideLock = true;
                        }
                    }
                    else
                    {
                        if (_myDeckPage.CurrentEditDeck.PrimarySkill == ability.OverlordSkill)
                        {
                            overrideLock = true;
                        }
                    }
                }

                overrideLock = false;
                abilityInstance = new OverlordAbilityItem(_abilitiesGroup.transform, ability, overrideLock);
                abilityInstance.OverlordAbilitySelected += OverlordAbilitySelectedHandler;

                _overlordAbilities.Add(abilityInstance);
            }
        }

        private void ResetOverlordAbilities()
        {
            foreach (OverlordAbilityItem abilityInstance in _overlordAbilities)
            {
                abilityInstance.Dispose();
            }

            _overlordAbilities.Clear();
        }

        private void OverlordAbilitySelectedHandler(OverlordAbilityItem ability)
        {
            if (ability == null)
                return;

            if (_singleSelectionMode)
            {
                foreach (OverlordAbilityItem item in _overlordAbilities)
                {
                    if (ability != item)
                    {
                        item.Deselect();
                    }
                }

                ability.Select();

                _skillName.text = ability.Skill.Title;
                _skillDescription.text = ability.Skill.Description;
            }
            else
            {
                if (ability.IsSelected)
                {
                    ability.Deselect();
                }
                else
                {
                    if (_overlordAbilities.FindAll(x => x.IsSelected).Count < 2)
                    {
                        ability.Select();

                        _skillName.text = ability.Skill.Title;
                        _skillDescription.text = ability.Skill.Description;
                    }
                }
            }

            UpdateSkillIconAndDescriptionDisplay();
        }

        private class OverlordAbilityItem : IDisposable
        {
            public event Action<OverlordAbilityItem> OverlordAbilitySelected;

            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly GameObject _selfObject;

            private readonly GameObject _lockedObject;

            private readonly Button _selectButton;

            private readonly GameObject _glowObj;

            private readonly Image _abilityIconImage;

            public readonly HeroSkill Skill;

            public bool IsSelected { get; private set; }

            public bool IsUnlocked { get; private set; }

            public OverlordAbilityItem(Transform root, HeroSkill skill, bool overrideLock = false)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                Skill = skill;

                _selfObject =
                    Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>(
                            "Prefabs/UI/Elements/OverlordAbilityPopupAbilityItem"), root, false);

                _lockedObject = _selfObject.transform.Find("Object_Locked").gameObject;
                _glowObj = _selfObject.transform.Find("Glow").gameObject;
                _abilityIconImage = _selfObject.transform.Find("AbilityIcon").GetComponent<Image>();
                _selectButton = _selfObject.GetComponent<Button>();

                _selectButton.onClick.AddListener(SelectButtonOnClickHandler);

                //IsUnlocked = Skill != null ? Skill.Unlocked : false;
                IsUnlocked = (Skill != null);

                _abilityIconImage.sprite = IsUnlocked ?
                    _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + Skill.IconPath) :
                     _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");

                _selectButton.interactable = IsUnlocked;

                _glowObj.SetActive(false);

                if (overrideLock && IsUnlocked)
                {
                    _lockedObject.SetActive(true);
                    _selectButton.interactable = false;
                }
            }

            public void Dispose()
            {
                Object.Destroy(_selfObject);
            }

            public void Select()
            {
                IsSelected = true;

                _glowObj.SetActive(IsSelected);
            }

            public void Deselect()
            {
                IsSelected = false;

                _glowObj.SetActive(IsSelected);
            }

            private void SelectButtonOnClickHandler()
            {
                OverlordAbilitySelected?.Invoke(this);
            }
        }

        #endregion
    }
}