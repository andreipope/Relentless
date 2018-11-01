using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class HordeSelectionPage : IUIElement
    {
        private const int HordeItemSpace = 570, HordeContainerXoffset = 60;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private ISoundManager _soundManager;

        private IAppStateManager _appStateManager;

        private IMatchManager _matchManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private GameObject _selfPage;

        private GameObject _battleButtonGlow;

        private Button _backButton,
            _battleButton,
            _battleButtonWarning,
            _leftArrowButton,
            _rightArrowButton,
            _deleteButton,
            _editButton;

        private TextMeshProUGUI _gooValueText;

        private ButtonShiftingContent _buttonArmy;

        private Image _firstSkill, _secondSkill;

        private Transform _containerOfDecks, _hordeSelection;

        private List<HordeDeckObject> _hordeDecks;
        private HordeDeckObject _editingDeck;

        private int _selectedDeckId = -1;

        private int _scrolledDeck = -1;

        // new horde deck object
        private GameObject _newHordeDeckObject;

        private Button _newHordeDeckButton;

        private IAnalyticsManager _analyticsManager;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _matchManager = GameClient.Get<IMatchManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/HordeSelectionPage"),
                _uiManager.Canvas.transform, false);

            _containerOfDecks = _selfPage.transform.Find("Panel_DecksContainer/Group");

            _gooValueText = _selfPage.transform.Find("GooValue/Value").GetComponent<TextMeshProUGUI>();

            _buttonArmy = _selfPage.transform.Find("Button_Army").GetComponent<ButtonShiftingContent>();
            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _battleButton = _selfPage.transform.Find("Button_Battle").GetComponent<Button>();
            _battleButtonWarning = _selfPage.transform.Find("Button_Battle_Warning").GetComponent<Button>();
            _leftArrowButton = _selfPage.transform.Find("Button_LeftArrow").GetComponent<Button>();
            _rightArrowButton = _selfPage.transform.Find("Button_RightArrow").GetComponent<Button>();

            _editButton =
                _selfPage.transform.Find("Panel_DecksContainer/SelectionMask/Selection/Panel_SelectedBlock/Panel_SelectedHordeObjects/Button_Edit")
                    .GetComponent<Button>();
            _deleteButton =
                _selfPage.transform.Find("Panel_DecksContainer/SelectionMask/Selection/Panel_SelectedBlock/Panel_SelectedHordeObjects/Button_Delete")
                    .GetComponent<Button>();
            _firstSkill =
                _selfPage.transform
                    .Find("Panel_DecksContainer/SelectionMask/Selection/Panel_SelectedBlock/Panel_SelectedHordeObjects/Image_FirstSkil/Image_Skill")
                    .GetComponent<Image>();
            _secondSkill =
                _selfPage.transform
                    .Find("Panel_DecksContainer/SelectionMask/Selection/Panel_SelectedBlock/Panel_SelectedHordeObjects/Image_SecondSkil/Image_Skill")
                    .GetComponent<Image>();

            _hordeSelection = _selfPage.transform.Find("Panel_DecksContainer/SelectionMask/Selection");

            _battleButtonGlow = _selfPage.transform.Find("Button_Battle/BattleButtonGlowing").gameObject;

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

            _firstSkill.GetComponent<MultiPointerClickHandler>().SingleClickReceived += () =>
                SkillButtonOnSingleClickHandler(0);
            _secondSkill.GetComponent<MultiPointerClickHandler>().SingleClickReceived += () =>
                SkillButtonOnSingleClickHandler(1);

            _firstSkill.GetComponent<MultiPointerClickHandler>().DoubleClickReceived += () =>
                SkillButtonOnDoubleClickHandler(0);
            _secondSkill.GetComponent<MultiPointerClickHandler>().DoubleClickReceived += () =>
                SkillButtonOnDoubleClickHandler(1);

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
        }

        public void Dispose()
        {
        }

        private void FillHordeDecks()
        {
            ResetHordeDecks();
            _hordeDecks = new List<HordeDeckObject>();

            for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count; i++)
            {
                HordeDeckObject hordeDeck =
                    new HordeDeckObject(
                        _containerOfDecks,
                        _dataManager.CachedDecksData.Decks[i],
                        _dataManager.CachedHeroesData.HeroesParsed.Find(x =>
                            x.HeroId == _dataManager.CachedDecksData.Decks[i].HeroId),
                        i);
                hordeDeck.HordeDeckSelected += HordeDeckSelectedHandler;

                _hordeDecks.Add(hordeDeck);
            }

            _newHordeDeckObject.transform.localPosition = Vector3.right * HordeItemSpace * _hordeDecks.Count;
        }

        private void ResetHordeDecks()
        {
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

        private async void DeckDeletingHandler(HordeDeckObject deck)
        {
            try
            {
                await _backendFacade.DeleteDeck(
                    _backendDataControlMediator.UserDataModel.UserId,
                    deck.SelfDeck.Id
                );
                _dataManager.CachedDecksData.Decks.Remove(deck.SelfDeck);
                _dataManager.CachedUserLocalData.LastSelectedDeckId = -1;
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                Debug.Log($" ====== Delete Deck {deck.SelfDeck.Id} Successfully ==== ");
            }
            catch (Exception e)
            {
                Debug.Log("Result === " + e);
                OpenAlertDialog($"Not able to Delete Deck {deck.SelfDeck.Id}: " + e.Message);
                return;
            }

            LoadDeckObjects();
        }

        private void HordeDeckSelectedHandler(HordeDeckObject horde)
        {
            if (_hordeSelection.gameObject.activeSelf)
            {
                HordeDeckObject selectedHorde = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
                selectedHorde.Deselect();
            }

            horde.Select();

			//TODO remove once we complete the ability selection process. For now, just hard coding values like everywhere else for overlord abilities.
            horde.SelfDeck.PrimarySkill = 0;
            horde.SelfDeck.SecondarySkill = 1;
            
            _firstSkill.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" +
                 horde.SelfHero.Skills[horde.SelfDeck.PrimarySkill].IconPath);
            _secondSkill.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" +
               horde.SelfHero.Skills[horde.SelfDeck.SecondarySkill].IconPath);

            _selectedDeckId = (int) horde.SelfDeck.Id;

            _dataManager.CachedUserLocalData.LastSelectedDeckId = _selectedDeckId;

            _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);

            _hordeSelection.gameObject.SetActive(true);
            RepositionSelection();
            BattleButtonUpdate();
        }

        private void RepositionSelection()
        {
            if(_hordeSelection.gameObject.activeSelf)
            {
                _hordeSelection.position = _hordeDecks.Find(x => x.SelfDeck.Id == _selectedDeckId).SelectionContainer.transform.position;
            }
        }

        private void BattleButtonUpdate()
        {
            bool canStartBattle =
#if !DEV_MODE
                _hordeDecks.Count != 0 &&
                _selectedDeckId != -1 &&
                _hordeDecks.First(o => o.SelfDeck.Id == _selectedDeckId).SelfDeck.GetNumCards() ==
                Constants.MinDeckSize;
#else
                true;
#endif
            _battleButton.interactable = canStartBattle;
            _battleButtonGlow.SetActive(canStartBattle);
            _battleButtonWarning.gameObject.SetActive(!canStartBattle);
        }

        private void LoadDeckObjects()
        {
            FillHordeDecks();

            _newHordeDeckObject.transform.SetAsLastSibling();
            _newHordeDeckObject.SetActive(true);

            HordeDeckObject deck =
                _hordeDecks.Find(x => x.SelfDeck.Id == _dataManager.CachedUserLocalData.LastSelectedDeckId);
            if (deck != null)
            {
                HordeDeckSelectedHandler(deck);
            }

            if (_hordeDecks.Count > 0)
            {
                HordeDeckObject foundDeck = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
                if (foundDeck == null)
                {
                    _selectedDeckId = (int) _hordeDecks[0].SelfDeck.Id;
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

            _containerOfDecks.transform.localPosition =
                new Vector3(HordeContainerXoffset - HordeItemSpace * _scrolledDeck, 420, 0);
            RepositionSelection();
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        private void SwitchOverlordObject(int direction)
        {
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
                _containerOfDecks.transform.localPosition =
                    new Vector3(HordeContainerXoffset - HordeItemSpace * _scrolledDeck, 420, 0);
                RepositionSelection();
            }
        }

        public class HordeDeckObject
        {
            public Transform SelectionContainer;

            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly ISoundManager _soundManager;

            private readonly GameObject _selfObject;

            private readonly GameObject _background;

            private readonly Image _setTypeIcon;

            private readonly Image _hordePicture;

            private readonly TextMeshProUGUI _descriptionText;

            private readonly TextMeshProUGUI _cardsInDeckCountText;

            private readonly Button _buttonSelect;

            public HordeDeckObject(Transform parent, Deck deck, Hero hero, int index)
            {
                SelfDeck = deck;
                SelfHero = hero;

                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                _soundManager = GameClient.Get<ISoundManager>();

                _selfObject =
                    Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>(
                            "Prefabs/UI/Elements/Item_HordeSelectionObject"), parent, false);
                _selfObject.transform.localPosition = Vector3.right * HordeItemSpace * index;

                SelectionContainer = _selfObject.transform.Find("SelectionContainer");

                _background = _selfObject.transform.Find("Image_BaackgroundGeneral").gameObject;

                _setTypeIcon = _selfObject.transform.Find("Panel_HordeType/Image").GetComponent<Image>();
                _hordePicture = _selfObject.transform.Find("Image_HordePicture").GetComponent<Image>();

                _descriptionText = _selfObject.transform.Find("Panel_Description/Text_Description")
                    .GetComponent<TextMeshProUGUI>();
                _cardsInDeckCountText = _selfObject.transform.Find("Panel_DeckFillInfo/Text_CardsCount")
                    .GetComponent<TextMeshProUGUI>();

                _buttonSelect = _selfObject.transform.Find("Button_Select").GetComponent<Button>();

                _cardsInDeckCountText.text = SelfDeck.GetNumCards() + "/" + Constants.MaxDeckSize;
                _descriptionText.text = deck.Name;

                _setTypeIcon.sprite =
                    _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ElementIcons/Icon_element_" +
                        SelfHero.Element.ToLowerInvariant());
                _hordePicture.sprite =
                    _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/ChooseHorde/hordeselect_deck_" +
                        SelfHero.Element.ToLowerInvariant());

                _buttonSelect.onClick.AddListener(SelectButtonOnClickHandler);
            }

            public event Action<HordeDeckObject> HordeDeckSelected;

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
                _background.SetActive(false);

                IsSelected = true;
            }

            public void Deselect()
            {
                if (!IsSelected)
                    return;

                _buttonSelect.gameObject.SetActive(true);
                _background.SetActive(true);

                IsSelected = false;
            }

            private void SelectButtonOnClickHandler()
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
                HordeDeckSelected?.Invoke(this);
            }
        }

        #region Buttons Handlers

        private void CollectionButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _appStateManager.ChangeAppState(Enumerators.AppState.ARMY);
        }

        private void BackButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _appStateManager.ChangeAppState(Enumerators.AppState.PlaySelection);
        }

        private void BattleButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _uiManager.GetPage<GameplayPage>().CurrentDeckId = _selectedDeckId;
            _matchManager.FindMatch();
        }

        private void BattleButtonWarningOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

