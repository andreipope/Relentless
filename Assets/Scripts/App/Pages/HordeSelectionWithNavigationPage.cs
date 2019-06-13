using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class HordeSelectionWithNavigationPage : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(HordeSelectionWithNavigationPage));

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private ITutorialManager _tutorialManager;

        public SelectDeckTab HordeSelectDeckTab;
        public HordeEditingTab HordeEditTab;
        public HordeRenameTab RenameTab;
        public OverlordSelectionTab SelectOverlordTab;
        public OverlordSkillSelectionTab SelectOverlordSkillTab;

        private GameObject _selfPage;

        private GameObject[] _tabObjects;

        private TextMeshProUGUI _textSelectOverlordSkillDeckName;

        private CardHighlightingVFXItem _highlightingVFXItem;

        public event Action<Tab> EventChangeTab;

        #region Cache Data

        public enum Tab
        {
            None = -1,
            SelectDeck = 0,
            Rename = 1,
            Editing = 2,
            SelectOverlord = 3,
            SelectOverlordSkill = 4
        }

        private Tab _tab;

        public int SelectDeckIndex;

        public int _selectedDeckId;

        public Deck CurrentEditDeck;

        public OverlordUserInstance CurrentEditOverlord;

        public bool IsEditingNewDeck;

        public bool IsRenameWhileEditing;

        #endregion

        #region IUIElement

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            SelectDeckIndex = 0;

            HordeSelectDeckTab = new SelectDeckTab();
            HordeSelectDeckTab.Init();

            HordeEditTab = new HordeEditingTab();
            HordeEditTab.Init();

            RenameTab = new HordeRenameTab();
            RenameTab.Init();

            SelectOverlordTab = new OverlordSelectionTab();
            SelectOverlordTab.Init();

            SelectOverlordSkillTab = new OverlordSkillSelectionTab();
            SelectOverlordSkillTab.Init();
        }

        public void Update()
        {
            HordeEditTab.Update();
            SelectOverlordTab.Update();
            SelectOverlordSkillTab.Update();
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MyDecksPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_DECKS);
            _uiManager.DrawPopup<AreaBarPopup>();

            _textSelectOverlordSkillDeckName = _selfPage.transform.Find("Tab_SelectOverlordSkill/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();

            _highlightingVFXItem = new CardHighlightingVFXItem(Object.Instantiate(
            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/UI/ArmyCardSelection"), _selfPage.transform, true));

            UpdatePageScaleToMatchResolution();

            GameObject selectDeckObj = _selfPage.transform.Find("Tab_SelectDeck").gameObject;
            GameObject renameDeckObj = _selfPage.transform.Find("Tab_Rename").gameObject;
            GameObject editingTabObj = _selfPage.transform.Find("Tab_Editing").gameObject;
            GameObject selectOverlordObj = _selfPage.transform.Find("Tab_SelectOverlord").gameObject;
            GameObject selectOverlordSkillObj = _selfPage.transform.Find("Tab_SelectOverlordSkill").gameObject;

            HordeSelectDeckTab.Show(selectDeckObj);
            HordeEditTab.Load(editingTabObj);
            RenameTab.Show(renameDeckObj);
            SelectOverlordTab.Show(selectOverlordObj);
            SelectOverlordSkillTab.Show(selectOverlordSkillObj);

            _selectedDeckId = (int)_dataManager.CachedDecksData.Decks[0].Id.Id;

            _tabObjects = new[]
            {
                selectDeckObj,
                renameDeckObj,
                editingTabObj,
                selectOverlordObj,
                selectOverlordSkillObj
            };

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

            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }

        public void Dispose()
        {
            HordeSelectDeckTab.Dispose();
            HordeEditTab.Dispose();
            SelectOverlordTab.Dispose();
            SelectOverlordSkillTab.Dispose();
            RenameTab.Dispose();
        }

        #endregion

        private void UpdatePageScaleToMatchResolution()
        {
            float screenRatio = (float)Screen.width/Screen.height;
            if(screenRatio < 1.76f)
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
                    _selectedDeckId = (int)_dataManager.CachedUserLocalData.TutorialSavedDeck.Id.Id;
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
                CurrentEditOverlord = _dataManager.CachedOverlordData.Overlords.Single(overlord => overlord.Prototype.Id == CurrentEditDeck.OverlordId);
                IsEditingNewDeck = false;
            }
        }

        public void OpenDeckPage(int deckId)
        {
            _selectedDeckId = deckId;
            AssignCurrentDeck();
        }

        public void AssignNewDeck()
        {
            CurrentEditDeck = CreateNewDeckData();
            IsEditingNewDeck = true;
        }

        private Deck CreateNewDeckData()
        {
            Deck deck = new Deck(
                new DeckId(-1),
                CurrentEditOverlord.Prototype.Id,
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

            for (int i = 0; i < _tabObjects.Length;++i)
            {
                GameObject tabObject = _tabObjects[i];
                tabObject.SetActive(i == (int)newTab);
            }

            UpdateAreaBarPopup(newTab != Tab.Editing);

            switch (newTab)
            {
                case Tab.None:
                    break;
                case Tab.SelectDeck:
                    // TODO : object.InputFieldApplyFilter
                    HordeSelectDeckTab.InputFieldApplyFilter();
                    break;
                case Tab.Rename:
                    RenameTab.SetName(CurrentEditDeck.Name);
                    break;
                case Tab.Editing:
                    HordeEditTab.Show(_selectedDeckId);
                    break;
                case Tab.SelectOverlord:
                    break;
                case Tab.SelectOverlordSkill:
                    _textSelectOverlordSkillDeckName.text = CurrentEditDeck.Name;
                    break;
            }

            EventChangeTab?.Invoke(_tab);

            if (oldTabl != Tab.None && oldTabl != newTab)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.HordeTabChanged);
            }
        }

        private void UpdateAreaBarPopup(bool isShow)
        {
            var areaBarPopUp = GameClient.Get<IUIManager>().GetPopup<AreaBarPopup>();
            if(isShow)
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
    }
}
