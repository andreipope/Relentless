using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using OneOf;
using OneOf.Types;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class HordeSelectionWithNavigationPage : IUIElement
    {
        public enum Tab
        {
            None = -1,
            SelectDeck = 0,
            Editing = 1
        }

        private static readonly ILog Log = Logging.GetLog(nameof(HordeSelectionWithNavigationPage));

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private ITutorialManager _tutorialManager;

        private BackendDataSyncService _backendDataSyncService;

        public SelectDeckTab HordeSelectDeckTab;
        public HordeEditingTab HordeEditTab;

        private GameObject _selfPage;

        private GameObject _selectDeckGameObject;

        private GameObject _editingTabGameObject;

        #region Cache Data

        private Tab _tab;

        public int SelectDeckIndex;
        public int SelectedDeckId;
        public Deck CurrentEditDeck;

        public bool IsEditingNewDeck;

        #endregion

        #region IUIElement

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _backendDataSyncService = GameClient.Get<BackendDataSyncService>();

            SelectDeckIndex = 0;

            HordeSelectDeckTab = new SelectDeckTab();
            HordeSelectDeckTab.Init();

            HordeEditTab = new HordeEditingTab();
            HordeEditTab.Init();
        }

        public void Update()
        {
            HordeEditTab.Update();
        }

        public async void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MyDecksPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_DECKS);
            _uiManager.DrawPopup<AreaBarPopup>();

            UpdatePageScaleToMatchResolution();

            _selectDeckGameObject = _selfPage.transform.Find("Tab_SelectDeck").gameObject;
            _editingTabGameObject = _selfPage.transform.Find("Tab_Editing").gameObject;

            HordeSelectDeckTab.Show(_selectDeckGameObject);
            HordeSelectDeckTab.UpdateDeckInfoObjects(true);
            ShowTabGameObject(Tab.SelectDeck);

            if (_backendDataSyncService.IsCollectionDataDirty)
            {
                OneOf<Success, Exception> result = await _backendDataSyncService.UpdateCardCollectionWithUi(false);
                if (result.IsT1)
                {
                    Log.Warn(result.AsT1);

                    FailAndGoToMainMenu("Failed to update card collection. Please try again.");
                    return;
                }
            }

            HordeEditTab.Load(_editingTabGameObject);
            SelectedDeckId = (int) _dataManager.CachedDecksData.Decks[0].Id.Id;

            SelectOverlordAbilitiesPopup.OnSelectOverlordSkill += SelectOverlordAbilitiesHandler;
            SelectOverlordAbilitiesPopup.OnSaveSelectedSkill += SaveOverlordAbilitiesHandler;
            RenamePopup.OnSelectDeckName += SelectDeckNameHandler;
            RenamePopup.OnSaveNewDeckName += SaveNewDeckNameHandler;

            LoadTabs();
        }

        public void Hide()
        {
            Dispose();

            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;

            SelectOverlordAbilitiesPopup.OnSelectOverlordSkill -= SelectOverlordAbilitiesHandler;
            SelectOverlordAbilitiesPopup.OnSaveSelectedSkill -= SaveOverlordAbilitiesHandler;
            RenamePopup.OnSelectDeckName -= SelectDeckNameHandler;
            RenamePopup.OnSaveNewDeckName -= SaveNewDeckNameHandler;

            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }

        public void Dispose()
        {
            HordeSelectDeckTab.Dispose();
            HordeEditTab.Dispose();
        }

        #endregion

        private void UpdatePageScaleToMatchResolution()
        {
            float screenRatio = (float) Screen.width / Screen.height;
            if (screenRatio < 1.76f)
            {
                _selfPage.transform.localScale = Vector3.one * 0.93f;
            }
        }

        #region UI Handlers

        public List<Deck> GetDeckList()
        {
            if (_tutorialManager.IsTutorial)
            {
                List<Deck> tutorialDeckList = new List<Deck>();
                if (_dataManager.CachedUserLocalData.TutorialSavedDeck != null)
                {
                    tutorialDeckList.Add(_dataManager.CachedUserLocalData.TutorialSavedDeck);
                    SelectedDeckId = (int) _dataManager.CachedUserLocalData.TutorialSavedDeck.Id.Id;
                }

                return tutorialDeckList;
            }

            return _dataManager.CachedDecksData.Decks.ToList();
        }

        #endregion

        #region Data and State

        public void AssignCurrentDeck(int deckIndex)
        {
            SelectDeckIndex = deckIndex;
            AssignCurrentDeck();
        }

        public void AssignCurrentDeck()
        {
            Deck deck = HordeSelectDeckTab.GetSelectedDeck();
            if (deck != null)
            {
                CurrentEditDeck = deck.Clone();

                //CurrentEditOverlord = _dataManager.CachedOverlordData.Overlords.Single(overlord => overlord.Prototype.Id == CurrentEditDeck.OverlordId);
                IsEditingNewDeck = false;
            }
        }

        public void OpenDeckPage(int deckId)
        {
            SelectedDeckId = deckId;
            AssignCurrentDeck();
        }

        public void AssignNewDeck(OverlordId overlordId)
        {
            CurrentEditDeck = CreateNewDeckData(overlordId);
            IsEditingNewDeck = true;
        }

        private void SelectOverlordAbilitiesHandler(Enumerators.Skill primarySkill, Enumerators.Skill secondarySkill)
        {
            CurrentEditDeck.PrimarySkill = primarySkill;
            CurrentEditDeck.SecondarySkill = secondarySkill;
        }

        private void SaveOverlordAbilitiesHandler(Enumerators.Skill primarySkill, Enumerators.Skill secondarySkill)
        {
            CurrentEditDeck.PrimarySkill = primarySkill;
            CurrentEditDeck.SecondarySkill = secondarySkill;

            HordeEditTab.GetCustomDeck().ChangeAbilities(primarySkill, secondarySkill);
        }

        private void SelectDeckNameHandler(string deckName)
        {
            CurrentEditDeck.Name = deckName;
            ChangeTab(Tab.Editing);
        }

        private void SaveNewDeckNameHandler(string deckName)
        {
            CurrentEditDeck.Name = deckName;
            HordeSelectDeckTab.ChangeSelectedDeckName(deckName);
            HordeEditTab.GetCustomDeck().ChangeDeckName(deckName);
        }

        private Deck CreateNewDeckData(OverlordId overlordId)
        {
            Deck deck = new Deck(
                new DeckId(-1),
                overlordId,
                GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().GenerateDeckName(),
                new List<DeckCardData>(),
                0,
                0
            );
            return deck;
        }

        public void ChangeTab(Tab newTab)
        {
            Log.Info("change tab to = " + newTab);
            Tab oldTabl = _tab;
            _tab = newTab;

            ShowTabGameObject(newTab);

            UpdateAreaBarPopup(newTab != Tab.Editing);

            switch (newTab)
            {
                case Tab.None:
                    break;
                case Tab.SelectDeck:
                    _uiManager.GetPopup<ElementFilterPopup>().ResetSelectedFactionList();
                    HordeSelectDeckTab.InputFieldApplyFilter();
                    break;
                case Tab.Editing:
                    HordeEditTab.Show(SelectedDeckId);
                    break;
            }

            if (oldTabl != Tab.None && oldTabl != newTab)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.HordeTabChanged);
            }
        }

        private void ShowTabGameObject(Tab tab)
        {
            switch (tab) {
                case Tab.SelectDeck:
                    _selectDeckGameObject.SetActive(true);
                    _editingTabGameObject.SetActive(false);
                    break;
                case Tab.Editing:
                    _selectDeckGameObject.SetActive(false);
                    _editingTabGameObject.SetActive(true);
                    break;
                case Tab.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(tab), tab, null);
            }
        }

        private void UpdateAreaBarPopup(bool isShow)
        {
            var areaBarPopUp = GameClient.Get<IUIManager>().GetPopup<AreaBarPopup>();
            if (isShow)
                areaBarPopUp.Show();
            else
            {
                areaBarPopUp.Hide();
            }
        }

        #endregion

        #region Display

        private void LoadTabs()
        {
            _tab = Tab.None;
            ChangeTab(Tab.SelectDeck);
        }

        /*private void ApplyDeckByDefault()
        {
            _cacheDeckListToDisplay = GetDeckList();
            _deckPageIndex = 0;
            UpdateDeckInfoObjects();
        }*/

        #endregion

        private void FailAndGoToMainMenu(string customMessage = null)
        {
            _uiManager.HidePopup<LoadingOverlayPopup>();
            _uiManager.DrawPopup<WarningPopup>(customMessage ?? "Something went wrong.\n Please try again.");
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU, true);
        }
    }
}
