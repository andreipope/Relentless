// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using TMPro;
using System;
using System.Linq;
using DG.Tweening;
using LoomNetwork.CZB.Data;
using Deck = LoomNetwork.CZB.Data.Deck;
using Hero = LoomNetwork.CZB.Data.Hero;

namespace LoomNetwork.CZB
{
    public class HordeSelectionPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IDataManager _dataManager;
        private ISoundManager _soundManager;
        private IAppStateManager _appStateManager;
        private IMatchManager _matchManager;

        private GameObject _selfPage;

        private Button _backButton,
                        _battleButton,
                       _leftArrowButton,
                       _rightArrowButton,
					   _deleteButton,
					   _editButton;

        private ButtonShiftingContent _buttonArmy;

        private Image _firstSkill,
                      _secondSkill;

        private Transform _containerOfDecks, _hordeSelection;

        private List<HordeDeckObject> _hordeDecks;
        private int _selectedDeck = -1;
        private int _scrolledDeck = -1;

        private const int HORDE_ITEM_SPACE = 570,
                            HORDE_CONTAINER_XOFFSET = 60;
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

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/HordeSelectionPage"), _uiManager.Canvas.transform, false);

            _containerOfDecks = _selfPage.transform.Find("Panel_DecksContainer/Group");

            _buttonArmy = _selfPage.transform.Find("Button_Army").GetComponent<ButtonShiftingContent>();
            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _battleButton = _selfPage.transform.Find("Button_Battle").GetComponent<Button>();
            _leftArrowButton = _selfPage.transform.Find("Button_LeftArrow").GetComponent<Button>();
            _rightArrowButton = _selfPage.transform.Find("Button_RightArrow").GetComponent<Button>();


            _editButton = _selfPage.transform.Find("Panel_DecksContainer/Selection/Panel_SelectedHordeObjects/Button_Edit").GetComponent<Button>();
            _deleteButton = _selfPage.transform.Find("Panel_DecksContainer/Selection/Panel_SelectedHordeObjects/Button_Delete").GetComponent<Button>();
            _firstSkill = _selfPage.transform.Find("Panel_DecksContainer/Selection/Panel_SelectedHordeObjects/Image_FirstSkil/Image_Skill").GetComponent<Image>();
            _secondSkill = _selfPage.transform.Find("Panel_DecksContainer/Selection/Panel_SelectedHordeObjects/Image_SecondSkil/Image_Skill").GetComponent<Image>();


            _hordeSelection = _selfPage.transform.Find("Panel_DecksContainer/Selection");

            // new horde deck object
            _newHordeDeckObject = _containerOfDecks.transform.Find("Item_HordeSelectionNewHorde").gameObject;
            _newHordeDeckButton = _newHordeDeckObject.transform.Find("Image_BaackgroundGeneral").GetComponent<Button>();

            _buttonArmy.onClick.AddListener(CollectionButtonOnClickHandler);
            _backButton.onClick.AddListener(BackButtonOnClickHandler);
            _battleButton.onClick.AddListener(BattleButtonOnClickHandler);
            _leftArrowButton.onClick.AddListener(LeftArrowButtonOnClickHandler);
            _rightArrowButton.onClick.AddListener(RightArrowButtonOnClickHandler);

            _editButton.onClick.AddListener(EditButtonOnClickHandler);
            _deleteButton.onClick.AddListener(DeleteButtonOnClickHandler);

            _firstSkill.GetComponent<MultiPointerClickHandler>().SingleClickReceived += () => SkillButtonOnSingleClickHandler(0);
            _secondSkill.GetComponent<MultiPointerClickHandler>().SingleClickReceived += () => SkillButtonOnSingleClickHandler(1);

            _firstSkill.GetComponent<MultiPointerClickHandler>().DoubleClickReceived += () => SkillButtonOnDoubleClickHandler(0);
            _secondSkill.GetComponent<MultiPointerClickHandler>().DoubleClickReceived += () => SkillButtonOnDoubleClickHandler(1);

            _newHordeDeckButton.onClick.AddListener(NewHordeDeckButtonOnClickHandler);

            _battleButton.interactable = true;

