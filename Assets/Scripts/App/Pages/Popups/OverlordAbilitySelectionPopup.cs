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

        private Button _continueButton;

        private Button _cancelButton;

        private GameObject _abilitiesGroup;

        private TextMeshProUGUI _title;

        private TextMeshProUGUI _skillName;

        private TextMeshProUGUI _skillDescription;

        private Image _heroImage;

        private List<AbilityInstance> _abilities;

        private Hero _selectedHero;

        private Canvas _backLayerCanvas;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _abilities = new List<AbilityInstance>();
        }

        public void Dispose()
        {
            foreach (AbilityInstance abilityInstance in _abilities)
            {
                abilityInstance.Dispose();
            }

            _abilities.Clear();
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

            _abilities.Clear();

            for (int i = 0; i < AbilityListSize; i++)
            {
                AbilityInstance abilityInstance = new AbilityInstance(_abilitiesGroup.transform);
                abilityInstance.SelectionChanged += AbilityInstanceOnSelectionChanged;
                _abilities.Add(abilityInstance);
            }
        }

        public void Show(object data)
        {
            Show();
            _selectedHero = (Hero)data;
            FillInfo(_selectedHero);
            _abilities[0].IsSelected = true;
            AbilityInstanceOnSelectionChanged(_abilities[0]);
        }

        public void Update()
        {
        }

        public void ContinueButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            PopupHiding?.Invoke();

            _uiManager.HidePopup<OverlordAbilitySelectionPopup>();
        }

        public void CancelButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _uiManager.HidePopup<OverlordAbilitySelectionPopup>();
        }

        private void FillInfo(Hero heroData)
        {
            _heroImage.sprite =
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" + heroData.Element.ToLower());
            _heroImage.SetNativeSize();

            for (int i = 0; i < AbilityListSize; i++)
            {
                HeroSkill skill = null;
                if (i < heroData.Skills.Count)
                {
                    skill = heroData.Skills[i];
                }

                _abilities[i].Skill = skill;
                _abilities[i].AllowMultiSelect = false;
            }
        }

        private void AbilityInstanceOnSelectionChanged(AbilityInstance ability)
        {
            _skillName.text = ability.Skill.Title;
            _skillDescription.text = ability.Skill.Description;
            int index = _selectedHero.Skills.IndexOf(ability.Skill);
            _selectedHero.PrimarySkill = index;
        }

        private class AbilityInstance : IDisposable
        {
            public readonly GameObject SelfObject;

            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly Toggle _abilityToggle;

            private readonly GameObject _glowObj;

            private readonly Image _abilityIconImage;

            private readonly Transform _parentGameObject;

            private HeroSkill _skill;

            private bool _isSelected;

            public AbilityInstance(Transform root)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                _parentGameObject = root;
                SelfObject =
                    Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>(
                            "Prefabs/UI/Elements/OverlordAbilityPopupAbilityItem"), root, false);

                _abilityToggle = SelfObject.GetComponent<Toggle>();
                _abilityToggle.group = root.GetComponent<ToggleGroup>();

                _glowObj = SelfObject.transform.Find("Glow").gameObject;
                _abilityIconImage = SelfObject.transform.Find("AbilityIcon").GetComponent<Image>();

                _abilityToggle.onValueChanged.AddListener(OnToggleValueChanged);

                UpdateUIState();
            }

            public event Action<AbilityInstance> SelectionChanged;

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    _abilityToggle.isOn = value;
                }
            }

            public bool AllowMultiSelect
            {
                set => _abilityToggle.group = value ? null : _parentGameObject.GetComponent<ToggleGroup>();
            }

            public HeroSkill Skill
            {
                get => _skill;
                set
                {
                    if (_skill == value)
                        return;

                    _skill = value;
                    UpdateUIState();
                }
            }

            public void Dispose()
            {
                Object.Destroy(SelfObject);
            }

            private void OnToggleValueChanged(bool selected)
            {
                _isSelected = selected;
                UpdateUIState();

                SelectionChanged?.Invoke(this);
            }

            private void UpdateUIState()
            {
                _glowObj.SetActive(_isSelected);

                _abilityToggle.interactable = Skill != null;
                if (Skill != null)
                {
                    _abilityIconImage.sprite =
                        _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + Skill.IconPath);
                }
                else
                {
                    _abilityIconImage.sprite =
                        _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/overlordability_locked");
                }
            }
        }
    }
}
