using System;
using System.Collections.Generic;
using System.Linq;
using LoomNetwork.CZB.BackendCommunication;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class HordeSelectionPage : IUIElement
    {
        private const int KHordeItemSpace = 570, KHordeContainerXoffset = 60;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private ILocalizationManager _localizationManager;

        private IDataManager _dataManager;

        private ISoundManager _soundManager;

        private IAppStateManager _appStateManager;

        private IMatchManager _matchManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private GameObject _selfPage;

        private Button _backButton, _battleButton, _battleButtonWarning, _leftArrowButton, _rightArrowButton, _deleteButton, _editButton;

        private TextMeshProUGUI _gooValueText;

        private ButtonShiftingContent _buttonArmy;

        private Image _firstSkill, _secondSkill;

        private Transform _containerOfDecks, _hordeSelection;

        private List<HordeDeckObject> _hordeDecks;

        private int _selectedDeckId = -1;

        private int _scrolledDeck = -1;

        // private int _decksCount = 3;

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
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/HordeSelectionPage"), _uiManager.Canvas.transform, false);

            _containerOfDecks = _selfPage.transform.Find("Panel_DecksContainer/Group");

            _gooValueText = _selfPage.transform.Find("GooValue/Value").GetComponent<TextMeshProUGUI>();

            _buttonArmy = _selfPage.transform.Find("Button_Army").GetComponent<ButtonShiftingContent>();
            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _battleButton = _selfPage.transform.Find("Button_Battle").GetComponent<Button>();
            _battleButtonWarning = _selfPage.transform.Find("Button_Battle_Warning").GetComponent<Button>();
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
            _battleButtonWarning.onClick.AddListener(BattleButtonWarningOnClickHandler);
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

            _gooValueText.text = GameClient.Get<IPlayerManager>().GetGoo().ToString();

            // todod improve I guess
            _selectedDeckId = _dataManager.CachedUserLocalData.LastSelectedDeckId;
            _hordeSelection.gameObject.SetActive(false);

            LoadDeckObjects();
        }

        public void Hide()
        {
            if (_selfPage == null)

                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;

            /*ResetHordeDecks();

            _scrolledDeck = -1;*/
        }

        public void Dispose()
        {
        }

        private void FillHordeDecks()
        {
            ResetHordeDecks();
            _hordeDecks = new List<HordeDeckObject>();

            HordeDeckObject hordeDeck = null;
            for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count; i++)
            {
                hordeDeck = new HordeDeckObject(_containerOfDecks, _dataManager.CachedDecksData.Decks[i], _dataManager.CachedHeroesData.HeroesParsed.Find(x => x.HeroId == _dataManager.CachedDecksData.Decks[i].HeroId), i);
                hordeDeck.HordeDeckSelectedEvent += HordeDeckSelectedEventHandler;
                hordeDeck.DeleteDeckEvent += DeleteDeckEventHandler;

                _hordeDecks.Add(hordeDeck);
            }

            _newHordeDeckObject.transform.localPosition = Vector3.right * KHordeItemSpace * _hordeDecks.Count;
        }

        private void ResetHordeDecks()
        {
            _hordeSelection.SetParent(_containerOfDecks.parent, false);
            _hordeSelection.gameObject.SetActive(false);
            if (_hordeDecks != null)
            {
                foreach (HordeDeckObject element in _hordeDecks)
                {
                    element.Dispose();
                }

                _hordeDecks.Clear();
                _hordeDecks = null;
            }
        }

        private async void DeleteDeckEventHandler(HordeDeckObject deck)
        {
            // HACK for offline mode in online mode, local data should only be saved after
            // backend operation has succeeded
            _dataManager.CachedDecksData.Decks.Remove(deck.SelfDeck);
            _dataManager.CachedUserLocalData.LastSelectedDeckId = -1;
            _dataManager.CachedDecksLastModificationTimestamp = Utilites.GetCurrentUnixTimestampMillis();
            await _dataManager.SaveCache(Enumerators.CacheDataType.DecksData);
            await _dataManager.SaveCache(Enumerators.CacheDataType.UserLocalData);

            try
            {
                await _backendFacade.DeleteDeck(_backendDataControlMediator.UserDataModel.UserId, deck.SelfDeck.Id, _dataManager.CachedDecksLastModificationTimestamp);
                Debug.Log($" ====== Delete Deck {deck.SelfDeck.Id} Successfully ==== ");
            } catch (Exception e)
            {
                // HACK for offline mode
                if (false)
                {
                    Debug.Log("Result === " + e);
                    OpenAlertDialog($"Not able to Delete Deck {deck.SelfDeck.Id}: " + e.Message);
                    return;
                }
            }

            LoadDeckObjects();
        }

        private void HordeDeckSelectedEventHandler(HordeDeckObject deck)
        {
            if (_hordeSelection.gameObject.activeSelf)
            {
                HordeDeckObject horde = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
                horde.Deselect();
            }

            deck.Select();

            _firstSkill.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/heroability_" + deck.SelfHero.Element.ToUpper() + "_" + deck.SelfHero.Skills[deck.SelfHero.PrimarySkill].Skill.ToLower());
            _secondSkill.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/heroability_" + deck.SelfHero.Element.ToUpper() + "_" + deck.SelfHero.Skills[deck.SelfHero.SecondarySkill].Skill.ToLower());

            _hordeSelection.transform.SetParent(deck.SelectionContainer, false);
            _hordeSelection.gameObject.SetActive(true);

            _selectedDeckId = (int)deck.SelfDeck.Id;
            _dataManager.CachedUserLocalData.LastSelectedDeckId = _selectedDeckId;

            _dataManager.SaveCache(Enumerators.CacheDataType.UserLocalData);
            deck.SelectionContainer.parent.SetAsLastSibling();

            BattleButtonUpdate();
        }

        private void BattleButtonUpdate()
        {
#if !DEV_MODE
            if ((_hordeDecks.Count == 0) || (_selectedDeckId == -1) || (_hordeDecks.First(o => o.SelfDeck.Id == _selectedDeckId).SelfDeck.GetNumCards() < Constants.MinDeckSize))
            {
                _battleButton.interactable = false;
                _battleButtonWarning.gameObject.SetActive(true);
            }

#else
            _battleButton.interactable = true;
            _battleButtonWarning.gameObject.SetActive(false);
#endif
        }

        private void LoadDeckObjects()
        {
            FillHordeDecks();

            _newHordeDeckObject.transform.SetAsLastSibling();
            _newHordeDeckObject.SetActive(true);

            HordeDeckObject deck = _hordeDecks.Find(x => x.SelfDeck.Id == _dataManager.CachedUserLocalData.LastSelectedDeckId);
            if (deck != null)
            {
                HordeDeckSelectedEventHandler(deck);
            }

            if (_hordeDecks.Count > 0)
            {
                HordeDeckObject foundDeck = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
                if (foundDeck == null)
                {
                    _selectedDeckId = (int)_hordeDecks[0].SelfDeck.Id;
                }
            }
            else
            {
                _selectedDeckId = -1;
            }

            CenterTheSelectedDeck();
            BattleButtonUpdate();
        }

        private void CenterTheSelectedDeck()
        {
            if (_hordeDecks.Count < 1)

                return;

            _scrolledDeck = _hordeDecks.IndexOf(_hordeDecks.Find(x => x.IsSelected));

            if (_scrolledDeck < 2)
            {
                _scrolledDeck = 0;
            }
            else
            {
                _scrolledDeck--;
            }

            _containerOfDecks.transform.localPosition = new Vector3(KHordeContainerXoffset - (KHordeItemSpace * _scrolledDeck), 420, 0);
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        private void SwitchOverlordObject(int direction)
        {
            bool isChanged = false;

            if (_hordeDecks.Count < 1)

                return;

            int oldIndex = _scrolledDeck;
            _scrolledDeck += direction;

            if (_scrolledDeck > _hordeDecks.Count - 2)
            {
                _scrolledDeck = _hordeDecks.Count - 2;
            }

            if (_scrolledDeck < 0)
            {
                _scrolledDeck = 0;
            }

            if (oldIndex != _scrolledDeck)
            {
                _containerOfDecks.transform.localPosition = new Vector3(KHordeContainerXoffset - (KHordeItemSpace * _scrolledDeck), 420, 0);
            }
        }

        private bool ShowConnectionLostPopupIfNeeded()
        {
            // HACK for offline mode
            return false;
            if (_backendFacade.IsConnected)
            {
                return false;
            }

            _uiManager.DrawPopup<WarningPopup>("Sorry, modifications are only available in online mode.");
            return true;
        }

        public class HordeDeckObject
        {
            public Transform SelectionContainer;

            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly ISoundManager _soundManager;

            private readonly GameObject _selfObject;

            private readonly Image _setTypeIcon;

            private readonly Image _hordePicture;

            private readonly TextMeshProUGUI _descriptionText;

            private readonly TextMeshProUGUI _cardsInDeckCountText;

            private readonly Button _buttonSelect;

            private IUIManager _uiManager;

            private IAppStateManager _appStateManager;

            private IDataManager _dataManager;

            public HordeDeckObject(Transform parent, Deck deck, Hero hero, int index)
            {
                SelfDeck = deck;
                SelfHero = hero;

                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                _uiManager = GameClient.Get<IUIManager>();
                _appStateManager = GameClient.Get<IAppStateManager>();
                _dataManager = GameClient.Get<IDataManager>();
                _soundManager = GameClient.Get<ISoundManager>();

                _selfObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/Item_HordeSelectionObject"), parent, false);
                _selfObject.transform.localPosition = Vector3.right * KHordeItemSpace * index;

                SelectionContainer = _selfObject.transform.Find("SelectionContainer");

                _setTypeIcon = _selfObject.transform.Find("Panel_HordeType/Image").GetComponent<Image>();
                _hordePicture = _selfObject.transform.Find("Image_HordePicture").GetComponent<Image>();

                _descriptionText = _selfObject.transform.Find("Panel_Description/Text_Description").GetComponent<TextMeshProUGUI>();
                _cardsInDeckCountText = _selfObject.transform.Find("Panel_DeckFillInfo/Text_CardsCount").GetComponent<TextMeshProUGUI>();

                _buttonSelect = _selfObject.transform.Find("Button_Select").GetComponent<Button>();

                _cardsInDeckCountText.text = SelfDeck.GetNumCards() + "/" + Constants.MaxDeckSize;
                _descriptionText.text = deck.Name;

                _setTypeIcon.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ElementIcons/Icon_element_" + SelfHero.Element.ToLower());
                _hordePicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ChooseHorde/hordeselect_deck_" + SelfHero.Element.ToLower());

                _buttonSelect.onClick.AddListener(SelectButtonOnClickHandler);
            }

            public event Action<HordeDeckObject> HordeDeckSelectedEvent;

            public event Action<HordeDeckObject> DeleteDeckEvent;

            public Deck SelfDeck { get; }

            public Hero SelfHero { get; }

            public bool IsSelected { get; private set; }

            public void Dispose()
            {
                Object.Destroy(_selfObject);
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
                _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);
                HordeDeckSelectedEvent?.Invoke(this);
            }
        }

        #region Buttons Handlers

        private void CollectionButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);
            _appStateManager.ChangeAppState(Enumerators.AppState.Collection);
        }

        private void BackButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);
            _appStateManager.ChangeAppState(Enumerators.AppState.MainMenu);
        }

        private void BattleButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);
            _uiManager.GetPage<GameplayPage>().CurrentDeckId = _selectedDeckId;
            _matchManager.FindMatch(Enumerators.MatchType.Local);
        }

        private void BattleButtonWarningOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);

