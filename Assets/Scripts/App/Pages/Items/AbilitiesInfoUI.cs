using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class AbilitiesInfoUI
    {
        private Image _overlordPrimarySkillImage;
        private Image _overlordSecondarySkillImage;

        private Button _buttonChange;

        private Deck _deck;
        private OverlordId _overlordId;

        public void Load(GameObject obj)
        {
            _overlordPrimarySkillImage = obj.transform.Find("Overlord_Skill_Primary/Image").GetComponent<Image>();
            _overlordSecondarySkillImage = obj.transform.Find("Overlord_Skill_Secondary/Image").GetComponent<Image>();

            //change ability
            _buttonChange = obj.transform.Find("Button_Change").GetComponent<Button>();
            _buttonChange.onClick.AddListener(ButtonChangeAbilityHandler);
        }

        private void ButtonChangeAbilityHandler()
        {
            GameClient.Get<IUIManager>().DrawPopup<ChampionAbilitiesPopup>(_deck);
            ChampionAbilitiesPopup.OnSaveSelectedSkill += OnSaveSelectedSkill;
        }

        public void ShowAbilities(Deck deck)
        {
            _deck = deck;
            _overlordPrimarySkillImage.sprite = DataUtilities.GetAbilityIcon(deck.OverlordId, deck.PrimarySkill);
            _overlordSecondarySkillImage.sprite = DataUtilities.GetAbilityIcon(deck.OverlordId, deck.SecondarySkill);
        }

        private void OnSaveSelectedSkill(Enumerators.Skill primarySkill, Enumerators.Skill secondarySkill)
        {
            if (_deck == null)
                return;

            _deck.PrimarySkill = primarySkill;
            _deck.SecondarySkill = secondarySkill;

            ShowAbilities(_deck);
        }

        public void Dispose()
        {
            ChampionAbilitiesPopup.OnSaveSelectedSkill -= OnSaveSelectedSkill;
        }
    }
}
