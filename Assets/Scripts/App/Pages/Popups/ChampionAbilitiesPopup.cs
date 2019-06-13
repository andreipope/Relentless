using System;
using System.Collections.Generic;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ChampionAbilitiesPopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(CardInfoWithSearchPopup));

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private IDataManager _dataManager;

        private GameObject _abilityBarPrefab;

        private Button _buttonCancel;
        private Button _buttonSave;

        private RectTransform _allAbilitiesContent;

        private Deck _deck;

        private List<AbilityBarUI> _abilitiesBar;

        public static Action<SkillId> OnSelectSkill;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
        }

        public void Show(object data)
        {
            Deck deck = (Deck)data;
            _deck = deck.Clone();
            Show();
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/ChampionAbilityPopup"),
                _uiManager.Canvas2.transform,
                false);

            _abilityBarPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/AbilityBarUI");

            _buttonCancel = Self.transform.Find("Abilities/Panel_BG/Bottom_Panel/Button_Cancel").GetComponent<Button>();
            _buttonCancel.onClick.AddListener(ButtonCancelHandler);

            _buttonSave = Self.transform.Find("Abilities/Panel_BG/Bottom_Panel/Button_Save").GetComponent<Button>();
            _buttonSave.onClick.AddListener(ButtonSaveHandler);

            _allAbilitiesContent = Self.transform.Find("Abilities/Panel_BG/Ability_List/Element/Scroll View").GetComponent<ScrollRect>().content;

            ShowOverlordAbilities();

            OnSelectSkill += SelectSkill;
        }

        private void SelectSkill(SkillId skillId)
        {
            AbilityBarUI abilityBarUi = _abilitiesBar.Find(abilityBar => abilityBar.SelectedSkillId == skillId);

            bool isSkillUnLocked = DataUtilities.IsSkillLocked(_deck.OverlordId, skillId);
            if (!isSkillUnLocked)
            {
                return;
            }

            if (abilityBarUi.IsSelected)
            {
                abilityBarUi.SelectAbility(false);
            }
            else
            {
                if (abilityBarUi.SelectedSkill == Enumerators.Skill.NONE)
                {

                    abilityBarUi.SelectAbility(true);

                    if (_deck.PrimarySkill == Enumerators.Skill.NONE)
                    {
                        //_deck.PrimarySkill =
                    }
                    else if (_deck.SecondarySkill == Enumerators.Skill.NONE)
                    {
                        //_deck.SecondarySkill =
                    }
                }
            }
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

        private void ButtonSaveHandler()
        {
            //TODO: save the change abilities
            SaveAbilities();

            Hide();
        }

        private void ButtonCancelHandler()
        {
            Hide();
        }

        private void SaveAbilities()
        {

        }

        public void Hide()
        {
            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
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


