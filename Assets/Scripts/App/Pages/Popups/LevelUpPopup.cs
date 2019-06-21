using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using log4net;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class LevelUpPopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(LevelUpPopup));
        private readonly WaitForSeconds _experienceFillWait = new WaitForSeconds(1);

        private const string _hideParameterName = "Hide";

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private IUIManager _uiManager;

        private Button _buttonOk;

        private TextMeshProUGUI _currentLevel;

        private TextMeshProUGUI _skillName;

        private TextMeshProUGUI _skillDescription;

        private TextMeshProUGUI _noRewardMessage;

        private HorizontalLayoutGroup _abilitiesGroup;

        private GameObject _rewardSkillObject;

        private GameObject _rewardBoosterPackObject;

        private GameObject _noRewardObject;

        private List<AbilityViewItem> _abilities;

        private AbilityViewItem _newOpenAbility;

        private Animator _backgroundAnimator, _containerAnimator;

        private OverlordUserInstance _selectedOverlord;

        public GameObject Self { get; private set; }

        private int _abilityListSize = 5;

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _abilities = new List<AbilityViewItem>();
        }

        public void Dispose()
        {
            ClearSkillInfo();
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
            throw new InvalidOperationException("Use Show with parameter");
        }

        public void Show(object data)
        {
            if (Self != null)
                return;

            EndMatchResults endMatchResults = (EndMatchResults) data;
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LevelUpPopup"), _uiManager.Canvas3.transform, false);

            _buttonOk = Self.transform.Find("Pivot/levelup_panel/UI/Button_Continue").GetComponent<Button>();
            _buttonOk.onClick.AddListener(OnClickOkButtonEventHandler);

            _rewardSkillObject = Self.transform.Find("Pivot/levelup_panel/UI/RewardSkill_Panel").gameObject;
            _rewardBoosterPackObject = Self.transform.Find("Pivot/levelup_panel/UI/RewardBoosterPack_Panel").gameObject;

            _noRewardObject = Self.transform.Find("Pivot/levelup_panel/UI/NoReward_Panel").gameObject;

            _abilitiesGroup = _rewardSkillObject.transform.Find("Abilities").GetComponent<HorizontalLayoutGroup>();

            _noRewardMessage = _noRewardObject.transform.Find("Message").GetComponent<TextMeshProUGUI>();

            _currentLevel = Self.transform.Find("Pivot/levelup_panel/UI/Text_Level").GetComponent<TextMeshProUGUI>();
            _skillName = _rewardSkillObject.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
            _skillDescription = _rewardSkillObject.transform.Find("SkillDescription").GetComponent<TextMeshProUGUI>();

            _backgroundAnimator = Self.transform.Find("Background").GetComponent<Animator>();
            _containerAnimator = Self.transform.Find("Pivot").GetComponent<Animator>();

            _backgroundAnimator.GetComponent<AnimationEventTriggering>().AnimationEventTriggered += AnimationEventTriggeredHandler;

            Self.SetActive(true);

            IDataManager dataManager = GameClient.Get<IDataManager>();

            _selectedOverlord = dataManager.CachedOverlordData.GetOverlordById(endMatchResults.OverlordId);
            _currentLevel.text = _selectedOverlord.UserData.Level.ToString();

            _newOpenAbility = null;

            FillInfo(endMatchResults);
        }

        private void FillInfo(EndMatchResults endMatchResults)
        {
            _noRewardObject.SetActive(false);
            _rewardSkillObject.SetActive(false);
            _rewardBoosterPackObject.SetActive(false);

            LevelReward levelReward = endMatchResults.LevelRewards.OrderBy(reward => reward.Level).FirstOrDefault();
            switch (levelReward)
            {
                case OverlordSkillRewardItem overlordSkillRewardItem:
                    _rewardSkillObject.SetActive(true);
                    FillRewardSkillInfo(overlordSkillRewardItem.SkillIndex);
                    AbilityInstanceOnSelectionChanged(_newOpenAbility);
                    break;
                case BoosterPackRewardItem boosterPackRewardItem:
                    _rewardBoosterPackObject.SetActive(true);
                    FillRewardBoosterPackInfo(boosterPackRewardItem.Amount);
                    break;
                case null:
                    _noRewardObject.SetActive(true);
                    FillNoRewardInfo(endMatchResults.CurrentLevel);
                    break;
            }
        }

        private void FillNoRewardInfo(int currentLevel)
        {
            if (currentLevel >= _dataManager.CachedOverlordLevelingData.MaxLevel)
            {
                _noRewardMessage.text = "You've collected all the rewards available for this champion!";
            }
            else
            {
                LevelReward nextLevelReward =
                    _dataManager.CachedOverlordLevelingData.Rewards
                        .OrderBy(reward => reward.Level)
                        .FirstOrDefault(reward => reward.Level > currentLevel);

                Assert.IsNotNull(nextLevelReward);
                _noRewardMessage.text = $"Get next reward at level {nextLevelReward?.Level ?? 0}!";
            }
        }

        private void FillRewardBoosterPackInfo(int amount)
        {
            // TODO: show amount?
        }

        private void FillRewardSkillInfo(int skillIndex)
        {
            ClearSkillInfo();

            for (int i = 0; i < _abilityListSize; i++)
            {
                AbilityViewItem abilityInstance = new AbilityViewItem(_abilitiesGroup.transform);

                if (i < _selectedOverlord.Skills.Count && _selectedOverlord.Skills[i].UserData.IsUnlocked)
                {
                    abilityInstance.Skill = _selectedOverlord.Skills[i].Prototype;
                }
                bool isDefault = skillIndex == i;
                abilityInstance.UpdateUIState(isDefault);
                _abilities.Add(abilityInstance);
            }

            _newOpenAbility = _abilities[skillIndex];
        }

        private void ClearSkillInfo()
        {
            foreach (AbilityViewItem abilityViewItem in _abilities)
            {
                abilityViewItem.Dispose();
            }

            _abilities.Clear();
        }

        public void Update()
        {
        }

        private void AbilityInstanceOnSelectionChanged(AbilityViewItem ability)
        {
            //Frequent crash here
            //this null check will allow QA to
            //confirm if there's anything wrong when Skill == null
            //visually
            if (ability.Skill != null)
            {
                _skillName.text = ability.Skill.Title;
                _skillDescription.text = ability.Skill.Description;
            }
            else
            {
                Log.Warn("Error: Ability Skill was null " + ability);
            }
        }

        private void AnimationEventTriggeredHandler(string animationName)
        {

            switch (animationName)
            {
                case "SetGlow":
                    _newOpenAbility?.UpdateUIState();
                    break;
                case "SetSkill":
                    _newOpenAbility?.UpdateUIState();
                    break;
                case "HideEnd":
                    _uiManager.HidePopup<LevelUpPopup>();
                    break;
                default:
                    break;
            }
        }

        private void OnClickOkButtonEventHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _backgroundAnimator.SetTrigger(_hideParameterName);
            _containerAnimator.SetTrigger(_hideParameterName);
        }

        private class AbilityViewItem
        {
            public readonly GameObject SelfObject;

            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly GameObject _glowObj;

            private readonly Image _abilityIconImage;

            private readonly Transform _parentGameObject;

            private OverlordSkillPrototype _skill;

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

            public OverlordSkillPrototype Skill
            {
                get => _skill;
                set
                {
                    if (_skill == value)
                        return;

                    _skill = value;
                }
            }

            public void Dispose()
            {
                Object.Destroy(SelfObject);
            }


            public void UpdateUIState(bool isDefault = false)
            {
                _glowObj.SetActive(_isSelected);

                if (Skill != null && !isDefault)
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
