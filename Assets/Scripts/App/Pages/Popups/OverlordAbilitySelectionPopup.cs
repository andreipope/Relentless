// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
	public class OverlordAbilitySelectionPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
		private IDataManager _dataManager;
        private GameObject _selfPage;

		private Button _continueButton;
		private Button _buyButton;
		private GameObject _abilitiesGroup;
		private TextMeshProUGUI _title;
		private TextMeshProUGUI _skillName;
		private TextMeshProUGUI _skillDescription;
		private Image _heroImage;
        private List<AbilityInstance> _abilities;
		private Hero _heroData;

        private const int ABILITY_LIST_SIZE = 5;
        private const int MAX_SELECTED_ABILITIES = 2;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
			_dataManager = GameClient.Get<IDataManager> ();
           
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

        public void CloseButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            Hide();
        }

        public void Hide()
        {
            OnHidePopupEvent?.Invoke();

            if (_selfPage == null)
                return;

            _selfPage.SetActive (false);
            GameObject.Destroy (_selfPage);
            _selfPage = null;
		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/OverlordAbilityPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas3.transform, false);

            _continueButton = _selfPage.transform.Find("Button_Continue").GetComponent<Button>();
            _continueButton.onClick.AddListener(CloseButtonHandler);

            _title = _selfPage.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            _skillName = _selfPage.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
            _skillDescription = _selfPage.transform.Find("SkillDescription").GetComponent<TextMeshProUGUI>();

            _abilitiesGroup = _selfPage.transform.Find ("Abilities").gameObject;

            _heroImage = _selfPage.transform.Find("HeroImage").GetComponent<Image>();

            _abilities.Clear ();

            for (int i = 0; i < ABILITY_LIST_SIZE; i++)
            {
                AbilityInstance abilityInstance = new AbilityInstance(_abilitiesGroup.transform);
                abilityInstance.SelectionChanged += AbilityInstanceOnSelectionChanged;
                _abilities.Add(abilityInstance);
            }
        }

        public void Show(object data)
		{
            Show();

			FillInfo ((Hero) data);
		    _abilities[0].IsSelected = true;
            AbilityInstanceOnSelectionChanged (_abilities [0]);
        }

        public void Update()
        {

        }

		private void FillInfo (Hero heroData) {
			_heroData = heroData;
			_heroImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite> ("Images/Heroes/hero_" + heroData.element.ToLower ());
			_heroImage.SetNativeSize ();

		    for (int i = 0; i < ABILITY_LIST_SIZE; i++)
		    {
		        HeroSkill skill = null;
		        if (i < heroData.skills.Count)
		        {
		            skill = heroData.skills[i];
		        }
                _abilities[i].Skill = skill;
		        _abilities[i].AllowMultiSelect = false;
		    }
		}

        private void AbilityInstanceOnSelectionChanged(AbilityInstance ability) {
            _skillName.text = ability.Skill.title;
            _skillDescription.text = ability.Skill.description;
        }

        private class AbilityInstance : IDisposable
        {
            public GameObject SelfObject;

            public event Action<AbilityInstance> SelectionChanged;

            private ILoadObjectsManager _loadObjectsManager;
            private HeroSkill _skill;

            private Toggle _abilityToggle;
            private Image _glowImage;
            private Image _abilityIconImage;
            private bool _allowMultiSelect;
            private bool _isSelected;
            private Transform _parentGameObject;

            public bool IsSelected
            {
                get
                {
                    return _isSelected;
                }
                set
                {
                    _isSelected = value;
                    _abilityToggle.isOn = value;
                }
            }

            public bool AllowMultiSelect
            {
                get
                {
                    return _allowMultiSelect;
                }
                set
                {
                    _abilityToggle.group = value ? null : _parentGameObject.GetComponent<ToggleGroup>();
                }
            }

            public HeroSkill Skill
            {
                get
                {
                    return _skill;
                }
                set
                {
                    if (_skill == value)
                        return;

                    _skill = value;
                    UpdateUIState();
                }
            }

            public AbilityInstance(Transform root) {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                _parentGameObject = root;
                SelfObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OverlordAbilityPopupAbilityItem"), root, false);

                _abilityToggle = SelfObject.GetComponent<Toggle>();
                _abilityToggle.group = root.GetComponent<ToggleGroup>();

                _glowImage = SelfObject.transform.Find("Glow").GetComponent<Image>();
                _abilityIconImage = SelfObject.transform.Find("AbilityIcon").GetComponent<Image>();

                _abilityToggle.onValueChanged.AddListener(OnToggleValueChanged);

                UpdateUIState();
            }

            private void OnToggleValueChanged(bool selected) {
                _isSelected = selected;
                UpdateUIState();

                SelectionChanged?.Invoke(this);
            }

            private void UpdateUIState() {
                _glowImage.gameObject.SetActive(_isSelected);

                _abilityToggle.interactable = Skill != null;
                if (Skill != null)
                {
                    _abilityIconImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/Icons/" + Skill.iconPath);
                } else
                {
                    _abilityIconImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/Icons/overlordability_locked");
                }
            }

            public void Dispose() {
                MonoBehaviour.Destroy(SelfObject);
            }
        }
    }
}