// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using System.Collections.Generic;
using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class HeroSelectionPage : IUIElement
    {
        private const float SCROLL_ANIMATION_DURATION = 0.5f;

        private const int LOOP_START_FAKE_HERO_COUNT = 1;

        private const int LOOP_END_FAKE_HERO_COUNT = 2;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private ISoundManager _soundManager;

        private IAppStateManager _appStateManager;

        private GameObject _selfPage;

        private Button _backButton, _continueButton, _leftArrowButton, _rightArrowButton;

        private OverlordObject _currentOverlordObject;

        private List<OverlordObject> _overlordObjects;

        private List<OverlordObject> _loopFakeOverlordObjects;

        private HorizontalLayoutGroup _overlordsContainer;

        private GameObject _rightContentObject;

        private int _selectedHeroIndex;

        private Sequence _heroSelectScrollSequence;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/HeroSelectionPage"), _uiManager.Canvas.transform, false);

            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _continueButton = _selfPage.transform.Find("Button_Continue").GetComponent<Button>();
            _leftArrowButton = _selfPage.transform.Find("Button_LeftArrow").GetComponent<Button>();
            _rightArrowButton = _selfPage.transform.Find("Button_RightArrow").GetComponent<Button>();

            _backButton.onClick.AddListener(BackButtonOnClickHandler);
            _continueButton.onClick.AddListener(ContinueButtonOnClickHandler);
            _leftArrowButton.onClick.AddListener(LeftArrowButtonOnClickHandler);
            _rightArrowButton.onClick.AddListener(RightArrowButtonOnClickHandler);

            _overlordsContainer = _selfPage.transform.Find("Panel_OverlordContent/Group").GetComponent<HorizontalLayoutGroup>();
            _rightContentObject = _selfPage.transform.Find("Panel_OverlordContent/Panel_OverlordInfo").gameObject;

            FillOverlordObjects();
            SetSelectedHeroIndexAndUpdateScrollPosition(0, false, force: true);
        }

        public void Hide()
        {
            if (_selfPage == null)

                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;

            /*SetSelectedHeroIndexAndUpdateScrollPosition(0, false);
            ResetOverlordObjects();*/
        }

        public void Dispose()
        {
            _heroSelectScrollSequence?.Kill();
        }

        private bool SetSelectedHeroIndexAndUpdateScrollPosition(int heroIndex, bool animateTransition, bool selectOverlordObject = true, bool force = false)
        {
            if (!force && (heroIndex == _selectedHeroIndex))
            {
                return false;
            }

            _selectedHeroIndex = heroIndex;

            RectTransform overlordContainerRectTransform = _overlordsContainer.GetComponent<RectTransform>();
            _heroSelectScrollSequence?.Kill();
            if (animateTransition)
            {
                _heroSelectScrollSequence = DOTween.Sequence();
                _heroSelectScrollSequence.Append(DOTween.To(() => overlordContainerRectTransform.anchoredPosition, v => overlordContainerRectTransform.anchoredPosition = v, CalculateOverlordContainerShiftForHeroIndex(_selectedHeroIndex), SCROLL_ANIMATION_DURATION)).AppendCallback(() => _heroSelectScrollSequence = null);
            } else
            {
                overlordContainerRectTransform.anchoredPosition = CalculateOverlordContainerShiftForHeroIndex(_selectedHeroIndex);
            }

            if (selectOverlordObject)
            {
                _overlordObjects[_selectedHeroIndex].Select(animateTransition);
            }

            return true;
        }

        private void FillOverlordObjects()
        {
            ResetOverlordObjects();

            _overlordObjects = new List<OverlordObject>();
            _loopFakeOverlordObjects = new List<OverlordObject>();

            for (int i = LOOP_START_FAKE_HERO_COUNT - 1; i >= 0; i--)
            {
                int index = _dataManager.CachedHeroesData.heroes.Count - i - 1;
                Hero hero = _dataManager.CachedHeroesData.heroes[index];
                OverlordObject current = new OverlordObject(_overlordsContainer.GetComponent<RectTransform>(), _rightContentObject, hero);
                current.SelfObject.name += $" #{index}";
                _loopFakeOverlordObjects.Add(current);
            }

            for (int i = 0; i < _dataManager.CachedHeroesData.heroes.Count; i++)
            {
                Hero hero = _dataManager.CachedHeroesData.heroes[i];
                OverlordObject current = new OverlordObject(_overlordsContainer.GetComponent<RectTransform>(), _rightContentObject, hero);
                current.SelfObject.name += $" #{i}";
                current.OverlordObjectSelectedEvent += OverlordObjectSelectedEventHandler;
                _overlordObjects.Add(current);
            }

            for (int i = 0; i < LOOP_END_FAKE_HERO_COUNT; i++)
            {
                Hero hero = _dataManager.CachedHeroesData.heroes[i];
                OverlordObject current = new OverlordObject(_overlordsContainer.GetComponent<RectTransform>(), _rightContentObject, hero);
                current.SelfObject.name += $" #{i}";
                _loopFakeOverlordObjects.Add(current);
            }

            _overlordObjects[0].Select(false);
        }

        private void OverlordObjectSelectedEventHandler(OverlordObject overlordObject)
        {
            foreach (OverlordObject item in _overlordObjects)
            {
                if (!item.Equals(overlordObject))
                {
                    item.Deselect();
                } else
                {
                    item.Select();
                }
            }

            _currentOverlordObject = overlordObject;
        }

        private void SwitchOverlordObject(int direction)
        {
            int newIndex = _selectedHeroIndex;
            newIndex += direction;

            if (newIndex < 0)
            {
                SetSelectedHeroIndexAndUpdateScrollPosition(_overlordObjects.Count, false, false);
                SetSelectedHeroIndexAndUpdateScrollPosition(_overlordObjects.Count - 1, true);
                _loopFakeOverlordObjects[LOOP_START_FAKE_HERO_COUNT].Deselect(force: true);
            } else if (newIndex >= _overlordObjects.Count)
            {
                SetSelectedHeroIndexAndUpdateScrollPosition(-1, false, false);
                SetSelectedHeroIndexAndUpdateScrollPosition(0, true);
                _loopFakeOverlordObjects[LOOP_START_FAKE_HERO_COUNT - 1].Deselect(force: true);
            } else
            {
                SetSelectedHeroIndexAndUpdateScrollPosition(newIndex, true);
            }
        }

        private Vector2 CalculateOverlordContainerShiftForHeroIndex(int heroIndex)
        {
            return Vector2.left * (heroIndex + LOOP_START_FAKE_HERO_COUNT) * _overlordsContainer.spacing;
        }

        private void ResetOverlordObjects()
        {
            if (_overlordObjects != null)
            {
                foreach (OverlordObject item in _overlordObjects)
                {
                    item.Dispose();
                }

                _overlordObjects.Clear();
                _overlordObjects = null;

                foreach (OverlordObject item in _loopFakeOverlordObjects)
                {
                    item.Dispose();
                }

                _loopFakeOverlordObjects.Clear();
                _loopFakeOverlordObjects = null;
            }
        }

        public class OverlordObject
        {
            public event Action<OverlordObject> OverlordObjectSelectedEvent;

            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly Image _highlightImage;

            private readonly Image _glowImage;

            private readonly Image _overlordPicture;

            private readonly Image _overlordPictureGray;

            private readonly Image _elementIcon;

            private readonly Sprite _overlordPictureSprite;

            private readonly Sprite _elementIconSprite;

            private readonly TextMeshProUGUI _overlordNameText;

            private readonly TextMeshProUGUI _overlordDescriptionText;

            private readonly TextMeshProUGUI _overlordShortDescription;

            private Sequence _stateChangeSequence;

            public OverlordObject(Transform parent, GameObject informationPanelObject, Hero hero)
            {
                SelfHero = hero;

                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                SelfObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/Item_OverlordSelectionOverlord"), parent, false);
                SelfObject.gameObject.name = hero.FullName;

                _highlightImage = SelfObject.transform.Find("Image_Highlight").gameObject.GetComponent<Image>();
                _glowImage = SelfObject.transform.Find("Image_Glow").gameObject.GetComponent<Image>();

                _overlordPicture = SelfObject.transform.Find("Image_OverlordPicture").GetComponent<Image>();
                _overlordPictureGray = SelfObject.transform.Find("Image_OverlordPictureGray").GetComponent<Image>();
                _elementIcon = informationPanelObject.transform.Find("Panel_Type/Image_ElementType").GetComponent<Image>();

                _overlordNameText = informationPanelObject.transform.Find("Text_OverlordName").GetComponent<TextMeshProUGUI>();
                _overlordDescriptionText = informationPanelObject.transform.Find("Text_LongDescription").GetComponent<TextMeshProUGUI>();
                _overlordShortDescription = informationPanelObject.transform.Find("Text_ShortDescription").GetComponent<TextMeshProUGUI>();

                _overlordPictureSprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ChooseOverlord/portrait_" + SelfHero.element.ToLower() + "_hero");
                _overlordPictureGray.sprite = _overlordPicture.sprite = _overlordPictureSprite;
                _elementIconSprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ElementIcons/Icon_element_" + SelfHero.element.ToLower());

                _overlordPictureGray.sprite = _overlordPicture.sprite = _overlordPictureSprite;

                Deselect(false, true);
            }

            public GameObject SelfObject { get; }

            public Hero SelfHero { get; }

            public bool IsSelected { get; private set; }

            public void Dispose()
            {
                Object.Destroy(SelfObject);
            }

            public void Select(bool animateTransition = true)
            {
                if (IsSelected)

                    return;

                IsSelected = true;

                SelfObject.gameObject.name = SelfHero.name + " (Selected)";

                _overlordNameText.text = SelfHero.name;
                _overlordDescriptionText.text = SelfHero.longDescription;
                _overlordShortDescription.text = SelfHero.shortDescription;

                _elementIcon.sprite = _elementIconSprite;

                SetUIActiveState(true, animateTransition, false);

                OverlordObjectSelectedEvent?.Invoke(this);
            }

            public void Deselect(bool animateTransition = true, bool force = false)
            {
                if (!IsSelected && !force)

                    return;

                IsSelected = false;

                SelfObject.gameObject.name = SelfHero.name;

                SetUIActiveState(false, animateTransition, force);
            }

            private void SetUIActiveState(bool active, bool animateTransition, bool forceResetAlpha)
            {
                float duration = animateTransition?SCROLL_ANIMATION_DURATION:0f;
                float targetAlpha = active?1f:0f;

                _stateChangeSequence?.Kill();
                _stateChangeSequence = DOTween.Sequence();

                Action<Image, bool> applyAnimation = (image, invert) =>
                {
                    image.gameObject.SetActive(true);
                    if (forceResetAlpha)
                    {
                        image.color = image.color.SetAlpha(invert?targetAlpha:1f - targetAlpha);
                    }

                    _stateChangeSequence.Insert(0f, image.DOColor(image.color.SetAlpha(invert?1f - targetAlpha:targetAlpha), duration).OnComplete(() => image.gameObject.SetActive(invert?!active:active)));
                };

                applyAnimation(_overlordPicture, false);
                applyAnimation(_overlordPictureGray, true);
                applyAnimation(_highlightImage, false);
                applyAnimation(_glowImage, false);
            }
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

            OverlordAbilitySelectionPopup.OnHidePopupEvent += AbilityPopupClosedEvent;
            _uiManager.DrawPopup<OverlordAbilitySelectionPopup>(_currentOverlordObject.SelfHero);
        }

        private void AbilityPopupClosedEvent()
        {
            OverlordAbilitySelectionPopup.OnHidePopupEvent -= AbilityPopupClosedEvent;
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
    }
}
