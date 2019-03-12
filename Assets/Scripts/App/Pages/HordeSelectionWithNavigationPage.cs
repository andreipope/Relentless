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
                       _buttonRightArrow;

        public Button ButtonSaveRenameDeck;

        private TMP_InputField _inputFieldRenameDeckName,
                               _inputFieldSearchDeckName;

        private TextMeshProUGUI _textSelectOverlordSkillDeckname;

        private GameObject _imagePageDotNormal,
                        _imagePageDotSelected;

        private List<DeckInfoObject> _deckInfoObjectList;
        
        public GameObject DragAreaDeck, 
                          DragAreaCollections; 
                         
        private CardHighlightingVFXItem _highlightingVFXItem;   
        
        public event Action<TAB> EventChangeTab;

        #region Cache Data
        
        private const int _deckInfoAmountPerPage = 3;

        private const int _defaultDeckIndex = 0;

        public enum TAB
        {
            NONE = -1,
            SELECT_DECK = 0,
            RENAME = 1,
            EDITING = 2,
            SELECT_OVERLORD = 3,
            SELECT_OVERLORD_SKILL = 4,
        }
        
        private TAB _tab;
        
        public int SelectDeckIndex;

        public Deck CurrentEditDeck;

        public Hero CurrentEditHero;

        public bool IsEditingNewDeck;
        
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
             
            DragAreaDeck = _selfPage.transform.Find("Tab_Editing/Panel_Content/DragArea_Deck").gameObject;
            DragAreaCollections = _selfPage.transform.Find("Tab_Editing/Panel_Content/DragArea_Collections").gameObject;
            
            _highlightingVFXItem = new CardHighlightingVFXItem(Object.Instantiate(
            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/UI/ArmyCardSelection"), _selfPage.transform, true));

            HordeEditTab.Show(_selfPage);
            SelectOverlordTab.Show(_selfPage);
            SelectOverlordSkillTab.Show(_selfPage);
            
            LoadButtons();
            LoadObjects();            
            LoadTabs();

            UpdatePageScaleToMatchResolution();          
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
            ChangeTab(TAB.SELECT_OVERLORD);
        }
        
        private void ButtonLeftArrowHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonLeftArrow.name))
                return;

            MoveDeckPageIndex(-1);
            UpdateDeckInfoObjects(); 
            ChangeSelectDeckIndex(0);           
        }
        
        private void ButtonRightArrowHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonRightArrow.name))
                return;

            MoveDeckPageIndex(1);
            UpdateDeckInfoObjects();
            ChangeSelectDeckIndex(0);          
        }
        
        private void ButtonBackHandler()
        {
            ChangeTab(TAB.SELECT_DECK);
        }
        
        private void ButtonSelectDeckFilterHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonSelectDeckFilter.name))
                return;

            _uiManager.DrawPopup<ElementFilterPopup>();
            ElementFilterPopup popup = _uiManager.GetPopup<ElementFilterPopup>();
            popup.ActionPopupHiding += FilterPopupHidingHandler;
        }
        
        private void FilterPopupHidingHandler(Enumerators.SetType selectedSetType)
        {
            ApplyDeckFilter(selectedSetType);
            ElementFilterPopup popup = _uiManager.GetPopup<ElementFilterPopup>();
            popup.ActionPopupHiding -= FilterPopupHidingHandler;
        }

        private void ButtonEditHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonEdit.name))
                return;

            AssignCurrentDeck(false);
            ChangeTab(TAB.EDITING);
        }        
        
        private void ButtonDeleteHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonDelete.name))
                return;

            if (GetDeckList().Count <= 1)
            {
                OpenAlertDialog("Sorry, Not able to delete Last Deck.");
                return;
            }

            Deck deck = GetSelectedDeck();
            if (deck != null)
            {
                _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmDeleteDeckReceivedHandler;
                _uiManager.DrawPopup<QuestionPopup>("Do you really want to delete " + deck.Name + "?");
            }
        }
        
        private void ButtonRenameHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonRename.name))
                return;

            ChangeTab(TAB.RENAME);
        }
        
        private void ButtonSaveRenameDeckHandler()
        {
            Deck deck = GetSelectedDeck();
            string newName = _inputFieldRenameDeckName.text;
            HordeEditTab.ProcessRenameDeck(deck, newName);
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
                return;
                
            Deck deck = GetSelectedDeck();
            ProcessDeleteDeck(deck);

            _analyticsManager.SetEvent(AnalyticsManager.EventDeckDeleted);
        }

        #endregion

        #region Data and State

        public List<Deck> GetDeckList()
        {
            return _dataManager.CachedDecksData.Decks.ToList();
        }
        
        private Deck GetSelectedDeck()
        {
            List<Deck> deckList = GetDeckList();
            return deckList[SelectDeckIndex];
        }

        public void AssignCurrentDeck(bool isNewDeck)
        {
            IsEditingNewDeck = isNewDeck;
            if(IsEditingNewDeck)
            {
                CurrentEditDeck = CreateNewDeckData();
            }
            else
            {
                CurrentEditDeck = GetSelectedDeck().Clone();
                CurrentEditHero = _dataManager.CachedHeroesData.Heroes[CurrentEditDeck.HeroId];
            }
        }

        private Deck CreateNewDeckData()
        {
            Deck deck = new Deck(
                -1,
                CurrentEditHero.HeroId,
                GenerateDeckName(),                
                new List<DeckCardData>(),
                0,
                0
            );
            return deck;
        }
        
        private string GenerateDeckName()
        {
            int index = _dataManager.CachedDecksData.Decks.Count;
            string newName = "HORDE " + index;
            while (true)
            {
                bool isNameCollide = false;
                for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count; ++i)
                {
                    if (string.Equals(_dataManager.CachedDecksData.Decks[i].Name,newName))
                    {
                        isNameCollide = true;
                        ++index;
                        newName = "HORDE " + index;
                        break;
                    }
                }
                if (!isNameCollide)
                    return newName;
            }
        }

        public void ChangeTab(TAB newTab)
        {
            _tab = newTab;            
            
            for(int i=0; i<_tabObjects.Length;++i)
            {
                GameObject tabObject = _tabObjects[i];
                tabObject.SetActive(i == (int)newTab);
            }

            UpdateShowBackButton
            (
                newTab == TAB.EDITING || 
                newTab == TAB.RENAME ||
                newTab == TAB.SELECT_OVERLORD ||
                newTab == TAB.SELECT_OVERLORD_SKILL
            );
            
            UpdateShowAutoButton
            (
                newTab == TAB.EDITING
            );
            
            switch (newTab)
            {
                case TAB.NONE:
                    break;
                case TAB.SELECT_DECK:
                    _inputFieldSearchDeckName.text = "";
                    ApplyDeckByLastSelected();
                    break;
                case TAB.RENAME:
                    _inputFieldRenameDeckName.text = GetSelectedDeck().Name;
                    break;
                case TAB.EDITING:                                      
                    break;
                case TAB.SELECT_OVERLORD:                    
                    break;
                case TAB.SELECT_OVERLORD_SKILL:
                    _textSelectOverlordSkillDeckname.text = CurrentEditDeck.Name;
                    break;
                default:
                    break;
            }            
            
            EventChangeTab?.Invoke(_tab);
        }
        
        private void ChangeSelectDeckIndex(int newIndexInPage)
        {
            UpdateSelectedDeckDisplay(newIndexInPage);
            SelectDeckIndex = newIndexInPage + _deckPageIndex * _deckInfoAmountPerPage;
        }

        private void UpdateShowBackButton(bool isShow)
        {
            _trayButtonBack.gameObject.SetActive(isShow);
        }

        private void UpdateShowAutoButton(bool isShow)
        {
            _trayButtonAuto.gameObject.SetActive(isShow);
        }

        private async void ProcessDeleteDeck(Deck currentDeck)
        {
            try
            {
                _dataManager.CachedDecksData.Decks.Remove(currentDeck);
                _dataManager.CachedUserLocalData.LastSelectedDeckId = -1;
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                await _dataManager.SaveCache(Enumerators.CacheDataType.HEROES_DATA);

                await _backendFacade.DeleteDeck(
                    _backendDataControlMediator.UserDataModel.UserId,
                    currentDeck.Id
                );

                Debug.Log($" ====== Delete Deck {currentDeck.Id} Successfully ==== ");
            }
            catch (TimeoutException exception)
            {
                Helpers.ExceptionReporter.LogException(Log, exception);
                Debug.LogWarning(" Time out == " + exception);
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(exception, true);
            }
            catch (Client.RpcClientException exception)
            {
                Helpers.ExceptionReporter.LogException(Log, exception);
                Debug.LogWarning(" RpcException == " + exception);
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(exception, true);
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogException(Log, e);
                Debug.Log("Result === " + e);
                OpenAlertDialog($"Not able to Delete Deck {currentDeck.Id}: " + e.Message);
                return;
            }

            ChangeTab(TAB.SELECT_DECK);
        }
        
        private void MoveDeckPageIndex(int direction)
        {
            _deckPageIndex = Mathf.Clamp(_deckPageIndex + direction, 0, GetDeckPageAmount(_cacheDeckListToDisplay) - 1);
        }
        
        private int GetDeckPageAmount(List<Deck> deckList)
        {
            return Mathf.CeilToInt((float) deckList.Count / _deckInfoAmountPerPage);
        }
        
        private List<Deck> GetDeckListByElementToDisplay(Enumerators.SetType setType)
        {
            List<Deck> deckList = GetDeckList();

            List<Deck> deckListToDisplay = new List<Deck>();
            for(int i=0; i<deckList.Count; ++i)
            {
                Hero hero = _dataManager.CachedHeroesData.Heroes[deckList[i].HeroId];
                if( setType == Enumerators.SetType.NONE || 
                    setType == hero.HeroElement )
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
            for(int i=0; i<deckList.Count; ++i)
            {
                string deckName = deckList[i].Name.Trim().ToLower();
                if(deckName.Contains(keyword))
                    deckListToDisplay.Add(deckList[i]);                                        
            }
            
            if(deckListToDisplay.Count <= 0)
            {
                OpenAlertDialog($"No deck found for keyword '{_inputFieldSearchDeckName.text.Trim()}'");
                return deckList;
            }

            return deckListToDisplay;
        }
        
        private List<Deck> GetDeckListFromSelectedPageToDisplay(List<Deck> deckList)
        {
            List<Deck> deckListFromSelectedPageToDisplay = new List<Deck>();
            
            int startIndex = _deckPageIndex * _deckInfoAmountPerPage;
            int endIndex = (_deckPageIndex + 1) * _deckInfoAmountPerPage;
            for( int i=0; i<deckList.Count; ++i)
            {   
                if(i >= startIndex && i < endIndex)
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
            _tab = TAB.NONE;
            ChangeTab(TAB.SELECT_DECK);
        }
        
        private void LoadButtons()
        {
            _buttonNewDeck = _selfPage.transform.Find("Tab_SelectDeck/Panel_Content/Button_BuildNewDeck").GetComponent<Button>();
            _buttonNewDeck.onClick.AddListener(ButtonNewDeckHandler);
            _buttonNewDeck.onClick.AddListener(PlayClickSound);
            
            _buttonLeftArrow = _selfPage.transform.Find("Tab_SelectDeck/Panel_Content/Button_LeftArrow").GetComponent<Button>();
            _buttonLeftArrow.onClick.AddListener(ButtonLeftArrowHandler);
            _buttonLeftArrow.onClick.AddListener(PlayClickSound);
            
            _buttonRightArrow = _selfPage.transform.Find("Tab_SelectDeck/Panel_Content/Button_RightArrow").GetComponent<Button>();
            _buttonRightArrow.onClick.AddListener(ButtonRightArrowHandler);
            _buttonRightArrow.onClick.AddListener(PlayClickSound);
            
            _buttonBack = _selfPage.transform.Find("Panel_Frame/Image_ButtonBackTray/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);
            _buttonBack.onClick.AddListener(PlayClickSound);
            
            _buttonSelectDeckFilter = _selfPage.transform.Find("Tab_SelectDeck/Panel_FrameComponents/Upper_Items/Button_Filter").GetComponent<Button>();
            _buttonSelectDeckFilter.onClick.AddListener(ButtonSelectDeckFilterHandler);
            _buttonSelectDeckFilter.onClick.AddListener(PlayClickSound);            
            
            _buttonEdit = _selfPage.transform.Find("Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Edit").GetComponent<Button>();
            _buttonEdit.onClick.AddListener(ButtonEditHandler);
            _buttonEdit.onClick.AddListener(PlayClickSound);
            
            _buttonDelete = _selfPage.transform.Find("Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Delete").GetComponent<Button>();
            _buttonDelete.onClick.AddListener(ButtonDeleteHandler);
            _buttonDelete.onClick.AddListener(PlayClickSound);
            
            _buttonRename = _selfPage.transform.Find("Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Rename").GetComponent<Button>();
            _buttonRename.onClick.AddListener(ButtonRenameHandler);
            _buttonRename.onClick.AddListener(PlayClickSound);
            
            ButtonSaveRenameDeck = _selfPage.transform.Find("Tab_Rename/Panel_FrameComponents/Lower_Items/Button_Save").GetComponent<Button>();
            ButtonSaveRenameDeck.onClick.AddListener(ButtonSaveRenameDeckHandler);
            ButtonSaveRenameDeck.onClick.AddListener(PlayClickSound);            
        }

        private void LoadObjects()
        {
            _deckInfoObjectList.Clear();
            for(int i=0; i<3; ++i)
            {
                DeckInfoObject deckInfoObject = new DeckInfoObject();
                string path = $"Tab_SelectDeck/Panel_Content/Image_DeckThumbnailNormal_{i + 1}";
                deckInfoObject._button = _selfPage.transform.Find(path).GetComponent<Button>();
                deckInfoObject._textDeckName = _selfPage.transform.Find(path+"/Text_DeckName").GetComponent<TextMeshProUGUI>();
                deckInfoObject._textCardsAmount = _selfPage.transform.Find(path+"/Text_CardsAmount").GetComponent<TextMeshProUGUI>();
                deckInfoObject._imageOverlordThumbnail = _selfPage.transform.Find(path+"/Image_DeckThumbnail").GetComponent<Image>();
                deckInfoObject._imageAbilityIcons = new Image[]
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
            List<Deck> deckListToDisplay = GetDeckListFromSelectedPageToDisplay(_cacheDeckListToDisplay);       
            
            for (int i=0; i<_deckInfoObjectList.Count; ++i)
            {
                DeckInfoObject deckInfoObject = _deckInfoObjectList[i];
                if(i>=deckListToDisplay.Count)
                {
                    deckInfoObject._button.gameObject.SetActive(false);
                    continue;
                }

                deckInfoObject._button.gameObject.SetActive(true);
                Deck deck = deckListToDisplay[i];
                
                string deckName = deck.Name;
                int cardsAmount = deck.GetNumCards();
                Hero hero = _dataManager.CachedHeroesData.Heroes[deck.HeroId];

                deckInfoObject._textDeckName.text = deckName;
                deckInfoObject._textCardsAmount.text = $"{cardsAmount}/{Constants.MaxDeckSize}";
                deckInfoObject._imageOverlordThumbnail.sprite = GetOverlordThumbnailSprite(hero.HeroElement);

                if(deck.PrimarySkill == Enumerators.OverlordSkill.NONE)
                {
                    deckInfoObject._imageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
                }
                else
                {
                    string iconPath = hero.GetSkill(deck.PrimarySkill).IconPath;
                    deckInfoObject._imageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
                }
                
                if(deck.SecondarySkill == Enumerators.OverlordSkill.NONE)
                {
                    deckInfoObject._imageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
                }
                else
                {
                    string iconPath = hero.GetSkill(deck.SecondarySkill).IconPath;
                    deckInfoObject._imageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);                
                }
                
                deckInfoObject._button.onClick.RemoveAllListeners();
                int index = i;
                deckInfoObject._button.onClick.AddListener(() =>
                {
                    ChangeSelectDeckIndex(index);
                });
            }

            UpdatePageDotObjects(_cacheDeckListToDisplay);
            ChangeSelectDeckIndex(_defaultDeckIndex);
        }
        
        private void UpdatePageDotObjects(List<Deck> deckList)
        {
            foreach (Transform child in _paginationGroup)
                Object.Destroy(child.gameObject);
            
            int page = _deckPageIndex;
            int maxPage = GetDeckPageAmount(deckList);
            
            for(int i=0; i<maxPage; ++i)
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
            
            _deckPageIndex = SelectDeckIndex / _deckInfoAmountPerPage;
            int indexInPage = SelectDeckIndex % _deckInfoAmountPerPage;
            
            UpdateDeckInfoObjects();
            ChangeSelectDeckIndex(indexInPage);
        }

        public void ApplyDeckFilter(Enumerators.SetType setType)
        {
            _inputFieldSearchDeckName.text = "";
            _cacheDeckListToDisplay = GetDeckListByElementToDisplay(setType);
            _deckPageIndex = 0;
            UpdateDeckInfoObjects();
        }
        
        private void ApplyDeckSearch()
        {
            _cacheDeckListToDisplay = GetDeckListBySearchKeywordToDisplay();
            _deckPageIndex = 0;
            UpdateDeckInfoObjects();
        }

        private void UpdateSelectedDeckDisplay(int selectedDeckIndex)
        {
            for (int i = 0; i < _deckInfoObjectList.Count; ++i)
            {
                DeckInfoObject deckInfoObject = _deckInfoObjectList[i];
                Sprite sprite = (i == selectedDeckIndex ? _spriteDeckThumbnailSelected : _spriteDeckThumbnailNormal);
                deckInfoObject._button.GetComponent<Image>().sprite = sprite;
            }
        }

        private Sprite GetOverlordThumbnailSprite(Enumerators.SetType heroElement)
        {
            string path = "Images/UI/MyDecks/OverlordDeckThumbnail";
            switch(heroElement)
            {
                case Enumerators.SetType.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_air"); 
                case Enumerators.SetType.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_fire"); 
                case Enumerators.SetType.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_earth"); 
                case Enumerators.SetType.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_toxic"); 
                case Enumerators.SetType.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_water"); 
                case Enumerators.SetType.LIFE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/deck_thumbnail_life"); 
                default:
                    Debug.Log($"No Overlord thumbnail found for setType {heroElement}");
                    return null;
            }        
        }

        //TODO Constructor
        private class DeckInfoObject
        {
            public Button _button;
            public TextMeshProUGUI _textDeckName;
            public Image _imageOverlordThumbnail;
            public Image[] _imageAbilityIcons;
            public TextMeshProUGUI _textCardsAmount;
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
