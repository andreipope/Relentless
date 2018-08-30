// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using UnityEngine;
using UnityEngine.UI;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Gameplay;
using TMPro;
using System.Collections.Generic;

namespace LoomNetwork.CZB
{
    public class CreditsPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IAppStateManager _stateManager;
        private ISoundManager _soundManager;
        private IDataManager _dataManager;

        private GameObject _selfPage;

        private Button _buttonBack;
        private ButtonShiftingContent _buttonThanks;

        private GameObject _creditListItemPrefab,
                           _creditSubsectionListItemPrefab;
        private ScrollRect _creditsListScroll;
        private List<CreditView> _credits;

        private Transform _panelCreditsList;

        private bool _isActive;

        public void Init()
        {
            _credits = new List<CreditView>();
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _creditListItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/CreditListItem");
            _creditSubsectionListItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/CreditSubSectionListItem");
        }

        public void Update()
        {
            if(_isActive)
            {
                _creditsListScroll.verticalNormalizedPosition -= Time.deltaTime / 70;
            }
        }

        public void Show()
        {
            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/CreditsPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _creditsListScroll = _selfPage.transform.Find("Panel_CreditsList").GetComponent<ScrollRect>();
            _panelCreditsList = _selfPage.transform.Find("Panel_CreditsList/Group");

            _buttonBack = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _buttonThanks = _selfPage.transform.Find("Panel_CreditsList/Group/ButtonSpace/Button_Thanks").GetComponent<ButtonShiftingContent>();

            _buttonBack.onClick.AddListener(BackButtonOnClickHandler);
            _buttonThanks.onClick.AddListener(BackButtonOnClickHandler);

            _isActive = true;
            _creditsListScroll.verticalNormalizedPosition = 1;

            FillCredits();
        }

        public void Hide()
        {
            _isActive = false;

            if (_selfPage == null)
                return;

            _selfPage.SetActive (false);
            GameObject.Destroy (_selfPage);
            _selfPage = null;
        }

        public void Dispose()
        {

        }

        private void FillCredits()
        {
            CreditView credit = null;
            CreditSubSectionView section = null;
            for (int i = 0; i < _dataManager.CachedCreditsData.creditsInfo.Count; i++)
            {
                if (i > 0)
                    section = new CreditSubSectionView(_creditSubsectionListItemPrefab, _panelCreditsList, _dataManager.CachedCreditsData.creditsInfo[i].subsectionType);
                for (int j = 0; j < _dataManager.CachedCreditsData.creditsInfo[i].credits.Count; j++)
                {
                    credit = new CreditView(_creditListItemPrefab, _panelCreditsList, _dataManager.CachedCreditsData.creditsInfo[i].credits[j].FullName, _dataManager.CachedCreditsData.creditsInfo[i].credits[j].Post);
                    _credits.Add(credit);
                }
            }
            _buttonThanks.transform.parent.SetAsLastSibling();
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        private void BackButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
        }
    }

    public class CreditView
    {
        public GameObject selfObject;
        public TextMeshProUGUI fullNameText;
        public TextMeshProUGUI postText;

        public CreditView() { }

        public CreditView(GameObject prefab, Transform parent, string name, string post)
        {
            selfObject = MonoBehaviour.Instantiate(prefab, parent, false);
            fullNameText = selfObject.transform.Find("Text_Name").GetComponent<TextMeshProUGUI>();
            postText = selfObject.transform.Find("Text_Post").GetComponent<TextMeshProUGUI>();
            fullNameText.text = name;
            if (String.IsNullOrWhiteSpace(name))
            {
                fullNameText.gameObject.SetActive(false);
            }
            postText.text = post;
        }
    }

    public class CreditSubSectionView
    {
        public GameObject selfObject;
        public TextMeshProUGUI sectionText;

        public CreditSubSectionView() { }

        public CreditSubSectionView(GameObject prefab, Transform parent, string section)
        {
            selfObject = MonoBehaviour.Instantiate(prefab, parent, false);
            sectionText = selfObject.transform.Find("Text_Section").GetComponent<TextMeshProUGUI>();
            sectionText.text = section;
        }
    }
}