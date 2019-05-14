using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using DG.Tweening;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
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

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;
        
        private IAnalyticsManager _analyticsManager;

        public HordeEditingTab HordeEditTab;

        public OverlordSelectionTab SelectOverlordTab;

        public OverlordSkillSelectionTab SelectOverlordSkillTab;
        
        private GameObject _selfPage;

        private GameObject[] _tabObjects;

        private Sprite _spriteDeckThumbnailNormal,
                       _spriteDeckThumbnailSelected;

        private Transform _trayButtonBack,
                          _trayButtonAuto,
                          _paginationGroup;
                          
        public Transform LocatorCollectionCards,
                         LocatorDeckCards;

        private Button _buttonNewDeck,
                       _buttonBack,
                       _buttonSelectDeckFilter,
                       _buttonEdit,
                       _buttonDelete,
                       _buttonRename,
                       _buttonLeftArrow,
                       _buttonRightArrow,
                       _buttonSaveRenameDeck;

        private TMP_InputField _inputFieldRenameDeckName,
                               _inputFieldSearchDeckName;

        private TextMeshProUGUI _textSelectOverlordSkillDeckname;

        private GameObject _imagePageDotNormal,
                        _imagePageDotSelected;

        private List<DeckInfoObject> _deckInfoObjectList;
        
        public SimpleScrollNotifier DragAreaDeck, 
                          DragAreaCollections; 
                         
        private CardHighlightingVFXItem _highlightingVFXItem;   
        
        public event Action<Tab> EventChangeTab;

        #region Cache Data
        
        private const int _deckInfoAmountPerPage = 4;

        public enum Tab
        {
            None = -1,
            SelectDeck = 0,
            Rename = 1,
            Editing = 2,
            SelectOverlord = 3,
            SelecOverlordSkill = 4,
        }
        
        private Tab _tab;
        
        public int SelectDeckIndex;

        public Deck CurrentEditDeck;

        public OverlordModel CurrentEditOverlord;

        public bool IsEditingNewDeck;    
        
        public bool IsRenameWhileEditing;    
        
        private int _deckPageIndex;

        private List<Deck> _cacheDeckListToDisplay;

        #endregion

        #region IUIElement

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            
            _deckInfoObjectList = new List<DeckInfoObject>();
            _cacheDeckListToDisplay = new List<Deck>();
            SelectDeckIndex = 0;       

            HordeEditTab = new HordeEditingTab();
            HordeEditTab.Init();
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
            
            _inputFieldRenameDeckName = _selfPage.transform.Find("Tab_Rename/Panel_Content/InputText_DeckName").GetComponent<TMP_InputField>();
            _inputFieldRenameDeckName.onEndEdit.AddListener(OnInputFieldRenameEndedEdit);
            _inputFieldRenameDeckName.text = "Deck Name";
            
            _inputFieldSearchDeckName = _selfPage.transform.Find("Tab_SelectDeck/Panel_FrameComponents/Upper_Items/InputText_SearchDeckName").GetComponent<TMP_InputField>();
            _inputFieldSearchDeckName.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            _inputFieldSearchDeckName.text = "SEARCH";

            _trayButtonBack = _selfPage.transform.Find("Panel_Frame/Image_ButtonBackTray");
            _trayButtonBack.gameObject.SetActive(false);
            
            _trayButtonAuto = _selfPage.transform.Find("Panel_Frame/Image_ButtonAutoTray");
            _trayButtonAuto.gameObject.SetActive(false);
            
            _paginationGroup = _selfPage.transform.Find("Tab_SelectDeck/Panel_Content/Pagination_Group");

            _imagePageDotNormal = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/DeckSelection/Image_CircleDot_Normal");
            _imagePageDotSelected = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/DeckSelection/Image_CircleDot_Selected");            
            
            LocatorCollectionCards = _selfPage.transform.Find("Tab_Editing/Panel_Content/Locator_CollectionCards");            
            LocatorDeckCards = _selfPage.transform.Find("Tab_Editing/Panel_Content/Locator_DeckCards");

            _spriteDeckThumbnailNormal = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/deck_thumbnail_normal");
            _spriteDeckThumbnailSelected = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/deck_thumbnail_selected");           

            _textSelectOverlordSkillDeckname = _selfPage.transform.Find("Tab_SelectOverlordSkill/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();            
             
            DragAreaDeck = _selfPage.transform.Find("Tab_Editing/Panel_Content/DragArea_Deck").GetComponent<SimpleScrollNotifier>();
            DragAreaCollections = _selfPage.transform.Find("Tab_Editing/Panel_Content/DragArea_Collections").GetComponent<SimpleScrollNotifier>();
            
            _highlightingVFXItem = new CardHighlightingVFXItem(Object.Instantiate(
            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/UI/ArmyCardSelection"), _selfPage.transform, true));
            
            UpdatePageScaleToMatchResolution();

            HordeEditTab.Show(_selfPage);
            SelectOverlordTab.Show(_selfPage);
            SelectOverlordSkillTab.Show(_selfPage);
            
            LoadButtons();
            LoadObjects();            
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
            HordeEditTab.Dispose();
            SelectOverlordTab.Dispose();
            SelectOverlordSkillTab.Dispose();
            _deckInfoObjectList.Clear();  
            _cacheDeckListToDisplay.Clear();
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

        private void ButtonNewDeckHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonNewDeck.name))
                return;

            if (_dataManager.CachedDecksData.Decks.Count >= Constants.MaxDecksCount && !_tutorialManager.IsTutorial)
            {
                _uiManager.DrawPopup<WarningPopup>(Constants.ErrorMessageForMaxDecks);
                return;
            }


            PlayClickSound();
            ChangeTab(Tab.SelectOverlord);
        }
        
        private void ButtonLeftArrowHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonLeftArrow.name))
                return;
                
            PlayClickSound();
            int previousIndex = _deckPageIndex;
            MoveDeckPageIndex(-1);
            
            if (previousIndex == _deckPageIndex)
                return;
                
            UpdateDeckInfoObjects(); 
            ChangeSelectDeckIndex(GetDefaultDeckIndex());          
        }
        
        private void ButtonRightArrowHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonRightArrow.name))
                return;

            PlayClickSound();
            int previousIndex = _deckPageIndex;
            MoveDeckPageIndex(1);
            if (previousIndex == _deckPageIndex)
                return;
                
            UpdateDeckInfoObjects();
            ChangeSelectDeckIndex(GetDefaultDeckIndex());       
        }
        
        private void ButtonBackHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonBack.name))
                return;

            PlayClickSound();
            if (_tab == Tab.Editing)
            {
                _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmSaveDeckHandler;
                _uiManager.DrawPopup<QuestionPopup>("Would you like to save your progress?");
            }
            else
            {
                ChangeTab(Tab.SelectDeck);
            }
        }
        
        private void ConfirmSaveDeckHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmSaveDeckHandler;
            
            if (status)
            {
                HordeEditTab.SaveDeck(Tab.SelectDeck);
            }
            else
            {                
                ChangeTab(Tab.SelectDeck);        
            }  
        }
        
        private void ButtonSelectDeckFilterHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonSelectDeckFilter.name))
                return;

            PlayClickSound();
            _uiManager.DrawPopup<ElementFilterPopup>();
            ElementFilterPopup popup = _uiManager.GetPopup<ElementFilterPopup>();
            popup.ActionPopupHiding += FilterPopupHidingHandler;
        }
        
        private void FilterPopupHidingHandler()
        {
            if(CheckAvailableDeckExist())
            {   
                ApplyDeckFilter();
                ElementFilterPopup popup = _uiManager.GetPopup<ElementFilterPopup>();
                popup.ActionPopupHiding -= FilterPopupHidingHandler;
            }
            else
            {
                _uiManager.DrawPopup<WarningPopup>("No decks found for the selected faction.");
                _uiManager.DrawPopup<ElementFilterPopup>();
            }
        }

        private void ButtonEditHandler()
        {            
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonEdit.name))
                return;
                
            PlayClickSound();
            ChangeTab(Tab.Editing);
        }        
        
        private void ButtonDeleteHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonDelete.name))
                return;

            PlayClickSound();
            if (GetDeckList().Count <= 1)
            {
                OpenAlertDialog("Cannot delete. You must have at least one deck.");
                return;
            }



            Deck deck = GetSelectedDeck();
            if (deck != null)
            {
                _buttonDelete.enabled = false;
                _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmDeleteDeckReceivedHandler;
                _uiManager.DrawPopup<QuestionPopup>("Are you sure you want to delete " + deck.Name + "?");
            }
        }
        
        private void ButtonRenameHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonRename.name))
                return;
            
            PlayClickSound();
            ChangeTab(Tab.Rename);
        }
        
        private void ButtonSaveRenameDeckHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonSaveRenameDeck.name))
                return;

            PlayClickSound();
            string newName = _inputFieldRenameDeckName.text;
            HordeEditTab.RenameDeck(newName);
        }

        public void OnInputFieldRenameEndedEdit(string value)
        {
            
        }
        
        public void OnInputFieldSearchEndedEdit(string value)
        {
            ApplyDeckSearch();
        }
        
        private void ConfirmDeleteDeckReceivedHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmDeleteDeckReceivedHandler;

            if (!status)
            {
                _buttonDelete.enabled = true;
                return;
            }
                
            Deck deck = GetSelectedDeck();

            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
            deckGeneratorController.FinishDeleteDeck += FinishDeleteDeck;
            deckGeneratorController.ProcessDeleteDeck(deck);

            _analyticsManager.SetEvent(AnalyticsManager.EventDeckDeleted);
        }
        
        private void FinishDeleteDeck(bool success, Deck deck)
        {
            _buttonDelete.enabled = true;
            
            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().FinishDeleteDeck -= FinishDeleteDeck; 

            _cacheDeckListToDisplay = GetDeckList();
            SelectDeckIndex = Mathf.Min(SelectDeckIndex, _cacheDeckListToDisplay.Count-1);
            ChangeTab(Tab.SelectDeck);
        }

        #endregion

        #region Data and State
        
        private int GetDefaultDeckIndex()
        {
            return _deckPageIndex == 0 ? 1 : 0;
        }

        public List<Deck> GetDeckList()
        {
            if (_tutorialManager.IsTutorial)
            {
                List<Deck> tutorialDeckList = new List<Deck>();
                if (_dataManager.CachedUserLocalData.TutorialSavedDeck != null)
                {
                    tutorialDeckList.Add(_dataManager.CachedUserLocalData.TutorialSavedDeck);
                }
                else
                {
                    tutorialDeckList.Add(_dataManager.CachedDecksData.Decks[0]);
                }
                return tutorialDeckList;
            }
            else
            {
                return _dataManager.CachedDecksData.Decks.ToList();
            }
        }
        
        private Deck GetSelectedDeck()
        {
            List<Deck> deckList = GetDeckList();
            if(deckList.Count <= 0)
            {
                SelectDeckIndex = 0;
                return null;
            }
            else if(SelectDeckIndex < 0 || SelectDeckIndex >= deckList.Count)
            {
                SelectDeckIndex = 0;
            }
            return deckList[SelectDeckIndex];
        }

        public void AssignCurrentDeck()
        { 
            CurrentEditDeck = GetSelectedDeck().Clone();
            CurrentEditOverlord = _dataManager.CachedOverlordData.Overlords[CurrentEditDeck.OverlordId];
            IsEditingNewDeck = false;
        }

        public void AssignCurrentDeck(int deckIndex)
        {
            SelectDeckIndex = deckIndex;
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
                -1,
                CurrentEditOverlord.OverlordId,
                GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().GenerateDeckName(),                
                new List<DeckCardData>(),
                0,
                0
            );
            return deck;
        }

        public void ChangeTab(Tab newTab)
        {
            Tab oldTabl = _tab;
            _tab = newTab;            
            
            for (int i = 0; i < _tabObjects.Length;++i)
            {
                GameObject tabObject = _tabObjects[i];
                tabObject.SetActive(i == (int)newTab);
            }

            UpdateShowBackButton
            (
                newTab == Tab.Editing || 
                newTab == Tab.Rename ||
                newTab == Tab.SelectOverlord ||
                newTab == Tab.SelecOverlordSkill
            );
            
            UpdateShowAutoButton
            (
                newTab == Tab.Editing
            );
            
            switch (newTab)
            {
                case Tab.None:
                    break;
                case Tab.SelectDeck:
                    _inputFieldSearchDeckName.text = "";
                    ApplyDeckByLastSelected();
                    break;
                case Tab.Rename:
                    _inputFieldRenameDeckName.text = CurrentEditDeck.Name;
                    break;
                case Tab.Editing:
                    break;
                case Tab.SelectOverlord:                    
                    break;
                case Tab.SelecOverlordSkill:
                    _textSelectOverlordSkillDeckname.text = CurrentEditDeck.Name;
                    break;
                default:
                    break;
            }

            EventChangeTab?.Invoke(_tab);

            if (oldTabl != Tab.None && oldTabl != newTab)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.HordeTabChanged);
            }
        }
        
        private void ChangeSelectDeckIndex(int newIndexInPage)
        {
            UpdateSelectedDeckDisplay(newIndexInPage);
            if(_deckPageIndex == 0)
            {
                SelectDeckIndex = newIndexInPage-1;
            }
            else
            {
                SelectDeckIndex = newIndexInPage + (_deckPageIndex-1) * _deckInfoAmountPerPage + (_deckInfoAmountPerPage-1);
            }
            
            if (_tutorialManager.IsTutorial && _dataManager.CachedDecksData.Decks.Count > 1)
            {
                SelectDeckIndex = 1;
            }
            
            AssignCurrentDeck();    
        }

        private void UpdateShowBackButton(bool isShow)
        {
            _trayButtonBack.gameObject.SetActive(isShow);
        }

        private void UpdateShowAutoButton(bool isShow)
        {
            _trayButtonAuto.gameObject.SetActive(isShow);
        }
        
        private void MoveDeckPageIndex(int direction)
        {
            _deckPageIndex = Mathf.Clamp(_deckPageIndex + direction, 0, GetDeckPageAmount(_cacheDeckListToDisplay) - 1);
        }
        
        private int GetDeckPageAmount(List<Deck> deckList)
        {
            if(deckList.Count <= _deckInfoAmountPerPage-1)
            {
                return 1;
            }
            else
            {
                return (deckList.Count - _deckInfoAmountPerPage) / _deckInfoAmountPerPage + 2;
            }            
        }
        
        private List<Deck> GetDeckListByElementToDisplay(Enumerators.Faction faction)
        {
            List<Deck> deckList = GetDeckList();

            List<Deck> deckListToDisplay = new List<Deck>();
            for (int i = 0; i < deckList.Count; ++i)
            {
                OverlordModel overlord = _dataManager.CachedOverlordData.Overlords[deckList[i].OverlordId];
                if( faction == overlord.Faction )
                        deckListToDisplay.Add(deckList[i]);                
            }

            return deckListToDisplay;
        }
        
        private List<Deck> GetDeckListBySearchKeywordToDisplay()
        {
            List<Deck> deckList = GetDeckList();
            string keyword = _inputFieldSearchDeckName.text.Trim().ToLower();
            
            if(string.IsNullOrEmpty(keyword))            
                return deckList;            

            List<Deck> deckListToDisplay = new List<Deck>();
            for (int i = 0; i < deckList.Count; ++i)
            {
                string deckName = deckList[i].Name.Trim().ToLower();
                if(deckName.Contains(keyword))
                    deckListToDisplay.Add(deckList[i]);                                        
            }
            
            if(deckListToDisplay.Count <= 0)
            {
                OpenAlertDialog($"No decks found with that search.");
                return deckList;
            }

            return deckListToDisplay;
        }
        
        private List<Deck> GetDeckListFromSelectedPageToDisplay(List<Deck> deckList, bool displayNewDeckButton = false)
        {
            List<Deck> deckListFromSelectedPageToDisplay = new List<Deck>();
            
            int startIndex = 0;
            int endIndex = _deckInfoAmountPerPage-1;
            if(!displayNewDeckButton)
            {
                startIndex = (_deckInfoAmountPerPage-1) + (_deckPageIndex-1) * _deckInfoAmountPerPage;
                endIndex = startIndex + _deckInfoAmountPerPage;
            }

            for (int i = 0; i < deckList.Count; ++i)
            {
                if (i >= startIndex && i < endIndex)
                {
                    deckListFromSelectedPageToDisplay.Add(deckList[i]);
                }
            }

            return deckListFromSelectedPageToDisplay;
        }

        #endregion

        #region Display

        private void LoadTabs()
        {
            _tab = Tab.None;
            ChangeTab(Tab.SelectDeck);
        }
        
        private void LoadButtons()
        {
            _buttonNewDeck = _selfPage.transform.Find("Tab_SelectDeck/Panel_Content/Button_BuildNewDeck").GetComponent<Button>();
            _buttonNewDeck.onClick.AddListener(ButtonNewDeckHandler);
            
            _buttonLeftArrow = _selfPage.transform.Find("Tab_SelectDeck/Panel_Content/Button_LeftArrow").GetComponent<Button>();
            _buttonLeftArrow.onClick.AddListener(ButtonLeftArrowHandler);
            
            _buttonRightArrow = _selfPage.transform.Find("Tab_SelectDeck/Panel_Content/Button_RightArrow").GetComponent<Button>();
            _buttonRightArrow.onClick.AddListener(ButtonRightArrowHandler);
            
            _buttonBack = _selfPage.transform.Find("Panel_Frame/Image_ButtonBackTray/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);
            
            _buttonSelectDeckFilter = _selfPage.transform.Find("Tab_SelectDeck/Panel_FrameComponents/Upper_Items/Button_Filter").GetComponent<Button>();
            _buttonSelectDeckFilter.onClick.AddListener(ButtonSelectDeckFilterHandler);           
            
            _buttonEdit = _selfPage.transform.Find("Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Edit").GetComponent<Button>();
            _buttonEdit.onClick.AddListener(ButtonEditHandler);
            
            _buttonDelete = _selfPage.transform.Find("Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Delete").GetComponent<Button>();
            _buttonDelete.onClick.AddListener(ButtonDeleteHandler);
            
            _buttonRename = _selfPage.transform.Find("Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Rename").GetComponent<Button>();
            _buttonRename.onClick.AddListener(ButtonRenameHandler);
            
            _buttonSaveRenameDeck = _selfPage.transform.Find("Tab_Rename/Panel_FrameComponents/Lower_Items/Button_Save").GetComponent<Button>();
            _buttonSaveRenameDeck.onClick.AddListener(ButtonSaveRenameDeckHandler);           
        }

        private void LoadObjects()
        {
            _deckInfoObjectList.Clear();
            for (int i = 0; i < 4; ++i)
            {
                DeckInfoObject deckInfoObject = new DeckInfoObject();
                
                string path = $"Tab_SelectDeck/Panel_Content/Button_DeckSelect_{i}";
                deckInfoObject.Button = _selfPage.transform.Find(path).GetComponent<Button>();
                deckInfoObject.TextDeckName = _selfPage.transform.Find(path+"/Text_DeckName").GetComponent<TextMeshProUGUI>();
                deckInfoObject.TextCardsAmount = _selfPage.transform.Find(path+"/Text_CardsAmount").GetComponent<TextMeshProUGUI>();
                deckInfoObject.ImagePanel = _selfPage.transform.Find(path+"/Image_DeckThumbnailNormal").GetComponent<Image>();                
                deckInfoObject.ImageOverlordThumbnail = _selfPage.transform.Find(path+"/Image_DeckThumbnail").GetComponent<Image>();
                deckInfoObject.ImageAbilityIcons = new Image[]
                {
                    _selfPage.transform.Find(path+"/Image_SkillIcon_1").GetComponent<Image>(),
                    _selfPage.transform.Find(path+"/Image_SkillIcon_2").GetComponent<Image>()
                };
                _deckInfoObjectList.Add(deckInfoObject);
            }

            _tabObjects = new GameObject[]
            {
                _selfPage.transform.Find("Tab_SelectDeck").gameObject,
                _selfPage.transform.Find("Tab_Rename").gameObject,
                _selfPage.transform.Find("Tab_Editing").gameObject,
                _selfPage.transform.Find("Tab_SelectOverlord").gameObject,
                _selfPage.transform.Find("Tab_SelectOverlordSkill").gameObject
            };
        }
        
        public void UpdateDeckInfoObjects()
        {
            bool displayNewDeckButton = (_deckPageIndex == 0);
            _buttonNewDeck.gameObject.SetActive(displayNewDeckButton);
            _deckInfoObjectList[0].Button.gameObject.SetActive(!displayNewDeckButton);
            
            List<Deck> deckListToDisplay = GetDeckListFromSelectedPageToDisplay(_cacheDeckListToDisplay, displayNewDeckButton);
           
            int startObjectIndex = displayNewDeckButton?1:0;
            int deckDataIndex = 0;

            for (int i=startObjectIndex; i < _deckInfoObjectList.Count; ++i, ++deckDataIndex)
            {
                
                DeckInfoObject deckInfoObject = _deckInfoObjectList[i];
                if(deckDataIndex >= deckListToDisplay.Count)
                {
                    deckInfoObject.Button.gameObject.SetActive(false);
                    continue;
                }

                int index = i;
                deckInfoObject.Button.gameObject.SetActive(true);

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR
                MultiPointerClickHandler multiPointerClickHandler = deckInfoObject.Button.gameObject.AddComponent<MultiPointerClickHandler>();                
                multiPointerClickHandler.DoubleClickReceived += ()=>
                {                    
                    ChangeSelectDeckIndex(index);
                    ButtonEditHandler();
                    PlayClickSound();
                };
#endif
                
                Deck deck = deckListToDisplay[deckDataIndex];
                
                string deckName = deck.Name;
                int cardsAmount = deck.GetNumCards();
                OverlordModel overlord = _dataManager.CachedOverlordData.Overlords[deck.OverlordId];

                deckInfoObject.TextDeckName.text = deckName;
                if (_tutorialManager.IsTutorial)
                {
                    deckInfoObject.TextCardsAmount.text = $"{cardsAmount}/{_tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount}";
                }
                else
                {
                    deckInfoObject.TextCardsAmount.text = $"{cardsAmount}/{Constants.MaxDeckSize}";               
                }
                deckInfoObject.ImageOverlordThumbnail.sprite = GetOverlordThumbnailSprite(overlord.Faction);

                if(deck.PrimarySkill == Enumerators.Skill.NONE)
                {
                    deckInfoObject.ImageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
                }
                else
                {
                    string iconPath = overlord.GetSkill(deck.PrimarySkill).IconPath;
                    deckInfoObject.ImageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
                }
                
                if(deck.SecondarySkill == Enumerators.Skill.NONE)
                {
                    deckInfoObject.ImageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
                }
                else
                {
                    string iconPath = overlord.GetSkill(deck.SecondarySkill).IconPath;
                    deckInfoObject.ImageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);                
                }
                
                deckInfoObject.Button.onClick.RemoveAllListeners();                
                deckInfoObject.Button.onClick.AddListener(() =>
                {
                    ChangeSelectDeckIndex(index);
                    PlayClickSound();
                });
            }

            UpdatePageDotObjects(_cacheDeckListToDisplay);
            ChangeSelectDeckIndex(GetDefaultDeckIndex());
        }
        
        private void UpdatePageDotObjects(List<Deck> deckList)
        {
            foreach (Transform child in _paginationGroup)
            {
                Object.Destroy(child.gameObject);
            }
            
            int page = _deckPageIndex;
            int maxPage = GetDeckPageAmount(deckList);
            
            for (int i = 0; i < maxPage; ++i)
            {
                GameObject pageDot = Object.Instantiate
                (
                    i == page? _imagePageDotSelected:_imagePageDotNormal
                );
                pageDot.transform.SetParent(_paginationGroup);
                pageDot.transform.localScale = _imagePageDotNormal.transform.localScale;
                pageDot.SetActive(true);
            }
        }
        
        private void ApplyDeckByDefault()
        {
            _cacheDeckListToDisplay = GetDeckList();
            _deckPageIndex = 0;
            UpdateDeckInfoObjects();
        }
        
        private void ApplyDeckByLastSelected()
        {
            _cacheDeckListToDisplay = GetDeckList();
            
            int indexInPage = 0;
            if(SelectDeckIndex < _deckInfoAmountPerPage-1)
            {
                _deckPageIndex = 0;
                indexInPage = SelectDeckIndex + 1;
            }
            else
            {
                int deckIndexAfterSubtractFistPage = SelectDeckIndex - (_deckInfoAmountPerPage - 1);
                _deckPageIndex = (deckIndexAfterSubtractFistPage / _deckInfoAmountPerPage) + 1;
                indexInPage = deckIndexAfterSubtractFistPage % _deckInfoAmountPerPage;
            }

            UpdateDeckInfoObjects();
            ChangeSelectDeckIndex(indexInPage);
        }

        public void ApplyDeckFilter()
        {
            _inputFieldSearchDeckName.text = "";            
            
            ElementFilterPopup elementFilterPopup = _uiManager.GetPopup<ElementFilterPopup>();
            if(elementFilterPopup.SelectedFactionList.Count == elementFilterPopup.AvailableFactionList.Count)
            {
                _cacheDeckListToDisplay = GetDeckList();
            }
            else
            {
                List<Deck> decks = new List<Deck>();
                List<Deck> deckListByFaction;
                foreach (Enumerators.Faction faction in elementFilterPopup.SelectedFactionList)
                {
                    deckListByFaction = GetDeckListByElementToDisplay(faction);
                    if (deckListByFaction.Count <= 0)
                        continue;
    
                    decks = decks.Union(deckListByFaction).ToList();
                }
                _cacheDeckListToDisplay = decks;
            }
            
            _deckPageIndex = 0;
            UpdateDeckInfoObjects();
        }
        
        private void ApplyDeckSearch()
        {
            _cacheDeckListToDisplay = GetDeckListBySearchKeywordToDisplay();
            _deckPageIndex = 0;
            UpdateDeckInfoObjects();
        }
        
        private bool CheckAvailableDeckExist()
        {
            bool isAvailable = false;
            ElementFilterPopup elementFilterPopup = _uiManager.GetPopup<ElementFilterPopup>();
            List<Deck> deckListByFaction;
            foreach(Enumerators.Faction faction in elementFilterPopup.SelectedFactionList)
            {
                deckListByFaction = GetDeckListByElementToDisplay(faction);
                if (deckListByFaction.Count > 0)
                {
                    isAvailable = true;
                    break;
                }
            }
            return isAvailable;
        }

        private void UpdateSelectedDeckDisplay(int selectedDeckIndex)
        {
            for (int i = 0; i < _deckInfoObjectList.Count; ++i)
            {
                DeckInfoObject deckInfoObject = _deckInfoObjectList[i];
                Sprite sprite = (i == selectedDeckIndex ? _spriteDeckThumbnailSelected : _spriteDeckThumbnailNormal);
                deckInfoObject.ImagePanel.sprite = sprite;
            }
        }

        private Sprite GetOverlordThumbnailSprite(Enumerators.Faction overlordFaction)
        {
            string path = "Images/UI/MyDecks/OverlordDeckThumbnail";
            switch(overlordFaction)
            {
                case Enumerators.Faction.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_air"); 
                case Enumerators.Faction.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_fire"); 
                case Enumerators.Faction.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_earth"); 
                case Enumerators.Faction.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_toxic"); 
                case Enumerators.Faction.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_water"); 
                case Enumerators.Faction.LIFE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_life"); 
                default:
                    Log.Info($"No Overlord thumbnail found for faction {overlordFaction}");
                    return null;
            }        
        }

        private class DeckInfoObject
        {
            public Button Button;
            public TextMeshProUGUI TextDeckName;
            public Image ImagePanel;
            public Image ImageOverlordThumbnail;
            public Image[] ImageAbilityIcons;
            public TextMeshProUGUI TextCardsAmount;
        }

#endregion

#region Util

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
        
        public void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

#endregion
    }
}
