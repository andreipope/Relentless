// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using TMPro;
using System;
using Deck = LoomNetwork.CZB.Data.Deck;
using Hero = LoomNetwork.CZB.Data.Hero;

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
                       _rightArrowButton;

        private Transform _containerOfDecks;

        private List<HordeDeckObject> _hordeDecks;
        private int _selectedDeck = -1;


        private int _leftDeckIndex = 0;
        private int _decksCount = 3;

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
            _leftDeckIndex = Mathf.Clamp(_selectedDeck, 0, _dataManager.CachedDecksData.decks.Count);

            LoadDeckObjects(_leftDeckIndex);
            _selfPage.SetActive(true);
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
            ResetHordeDecks();

            _leftDeckIndex = 0;
        }

        public void Dispose()
        {

        }


        private void FillHordeDecks(int startIndex, int count)
        {
            ResetHordeDecks();
            _hordeDecks = new List<HordeDeckObject>();

            HordeDeckObject hordeDeck = null;
            for (int i = startIndex; i < _dataManager.CachedDecksData.decks.Count; i++)
            {
                if (i >= count || startIndex >= _dataManager.CachedDecksData.decks.Count)
                    break;
                int id = i;

                hordeDeck = new HordeDeckObject(_containerOfDecks,
                                                _dataManager.CachedDecksData.decks[i],
                                                _dataManager.CachedHeroesData.Heroes.Find(x => x.heroId == _dataManager.CachedDecksData.decks[i].heroId),
                                                id);
                hordeDeck.HordeDeckSelectedEvent += HordeDeckSelectedEventHandler;
                hordeDeck.DeleteDeckEvent += DeleteDeckEventHandler;

                _hordeDecks.Add(hordeDeck);
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

        private async void DeleteDeckEventHandler(HordeDeckObject deck)
        {
            var deckName = _dataManager.CachedDecksData.decks[deck.DeckId].name;
            _dataManager.CachedDecksData.decks.RemoveAt(deck.DeckId);
            _dataManager.CachedUserLocalData.lastSelectedDeckId = -1;
            _dataManager.SaveAllCache();

            LoadDeckObjects(_leftDeckIndex);
            
            Debug.Log("Deleting Deck with " + deck.DeckId);
            
            await LoomManager.Instance.DeleteDeck(LoomManager.UserId, deckName, result => 
            {
                if (!string.IsNullOrEmpty(result))
                {
                    Debug.Log("Result === " + result);
                    OpenAlertDialog("Not able to Delete Deck..");
                }
                else
                    Debug.Log(" ====== Delete Deck Successfully ==== ");
					
            });
            
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
        }

        private void LoadDeckObjects(int startIndex)
        {
            FillHordeDecks(startIndex, _decksCount);

            if (_hordeDecks.Count < 3 && _dataManager.CachedDecksData.decks.Count < Constants.MAX_DECKS_AT_ALL)
            {
                _newHordeDeckObject.transform.SetAsLastSibling();
                _newHordeDeckObject.SetActive(true);
            }
            else
                _newHordeDeckObject.SetActive(false);

            var deck = _hordeDecks.Find(x => x.DeckId == _dataManager.CachedUserLocalData.lastSelectedDeckId);
            if (deck != null)
                HordeDeckSelectedEventHandler(deck);
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

            _leftDeckIndex = Mathf.Clamp(_leftDeckIndex - 1, 0, _dataManager.CachedDecksData.decks.Count);
            LoadDeckObjects(_leftDeckIndex);
        }

        private void RightArrowButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            if (_dataManager.CachedDecksData.decks.Count < 3)
                _leftDeckIndex = 0;
            else
                _leftDeckIndex = Mathf.Clamp(_leftDeckIndex + 1, 0, _dataManager.CachedDecksData.decks.Count - 2);

            LoadDeckObjects(_leftDeckIndex);
        }

        // new horde deck object
        private void NewHordeDeckButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            _uiManager.GetPage<DeckEditingPage>().CurrentDeckId = -1;

            _appStateManager.ChangeAppState(Enumerators.AppState.HERO_SELECTION);
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

            private Button _deleteButton,
                           _editButton,
                           _selectButton;

            private Image _firstSkillImage,
                          _secondSkillImage;

            private Image _setTypeIcon;
            private Image _hordePicture;

            private GameObject _selectedDeckObjectBackground,
                               _selectedDeckObjectControl;

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

                _selectButton = _selfObject.transform.Find("Button_Select").GetComponent<Button>();
                _editButton = _selfObject.transform.Find("Panel_SelectedHordeObjects/Button_Edit").GetComponent<Button>();
                _deleteButton = _selfObject.transform.Find("Panel_SelectedHordeObjects/Button_Delete").GetComponent<Button>();

                _firstSkillImage = _selfObject.transform.Find("Panel_SelectedHordeObjects/Image_FirstSkil/Image_Skill").GetComponent<Image>();
                _secondSkillImage = _selfObject.transform.Find("Panel_SelectedHordeObjects/Image_SecondSkil/Image_Skill").GetComponent<Image>();
                _setTypeIcon = _selfObject.transform.Find("Panel_HordeType/Image").GetComponent<Image>();
                _hordePicture = _selfObject.transform.Find("Image_HordePicture").GetComponent<Image>();

                _descriptionText = _selfObject.transform.Find("Panel_Description/Text_Description").GetComponent<TextMeshProUGUI>();
                _cardsInDeckCountText = _selfObject.transform.Find("Panel_DeckFillInfo/Text_CardsCount").GetComponent<TextMeshProUGUI>();

                _selectedDeckObjectBackground = _selfObject.transform.Find("Panel_SelectedBlock").gameObject;
                _selectedDeckObjectControl = _selfObject.transform.Find("Panel_SelectedHordeObjects").gameObject;

                _cardsInDeckCountText.text = SelfDeck.GetNumCards() + "/" + Constants.MAX_DECK_SIZE;
                _descriptionText.text = deck.name;

                _setTypeIcon.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ElementIcons/Icon_element_" + SelfHero.element.ToLower());
                _hordePicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ChooseHorde/hordeselect_deck_" + SelfHero.element.ToLower());
                _firstSkillImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/hero_icon_" + SelfHero.element.ToUpper());
                _secondSkillImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/hero_icon_" + SelfHero.element.ToUpper());

                _selectButton.onClick.AddListener(SelectButtonOnClickHandler);
                _editButton.onClick.AddListener(EditButtonOnClickHandler);
                _deleteButton.onClick.AddListener(DeleteButtonOnclickHandler);
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

                _selectedDeckObjectBackground.SetActive(IsSelected);
                _selectedDeckObjectControl.SetActive(IsSelected);
            }

            public void Deselect()
            {
                if (!IsSelected)
                    return;

                IsSelected = false;

                _selectedDeckObjectBackground.SetActive(IsSelected);
                _selectedDeckObjectControl.SetActive(IsSelected);
            }

            private void SelectButtonOnClickHandler()
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

                HordeDeckSelectedEvent?.Invoke(this);
            }

            private void DeleteButtonOnclickHandler()
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

                _uiManager.GetPopup<QuestionPopup>().ConfirmationEvent += ConfirmDeleteDeckEventHandler;
                _uiManager.DrawPopup<QuestionPopup>("Do you really wan't to delete " + SelfDeck.name + "?");
            }

            private void EditButtonOnClickHandler()
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

                _uiManager.GetPage<DeckEditingPage>().CurrentDeckId = DeckId;
                _appStateManager.ChangeAppState(Enumerators.AppState.DECK_EDITING);
            }

            private void ConfirmDeleteDeckEventHandler(bool status)
            {
                _uiManager.GetPopup<QuestionPopup>().ConfirmationEvent -= ConfirmDeleteDeckEventHandler;

                if (status)
                    DeleteDeckEvent?.Invoke(this);
            }
        }
    }
}