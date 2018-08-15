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
    public class OverlordAbilityTooltipPopup : IUIPopup
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
        private Image _abilityIconImage;
        private TextMeshProUGUI _abilityNameText;
        private TextMeshProUGUI _abilityDescriptionText;

        public void Init() {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager> ();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/OverlordAbilityTooltipPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas3.transform, false);

            _abilityIconImage = _selfPage.transform.Find("AbilityIcon/Image").GetComponent<Image>();
            _abilityNameText = _selfPage.transform.Find("AbilityName").GetComponent<TextMeshProUGUI>();
            _abilityDescriptionText = _selfPage.transform.Find("AbilityDescription").GetComponent<TextMeshProUGUI>();

            Hide();
        }

        public void Show() {
            throw new NotImplementedException();
        }

        public void Show(object data) {
            HeroSkill skill = (HeroSkill) data;
            _selfPage.SetActive(true);

            _abilityIconImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/Icons/" + skill.iconPath);
            _abilityNameText.text = skill.title;
            _abilityDescriptionText.text = skill.description;
        }

        public void Hide() {
            _selfPage.SetActive(false);
        }

        public void Update() {
            if (Input.GetMouseButtonDown(0))
            {
                Hide();
            }
        }

        public void Dispose() {

        }

        public void SetMainPriority() {

        }
    }
}

