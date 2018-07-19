// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



﻿using UnityEngine;
using UnityEngine.UI;
using LoomNetwork.CZB.Common;
using TMPro;
using LoomNetwork.CZB.Data;
using System;
using System.Collections.Generic;

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
        private List<OverlordObject> _overlordObjects;

        private Transform _overlordsContainer;

        private GameObject _rightContentObject;

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
            _rightContentObject = _selfPage.transform.Find("Panel_OverlordContent/Panel_OverlordInfo").gameObject;

            Hide();
        }

        public void Update()
        {
        }

        public void Show()
        {
            FillOverlordObjects();
            _selfPage.SetActive(true);
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
            _leftHeroIndex = 0;
            ResetOverlordObjects();
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
            SwitchOverlordObject(-1);
        }

        private void RightArrowButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            SwitchOverlordObject(1);
        }

        #endregion

        private void FillOverlordObjects()
        {
            ResetOverlordObjects();

            _overlordObjects = new List<OverlordObject>();

            OverlordObject current = null;
            foreach (var hero in _dataManager.CachedHeroesData.heroes)
            {
                current = new OverlordObject(_overlordsContainer, _rightContentObject, hero);
                current.OverlordObjectSelectedEvent += OverlordObjectSelectedEventHandler;
                _overlordObjects.Add(current);
            }

            _overlordObjects[0].Select();
        }

        private void OverlordObjectSelectedEventHandler(OverlordObject overlordObject)
        {
            foreach(var item in _overlordObjects)
            {
                if (!item.Equals(overlordObject))
                    item.Deselect();
                else
                    item.Select();
            }

            _currentOverlordObject = overlordObject;
        }

        private void SwitchOverlordObject(int direction)
        {
            bool isChanged = false;

            var oldIndex = _leftHeroIndex;
            _leftHeroIndex += direction;

            if (_leftHeroIndex < 0)
                _leftHeroIndex = _overlordObjects.Count - 1;
            else if (_leftHeroIndex >= _overlordObjects.Count - 1)
                _leftHeroIndex = 0;

            if (oldIndex != _leftHeroIndex)
                isChanged = true;

            if (isChanged)
            {
                _overlordsContainer.GetComponent<RectTransform>().anchoredPosition += Vector2.left * direction * 735;
                _overlordObjects[_leftHeroIndex].Select();
            }    
        }

        private void ResetOverlordObjects()
        {
            if(_overlordObjects != null)
            {
                foreach (var item in _overlordObjects)
                    item.Dispose();
                _overlordObjects.Clear();
                _overlordObjects = null;
            }
        }

        public class OverlordObject
        {
            public event Action<OverlordObject> OverlordObjectSelectedEvent;

            private ILoadObjectsManager _loadObjectsManager;

            private GameObject _selfObject;

            private GameObject _highlightObject,
                               _grayoutObject,
                               _glowObject,
                               _rightContentObject;

            private Image _overlordPicture;
            private Image _elementIcon;

            private Sprite _elementIconSprite;

            private TextMeshProUGUI _overlordNameText,
                                    _overlordDescriptionText,
                                    _overlordShortDescription;

            public Hero SelfHero { get; private set; }

            public bool IsSelected { get; private set; }

            public OverlordObject(Transform parent, GameObject rightContentObject, Hero hero)
            {
                SelfHero = hero;

                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                _selfObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/Item_OverlordSelectionOverlord"), parent, false);

                _highlightObject = _selfObject.transform.Find("Image_Highlight").gameObject;
                _glowObject = _selfObject.transform.Find("Image_Glow").gameObject;
                _grayoutObject = _selfObject.transform.Find("Image_Grayout").gameObject;
                _rightContentObject = rightContentObject; // _selfObject.transform.Find("Panel_RightContent").gameObject;

                _overlordPicture = _selfObject.transform.Find("Image_OverlordPicture").GetComponent<Image>();
                _elementIcon = _rightContentObject.transform.Find("Panel_Type/Image_ElementType").GetComponent<Image>();

                _overlordNameText = _rightContentObject.transform.Find("Text_OverlordName").GetComponent<TextMeshProUGUI>();
                _overlordDescriptionText = _rightContentObject.transform.Find("Text_LongDescription").GetComponent<TextMeshProUGUI>();
                _overlordShortDescription = _rightContentObject.transform.Find("Text_ShortDescription").GetComponent<TextMeshProUGUI>();

                _overlordPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ChooseOverlord/portrait_" + SelfHero.element.ToLower() + "_hero");
                _elementIconSprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ElementIcons/Icon_element_" + SelfHero.element.ToLower());

                //string[] split = SelfHero.name.Split(',');

                //_overlordNameText.text = split[0];

                //if (split.Length > 1)
                //    _overlordShortDescription.text = split[1].Substring(1);

                //_overlordDescriptionText.text = SelfHero.name;

                Deselect(true);
            }

            public void Dispose()
            {
                MonoBehaviour.Destroy(_selfObject);
            }

            public void Select()
            {
                if (IsSelected)
                    return;

                IsSelected = true;

                string[] split = SelfHero.name.Split(',');

                _overlordNameText.text = split[0];

                if (split.Length > 1)
                    _overlordShortDescription.text = split[1].Substring(1);

                _elementIcon.sprite = _elementIconSprite;

                _highlightObject.SetActive(true);
                _grayoutObject.SetActive(false);
                _glowObject.SetActive(true);
            //    _rightContentObject.SetActive(true);

                OverlordObjectSelectedEvent?.Invoke(this);
            }

            public void Deselect(bool immediately = false)
            {
                if (!IsSelected && !immediately)
                    return;

                IsSelected = false;

                _highlightObject.SetActive(false);
                _grayoutObject.SetActive(true);
                _glowObject.SetActive(false);
            //    _rightContentObject.SetActive(false);
            }
        }
    }
}     