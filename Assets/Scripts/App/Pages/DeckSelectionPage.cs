﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using CCGKit;
using TMPro;
using FullSerializer;
using System.IO;


namespace GrandDevs.CZB
{
    public class DeckSelectionPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;
		private IDataManager _dataManager;
        private QuestionPopup _questionPopup;

		private GameObject _selfPage;
        private Transform _selectedDeck;                     

        //private MenuButtonNoGlow _buttonBuy,
								//_buttonOpen,
        //                        _buttonCollection;
        private Button _buttonCreateDeck,
                       _buttonPlay,
                       _buttonBack,
                       _buttonCollection;

        private Transform _decksContainer;

        private Image _selectedDeckIcon;

        //private TMP_Text _selectedDeckName;

        private fsSerializer serializer = new fsSerializer();

        private int _deckToDelete;

        private bool _createDeckButtonPersist;

        private int _currentDeckId;

        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/DeckSelectionPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);


			//_buttonBuy = _selfPage.transform.Find("BuyButton").GetComponent<MenuButtonNoGlow>();
			//_buttonOpen = _selfPage.transform.Find("OpenButton").GetComponent<MenuButtonNoGlow>();
			_buttonBack = _selfPage.transform.Find("Header/BackButton").GetComponent<Button>();
			_buttonPlay = _selfPage.transform.Find("SelectedDeck/Button_Play/Button").GetComponent<Button>();
            _buttonCollection = _selfPage.transform.Find("CollectionButton/Button").GetComponent<Button>();

   //         _buttonBuy.onClickEvent.AddListener(BuyButtonHandler);
			//_buttonOpen.onClickEvent.AddListener(OpenButtonHandler);
			_buttonBack.onClick.AddListener(BackButtonHandler);
			_buttonPlay.onClick.AddListener(OnClickPlay);
            _buttonCollection.onClick.AddListener(CollectionButtonHandler);

            _selectedDeck = _selfPage.transform.Find("SelectedDeck");
			_selectedDeckIcon = _selectedDeck.Find("Deck/Mask/Icon").GetComponent<Image>();
            

            _decksContainer = _selfPage.transform.Find("DecksContainer");

            _buttonPlay.enabled = false;
            //_buttonPlay.transform.Find("Button").GetComponent<Image>().color = new Color(1,1,1,.5f);

            Hide();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage.SetActive(true);
            FillInfo();
        }

        public void Hide()
        {
            for (int i = 0; i < _decksContainer.childCount; i++)
            {
                MonoBehaviour.Destroy(_decksContainer.GetChild(i).gameObject);
            }
            _selfPage.SetActive(false);
        }

        public void Dispose()
        {
            
        }

        private void FillInfo()
        {
            int i = 0;
			foreach (var deck in _dataManager.CachedDecksData.decks)
			{
                var ind = i;
                string heroType = _dataManager.CachedHeroesData.heroes[deck.heroId].element.ToString();

                Transform deckObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/DeckItem")).transform;
                deckObject.SetParent(_decksContainer, false);
                deckObject.Find("Glow").gameObject.SetActive(false);
                deckObject.Find("HeroImage").GetComponent<Image>().sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/hero_" + heroType);
                //deckObject.Find("ActiveCard/Icon").GetComponent<Image>().sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/hero_" + heroType);
                deckObject.Find("Frame/CardsAmount/CardsAmountText").GetComponent<Text>().text = deck.GetNumCards().ToString();
                deckObject.Find("Frame/Name").GetComponent<Text>().text = deck.name;
                //deckObject.Find("ActiveCard/DeckName/DeckNameText").GetComponent<TMP_Text>().text = deck.name;
				deckObject.GetComponent<Button>().onClick.AddListener(() => { ChooseDeckHandler(deckObject); });
                deckObject.Find("Frame/EditButton").GetComponent<Button>().onClick.AddListener(() => { EditDeckHandler(deckObject); });
				deckObject.Find("Frame/EditButton").gameObject.SetActive(false);
				deckObject.Find("Frame/DeleteButton").GetComponent<Button>().onClick.AddListener(() => { DeleteDeckHandler(deckObject); });
                deckObject.Find("Frame/DeleteButton").gameObject.SetActive(false);
                deckObject.Find("Frame/HeroSkillIcon").GetComponent<Image>().sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/hero_icon_" + heroType);
                i++;
			}
            _createDeckButtonPersist = false;
            for (i = _dataManager.CachedDecksData.decks.Count; i < 8; i++)
                AddCreationDeckButton();
            ActivatePlayButton(false);

            //_selectedDeck.gameObject.SetActive(false);

            if(_questionPopup == null)
            {
				_questionPopup = _uiManager.GetPopup<QuestionPopup>() as QuestionPopup;
				_questionPopup.ConfirmationEvent += DeleteConfirmationHandler;
            }
            _currentDeckId = -1;

            _selectedDeck.Find("Deck").gameObject.SetActive(false);
        }

        private void AddCreationDeckButton()
        {
            Transform deckObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/DeckItemCreate")).transform;
            deckObject.SetParent(_decksContainer, false);
            deckObject.GetComponent<Button>().onClick.AddListener(CreateDeck);
            _createDeckButtonPersist = true;
        }
        private void ActivatePlayButton(bool isActive)
        {
            _buttonPlay.enabled = isActive;
            float a = isActive ? 1f : 0.5f;
            //_buttonPlay.transform.Find("Button").GetComponent<Image>().color = new Color(1, 1, 1, a);
        }

		#region Buttons Handlers

        

		private void BuyButtonHandler()
		{
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.SHOP);
        }
        private void OpenButtonHandler()
		{
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
        }
		private void BackButtonHandler()
		{
			GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
		}

        private void CollectionButtonHandler()
        {
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.COLLECTION);
        }
        public void OnClickPlay()
        {
            (_uiManager.GetPage<GameplayPage>() as GameplayPage).CurrentDeckId = _currentDeckId;
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.GAMEPLAY);
        }
		private void CreateDeck()
		{
            (_uiManager.GetPage<DeckEditingPage>() as DeckEditingPage).CurrentDeckId = -1;
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.HERO_SELECTION);
        }

        private void ChooseDeckHandler(Transform deck)
        {
            int id = GetDeckId(deck);
            if (id == _currentDeckId)
                return;
            
            if (_currentDeckId > -1)
				SetActive(_currentDeckId, false);
            _currentDeckId = id;
            //_selectedDeck.gameObject.SetActive(true);
            SetActive(id, true);

            if (_dataManager.CachedDecksData.decks[_currentDeckId].GetNumCards() < 30 && !Constants.DEV_MODE)
            {
                ActivatePlayButton(false);
                //   OpenAlertDialog("You should have 30 cards inside your deck to use it for battle");
                // return;
            }
            else
                ActivatePlayButton(true);
        }

        private void EditDeckHandler(Transform deck)
        {
            int id = GetDeckId(deck);
            _currentDeckId = id;
            (_uiManager.GetPage<DeckEditingPage>() as DeckEditingPage).CurrentDeckId = _currentDeckId;
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.DECK_EDITING);
        }

        private void DeleteDeckHandler(Transform deck)
        {
            int id = GetDeckId(deck);
            _deckToDelete = id;
            _uiManager.DrawPopup<QuestionPopup>("Do you really want delete " + _dataManager.CachedDecksData.decks[id].name + "?");
        }

        private void DeleteConfirmationHandler()
        {
			_dataManager.CachedDecksData.decks.RemoveAt(_deckToDelete);
			Transform deckObj = _decksContainer.GetChild(_deckToDelete);
			deckObj.SetParent(null);
			MonoBehaviour.Destroy(deckObj.gameObject);
			if (_currentDeckId == _deckToDelete)
			{
				//_selectedDeck.gameObject.SetActive(false);
                _currentDeckId = -1;
			}

            AddCreationDeckButton();
        }
		
        private int GetDeckId(Transform deck)
        {
            int id = -1;
			for (int i = 0; i < _decksContainer.childCount; i++)
			{
                if (_decksContainer.GetChild(i) == deck)
                {
                    id = i;
                    break;
                }
			}
            return id;
        }
		#endregion

		private void OpenAlertDialog(string msg)
		{
			_uiManager.DrawPopup<WarningPopup>(msg);
		}

		public void SetActive(int id, bool active)
		{
            Transform activatedDeck = _decksContainer.GetChild(id);
            Transform activeCard = activatedDeck.Find("Glow");
            activeCard.gameObject.SetActive(active);
			activatedDeck.Find("Frame/EditButton").gameObject.SetActive(active);
			activatedDeck.Find("Frame/DeleteButton").gameObject.SetActive(active);

            if (active)
            {
                _selectedDeckIcon.sprite = activatedDeck.Find("HeroImage").GetComponent<Image>().sprite;
                _selectedDeck.Find("Deck").gameObject.SetActive(true);

                Transform selectedCardAmountObject = _decksContainer.GetChild(id).Find("Frame/CardsAmount/CardsAmountText");
                if (selectedCardAmountObject != null)
                {
                    _selectedDeck.Find("Deck/Frame/CardsAmount/CardsAmountText").GetComponent<Text>().text = selectedCardAmountObject.GetComponent<Text>().text;
                    _selectedDeck.Find("Deck/Frame/HeroSkillIcon").GetComponent<Image>().sprite = _decksContainer.GetChild(id).Find("Frame/HeroSkillIcon").GetComponent<Image>().sprite;
                    _selectedDeck.Find("Deck/Frame/Name").GetComponent<Text>().text = _decksContainer.GetChild(id).Find("Frame/Name").GetComponent<Text>().text;
                }
            }
        }
    }
}
