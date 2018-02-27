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
		private QuestionPopup _questionPopup;

		private GameObject _selfPage;
        private Transform _selectedDeck;                     

        private MenuButtonNoGlow _buttonPlay,
			                    _buttonBuy,
								_buttonOpen,
								_buttonBack,
                                _buttonCollection;
        private Button _buttonCreateDeck;

        private Transform _decksContainer;

        private Image _selectedDeckIcon;

        private TMP_Text _selectedDeckName;

        private fsSerializer serializer = new fsSerializer();

        private int _deckToDelete;

        private bool _createDeckButtonPersist;

        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();

			_selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/DeckSelectionPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);


			_buttonBuy = _selfPage.transform.Find("BuyButton").GetComponent<MenuButtonNoGlow>();
			_buttonOpen = _selfPage.transform.Find("OpenButton").GetComponent<MenuButtonNoGlow>();
			_buttonBack = _selfPage.transform.Find("BackButton").GetComponent<MenuButtonNoGlow>();
			_buttonPlay = _selfPage.transform.Find("Button_Play").GetComponent<MenuButtonNoGlow>();
            _buttonCollection = _selfPage.transform.Find("CollectionButton").GetComponent<MenuButtonNoGlow>();

            _buttonBuy.onClickEvent.AddListener(BuyButtonHandler);
			_buttonOpen.onClickEvent.AddListener(OpenButtonHandler);
			_buttonBack.onClickEvent.AddListener(BackButtonHandler);
			_buttonPlay.onClickEvent.AddListener(OnClickPlay);
            _buttonCollection.onClickEvent.AddListener(CollectionButtonHandler);

            _selectedDeck = _selfPage.transform.Find("Decks/ActiveDeck/SelectedDeck");
			_selectedDeckIcon = _selectedDeck.Find("Icon").GetComponent<Image>();
            _selectedDeckName = _selectedDeck.Find("DeckName/DeckNameText").GetComponent<TMP_Text>();

            _decksContainer = _selfPage.transform.Find("Decks/DecksContainer");

            _buttonPlay.enabled = false;
            _buttonPlay.transform.Find("Button").GetComponent<Image>().color = new Color(1,1,1,.5f);

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
			foreach (var deck in GameManager.Instance.playerDecks)
			{
                var ind = i;
				Transform deckObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/DeckItem")).transform;
                deckObject.SetParent(_decksContainer, false);
                deckObject.Find("ActiveCard").gameObject.SetActive(false);
                deckObject.Find("CardsAmount/CardsAmountText").GetComponent<TMP_Text>().text = deck.GetNumCards().ToString();
                deckObject.Find("NormalCard/DeckName/DeckNameText").GetComponent<TMP_Text>().text = deck.name;
                deckObject.Find("ActiveCard/DeckName/DeckNameText").GetComponent<TMP_Text>().text = deck.name;
				deckObject.Find("Button").GetComponent<Button>().onClick.AddListener(() => { ChooseDeckHandler(deckObject); });
                deckObject.Find("EditButton").GetComponent<MenuButton>().onClickEvent.AddListener(() => { EditDeckHandler(deckObject); });
				deckObject.Find("EditButton").gameObject.SetActive(false);
				deckObject.Find("DeleteButton").GetComponent<MenuButton>().onClickEvent.AddListener(() => { DeleteDeckHandler(deckObject); });
                deckObject.Find("DeleteButton").gameObject.SetActive(false);
                i++;
			}
            _createDeckButtonPersist = false;
            AddCreationDeckButton();
            ActivatePlayButton(false);

            //if (GameManager.Instance.playerDecks.Count == 0 || GameManager.Instance.currentDeckId == -1)
                _selectedDeck.gameObject.SetActive(false);
            GameManager.Instance.currentDeckId = -1;

            if(_questionPopup == null)
            {
				_questionPopup = _uiManager.GetPopup<QuestionPopup>() as QuestionPopup;
				_questionPopup.ConfirmationEvent += DeleteConfirmationHandler;
            }

           // SetActive(GameManager.Instance.currentDeckId, true);
        }

        private void AddCreationDeckButton()
        {
            if (GameManager.Instance.playerDecks.Count < 8 && !_createDeckButtonPersist)
            {
                Transform deckObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/DeckItemCreate")).transform;
                deckObject.SetParent(_decksContainer, false);
                deckObject.GetComponent<Button>().onClick.AddListener(CreateDeck);
                _createDeckButtonPersist = true;
            }
        }

        private void ActivatePlayButton(bool isActive)
        {
            _buttonPlay.enabled = isActive;
            float a = isActive ? 1f : 0.5f;
            _buttonPlay.transform.Find("Button").GetComponent<Image>().color = new Color(1, 1, 1, a);
        }

		#region Buttons Handlers

        

		private void BuyButtonHandler()
		{
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.SHOP);
        }
        private void OpenButtonHandler()
		{
			OpenAlertDialog("Coming Soon");
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
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.GAMEPLAY);
        }
		private void CreateDeck()
		{
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.HERO_SELECTION);
        }

        private void ChooseDeckHandler(Transform deck)
        {
            int id = GetDeckId(deck);
            if (id == GameManager.Instance.currentDeckId)
                return;
            
            if (  GameManager.Instance.currentDeckId > -1)
				SetActive(GameManager.Instance.currentDeckId, false);
			GameManager.Instance.currentDeckId = id;
            _selectedDeck.gameObject.SetActive(true);
            SetActive(id, true);
            if (GameManager.Instance.playerDecks[GameManager.Instance.currentDeckId].GetNumCards() < GameManager.Instance.config.properties.maxDeckSize)
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
            GameManager.Instance.currentDeckId = id;
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.DECK_EDITING);
        }

        private void DeleteDeckHandler(Transform deck)
        {
            int id = GetDeckId(deck);
            _deckToDelete = id;
            _uiManager.DrawPopup<QuestionPopup>("Do you really want delete " + GameManager.Instance.playerDecks[id].name + "?");
        }

        private void DeleteConfirmationHandler()
        {
			GameManager.Instance.playerDecks.RemoveAt(_deckToDelete);
			Transform deckObj = _decksContainer.GetChild(_deckToDelete);
			deckObj.SetParent(null);
			MonoBehaviour.Destroy(deckObj.gameObject);
			if (GameManager.Instance.currentDeckId == _deckToDelete)
			{
				_selectedDeck.gameObject.SetActive(false);
				GameManager.Instance.currentDeckId = -1;
			}
            /*if(GameManager.Instance.playerDecks.Count > 0)
            {
                GameManager.Instance.currentDeckId = Mathf.Clamp(GameManager.Instance.currentDeckId, 0, GameManager.Instance.playerDecks.Count - 1);
                SetActive(GameManager.Instance.currentDeckId, true);
            }*/

            AddCreationDeckButton();

            var decksPath = Application.persistentDataPath + "/decks.json";
			Debug.Log(decksPath);
			fsData serializedData;
			serializer.TrySerialize(GameManager.Instance.playerDecks, out serializedData).AssertSuccessWithoutWarnings();
			var file = new StreamWriter(decksPath);
			var json = fsJsonPrinter.PrettyJson(serializedData);
			file.WriteLine(json);
			file.Close();
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
            Transform activeCard = activatedDeck.Find("ActiveCard");
            activeCard.gameObject.SetActive(active);
			activatedDeck.Find("EditButton").gameObject.SetActive(active);
			activatedDeck.Find("DeleteButton").gameObject.SetActive(active);

            if(active)
            {
				_selectedDeckIcon.sprite = activeCard.Find("Icon").GetComponent<Image>().sprite;
				_selectedDeckName.text = activeCard.Find("DeckName/DeckNameText").GetComponent<TMP_Text>().text;
            }
		}
    }
}
