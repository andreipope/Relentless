using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class OverlordAbilitySelectionPopup : IUIPopup
    {
        private const int AbilityListSize = 5;

        private const int MaxSelectedAbilities = 2;

        public event Action PopupHiding;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private ISoundManager _soundManager;

        private Button _continueButton;

        private Button _cancelButton;

        private GameObject _abilitiesGroup;

        private TextMeshProUGUI _title;

        private TextMeshProUGUI _skillName;

        private TextMeshProUGUI _skillDescription;

        private Image _heroImage;

        private List<OverlordAbilityItem> _overlordAbilities;

        private Hero _selectedHero;

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
            }


            Show();

            FillOverlordInfo(_selectedHero);
            FillOverlordAbilities();

            if (_singleSelectionMode)
            {
                OverlordAbilityItem ability = _overlordAbilities.Find(x => x.Skill.Skill == (_isPrimarySkillSelected ?
                 _selectedHero.PrimarySkill : _selectedHero.SecondarySkill));

                ability = ability == null ? _overlordAbilities[0] : ability;

                OverlordAbilitySelkectedHandler(ability);
            }
            else
            {
                if (_selectedSkills != null)
                {
                    OverlordAbilityItem ability;
                    foreach (HeroSkill skill in _selectedSkills)
                    {
                        ability = _overlordAbilities.Find(x => x.Skill.Skill == skill.Skill);
                        OverlordAbilitySelkectedHandler(ability);
                    }
                }
            }
        }

        public void Update()
        {
        }


        #region button handlers
        public void ContinueButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            PopupHiding?.Invoke();

            _uiManager.HidePopup<OverlordAbilitySelectionPopup>();
        }

        public void CancelButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _uiManager.HidePopup<OverlordAbilitySelectionPopup>();
        }

        #endregion

        private void FillOverlordAbilities()
        {
            ResetOverlordAbilities();

            OverlordAbilityItem abilityInstance;
            HeroSkill ability = null;

            for (int i = 0; i < AbilityListSize; i++)
            {
                ability = null;

                if (i < _selectedHero.Skills.Count)
                {
                    ability = _selectedHero.Skills[i];
                }

                abilityInstance = new OverlordAbilityItem(_abilitiesGroup.transform, ability);
                abilityInstance.OverlordAbilitySelected += OverlordAbilitySelkectedHandler;

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
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" + heroData.Element.ToLower());
            _heroImage.SetNativeSize();
        }

        private void OverlordAbilitySelkectedHandler(OverlordAbilityItem ability)
        {
            _skillName.text = ability.Skill.Title;
            _skillDescription.text = ability.Skill.Description;

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


                if (_isPrimarySkillSelected)
                {
                    _selectedHero.PrimarySkill = ability.Skill.Skill;
                }
                else
                {
                    _selectedHero.SecondarySkill = ability.Skill.Skill;
                }
            }
            else
            {
            }
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
                            "Prefabs/UI/Elements/OverlordAbilityPopupAbilityItem"), root, false);

                _glowObj = _selfObject.transform.Find("Glow").gameObject;
                _abilityIconImage = _selfObject.transform.Find("AbilityIcon").GetComponent<Image>();
                _selectButton = _selfObject.transform.Find("").GetComponent<Button>();

                _selectButton.onClick.AddListener(SelectButtonOnClickHandler);

                IsUnlocked = Skill != null ? Skill.Unlocked : false;

                _abilityIconImage.sprite = IsUnlocked ?
                            _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + Skill.IconPath) :
                             _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/overlordability_locked");

                _selectButton.interactable = IsUnlocked;
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
