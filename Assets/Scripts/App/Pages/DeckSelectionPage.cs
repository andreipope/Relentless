// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using TMPro;
using LoomNetwork.CZB.Data;
using System;
using DG.Tweening;

namespace LoomNetwork.CZB
{
    public class DeckSelectionPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IDataManager _dataManager;
        private ISoundManager _soundManager;
        private IAppStateManager _appStateManager;
        private IMatchManager _matchManager;

        private GameObject _selfPage;

        private Button _collectionButton,
                       _backButton,
                       _battleButton,
                       _leftArrowButton,
                       _rightArrowButton,
					   _deleteButton,
					   _editButton;

		private TextMeshProUGUI _textDescription;
		private TextMeshProUGUI _textCardsCount;
		private Image _hordePicture;
		private Image _hordePictureGlow;
		private Image _hordeType;
		private Image _firstSkill;
		private Image _secondSkill;
		private Image _metalBoxBG;

        private Transform _containerOfDecks;

        private List<HordeDeckObject> _hordeDecks;
        private int _selectedDeck = -1;


        private int _leftDeckIndex = -1;
      //  private int _decksCount = 3;

        // new horde deck object
        private GameObject _newHordeDeckObject;
        private Button _newHordeDeckButton;


        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _matchManager = GameClient.Get<IMatchManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/DeckSelectionPage"), _uiManager.Canvas.transform, false);

            _collectionButton = _selfPage.transform.Find("Button_Collection").GetComponent<Button>();
            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _battleButton = _selfPage.transform.Find("Button_Battle").GetComponent<Button>();
            _leftArrowButton = _selfPage.transform.Find("Button_LeftArrow").GetComponent<Button>();
            _rightArrowButton = _selfPage.transform.Find("Button_RightArrow").GetComponent<Button>();

            _collectionButton.onClick.AddListener(CollectionButtonOnClickHandler);
            _backButton.onClick.AddListener(BackButtonOnClickHandler);
            _battleButton.onClick.AddListener(BattleButtonOnClickHandler);
            _leftArrowButton.onClick.AddListener(LeftArrowButtonOnClickHandler);
            _rightArrowButton.onClick.AddListener(RightArrowButtonOnClickHandler);

            _containerOfDecks = _selfPage.transform.Find("Panel_DecksContainer/Group");

			_editButton = _selfPage.transform.Find ("Panel_DecksContainer/MetalBox_Selection/Panel_SelectedHordeObjects/Button_Edit").GetComponent<Button> ();
			_deleteButton = _selfPage.transform.Find ("Panel_DecksContainer/MetalBox_Selection/Panel_SelectedHordeObjects/Button_Delete").GetComponent<Button> ();

			_editButton.onClick.AddListener(EditButtonOnClickHandler);
			_deleteButton.onClick.AddListener (DeleteButtonOnClickHandler);

			_textDescription = _selfPage.transform.Find ("Panel_DecksContainer/MetalBox_Selection/Panel_Description/Text_Description").GetComponent<TextMeshProUGUI>();
			_textCardsCount = _selfPage.transform.Find ("Panel_DecksContainer/MetalBox_Selection/Panel_DeckFillInfo/Text_CardsCount").GetComponent<TextMeshProUGUI>();
			_hordePicture = _selfPage.transform.Find ("Panel_DecksContainer/MetalBox_Selection/Panel_SelectedBlock/Image_HordePicture").GetComponent<Image>();
			_hordePictureGlow = _selfPage.transform.Find ("Panel_DecksContainer/MetalBox_Selection/Panel_SelectedBlock/Image_Glow").GetComponent<Image>();
			_hordeType = _selfPage.transform.Find ("Panel_DecksContainer/MetalBox_Selection/Panel_HordeType/Image").GetComponent<Image>();
			_firstSkill = _selfPage.transform.Find ("Panel_DecksContainer/MetalBox_Selection/Panel_SelectedHordeObjects/Image_FirstSkil/Image_Skill").GetComponent<Image>();
			_secondSkill = _selfPage.transform.Find ("Panel_DecksContainer/MetalBox_Selection/Panel_SelectedHordeObjects/Image_SecondSkil/Image_Skill").GetComponent<Image>();
			_metalBoxBG = _selfPage.transform.Find ("Panel_DecksContainer/MetalBox_Selection").GetComponent<Image> ();

            // new horde deck object
            _newHordeDeckObject = _containerOfDecks.transform.Find("Item_HordeSelectionNewHorde").gameObject;
            _newHordeDeckButton = _newHordeDeckObject.transform.Find("Image_BaackgroundGeneral").GetComponent<Button>();

            _newHordeDeckButton.onClick.AddListener(NewHordeDeckButtonOnClickHandler);

            _battleButton.interactable = false;

