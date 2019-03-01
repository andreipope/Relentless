using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class OverlordAbilitySelectionPopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(OverlordAbilitySelectionPopup));

        private const int AbilityListSize = 5;

        private const int MaxSelectedAbilities = 2;

        public event Action PopupHiding;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private ISoundManager _soundManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private Button _continueButton;

        private Button _cancelButton;

        private GameObject _abilitiesGroup;

        private TextMeshProUGUI _title;

        private TextMeshProUGUI _skillName;

        private TextMeshProUGUI _skillDescription;

        private Image _heroImage;

        private List<OverlordAbilityItem> _overlordAbilities;

        private Hero _selectedHero;

        private Deck _selectedDeck;

        private Canvas _backLayerCanvas;

        public GameObject Self { get; private set; }

        private bool _singleSelectionMode = false;

        private bool _isPrimarySkillSelected = true;

        private List<HeroSkill> _selectedSkills;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _overlordAbilities = new List<OverlordAbilityItem>();
        }

        public void Dispose()
        {
            ResetOverlordAbilities();
        }

        public void Hide()
        {
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
            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/OverlordAbilityPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _backLayerCanvas = Self.transform.Find("Canvas_BackLayer").GetComponent<Canvas>();

            _continueButton = _backLayerCanvas.transform.Find("Button_Continue").GetComponent<Button>();
            _cancelButton = _backLayerCanvas.transform.Find("Button_Cancel").GetComponent<Button>();

            _continueButton.onClick.AddListener(ContinueButtonOnClickHandler);
            _cancelButton.onClick.AddListener(CancelButtonOnClickHandler);

            _title = _backLayerCanvas.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            _skillName = _backLayerCanvas.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
            _skillDescription = _backLayerCanvas.transform.Find("SkillDescription").GetComponent<TextMeshProUGUI>();

            _abilitiesGroup = Self.transform.Find("Abilities").gameObject;

            _heroImage = _backLayerCanvas.transform.Find("HeroImage").GetComponent<Image>();

            _skillName.text = "No Skills selected";
            _skillDescription.text = string.Empty;
        }

        public void Show(object data)
        {
            if (data is object[] param)
            {
                _singleSelectionMode = (bool)param[0];
                _selectedHero = (Hero)param[1];

                if (_singleSelectionMode)
                {
                    _isPrimarySkillSelected = (bool)param[2];
                }
                else
                {
                    _selectedSkills = (List<HeroSkill>)param[2];
                }

                if(param[3] != null)
                {
                    _selectedDeck = (Deck)param[3];
                }
            }


            Show();

            FillOverlordInfo(_selectedHero);
            FillOverlordAbilities();

            if (_singleSelectionMode)
            {
                OverlordAbilityItem ability = _overlordAbilities.Find(x => x.Skill.OverlordSkill == (_isPrimarySkillSelected ?
                 _selectedDeck.PrimarySkill : _selectedDeck.SecondarySkill));

                 if(ability == null)
                 {
                    if(_isPrimarySkillSelected && _selectedDeck.PrimarySkill != Enumerators.OverlordSkill.NONE)
                    {
                        ability = _overlordAbilities.Find(x => x.Skill.OverlordSkill != _selectedDeck.SecondarySkill);
                    }
                    else if (_selectedDeck.SecondarySkill != Enumerators.OverlordSkill.NONE)
                    {
                        ability = _overlordAbilities.Find(x => x.Skill.OverlordSkill != _selectedDeck.PrimarySkill);
                    }
                }

                OverlordAbilitySelectedHandler(ability);
            }
            else
            {
                if (_selectedSkills == null)
                {
                    _selectedSkills = _selectedHero.Skills.FindAll(x => x.Unlocked);

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

        public void Update()
        {
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
                        _selectedHero.PrimarySkill = ability.Skill.OverlordSkill;
                        _selectedHero.SecondarySkill = _selectedDeck.SecondarySkill;
                    }
                    else
                    {
                        _selectedHero.PrimarySkill = _selectedDeck.PrimarySkill;
                        _selectedHero.SecondarySkill = ability.Skill.OverlordSkill;
                    }
                }
            }
            else
            {
                List<OverlordAbilityItem> abilities = _overlordAbilities.FindAll(x => x.IsSelected);

                if (abilities.Count > 1)
                {
                    _selectedHero.PrimarySkill = abilities[0].Skill.OverlordSkill;
                    _selectedHero.SecondarySkill = abilities[1].Skill.OverlordSkill;
                }
                else if(abilities.Count == 1)
                {
                    _selectedHero.PrimarySkill = abilities[0].Skill.OverlordSkill;
                    _selectedHero.SecondarySkill = Enumerators.OverlordSkill.NONE;
                }
                else
                {
                    _selectedHero.PrimarySkill = Enumerators.OverlordSkill.NONE;
                    _selectedHero.SecondarySkill = Enumerators.OverlordSkill.NONE;
                }
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _uiManager.HidePopup<OverlordAbilitySelectionPopup>();

            if (_selectedDeck != null)
            {
                _selectedDeck.PrimarySkill = _selectedHero.PrimarySkill;
                _selectedDeck.SecondarySkill = _selectedHero.SecondarySkill;

                try
                {
                    await _backendFacade.EditDeck(_backendDataControlMediator.UserDataModel.UserId, _selectedDeck);
                }
                catch (Exception e)
                {
                    Helpers.ExceptionReporter.LogException(e);

                    Log.Warn($"got exception: {e.Message} ->> {e.StackTrace}");

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

                if (i < _selectedHero.Skills.Count)
                {
                    ability = _selectedHero.Skills[i];
                }

                if (_singleSelectionMode && ability != null && _selectedDeck != null)
                {
                    if (_isPrimarySkillSelected)
                    {
                        if (_selectedDeck.SecondarySkill == ability.OverlordSkill)
                        {
                            overrideLock = true;
                        }
                    }
                    else
                    {
                        if (_selectedDeck.PrimarySkill == ability.OverlordSkill)
                        {
                            overrideLock = true;
                        }
                    }
                }

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

        private void FillOverlordInfo(Hero heroData)
        {
            _heroImage.sprite =
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" + heroData.HeroElement.ToString().ToLowerInvariant());
            _heroImage.SetNativeSize();
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

                IsUnlocked = Skill != null ? Skill.Unlocked : false;

                _abilityIconImage.sprite = IsUnlocked ?
                    _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + Skill.IconPath) :
                     _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/overlordability_locked");

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
    }
}
