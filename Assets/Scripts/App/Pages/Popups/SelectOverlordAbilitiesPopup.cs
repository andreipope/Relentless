using System;
using System.Collections.Generic;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class SelectOverlordAbilitiesPopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(CardInfoWithSearchPopup));

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private IDataManager _dataManager;

        private GameObject _abilityBarPrefab;

        private TextMeshProUGUI _selectedAbilitiesCount;

        private Button _buttonCancel;
        private Button _buttonSave;
        private Button _buttonContinue;

        private RectTransform _allAbilitiesContent;

        private Deck _deck;
        private bool _openRenamePopup;

        private List<AbilityBarUI> _abilitiesBar;

        public static Action<SkillId> OnSelectSkill;
        public static Action<Enumerators.Skill, Enumerators.Skill> OnSelectOverlordSkill;
        public static Action<Enumerators.Skill, Enumerators.Skill> OnSaveSelectedSkill;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
        }

        public void Show(object data)
        {
            if (data is object[] param)
            {
                Deck deck = (Deck) param[0];
                _deck = deck.Clone();

                _openRenamePopup = (bool) param[1];
            }


            Show();
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/OverlordAbilityPopup"),
                _uiManager.Canvas2.transform,
                false);

            _abilityBarPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/AbilityBarUI");

            _selectedAbilitiesCount = Self.transform.Find("Abilities/Panel_BG/Top_Panel/Header_Small/Abilities_Selected").GetComponent<TextMeshProUGUI>();

            _buttonCancel = Self.transform.Find("Abilities/Panel_BG/Bottom_Panel/Panel_Deco/Button_Cancel").GetComponent<Button>();
            _buttonCancel.onClick.AddListener(ButtonCancelHandler);

            _buttonContinue = Self.transform.Find("Abilities/Panel_BG/Bottom_Panel/Panel_Deco/Button_Continue").GetComponent<Button>();
            _buttonContinue.onClick.AddListener(ButtonContinueHandler);

            _buttonSave = Self.transform.Find("Abilities/Panel_BG/Bottom_Panel/Panel_Deco/Button_Save").GetComponent<Button>();
            _buttonSave.onClick.AddListener(ButtonSaveHandler);

            if (_deck.Id.Id == -1)
            {
                _buttonSave.gameObject.SetActive(false);
                _buttonContinue.gameObject.SetActive(true);
            }
            else
            {
                _buttonSave.gameObject.SetActive(true);
                _buttonContinue.gameObject.SetActive(false);
            }

            _allAbilitiesContent = Self.transform.Find("Abilities/Panel_BG/Ability_List/Element/Scroll View").GetComponent<ScrollRect>().content;

            ShowOverlordAbilities();
            ShowAbilitiesInDeck();
            ShowAbilityCount();

            OnSelectSkill += SelectSkill;
        }

        private void SelectSkill(SkillId skillId)
        {
            AbilityBarUI abilityBarUi = _abilitiesBar.Find(abilityBar => abilityBar.SkillId == skillId);

            bool isSkillUnLocked = DataUtilities.IsSkillLocked(_deck.OverlordId, skillId);
            if (!isSkillUnLocked)
            {
                return;
            }

            int selectedAbilityCount = _abilitiesBar.FindAll(abilityBar => abilityBar.IsSelected).Count;
            if (abilityBarUi.IsSelected)
            {
                abilityBarUi.SelectAbility(false);

                selectedAbilityCount -= 1;
            }
            else
            {
                if (selectedAbilityCount < 2)
                {
                    abilityBarUi.SelectAbility(true);

                    selectedAbilityCount += 1;
                }
            }

            UpdateCountDisplay(selectedAbilityCount);
        }

        private void ShowOverlordAbilities()
        {
            _abilitiesBar = new List<AbilityBarUI>();
            OverlordUserInstance overlordUserInstance = _dataManager.CachedOverlordData.GetOverlordById(_deck.OverlordId);
            if (overlordUserInstance == null)
            {
                Log.Error($"overlord with {_deck.OverlordId} id not found");
                return;
            }

            for (int i = 0; i < overlordUserInstance.Skills.Count; i++)
            {
                GameObject abilityBarObj = Object.Instantiate(_abilityBarPrefab, _allAbilitiesContent);

                AbilityBarUI abilityBarUi = new AbilityBarUI();
                abilityBarUi.Init(abilityBarObj);
                abilityBarUi.FillAbility(overlordUserInstance.Skills[i]);

                _abilitiesBar.Add(abilityBarUi);
            }
        }

        private void ShowAbilitiesInDeck()
        {
            if (_deck.PrimarySkill != Enumerators.Skill.NONE)
            {
                SkillId skillId = DataUtilities.GetSkillId(_deck.OverlordId, _deck.PrimarySkill);
                AbilityBarUI abilityBarUi = _abilitiesBar.Find(ability => ability.SkillId == skillId);
                abilityBarUi.SelectAbility(true);
            }

            if (_deck.SecondarySkill != Enumerators.Skill.NONE)
            {
                SkillId skillId = DataUtilities.GetSkillId(_deck.OverlordId, _deck.SecondarySkill);
                AbilityBarUI abilityBarUi = _abilitiesBar.Find(ability => ability.SkillId == skillId);
                abilityBarUi.SelectAbility(true);
            }
        }

        private void ShowAbilityCount()
        {
            int count = 0;
            if (_deck.PrimarySkill != Enumerators.Skill.NONE)
            {
                count += 1;
            }

            if (_deck.SecondarySkill != Enumerators.Skill.NONE)
            {
                count += 1;
            }

            UpdateCountDisplay(count);
        }

        private void UpdateCountDisplay(int abilityCount)
        {
            _selectedAbilitiesCount.text = "<color=#FFFF00>"+ abilityCount +"/2</color> ABILITIES SELECTED";
        }

        private void ButtonSaveHandler()
        {
            DataUtilities.PlayClickSound();
            GetSelectedAbilities();
            Hide();
        }

        private void ButtonContinueHandler()
        {
            DataUtilities.PlayClickSound();
            GetSelectedAbilities(false);
            Hide();

            if(_openRenamePopup)
                _uiManager.DrawPopup<RenamePopup>(_deck);
        }

        private void ButtonCancelHandler()
        {
            DataUtilities.PlayClickSound();
            Hide();
        }

        private void GetSelectedAbilities(bool save = true)
        {
            DataUtilities.PlayClickSound();

            Enumerators.Skill primarySkill = Enumerators.Skill.NONE;
            Enumerators.Skill secondarySkill = Enumerators.Skill.NONE;

            List<AbilityBarUI> abilityBarUis = _abilitiesBar.FindAll(ability => ability.IsSelected);
            if (abilityBarUis.Count > 1)
            {
                primarySkill = DataUtilities.GetSkill(_deck.OverlordId, abilityBarUis[0].SkillId);
                secondarySkill = DataUtilities.GetSkill(_deck.OverlordId, abilityBarUis[1].SkillId);
            }
            else if (abilityBarUis.Count == 1)
            {
                primarySkill = DataUtilities.GetSkill(_deck.OverlordId, abilityBarUis[0].SkillId);
            }

            if (save)
            {
                SaveAbilities(primarySkill, secondarySkill);
            }
            else
            {
                OnSelectOverlordSkill?.Invoke(primarySkill, secondarySkill);
            }
        }

        private void SaveAbilities(Enumerators.Skill primarySkill, Enumerators.Skill secondarySkill)
        {
            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
            deckGeneratorController.FinishEditDeck += FinishEditSelectedAbilities;

            _deck.PrimarySkill = primarySkill;
            _deck.SecondarySkill = secondarySkill;

            deckGeneratorController.ProcessEditDeck(_deck);
        }

        private void FinishEditSelectedAbilities(bool success, Deck deck)
        {
            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
            deckGeneratorController.FinishEditDeck -= FinishEditSelectedAbilities;

            OnSaveSelectedSkill?.Invoke(deck.PrimarySkill, deck.SecondarySkill);
        }

        public void Hide()
        {
            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;

            OnSelectSkill -= SelectSkill;
        }

        public void Update()
        {

        }

        public void Dispose()
        {

        }

        public void SetMainPriority()
        {

        }
    }
}