            Hide();
        }

        public void Update()
        {
        }

        public void Show()
        {
            //todod improve I guess
            _selectedDeck = _dataManager.CachedUserLocalData.lastSelectedDeckId;

			Debug.Log (_selectedDeck);

            LoadDeckObjects();
            _selfPage.SetActive(true);
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
            ResetHordeDecks();

            _leftDeckIndex = -1;
        }

        public void Dispose()
        {

        }


        private void FillHordeDecks()
        {
            ResetHordeDecks();
            _hordeDecks = new List<HordeDeckObject>();

            HordeDeckObject hordeDeck = null;
            for (int i = 0; i < _dataManager.CachedDecksData.decks.Count; i++)
            {
                int id = i;
                hordeDeck = new HordeDeckObject(_containerOfDecks,
                                                _dataManager.CachedDecksData.decks[i],
                                                _dataManager.CachedHeroesData.Heroes.Find(x => x.heroId == _dataManager.CachedDecksData.decks[i].heroId),
                                                id);
                //hordeDeck.HordeDeckSelectedEvent += HordeDeckSelectedEventHandler;
                //hordeDeck.DeleteDeckEvent += DeleteDeckEventHandler;

                _hordeDecks.Add(hordeDeck);
            }
        }

		private void FillDeckInfo () {
			HordeDeckObject deck = null;
			if (_selectedDeck != null) {
				foreach (HordeDeckObject item in _hordeDecks) {
					if (item.DeckId == _selectedDeck) {
						deck = item;
						break;
					}
				}
			}

			Debug.Log (deck);

			if (deck != null) {
				_textDescription.gameObject.SetActive (true);
				_textCardsCount.gameObject.SetActive (true);
				_hordePicture.gameObject.SetActive (true);
				_hordePictureGlow.gameObject.SetActive (true);
				_hordeType.gameObject.SetActive (true);
				_firstSkill.gameObject.SetActive (true);
				_secondSkill.gameObject.SetActive (true);
				_metalBoxBG.enabled = true;
				_editButton.gameObject.SetActive (true);
				_deleteButton.gameObject.SetActive (true);

				_textDescription.text = deck.SelfDeck.name;
				_textCardsCount.text = deck.SelfDeck.GetNumCards () + "/" + Constants.MAX_DECK_SIZE;
				_hordePicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite> ("Images/UI/ChooseHorde/hordeselect_deck_" + deck.SelfHero.element.ToLower ());
				_hordeType.sprite = _loadObjectsManager.GetObjectByPath<Sprite> ("Images/UI/ElementIcons/Icon_element_" + deck.SelfHero.element.ToLower ());
				_firstSkill.sprite = _loadObjectsManager.GetObjectByPath<Sprite> ("Images/HeroesIcons/heroability_" + deck.SelfHero.element.ToUpper () + "_" + deck.SelfHero.skills [deck.SelfHero.primarySkill].skill.ToLower ());
				_secondSkill.sprite = _loadObjectsManager.GetObjectByPath<Sprite> ("Images/HeroesIcons/heroability_" + deck.SelfHero.element.ToUpper () + "_" + deck.SelfHero.skills [deck.SelfHero.secondarySkill].skill.ToLower ());
			} else {
				_textDescription.gameObject.SetActive (false);
				_textCardsCount.gameObject.SetActive (false);
				_hordePicture.gameObject.SetActive (false);
				_hordePictureGlow.gameObject.SetActive (false);
				_hordeType.gameObject.SetActive (false);
				_firstSkill.gameObject.SetActive (false);
				_secondSkill.gameObject.SetActive (false);
				_metalBoxBG.enabled = false;
				_editButton.gameObject.SetActive (false);
				_deleteButton.gameObject.SetActive (false);
			}
		}

        private void ResetHordeDecks()
        {
            if (_hordeDecks != null)
            {
                foreach (var element in _hordeDecks)
                    element.Dispose();
                _hordeDecks.Clear();
                _hordeDecks = null;
            }
        }

        private void DeleteDeckEventHandler(HordeDeckObject deck)
        {
            _dataManager.CachedDecksData.decks.RemoveAt(deck.DeckId);
            _dataManager.CachedUserLocalData.lastSelectedDeckId = -1;
            _dataManager.SaveAllCache();

            LoadDeckObjects();
        }

        private void HordeDeckSelectedEventHandler(HordeDeckObject deck)
        {
            foreach (var element in _hordeDecks)
            {
                if (!element.Equals(deck))
                    element.Deselect();
                else
                    element.Select();
            }

            if (deck.SelfDeck.GetNumCards() < Constants.MAX_DECK_SIZE && !Constants.DEV_MODE)
            {
                _battleButton.interactable = false;
                //   OpenAlertDialog("You should have 30 cards inside your deck to use it for battle");
                // return;
            }
            else
                _battleButton.interactable = true;

            _selectedDeck = deck.DeckId;
            _dataManager.CachedUserLocalData.lastSelectedDeckId = _selectedDeck;
            _dataManager.SaveAllCache();
			CenterTheSelectedDeck ();
        }

        private void LoadDeckObjects()
        {
            FillHordeDecks();

            _newHordeDeckObject.transform.SetAsLastSibling();
            _newHordeDeckObject.SetActive(true);

            var deck = _hordeDecks.Find(x => x.DeckId == _dataManager.CachedUserLocalData.lastSelectedDeckId);
            if (deck != null)
                HordeDeckSelectedEventHandler(deck);

            CenterTheSelectedDeck();
        }

        private void CenterTheSelectedDeck()
        {
            if (_hordeDecks.Count <= 1)
                return;

            int index = _hordeDecks.IndexOf(_hordeDecks.Find(x => x.IsSelected));

            _leftDeckIndex = index - 1;
            if (_leftDeckIndex < -1)
                _leftDeckIndex = -1;

			Debug.Log ("Centering Deck index:");
			Debug.Log (_leftDeckIndex);

			FillDeckInfo ();
			DOTween.KillAll ();
			DOTween.To (() => _containerOfDecks.GetComponent<RectTransform> ().anchoredPosition, x => _containerOfDecks.GetComponent<RectTransform> ().anchoredPosition = x, (Vector2.left * _leftDeckIndex * 580f), 0.5f);
        }


        #region Buttons Handlers

        private void CollectionButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _appStateManager.ChangeAppState(Enumerators.AppState.COLLECTION);
        }

        private void BackButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _appStateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }

        private void BattleButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            _uiManager.GetPage<GameplayPage>().CurrentDeckId = _selectedDeck;

            _matchManager.FindMatch(Enumerators.MatchType.LOCAL);
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

        // new horde deck object
        private void NewHordeDeckButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            _uiManager.GetPage<DeckEditingPage>().CurrentDeckId = -1;

            _appStateManager.ChangeAppState(Enumerators.AppState.HERO_SELECTION);
        }

		private void DeleteButtonOnClickHandler()
		{
			HordeDeckObject deck = null;
			foreach (HordeDeckObject item in _hordeDecks) {
				if (item.DeckId == _selectedDeck) {
					deck = item;
					break;
				}
			}

			if (deck != null) {
				_soundManager.PlaySound (Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

				_uiManager.GetPopup<QuestionPopup> ().ConfirmationEvent += ConfirmDeleteDeckEventHandler;

				_uiManager.DrawPopup<QuestionPopup> ("Do you really wan't to delete " + deck.SelfDeck.name + "?");
			}
		}

		private void EditButtonOnClickHandler()
		{
			if (_selectedDeck != null) {
				_soundManager.PlaySound (Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

				_uiManager.GetPage<DeckEditingPage> ().CurrentDeckId = _selectedDeck;
				_appStateManager.ChangeAppState (Enumerators.AppState.DECK_EDITING);
			}
		}

		private void ConfirmDeleteDeckEventHandler(bool status)
		{
			_uiManager.GetPopup<QuestionPopup>().ConfirmationEvent -= ConfirmDeleteDeckEventHandler;

			foreach (HordeDeckObject item in _hordeDecks) {
				if (item.DeckId == _selectedDeck) {
					if (status) {
						DeleteDeckEventHandler (item);
						break;
					}
				}
			}
		}

        //private void BuyButtonHandler()
        //{
        //          GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        //          GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.SHOP);
        //      }
        //      private void OpenButtonHandler()
        //{
        //          GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        //          GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
        //      }

        #endregion

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        private void SwitchOverlordObject(int direction)
        {
            bool isChanged = false;

            if (_hordeDecks.Count <= 1)
                return;

            var oldIndex = _leftDeckIndex;
            _leftDeckIndex += direction;

			Debug.Log ("What Happens");
			Debug.Log (oldIndex);
			Debug.Log (_leftDeckIndex);

            if (_leftDeckIndex < -1)
                _leftDeckIndex = _hordeDecks.Count - 1;
            else if (_leftDeckIndex >= _hordeDecks.Count)
                _leftDeckIndex = -1;

			Debug.Log (_leftDeckIndex);
            if (oldIndex != _leftDeckIndex)
                isChanged = true;
			Debug.Log (isChanged);
            if (isChanged)
            {
				if (_leftDeckIndex + 1 < _hordeDecks.Count) {
					_selectedDeck = _hordeDecks [_leftDeckIndex + 1].DeckId;
					_dataManager.CachedUserLocalData.lastSelectedDeckId = _selectedDeck;
					_dataManager.SaveAllCache ();
				} else {
					_selectedDeck = 999999;
				}

				FillDeckInfo ();

				DOTween.KillAll ();
				DOTween.To (() => _containerOfDecks.GetComponent<RectTransform> ().anchoredPosition, x => _containerOfDecks.GetComponent<RectTransform> ().anchoredPosition = x, (Vector2.left * _leftDeckIndex * 580f), 0.5f).OnComplete (() => {
				});
                //_containerOfDecks.GetComponent<RectTransform>().anchoredPosition = (Vector2.left * _leftDeckIndex * 580f);
            }
        }
        public class HordeDeckObject
        {
            public event Action<HordeDeckObject> HordeDeckSelectedEvent;
            public event Action<HordeDeckObject> DeleteDeckEvent;

            private ILoadObjectsManager _loadObjectsManager;
            private IUIManager _uiManager;
            private IAppStateManager _appStateManager;
            private IDataManager _dataManager;
            private ISoundManager _soundManager;

            private GameObject _selfObject;

            //private Button _deleteButton,
            //               _editButton,
            //               _selectButton;

            //private Image _firstSkillImage,
            //              _secondSkillImage;

            private Image _setTypeIcon;
            private Image _hordePicture;

            //private GameObject _selectedDeckObjectBackground,
            //                   _selectedDeckObjectControl;

            private TextMeshProUGUI _descriptionText,
                                    _cardsInDeckCountText;

            public int DeckId { get; private set; }

            public Deck SelfDeck { get; private set; }
            public Hero SelfHero { get; private set; }

            public bool IsSelected { get; private set; }

            public HordeDeckObject(Transform parent, Deck deck, Hero hero, int id)
            {
                DeckId = id;
                SelfDeck = deck;
                SelfHero = hero;

                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                _uiManager = GameClient.Get<IUIManager>();
                _appStateManager = GameClient.Get<IAppStateManager>();
                _dataManager = GameClient.Get<IDataManager>();
                _soundManager = GameClient.Get<ISoundManager>();

                _selfObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/Item_HordeSelectionObject"), parent, false);

                //_selectButton = _selfObject.transform.Find("Button_Select").GetComponent<Button>();
                //_editButton = _selfObject.transform.Find("Panel_SelectedHordeObjects/Button_Edit").GetComponent<Button>();
                //_deleteButton = _selfObject.transform.Find("Panel_SelectedHordeObjects/Button_Delete").GetComponent<Button>();

                //_firstSkillImage = _selfObject.transform.Find("Panel_SelectedHordeObjects/Image_FirstSkil/Image_Skill").GetComponent<Image>();
                //_secondSkillImage = _selfObject.transform.Find("Panel_SelectedHordeObjects/Image_SecondSkil/Image_Skill").GetComponent<Image>();
                _setTypeIcon = _selfObject.transform.Find("Panel_HordeType/Image").GetComponent<Image>();
                _hordePicture = _selfObject.transform.Find("Image_HordePicture").GetComponent<Image>();

                _descriptionText = _selfObject.transform.Find("Panel_Description/Text_Description").GetComponent<TextMeshProUGUI>();
                _cardsInDeckCountText = _selfObject.transform.Find("Panel_DeckFillInfo/Text_CardsCount").GetComponent<TextMeshProUGUI>();

                //_selectedDeckObjectBackground = _selfObject.transform.Find("Panel_SelectedBlock").gameObject;
                //_selectedDeckObjectControl = _selfObject.transform.Find("Panel_SelectedHordeObjects").gameObject;

                _cardsInDeckCountText.text = SelfDeck.GetNumCards() + "/" + Constants.MAX_DECK_SIZE;
                _descriptionText.text = deck.name;

                _setTypeIcon.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ElementIcons/Icon_element_" + SelfHero.element.ToLower());
                _hordePicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ChooseHorde/hordeselect_deck_" + SelfHero.element.ToLower());

                //var skillPrimary = SelfHero.skills[SelfHero.primarySkill];
                //var skillSecondary = SelfHero.skills[SelfHero.secondarySkill];

                //_firstSkillImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/heroability_" + SelfHero.element.ToUpper() + "_" + skillPrimary.skill.ToLower());
                //_secondSkillImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/heroability_" + SelfHero.element.ToUpper() + "_" + skillSecondary.skill.ToLower());

                //_selectButton.onClick.AddListener(SelectButtonOnClickHandler);
                //_editButton.onClick.AddListener(EditButtonOnClickHandler);
                //_deleteButton.onClick.AddListener(DeleteButtonOnclickHandler);
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

                //_selectedDeckObjectBackground.SetActive(IsSelected);
                //_selectedDeckObjectControl.SetActive(IsSelected);
            }

            public void Deselect()
            {
                if (!IsSelected)
                    return;

                IsSelected = false;

                //_selectedDeckObjectBackground.SetActive(IsSelected);
                //_selectedDeckObjectControl.SetActive(IsSelected);
            }

            private void SelectButtonOnClickHandler()
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

                HordeDeckSelectedEvent?.Invoke(this);
            }
        }
    }
}