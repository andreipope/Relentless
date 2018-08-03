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
	public class OverlordAbiltySelectionPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        public static Action OnHidePopupEvent;

        private const int ABILITY_LIST_SIZE = 5;
        private const int MAX_SELECTED_ABILITIES = 2;

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

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
			_dataManager = GameClient.Get<IDataManager> ();

			_selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/OverlordAbilityPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas3.transform, false);

			_continueButton = _selfPage.transform.Find("Button_Continue").GetComponent<Button>();
			_continueButton.onClick.AddListener(CloseButtonHandler);

			_title = _selfPage.transform.Find("Title").GetComponent<TextMeshProUGUI>();
			_skillName = _selfPage.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
			_skillDescription = _selfPage.transform.Find("SkillDescription").GetComponent<TextMeshProUGUI>();

			_abilitiesGroup = _selfPage.transform.Find ("Abilities").gameObject;

			_heroImage = _selfPage.transform.Find("HeroImage").GetComponent<Image>();

            Hide();
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
            _selfPage.SetActive(false);
		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            _selfPage.SetActive(true);
        }

        public void Show(object data)
		{
			FillInfo ((Hero) data);
		    _abilities[0].IsSelected = true;
            Show();
        }

        public void Update()
        {

        }

		private void FillInfo (Hero heroData) {
			_heroData = heroData;
			_heroImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite> ("Images/Heroes/hero_" + heroData.element.ToLower ());
			_heroImage.SetNativeSize ();

            _abilities = new List<AbilityInstance>();
		    for (int i = 0; i < ABILITY_LIST_SIZE; i++)
		    {
		        HeroSkill skill = heroData.skills[0];
		        if (i < heroData.skills.Count)
		        {
		            skill = heroData.skills[i];
		        }
                _abilities.Add(new AbilityInstance(_abilitiesGroup.transform, skill));
		    }

		    foreach (AbilityInstance abilityInstance in _abilities)
		    {
		        abilityInstance.SelectionChanged += AbilityInstanceOnSelectionChanged;
		    }
		}

        private void AbilityInstanceOnSelectionChanged(AbilityInstance ability) {
            _skillName.text = ability.Skill.title;
            _skillDescription.text = ability.Skill.description;
        }

        private class AbilityInstance : IDisposable
        {
            public GameObject SelfObject;
            public HeroSkill Skill;

            public event Action<AbilityInstance> SelectionChanged;

            private ILoadObjectsManager _loadObjectsManager;
            private Toggle _abilityToggle;
            private Image _glowImage;
            private Image _abilityIconImage;
            private bool _isSelected;

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

            public AbilityInstance(Transform root, HeroSkill skill) {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                Skill = skill;
                SelfObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OverlordAbilityPopupAbilityItem"));
                SelfObject.transform.SetParent(root);

                _abilityToggle = SelfObject.GetComponent<Toggle>();
                _abilityToggle.group = root.GetComponent<ToggleGroup>();

                _glowImage = SelfObject.transform.Find("Glow").GetComponent<Image>();
                _abilityIconImage = SelfObject.transform.Find("AbilityIcon").GetComponent<Image>();

                _abilityToggle.onValueChanged.AddListener(OnToggleValueChanged);

                if (Skill != null)
                {
                    _abilityIconImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/Icons/" + Skill.iconPath);
                } else
                {
                    _abilityIconImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/Icons/overlordability_locked");
                    _abilityToggle.interactable = false;
                }
            }

            private void OnToggleValueChanged(bool selected) {
                _isSelected = selected;
                _glowImage.gameObject.SetActive(selected);

                SelectionChanged?.Invoke(this);
            }

            public void Dispose() {
                MonoBehaviour.Destroy(SelfObject);
            }
        }
    }
}