            Hide();
        }

        public void Update()
        {
        }

        public void Show()
        {
            //todod improve I guess
            _selectedDeck = _dataManager.CachedUserLocalData.lastSelectedDeckId;
            _hordeSelection.gameObject.SetActive(false);

            LoadDeckObjects();
            _selfPage.SetActive(true);
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
            ResetHordeDecks();

            _scrolledDeck = -1;
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
                hordeDeck.HordeDeckSelectedEvent += HordeDeckSelectedEventHandler;
                hordeDeck.DeleteDeckEvent += DeleteDeckEventHandler;

                _hordeDecks.Add(hordeDeck);
            }
            _newHordeDeckObject.transform.localPosition = Vector3.right * HORDE_ITEM_SPACE * _hordeDecks.Count;
        }


        private void ResetHordeDecks()
        {
            _hordeSelection.SetParent(_containerOfDecks.parent, false);
            _hordeSelection.gameObject.SetActive(false);
            if (_hordeDecks != null)
            {
                foreach (var element in _hordeDecks)
                    element.Dispose();
                _hordeDecks.Clear();
                _hordeDecks = null;
            }
        }

        private async void DeleteDeckEventHandler(HordeDeckObject deck)
        {
            _dataManager.CachedDecksData.decks.RemoveAt(deck.DeckId);
            _dataManager.CachedUserLocalData.lastSelectedDeckId = -1;
            await _dataManager.SaveAllCache();
            
            LoadDeckObjects();

            try
            {
                await LoomManager.Instance.DeleteDeck(LoomManager.Instance.UserDataModel.UserId, deck.DeckId);
                CustomDebug.Log(" ====== Delete Deck Successfully ==== ");
            } catch (Exception e)
            {
                CustomDebug.Log("Result === " + e);
                OpenAlertDialog("Not able to Delete Deck: " + e.Message);
            }
        }

        private void HordeDeckSelectedEventHandler(HordeDeckObject deck)
        {
            if (_hordeSelection.gameObject.activeSelf)
            {
                var horde = _hordeDecks.Single((item) => item.DeckId == _selectedDeck);
                horde.Deselect();
            }
            deck.Select();

            _firstSkill.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/heroability_" + deck.SelfHero.element.ToUpper() + "_" + deck.SelfHero.skills[deck.SelfHero.primarySkill].skill.ToLower());
            _secondSkill.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/heroability_" + deck.SelfHero.element.ToUpper() + "_" + deck.SelfHero.skills[deck.SelfHero.secondarySkill].skill.ToLower());

            _hordeSelection.transform.SetParent(deck.selectionContainer, false);
            _hordeSelection.gameObject.SetActive(true);

            //if (deck.SelfDeck.GetNumCards() < Constants.MAX_DECK_SIZE && !Constants.DEV_MODE)
            //    _battleButton.interactable = false;
            //else
            //    _battleButton.interactable = true;

            _selectedDeck = deck.DeckId;
            _dataManager.CachedUserLocalData.lastSelectedDeckId = _selectedDeck;
            _dataManager.SaveAllCache();
            deck.selectionContainer.parent.SetAsLastSibling();
        }

        private void LoadDeckObjects()
        {
            FillHordeDecks();

            _newHordeDeckObject.transform.SetAsLastSibling();
            _newHordeDeckObject.SetActive(true);

            var deck = _hordeDecks.Find(x => x.DeckId == _dataManager.CachedUserLocalData.lastSelectedDeckId);
            if (deck != null)
                HordeDeckSelectedEventHandler(deck);

			if (_hordeDecks.Count > 0) {
				bool found = false;
				foreach (HordeDeckObject item in _hordeDecks) {
					if (item.DeckId == _selectedDeck) {
						found = true;
						break;
					}
				}
				if (!found) {
					_selectedDeck = _hordeDecks [0].DeckId;
				}
			} else {
				_selectedDeck = 9999;
			}

            CenterTheSelectedDeck();
        }

        private void CenterTheSelectedDeck()
        {
			if (_hordeDecks.Count < 1)
				return;

            _scrolledDeck = _hordeDecks.IndexOf(_hordeDecks.Find(x => x.IsSelected));

            if (_scrolledDeck < 2)
                _scrolledDeck = 0;
            else
                _scrolledDeck--;
 
            _containerOfDecks.transform.localPosition = new Vector3(HORDE_CONTAINER_XOFFSET - HORDE_ITEM_SPACE * _scrolledDeck, 420 , 0);
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

            if(_hordeDecks.Count == 0 || _selectedDeck == -1 || _selectedDeck >= _hordeDecks.Count || _hordeDecks[_selectedDeck].SelfDeck.GetNumCards() < Constants.MIN_DECK_SIZE && !Constants.DEV_MODE)
            {
                _uiManager.DrawPopup<WarningPopup>("Select a valid horde with " + Constants.MIN_DECK_SIZE + " cards.");
                return;
            }

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

        private void SkillButtonOnSingleClickHandler(int skillIndex)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            foreach (HordeDeckObject item in _hordeDecks) {
                if (item.DeckId == _selectedDeck)
                {
                    HeroSkill skill =
                        skillIndex == 0 ?
                            item.SelfHero.skills[item.SelfHero.primarySkill] :
                            item.SelfHero.skills[item.SelfHero.secondarySkill];

                    _uiManager.DrawPopup<OverlordAbilityTooltipPopup>(skill);
                    break;
                }
            }
        }

		private void SkillButtonOnDoubleClickHandler(int skillIndex)
		{
			_soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
			foreach (HordeDeckObject item in _hordeDecks) {
				if (item.DeckId == _selectedDeck) {
					_uiManager.DrawPopup<OverlordAbilitySelectionPopup> (item.SelfHero);
					break;
				}
			}
		}

        // new horde deck object
        private void NewHordeDeckButtonOnClickHandler()
        {
            if (ShowConnectionLostPopupIfNeeded())
                return;

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            _uiManager.GetPage<DeckEditingPage>().CurrentDeckId = -1;

            _appStateManager.ChangeAppState(Enumerators.AppState.HERO_SELECTION);
        }

		private void DeleteButtonOnClickHandler()
		{
		    if (ShowConnectionLostPopupIfNeeded())
		        return;

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

				_uiManager.DrawPopup<QuestionPopup> ("Do you really want to delete " + deck.SelfDeck.name + "?");
			}
		}

		private void EditButtonOnClickHandler()
		{
		    if (ShowConnectionLostPopupIfNeeded())
		        return;

			if (_selectedDeck != -1) {
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

            if (_hordeDecks.Count < 1)
                return;
            var oldIndex = _scrolledDeck;
            _scrolledDeck += direction;

            if (_scrolledDeck > _hordeDecks.Count - 2)
                _scrolledDeck = _hordeDecks.Count - 2;
            if (_scrolledDeck < 0)
                _scrolledDeck = 0;

            if (oldIndex != _scrolledDeck)
                _containerOfDecks.transform.localPosition = new Vector3(HORDE_CONTAINER_XOFFSET - HORDE_ITEM_SPACE * _scrolledDeck, 420, 0);
        }

        private bool ShowConnectionLostPopupIfNeeded() {
            if (LoomManager.Instance.IsConnected)
                return false;
            
            _uiManager.DrawPopup<WarningPopup>("Sorry, modifications are only available in online mode.");
            return true;
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

            private Image _setTypeIcon;
            private Image _hordePicture;

            private TextMeshProUGUI _descriptionText,
                                    _cardsInDeckCountText;

            private Button _buttonSelect;

            public int DeckId { get; private set; }

            public Deck SelfDeck { get; private set; }
            public Hero SelfHero { get; private set; }

            public bool IsSelected { get; private set; }

            public Transform selectionContainer;

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
                _selfObject.transform.localPosition = Vector3.right * HORDE_ITEM_SPACE * id;

                selectionContainer = _selfObject.transform.Find("SelectionContainer");

                _setTypeIcon = _selfObject.transform.Find("Panel_HordeType/Image").GetComponent<Image>();
                _hordePicture = _selfObject.transform.Find("Image_HordePicture").GetComponent<Image>();

                _descriptionText = _selfObject.transform.Find("Panel_Description/Text_Description").GetComponent<TextMeshProUGUI>();
                _cardsInDeckCountText = _selfObject.transform.Find("Panel_DeckFillInfo/Text_CardsCount").GetComponent<TextMeshProUGUI>();

                _buttonSelect = _selfObject.transform.Find("Button_Select").GetComponent<Button>();

                _cardsInDeckCountText.text = SelfDeck.GetNumCards() + "/" + Constants.MAX_DECK_SIZE;
                _descriptionText.text = deck.name;

                _setTypeIcon.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ElementIcons/Icon_element_" + SelfHero.element.ToLower());
                _hordePicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ChooseHorde/hordeselect_deck_" + SelfHero.element.ToLower());

                _buttonSelect.onClick.AddListener(SelectButtonOnClickHandler);
            }


            public void Dispose()
            {
                MonoBehaviour.Destroy(_selfObject);
            }

            public void Select()
            {
                if (IsSelected)
                    return;
                _buttonSelect.gameObject.SetActive(false);

                IsSelected = true;
            }

            public void Deselect()
            {
                if (!IsSelected)
                    return;
                _buttonSelect.gameObject.SetActive(true);

                IsSelected = false;
            }

            private void SelectButtonOnClickHandler()
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
               HordeDeckSelectedEvent?.Invoke(this);
            }
        }
    }
}