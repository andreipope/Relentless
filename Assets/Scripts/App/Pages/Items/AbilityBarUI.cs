
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityBarUI
{
    private GameObject _selfObj;

    private Image _abilityImage;

    private GameObject _lockedGroup;
    private GameObject _unlockedGroup;

    private TextMeshProUGUI _abilityName;
    private TextMeshProUGUI _abilityDescription;

    private ILoadObjectsManager _loadObjectsManager;

    private Button _buttonAbility;

    private Sprite _unSelectedAbilitySprite;
    private Sprite _selectedAbilitySprite;

    public SkillId SkillId;
    public bool IsSelected;


    public void Init(GameObject obj)
    {
        _selfObj = obj;
        _abilityImage = _selfObj.transform.Find("AbilityBar/Ability_Icon").GetComponent<Image>();

        _abilityName = _selfObj.transform.Find("AbilityBar/Unlock_Group/Ability_Name_BG/Ability_Name").GetComponent<TextMeshProUGUI>();
        _abilityDescription = _selfObj.transform.Find("AbilityBar/Unlock_Group/Ability_Description").GetComponent<TextMeshProUGUI>();

        _lockedGroup = _selfObj.transform.Find("AbilityBar/Lock_Group").gameObject;
        _unlockedGroup = _selfObj.transform.Find("AbilityBar/Unlock_Group").gameObject;

        _buttonAbility = _selfObj.transform.Find("AbilityBar").GetComponent<Button>();
        _buttonAbility.onClick.AddListener(ButtonAbilityHandler);


        _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        _selectedAbilitySprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ChooseAbility/selectability_selected");
        _unSelectedAbilitySprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ChooseAbility/selectability_idle");

        IsSelected = false;
    }

    private void ButtonAbilityHandler()
    {
        ChampionAbilitiesPopup.OnSelectSkill?.Invoke(SkillId);
    }

    public void SelectAbility(bool select)
    {
        IsSelected = select;
        _buttonAbility.image.sprite = IsSelected ? _selectedAbilitySprite : _unSelectedAbilitySprite;
        _abilityDescription.color = IsSelected ? Color.black : Color.white;
    }

    public void FillAbility(OverlordSkillUserInstance skill)
    {
        SkillId = skill.Prototype.Id;
        _abilityName.text = skill.Prototype.Title;
        _abilityDescription.text = skill.Prototype.Description;
        _abilityImage.sprite = DataUtilities.GetAbilityIcon(skill);

        if (skill.UserData.IsUnlocked)
        {
            _lockedGroup.SetActive(false);
            _unlockedGroup.SetActive(true);
        }
        else
        {
            _lockedGroup.SetActive(true);
            _unlockedGroup.SetActive(false);
        }
    }
}