#if !DEV_MODE
            if (_hordeDecks.Count == 0 ||
                _selectedDeckId == -1 ||
                _hordeDecks.First(o => o.SelfDeck.Id == _selectedDeckId).SelfDeck.GetNumCards() < Constants.MinDeckSize)
            {
                _uiManager.DrawPopup<WarningPopup>("Select a valid horde with " + Constants.MinDeckSize + " cards.");
            }
#endif
        }

        private void LeftArrowButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            SwitchOverlordObject(-1);
        }

        private void RightArrowButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            SwitchOverlordObject(1);
        }

        private void SkillButtonOnSingleClickHandler(int skillIndex)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            HordeDeckObject deck = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
            if (deck != null)
            {
                HeroSkill skill = skillIndex == 0 ?
                    deck.SelfHero.Skills[deck.SelfHero.PrimarySkill] :
                    deck.SelfHero.Skills[deck.SelfHero.SecondarySkill];

                _uiManager.DrawPopup<OverlordAbilityTooltipPopup>(skill);
            }
        }

        private void SkillButtonOnDoubleClickHandler(int skillIndex)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _editingDeck = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
            if (_editingDeck != null)
            {
                //_uiManager.GetPopup<OverlordAbilitySelectionPopup>().PopupHiding += AbilityPopupClosedEvent;
                _uiManager.DrawPopup<OverlordAbilitySelectionPopup>(_editingDeck.SelfHero);
            }
        }

        private void NewHordeDeckButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _uiManager.GetPage<HordeEditingPage>().CurrentDeckId = -1;

            _appStateManager.ChangeAppState(Enumerators.AppState.HERO_SELECTION);
        }

        private void DeleteButtonOnClickHandler()
        {
            HordeDeckObject deck = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
            if (deck != null)
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

                _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmDeleteDeckReceivedHandler;

                _uiManager.DrawPopup<QuestionPopup>("Do you really want to delete " + deck.SelfDeck.Name + "?");
            }
        }

        private void EditButtonOnClickHandler()
        {
            if (_selectedDeckId != -1)
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

                _uiManager.GetPage<HordeEditingPage>().CurrentDeckId = _selectedDeckId;
                HordeDeckObject deck = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
                _uiManager.GetPage<HordeEditingPage>().CurrentHeroId = deck.SelfHero.HeroId;
                _appStateManager.ChangeAppState(Enumerators.AppState.DECK_EDITING);
            }
        }

        private void ConfirmDeleteDeckReceivedHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmDeleteDeckReceivedHandler;

            if (!status)
                return;

            HordeDeckObject deckToDelete = _hordeDecks.FirstOrDefault(o => o.SelfDeck.Id == _selectedDeckId);
            if (deckToDelete != null)
            {
                DeckDeletingHandler(deckToDelete);
            }

            BattleButtonUpdate();

            _analyticsManager.SetEvent(AnalyticsManager.EventDeckDeleted);
        }

        #endregion

    }
}
