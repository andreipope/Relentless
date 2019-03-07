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

        private Image[] _imageSkillIcons;

        private TextMeshProUGUI[] _textSkillDescriptions;

        private TextMeshProUGUI _textSelectedAmount;
        
        private const int AbilityListSize = 5;

        private const int MaxSelectedAbilities = 2;

        public event Action PopupHiding;

        private ISoundManager _soundManager;

        private Button _continueButton;

        private GameObject _abilitiesGroup;

        private List<OverlordAbilityItem> _overlordAbilityItems;

        private Canvas _backLayerCanvas;
        
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
            _overlordAbilityItems = new List<OverlordAbilityItem>();
            
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

            PopupHiding += ProcessEditOverlordSkills;
        }
        
        public void Show(GameObject selfPage)
        {
            _selfPage = selfPage;
            
            _imageSelectOverlordSkillPortrait = _selfPage.transform.Find("Tab_SelectOverlordSkill/Panel_Content/Image_OverlordPortrait").GetComponent<Image>();                        
            
            _imageSkillIcons[0] = _selfPage.transform.Find("Tab_SelectOverlordSkill/Panel_Content/Image_SkillSlots/Image_SkillIcon_1").GetComponent<Image>();  
            _imageSkillIcons[1] = _selfPage.transform.Find("Tab_SelectOverlordSkill/Panel_Content/Image_SkillSlots/Image_SkillIcon_2").GetComponent<Image>();  
            
            _textSkillDescriptions[0] = _selfPage.transform.Find("Tab_SelectOverlordSkill/Panel_Content/Image_SkillSlots/Text_Desc_1").GetComponent<TextMeshProUGUI>();  
            _textSkillDescriptions[1] = _selfPage.transform.Find("Tab_SelectOverlordSkill/Panel_Content/Image_SkillSlots/Text_Desc_2").GetComponent<TextMeshProUGUI>();         
            
            _textSelectedAmount = _selfPage.transform.Find("Tab_SelectOverlordSkill/Panel_Content/Image_SelectAmount/Text_SelectedAmount").GetComponent<TextMeshProUGUI>();
            
            _backLayerCanvas = _selfPage.transform.Find("Tab_SelectOverlordSkill/Canvas_BackLayer").GetComponent<Canvas>();

            _continueButton = _backLayerCanvas.transform.Find("Button_Continue").GetComponent<Button>();
            _continueButton.onClick.AddListener(ContinueButtonOnClickHandler);
            _continueButton.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _abilitiesGroup = _backLayerCanvas.transform.Find("Abilities").gameObject;
        }
        
        public void Update()
        {

        }
        
        public void Dispose()
        {
            ResetItems();
        }
        
        private async void ProcessEditOverlordSkills()
        { 
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

            if (success)
                _myDeckPage.ChangeTab(MyDecksPage.TAB.EDITING);
        }
        
        private void UpdateSkillIconAndDescriptionDisplay()
        {
            List<OverlordAbilityItem> items = _overlordAbilityItems.FindAll(x => x.IsSelected);
            for(int i=0; i<2;++i)
            {
                if(i < items.Count)
                {
                    _imageSkillIcons[i].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + items[i].Skill.IconPath);
                    _textSkillDescriptions[i].text = items[i].Skill.Title + ":"+ items[i].Skill.Description;
               }
                else
                {
                     _imageSkillIcons[i].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
                    _textSkillDescriptions[i].text = "No selected skill";
                }
            }
            _textSelectedAmount.text = "" + items.Count + "/2";
        }
        
        private void UpdateOverlordPortrait()
        {
            _imageSelectOverlordSkillPortrait.sprite = _myDeckPage.MyDecksSelectOverlordTab.GetOverlordPortraitSprite
            (
                _myDeckPage.CurrentEditHero.HeroElement
            );
        }

        private void UpdateTabShow()
        {
            FillAvailableAbilities();
            UpdateSelectedSkills();         
        }
        
        private void UpdateSelectedSkills()
        {
            List<HeroSkill> selectedSkills = new List<HeroSkill>();
            if(_myDeckPage.CurrentEditDeck.PrimarySkill != Enumerators.OverlordSkill.NONE)
            {
                HeroSkill heroSkil = _myDeckPage.CurrentEditHero.GetSkill(_myDeckPage.CurrentEditDeck.PrimarySkill);
                selectedSkills.Add(heroSkil);
            }
             if(_myDeckPage.CurrentEditDeck.SecondarySkill != Enumerators.OverlordSkill.NONE)
            {
                HeroSkill heroSkil = _myDeckPage.CurrentEditHero.GetSkill(_myDeckPage.CurrentEditDeck.SecondarySkill);
                selectedSkills.Add(heroSkil);
            }
            
            foreach (HeroSkill skill in selectedSkills)
            {
                OverlordAbilityItem item = _overlordAbilityItems.Find(x => x.Skill.OverlordSkill == skill.OverlordSkill);
                OverlordAbilitySelectedHandler(item);
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
            
            List<OverlordAbilityItem> items = _overlordAbilityItems.FindAll(x => x.IsSelected);

            if (items.Count > 1)
            {
                _myDeckPage.CurrentEditHero.PrimarySkill = items[0].Skill.OverlordSkill;
                _myDeckPage.CurrentEditHero.SecondarySkill = items[1].Skill.OverlordSkill;
            }
            else if(items.Count == 1)
            {
                _myDeckPage.CurrentEditHero.PrimarySkill = items[0].Skill.OverlordSkill;
                _myDeckPage.CurrentEditHero.SecondarySkill = Enumerators.OverlordSkill.NONE;
            }
            else
            {
                _myDeckPage.CurrentEditHero.PrimarySkill = Enumerators.OverlordSkill.NONE;
                _myDeckPage.CurrentEditHero.SecondarySkill = Enumerators.OverlordSkill.NONE;
            }
            

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

        #endregion

        private void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        private void FillAvailableAbilities()
        {
            ResetItems();

            OverlordAbilityItem itemInstance;
            HeroSkill ability = null;

            for (int i = 0; i < AbilityListSize; i++)
            {
                ability = null;

                if (i < _myDeckPage.CurrentEditHero.Skills.Count)
                {
                    ability = _myDeckPage.CurrentEditHero.Skills[i];
                }

                itemInstance = new OverlordAbilityItem(_abilitiesGroup.transform, ability);
                itemInstance.OverlordAbilitySelected += OverlordAbilitySelectedHandler;

                _overlordAbilityItems.Add(itemInstance);
            }
        }

        private void ResetItems()
        {
            foreach (OverlordAbilityItem itemInstance in _overlordAbilityItems)
            {
                itemInstance.Dispose();
            }

            _overlordAbilityItems.Clear();
        }

        private void OverlordAbilitySelectedHandler(OverlordAbilityItem item)
        {
            if (item == null)
                return;
           
            if (item.IsSelected)
            {
                item.Deselect();
            }
            else
            {
                if (_overlordAbilityItems.FindAll(x => x.IsSelected).Count < 2)
                {
                    item.Select();
                }
            }            

            UpdateSkillIconAndDescriptionDisplay();
        }

        private class OverlordAbilityItem : IDisposable
        {
            public event Action<OverlordAbilityItem> OverlordAbilitySelected;

            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly GameObject _selfObject;

            private readonly Button _selectButton;

            private readonly GameObject _glowObj;

            private readonly Image _abilityIconImage;

            public readonly HeroSkill Skill;

            public bool IsSelected { get; private set; }

            public bool IsUnlocked { get; private set; }

            public OverlordAbilityItem(Transform root, HeroSkill skill)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                Skill = skill;

                _selfObject =
                    Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>(
                            "Prefabs/UI/Elements/DeckSelection/OverlordAbilityItem"), root, false);

                _glowObj = _selfObject.transform.Find("Glow").gameObject;
                _abilityIconImage = _selfObject.transform.Find("AbilityIcon").GetComponent<Image>();
                _selectButton = _selfObject.GetComponent<Button>();

                _selectButton.onClick.AddListener(SelectButtonOnClickHandler);

                IsUnlocked = Skill != null ? Skill.Unlocked : false;

                _abilityIconImage.sprite = IsUnlocked ?
                    _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + Skill.IconPath) :
                     _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");

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
                OverlordAbilitySelected?.Invoke(this);
            }
        }
    }
}