#if !DEV_MODE
            if ((_hordeDecks.Count == 0) || (_selectedDeckId == -1) || (_hordeDecks.First(o => o.SelfDeck.Id == _selectedDeckId).SelfDeck.GetNumCards() < Constants.MinDeckSize))
            {
                _uiManager.DrawPopup<WarningPopup>("Select a valid horde with " + Constants.MinDeckSize + " cards.");
            }

#endif
        }

        private void LeftArrowButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);

            SwitchOverlordObject(-1);
        }

        private void RightArrowButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);
            SwitchOverlordObject(1);
        }

        private void SkillButtonOnSingleClickHandler(int skillIndex)
        {
            _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);
            HordeDeckObject deck = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
            if (deck != null)
            {
                HeroSkill skill = skillIndex == 0?deck.SelfHero.Skills[deck.SelfHero.PrimarySkill]:deck.SelfHero.Skills[deck.SelfHero.SecondarySkill];

                _uiManager.DrawPopup<OverlordAbilityTooltipPopup>(skill);
            }
        }

        private void SkillButtonOnDoubleClickHandler(int skillIndex)
        {
            _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);
            HordeDeckObject deck = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
            if (deck != null)
            {
                _uiManager.DrawPopup<OverlordAbilitySelectionPopup>(deck.SelfHero);
            }
        }

        // new horde deck object
        private void NewHordeDeckButtonOnClickHandler()
        {
            if (ShowConnectionLostPopupIfNeeded())

                return;

            _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);

            _uiManager.GetPage<DeckEditingPage>().CurrentDeckId = -1;

            _appStateManager.ChangeAppState(Enumerators.AppState.HeroSelection);
        }

        private void DeleteButtonOnClickHandler()
        {
            if (ShowConnectionLostPopupIfNeeded())

                return;

            HordeDeckObject deck = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
            if (deck != null)
            {
                _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);

                _uiManager.GetPopup<QuestionPopup>().ConfirmationEvent += ConfirmDeleteDeckEventHandler;

                _uiManager.DrawPopup<QuestionPopup>("Do you really want to delete " + deck.SelfDeck.Name + "?");
            }
        }

        private void EditButtonOnClickHandler()
        {
            if (ShowConnectionLostPopupIfNeeded())

                return;

            if (_selectedDeckId != -1)
            {
                _soundManager.PlaySound(Enumerators.SoundType.Click, Constants.SfxSoundVolume, false, false, true);

                _uiManager.GetPage<DeckEditingPage>().CurrentDeckId = _selectedDeckId;
                _appStateManager.ChangeAppState(Enumerators.AppState.DeckEditing);
            }
        }

        private void ConfirmDeleteDeckEventHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationEvent -= ConfirmDeleteDeckEventHandler;

            if (!status)

                return;

            HordeDeckObject deckToDelete = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
            if (deckToDelete != null)
            {
                DeleteDeckEventHandler(deckToDelete);
            }

            BattleButtonUpdate();
        }

        // private void BuyButtonHandler()
        // {
        // GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        // GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.SHOP);
        // }
        // private void OpenButtonHandler()
        // {
        // GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        // GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
        // }

        #endregion
    }
}
