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

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
		private IDataManager _dataManager;
        private GameObject _selfPage;

		private ButtonShiftingContent _continueButton;
		private ButtonShiftingContent _buyButton;
		private GameObject _abilitiesGroup;
		private TextMeshProUGUI _title;
		private TextMeshProUGUI _skillName;
		private TextMeshProUGUI _skillDescription;
		private Image _heroImage;
		private Button _firstSkill;
		private Button _secondSkill;
		private Hero _heroData;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
			_dataManager = GameClient.Get<IDataManager> ();

			_selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/OverlordAbilityPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas3.transform, false);

			_continueButton = _selfPage.transform.Find("Button_Continue").GetComponent<ButtonShiftingContent>();
			_continueButton.onClick.AddListener(CloseButtonHandler);

			_title = _selfPage.transform.Find("Title").GetComponent<TextMeshProUGUI>();
			_skillName = _selfPage.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
			_skillDescription = _selfPage.transform.Find("SkillDescription").GetComponent<TextMeshProUGUI>();

			_abilitiesGroup = _selfPage.transform.Find ("Abilities").gameObject;

			_heroImage = _selfPage.transform.Find("HeroImage").GetComponent<Image>();

			_firstSkill = _abilitiesGroup.transform.GetChild (0).GetChild (2).GetComponent<Button> ();
			_secondSkill = _abilitiesGroup.transform.GetChild (1).GetChild (2).GetComponent<Button> ();

			_firstSkill.onClick.AddListener(delegate {
				SkillSelectOnClickHandler (_firstSkill);
			});

			_secondSkill.onClick.AddListener(delegate {
				SkillSelectOnClickHandler (_secondSkill);
			});

            Hide();
        }
			
		public void Dispose()
		{
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
			SkillSelectOnClickHandler (_firstSkill);
            Show();
        }

        public void Update()
        {

        }

		private void SkillSelectOnClickHandler (Button _skillButton) {
			HeroSkill skill = _heroData.skills [_skillButton.transform.parent.GetSiblingIndex()];
			_skillName.text = skill.title;
			_skillDescription.text = skill.description;

			for (int i = 0; i < _abilitiesGroup.transform.childCount; i++) {
				Transform item = _abilitiesGroup.transform.GetChild (i);
				if (item == _skillButton.transform.parent) {
					item.GetChild (0).gameObject.SetActive (true);
				} else {
					item.GetChild (0).gameObject.SetActive (false);
				}
			}
		}

		private void FillInfo (Hero heroData) {
			_heroData = heroData;
			_heroImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite> ("Images/Heroes/hero_" + heroData.element.ToLower ());
			_heroImage.SetNativeSize ();
			_abilitiesGroup.transform.GetChild(0).transform.GetChild(2).GetComponent<Image>().sprite = _loadObjectsManager.GetObjectByPath<Sprite> ("Images/UI/ICons/" + heroData.skills[0].iconPath);
			_abilitiesGroup.transform.GetChild(1).transform.GetChild(2).GetComponent<Image>().sprite = _loadObjectsManager.GetObjectByPath<Sprite> ("Images/UI/ICons/" + heroData.skills[1].iconPath);
		}
    }
}