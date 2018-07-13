// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



﻿using UnityEngine;
using UnityEngine.UI;
using LoomNetwork.CZB.Common;
using TMPro;
using LoomNetwork.CZB.Data;
using System;

namespace LoomNetwork.CZB
{
    public class HeroSelectionPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IDataManager _dataManager;
        private ISoundManager _soundManager;
        private IAppStateManager _appStateManager;

        private GameObject _selfPage;

        private Button _backButton,
                       _continueButton,
                       _leftArrowButton,
                       _rightArrowButton;

        private OverlordObject _currentOverlordObject;

        private Transform _overlordsContainer;

        private int _leftHeroIndex;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/HeroSelectionPage"), _uiManager.Canvas.transform, false);


            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _continueButton = _selfPage.transform.Find("Button_Continue").GetComponent<Button>();
            _leftArrowButton = _selfPage.transform.Find("Button_LeftArrow").GetComponent<Button>();
            _rightArrowButton = _selfPage.transform.Find("Button_RightArrow").GetComponent<Button>();

            _backButton.onClick.AddListener(BackButtonOnClickHandler);
            _continueButton.onClick.AddListener(ContinueButtonOnClickHandler);
            _leftArrowButton.onClick.AddListener(LeftArrowButtonOnClickHandler);
            _rightArrowButton.onClick.AddListener(RightArrowButtonOnClickHandler);

            _overlordsContainer = _selfPage.transform.Find("Panel_OverlordContent/Group");

            Hide();
        }

        public void Update()
        {
        }

        public void Show()
        {
            SelectOverlordObject(_leftHeroIndex);
            _selfPage.SetActive(true);
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
            _leftHeroIndex = 0;
            ResetOverlordObject();
        }

        public void Dispose()
        {

        }

        #region Buttons Handlers

        private void BackButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _appStateManager.ChangeAppState(Enumerators.AppState.DECK_SELECTION);
        }

        private void ContinueButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            _uiManager.GetPage<DeckEditingPage>().CurrentHeroId = _currentOverlordObject.SelfHero.heroId;

            _appStateManager.ChangeAppState(Enumerators.AppState.DECK_EDITING);
        }

        private void LeftArrowButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            _leftHeroIndex = Mathf.Clamp(_leftHeroIndex - 1, 0, _dataManager.CachedHeroesData.heroes.Count - 1);
            SelectOverlordObject(_leftHeroIndex);
        }

        private void RightArrowButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            _leftHeroIndex = Mathf.Clamp(_leftHeroIndex + 1, 0, _dataManager.CachedHeroesData.heroes.Count - 1);
            SelectOverlordObject(_leftHeroIndex);
        }

        #endregion


        private void SelectOverlordObject(int index)
        {
            ResetOverlordObject();

            _currentOverlordObject = new OverlordObject(_overlordsContainer, _dataManager.CachedHeroesData.heroes[index]);
        }


        private void ResetOverlordObject()
        {
            if (_currentOverlordObject != null)
            {
                _currentOverlordObject.Dispose();
                _currentOverlordObject = null;
            }
        }

        public class OverlordObject
        {
            private ILoadObjectsManager _loadObjectsManager;

            private GameObject _selfObject;

            private Image _overlordPicture;
            private Image _elementIcon;

            private TextMeshProUGUI _overlordNameText,
                                    _overlordDescriptionText,
                                    _overlordShortDescription;

            public Hero SelfHero { get; private set; }

            public OverlordObject(Transform parent, Hero hero)
            {
                SelfHero = hero;

                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                _selfObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/Item_OverlordSelectionOverlord"), parent, false);

                _overlordPicture = _selfObject.transform.Find("Image_OverlordPicture").GetComponent<Image>();
                _elementIcon = _selfObject.transform.Find("Panel_Type/Image_ElementType").GetComponent<Image>();

                _overlordNameText = _selfObject.transform.Find("Text_OverlordName").GetComponent<TextMeshProUGUI>();
                _overlordDescriptionText = _selfObject.transform.Find("Text_LongDescription").GetComponent<TextMeshProUGUI>();
                _overlordShortDescription = _selfObject.transform.Find("Text_ShortDescription").GetComponent<TextMeshProUGUI>();

                _overlordPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ChooseOverlord/portrait_" + SelfHero.element.ToLower() + "_hero");
                _elementIcon.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ElementIcons/Icon_element_" + SelfHero.element.ToLower());

                string[] split = SelfHero.name.Split(',');

                _overlordNameText.text = split[0];

                if (split.Length > 1)
                    _overlordShortDescription.text = split[1].Substring(1);

                //_overlordDescriptionText.text = SelfHero.name;
            }

            public void Dispose()
            {
                MonoBehaviour.Destroy(_selfObject);
            }
        }
    }
}     