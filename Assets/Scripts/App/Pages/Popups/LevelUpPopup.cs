using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class LevelUpPopup : IUIPopup
    {
        private readonly WaitForSeconds _experienceFillWait = new WaitForSeconds(1);

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private Button _buttonOk;

        private SpriteRenderer _selectHeroSpriteRenderer;

        private TextMeshProUGUI _currentLevel;

        private TextMeshProUGUI _skillName;

        private TextMeshProUGUI _skillDescription;

        private Transform _abilitiesGroup;

        private List<AbilityViewItem> _abilities;

        private Hero _selectedHero;

        public GameObject Self { get; private set; }

        private int _abilityListSize = 5;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _abilities = new List<AbilityViewItem>();
        }

        public void Dispose()
        {
            foreach (AbilityViewItem abilityViewItem in _abilities)
            {
                abilityViewItem.Dispose();
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
            if (Self != null)
                return;

            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LevelUpPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _selectHeroSpriteRenderer = Self.transform.Find("Pivot/LevelUpPopup/LevelUpPanel/SelectHero")
                .GetComponent<SpriteRenderer>();

            _buttonOk = Self.transform.Find("Pivot/LevelUpPopup/LevelUpPanel/UI/Button_Continue").GetComponent<Button>();
            _buttonOk.onClick.AddListener(OnClickOkButtonEventHandler);

            _abilitiesGroup = Self.transform.Find("Pivot/LevelUpPopup/LevelUpPanel/UI/Abilities");

            _currentLevel = Self.transform.Find("Pivot/LevelUpPopup/LevelUpPanel/UI/Text_Level")
                .GetComponent<TextMeshProUGUI>();

            _skillName = Self.transform.Find("Pivot/LevelUpPopup/LevelUpPanel/UI/SkillName")
                .GetComponent<TextMeshProUGUI>();

            _skillDescription = Self.transform.Find("Pivot/LevelUpPopup/LevelUpPanel/UI/SkillDescription")
                .GetComponent<TextMeshProUGUI>();

            Self.SetActive(true);

            int playerDeckId = GameClient.Get<IGameplayManager>().PlayerDeckId;
            IDataManager dataManager = GameClient.Get<IDataManager>();

            int heroId = dataManager.CachedDecksData.Decks.First(d => d.Id == playerDeckId).HeroId;

            _selectedHero = dataManager.CachedHeroesData.HeroesParsed[heroId];
            string heroName = _selectedHero.Element.ToLowerInvariant();
            _selectHeroSpriteRenderer.sprite =
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" + heroName.ToLowerInvariant());

            _currentLevel.text = _selectedHero.Level.ToString();

            _abilities.Clear();

            FillInfo();

            AbilityViewItem newOpenAbility = _abilities.FindLast((x) => x.Skill != null);
            newOpenAbility.IsSelected = true;
            AbilityInstanceOnSelectionChanged(newOpenAbility);
        }

        public void Show(object data)
        {
            Show();
        }

        private void FillInfo()
        {
            AbilityViewItem abilityInstance = null;
            Debug.LogError(_selectedHero.Skills.Count);
            for (int i = 0; i < _abilityListSize; i++)
            {
                abilityInstance = new AbilityViewItem(_abilitiesGroup);
                if (i < 2)//(_selectedHero.Skills[i].Unlocked)
                {
                    abilityInstance.Skill = _selectedHero.Skills[i];
                }
                _abilities.Add(abilityInstance);
            }
        }

        public void Update()
        {
        }

        private void AbilityInstanceOnSelectionChanged(AbilityViewItem ability)
        {
            _skillName.text = ability.Skill.Title;
            _skillDescription.text = ability.Skill.Description;
        }

        private void OnClickOkButtonEventHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _uiManager.HidePopup<LevelUpPopup>();
        }

        private class AbilityViewItem
        {
            public readonly GameObject SelfObject;

            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly GameObject _glowObj;

            private readonly Image _abilityIconImage;

            private readonly Transform _parentGameObject;

            private HeroSkill _skill;

            private bool _isSelected;

            public AbilityViewItem(Transform root)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                _parentGameObject = root;
                SelfObject =
                    Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>(
                            "Prefabs/UI/Elements/LevelUpPopupAbilityItem"), root, false);

                _glowObj = SelfObject.transform.Find("Glow").gameObject;
                _abilityIconImage = SelfObject.transform.Find("AbilityIcon").GetComponent<Image>();

                UpdateUIState();
            }


            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    _glowObj.SetActive(value);
                }
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


            private void UpdateUIState()
            {
                _glowObj.SetActive(_isSelected);

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
