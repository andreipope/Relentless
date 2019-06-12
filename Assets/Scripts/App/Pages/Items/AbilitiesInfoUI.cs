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

        public void Load(GameObject obj)
        {
            _overlordPrimarySkillImage = obj.transform.Find("Overlord_Skill_Primary/Image").GetComponent<Image>();
            _overlordSecondarySkillImage = obj.transform.Find("Overlord_Skill_Secondary/Image").GetComponent<Image>();

            _buttonChange = obj.transform.Find("Button_Change").GetComponent<Button>();
            _buttonChange.onClick.AddListener(ButtonChangeHandler);
        }

        private void ButtonChangeHandler()
        {
            Debug.LogError("Button change handler");
        }

        public void ShowAbilities(OverlordId overlordId, Enumerators.Skill primarySkill, Enumerators.Skill secondarySkill)
        {
            _overlordPrimarySkillImage.sprite = DataUtilities.GetAbilityIcon(overlordId, primarySkill);
            _overlordSecondarySkillImage.sprite = DataUtilities.GetAbilityIcon(overlordId, secondarySkill);
        }
    }
}
