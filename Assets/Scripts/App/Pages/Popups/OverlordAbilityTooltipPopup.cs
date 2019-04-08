using System;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class OverlordAbilityTooltipPopup : IUIPopup
    {
        private TextMeshProUGUI _abilityDescriptionText;

        private Image _abilityIconImage;

        private TextMeshProUGUI _abilityNameText;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
        }

        public void Show()
        {
            throw new NotImplementedException();
        }

        public void Show(object data)
        {
            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/OverlordAbilityTooltipPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _abilityIconImage = Self.transform.Find("AbilityIcon/Image").GetComponent<Image>();
            _abilityNameText = Self.transform.Find("AbilityName").GetComponent<TextMeshProUGUI>();
            _abilityDescriptionText = Self.transform.Find("AbilityDescription").GetComponent<TextMeshProUGUI>();

            OverlordSkillData skill = (OverlordSkillData) data;

            _abilityIconImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/Icons/" + skill.IconPath);
            _abilityNameText.text = skill.Title;
            _abilityDescriptionText.text = skill.Description;
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
            if (Input.GetMouseButtonDown(0))
            {
                Hide();
            }
        }

        public void Dispose()
        {
        }

        public void SetMainPriority()
        {
        }
    }
}
