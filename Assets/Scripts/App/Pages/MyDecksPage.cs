using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class MyDecksPage : IUIElement
    {
        private IUIManager _uiManager;
        
        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;
        
        private ITutorialManager _tutorialManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;
        
        private IAnalyticsManager _analyticsManager;
        
        private GameObject _selfPage;

        private GameObject[] _tabObjects;

        private Sprite _spriteDeckThumbnailNormal,
                       _spriteDeckThumbnailSelected;

        private Transform _trayButtonBack,
                          _trayButtonAuto,
                          _locatorCollectionCards,
                          _locatorDeckCards;

        private Button _buttonNewDeck,
                       _buttonBack,
                       _buttonSelectDeckFilter,
                       _buttonSelectDeckSearch,
                       _buttonEdit,
                       _buttonDelete,
                       _buttonRename,
                       _buttonSaveRenameDeck,
                       _buttonAuto,
                       _buttonEditDeckFilter,
                       _buttonEditDeckSearch,
                       _buttonEditDeckUpperLeftArrow,
                       _buttonEditDeckUpperRightArrow,
                       _buttonEditDeckLowerLeftArrow,
                       _buttonEditDeckLowerRightArrow,
                       _buttonSaveEditDeck,
                       _buttonSelectOverlordLeftArrow,
                       _buttonSelectOverlordRightArrow,
                       _buttonSelectOverlordContinue,
                       _buttonSelectOverlordSkillContinue;                           
                       
        private TMP_InputField _inputFieldDeckName;

        private TextMeshProUGUI _textEditDeckName,
                                _textEditDeckCardsAmount,
                                _textSelectOverlordName,
                                _textSelectOverlordDescription,
                                _textSelectOverlordDeckName,
                                _textSelectOverlordSkillDeckname;

        private List<DeckInfoObject> _deckInfoObjectList;

        private Image _imageSelectOverlordGlow,
                      _imageSelectOverlordPortrait,
                      _imageSelectOverlordSkillPortrait;

        private List<Transform> _selectOverlordIconList;        

        #region Cache Data
        
        private const int _numberOfDeckInfo = 3;

        private const int _defaultDeckIndex = 0;
        
        private readonly Dictionary<Enumerators.SetType, Enumerators.SetType> _setTypeAgainstDictionary =
            new Dictionary<Enumerators.SetType, Enumerators.SetType>
            {
                {
                    Enumerators.SetType.FIRE, Enumerators.SetType.WATER
                },
                {
                    Enumerators.SetType.TOXIC, Enumerators.SetType.FIRE
                },
                {
                    Enumerators.SetType.LIFE, Enumerators.SetType.TOXIC
                },
                {
                    Enumerators.SetType.EARTH, Enumerators.SetType.LIFE
                },
                {
                    Enumerators.SetType.AIR, Enumerators.SetType.EARTH
                },
                {
                    Enumerators.SetType.WATER, Enumerators.SetType.AIR
                }
            };

        private enum TAB
        {
            NONE = -1,
            SELECT_DECK = 0,
            RENAME = 1,
            EDITING = 2,
            SELECT_OVERLORD = 3,
            SELECT_OVERLORD_SKILL = 4,
        }
        
        private TAB _tab;
        
        private int _selectDeckIndex;

        private int _selectOverlordIndex;

        private Deck _currentEditDeck;

        private Hero _currentEditHero;

        private bool _isEditingNewDeck;

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
            _selectOverlordIconList = new List<Transform>();

            InitBoardCardPrefabsAndLists();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MyDecksPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);
            
            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_DECKS);
            _uiManager.DrawPopup<AreaBarPopup>();
            
            _inputFieldDeckName = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Rename/Panel_Content/InputText_DeckName").GetComponent<TMP_InputField>();
            _inputFieldDeckName.onEndEdit.AddListener(OnInputFieldEndedEdit);
            _inputFieldDeckName.text = "Deck Name";

            _trayButtonBack = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Frame/Image_ButtonBackTray");
            _trayButtonBack.gameObject.SetActive(false);
            
            _trayButtonAuto = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Frame/Image_ButtonAutoTray");
            _trayButtonAuto.gameObject.SetActive(false);
            
            _locatorCollectionCards = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Locator_CollectionCards");            
            _locatorDeckCards = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Locator_DeckCards");
            
            _spriteDeckThumbnailNormal = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_Content/Sprite_deck_thumbnail_normal").GetComponent<Image>().sprite;
            _spriteDeckThumbnailSelected = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_Content/Sprite_deck_thumbnail_selected").GetComponent<Image>().sprite;

            _textEditDeckName = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();
            _textEditDeckCardsAmount = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Lower_Items/Image_CardCounter/Text_CardsAmount").GetComponent<TextMeshProUGUI>();

            _textSelectOverlordName = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_Content/Text_SelectOverlord").GetComponent<TextMeshProUGUI>();
            _textSelectOverlordDescription = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_Content/Text_Desc").GetComponent<TextMeshProUGUI>();
            _textSelectOverlordDeckName = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();
            _textSelectOverlordSkillDeckname = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();
            
            _imageSelectOverlordGlow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_Content/Image_Glow").GetComponent<Image>();
            _imageSelectOverlordPortrait = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_Content/Image_OverlordPortrait").GetComponent<Image>();
            _imageSelectOverlordSkillPortrait = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_Content/Image_OverlordPortrait").GetComponent<Image>();

            _dragAreaDeck = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/DragArea_Deck").gameObject;
            _dragAreaCollections = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/DragArea_Collections").gameObject;

            LoadBoardCardComponents();
            LoadButtons();
            LoadObjects();            
            LoadTabs();
            ChangeDeckIndex(_defaultDeckIndex);              
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
            DisposeBoardCards();
            
            if (_draggingObject != null)
            {
                Object.Destroy(_draggingObject);
                _draggingObject = null;
                _isDragging = false;
            }   
            
            _deckInfoObjectList.Clear();
            _selectOverlordIconList.Clear();   
        }

        #endregion

        #region UI Handlers

        private void ButtonNewDeckHandler()
        {
            ChangeTab(TAB.SELECT_OVERLORD);
        }
        
        private void ButtonBackHandler()
        {
            ChangeTab(TAB.SELECT_DECK);
        }
        
        private void ButtonSelectDeckFilterHandler()
        {
        
        }
        
        private void ButtonSelectDeckSearchHandler()
        {

        }
        
        private void ButtonEditDeckFilterHandler()
        {
        
        }
        
        private void ButtonEditDeckSearchHandler()
        {

        }

        private void ButtonEditHandler()
        {
            AssignCurrentDeck(false);
            ChangeTab(TAB.EDITING);
        }        
        
        private void ButtonDeleteHandler()
        {
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
            ChangeTab(TAB.RENAME);
        }
        
        private void ButtonSaveRenameDeckHandler()
        {
            Deck deck = GetSelectedDeck();
            string newName = _inputFieldDeckName.text;
            ProcessRenameDeck(deck, newName);
        }

        private void ButtonSaveEditDeckHandler()
        {
            ProcessEditDeck(_currentEditDeck);            
        }
        
        private void ButtonAutoHandler()
        {
           
        }
        
        private void ButtonEditDeckUpperLeftArrowHandler()
        {
            MoveDeckPageIndex(-1);
            UpdateDeckCardPage();
        }
        
        private void ButtonEditDeckUpperRightArrowHandler()
        {
            MoveDeckPageIndex(1);
            UpdateDeckCardPage();
        }
        
        private void ButtonEditDeckLowerLeftArrowHandler()
        {

        }
        
        private void ButtonEditDeckLowerRightArrowHandler()
        {

        }
        
        private void ButtonSelectOverlordLeftArrowHandler()
        {
            ChangeOverlordIndex
            (
                Mathf.Clamp(_selectOverlordIndex - 1, 0, _selectOverlordIconList.Count - 1)
            );
        }

        private void ButtonSelectOverlordRightArrowHandler()
        {
            ChangeOverlordIndex
            (
                Mathf.Clamp(_selectOverlordIndex + 1, 0, _selectOverlordIconList.Count - 1)
            );
        }
        
        private void ButtonSelectOverlordContinueHandler()
        {
            _buttonSelectOverlordContinue.interactable = false;
            _currentEditHero = _dataManager.CachedHeroesData.Heroes[_selectOverlordIndex];
            AssignCurrentDeck(true);
            ProcessAddDeck();            
        }
        
        private async void ButtonSelectOverlordSkillContinueHandler()
        {
            _buttonSelectOverlordSkillContinue.interactable = false;
            Deck deck = _currentEditDeck;
        
            bool success = true;
        
            //TODO overlord skill
            Hero hero = _dataManager.CachedHeroesData.Heroes[deck.HeroId];
            deck.PrimarySkill = hero.PrimarySkill;
            deck.SecondarySkill = hero.SecondarySkill;

            try
            {
                await _backendFacade.EditDeck(_backendDataControlMediator.UserDataModel.UserId, deck);
            }
            catch (Exception e)
            {
                success = false;                
                Helpers.ExceptionReporter.LogException(e);
                Debug.LogWarning($"got exception: {e.Message} ->> {e.StackTrace}");

                OpenAlertDialog("Not able to edit Deck: \n" + e.Message);
            }
            _buttonSelectOverlordSkillContinue.interactable = true;

            if (success)
                ChangeTab(TAB.EDITING);
        }

        public void OnInputFieldEndedEdit(string value)
        {
            
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

        private List<Deck> GetDeckList()
        {
            return _dataManager.CachedDecksData.Decks;
        }
        
        private Deck GetSelectedDeck()
        {
            List<Deck> deckList = GetDeckList();
            return deckList[_selectDeckIndex];
        }

        private void AssignCurrentDeck(bool isNewDeck)
        {
            _isEditingNewDeck = isNewDeck;
            if(_isEditingNewDeck)
            {
                _currentEditDeck = CreateNewDeckData();
            }
            else
            {
                _currentEditDeck = GetSelectedDeck().Clone();
                _currentEditHero = _dataManager.CachedHeroesData.Heroes[_currentEditDeck.HeroId];
            }
        }

        private Deck CreateNewDeckData()
        {
            Deck deck = new Deck(
                -1,
                _currentEditHero.HeroId,
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

        private void ChangeTab(TAB newTab)
        {
            ResetDeckBoardCards();
            ResetCollectionsBoardCards();
            
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
                    UpdateDeckInfoObjects();
                    break;
                case TAB.RENAME:
                    _inputFieldDeckName.text = GetSelectedDeck().Name;
                    break;
                case TAB.EDITING:
                    _textEditDeckName.text = _currentEditDeck.Name;
                    _textEditDeckCardsAmount.text =  $"{_currentEditDeck.GetNumCards()}/{Constants.MaxDeckSize}";
                    LoadCollectionsCards(0,Enumerators.SetType.FIRE);
                    LoadDeckCards(_currentEditDeck);
                    UpdateDeckCardPage();
                    break;
                case TAB.SELECT_OVERLORD:
                    _textSelectOverlordDeckName.text = "NEW DECK";
                    ChangeOverlordIndex(0);
                    break;
                case TAB.SELECT_OVERLORD_SKILL:
                    _textSelectOverlordSkillDeckname.text = _currentEditDeck.Name;
                    break;
                default:
                    break;
            }
            
            _tab = newTab;
        }
        
        private void ChangeDeckIndex(int newIndex)
        {
            UpdateSelectedDeckDisplay(newIndex);
            _selectDeckIndex = newIndex;
        }
        
        private void ChangeOverlordIndex(int newIndex)
        {
            _selectOverlordIndex = newIndex;
            UpdateSelectedOverlordDisplay(_selectOverlordIndex);            
        }

        private void UpdateShowBackButton(bool isShow)
        {
            _trayButtonBack.gameObject.SetActive(isShow);
        }

        private void UpdateShowAutoButton(bool isShow)
        {
            _trayButtonAuto.gameObject.SetActive(isShow);
        }
        
        private bool VerifyDeckName(string deckName)
        {
            if (string.IsNullOrWhiteSpace(deckName))
            {
                OpenAlertDialog("Saving Deck with an empty name is not allowed.");
                return false;
            }
            return true;
        }

        public async void ProcessRenameDeck(Deck deckToSave, string newName)
        {
            _buttonSaveRenameDeck.interactable = false;
            
            if (!VerifyDeckName(newName))
            {
                _buttonSaveRenameDeck.interactable = true;
                return;
            }

            ProcessEditDeck(deckToSave);
        }
        
        public async void ProcessEditDeck(Deck deckToSave)
        {
            _buttonSaveRenameDeck.interactable = false;
            _buttonSaveEditDeck.interactable = false;
            
            if (!VerifyDeckName(deckToSave.Name))
            {
                _buttonSaveRenameDeck.interactable = true;
                _buttonSaveEditDeck.interactable = true;
                return;
            }

            List<Deck> deckList = GetDeckList();
            foreach (Deck deck in deckList)
            {
                if (deckToSave.Id != deck.Id &&
                    deck.Name.Trim().Equals(deckToSave.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    _buttonSaveRenameDeck.interactable = true;
                    _buttonSaveEditDeck.interactable = true;
                    OpenAlertDialog("Not able to Edit Deck: \n Deck Name already exists.");
                    return;
                }
            }
            
            bool success = true;
            try
            {
                await _backendFacade.EditDeck(_backendDataControlMediator.UserDataModel.UserId, deckToSave);

                for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count; i++)
                {
                    if (_dataManager.CachedDecksData.Decks[i].Id == deckToSave.Id)
                    {
                        _dataManager.CachedDecksData.Decks[i] = deckToSave;
                        break;
                    }
                }

                _analyticsManager.SetEvent(AnalyticsManager.EventDeckEdited);
                Debug.Log(" ====== Edit Deck Successfully ==== ");
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogException(e);

                success = false;

                if (e is Client.RpcClientException || e is TimeoutException)
                {
                    GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true);
                }
                else
                {
                    string message = e.Message;

                    string[] description = e.Message.Split('=');
                    if (description.Length > 0)
                    {
                        message = description[description.Length - 1].TrimStart(' ');
                        message = char.ToUpper(message[0]) + message.Substring(1);
                    }
                    if (_tutorialManager.IsTutorial)
                    {
                        message = Constants.ErrorMessageForConnectionFailed;
                    }
                    OpenAlertDialog("Not able to Edit Deck: \n" + message);
                }
            }
        
            if (success)
            {
                _dataManager.CachedUserLocalData.LastSelectedDeckId = (int)deckToSave.Id;
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                ChangeTab(TAB.SELECT_DECK);
                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.HordeSelection);
            }
            
            _buttonSaveRenameDeck.interactable = true;
            _buttonSaveEditDeck.interactable = true;
        }
        
        private async void ProcessAddDeck()
        {
            bool success = true;
            _currentEditDeck.HeroId = _currentEditHero.HeroId;
            _currentEditDeck.PrimarySkill = _currentEditHero.PrimarySkill;
            _currentEditDeck.SecondarySkill = _currentEditHero.SecondarySkill;

            try
            {
                long newDeckId = await _backendFacade.AddDeck(_backendDataControlMediator.UserDataModel.UserId, _currentEditDeck);
                _currentEditDeck.Id = newDeckId;
                _dataManager.CachedDecksData.Decks.Add(_currentEditDeck);
                _analyticsManager.SetEvent(AnalyticsManager.EventDeckCreated);
                Debug.Log(" ====== Add Deck " + newDeckId + " Successfully ==== ");

                if(_tutorialManager.IsTutorial)
                {
                    _dataManager.CachedUserLocalData.TutorialSavedDeck = _currentEditDeck;
                    await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                }
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogException(e);

                success = false;

                if (e is Client.RpcClientException || e is TimeoutException)
                {
                    GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true);
                }
                else
                {
                    OpenAlertDialog("Not able to Add Deck: \n" + e.Message);
                }
            }
            
            if (success)
            {
                _dataManager.CachedUserLocalData.LastSelectedDeckId = (int)_currentEditDeck.Id;
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);                

                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.HordeSaved);

                _selectDeckIndex = GetDeckList().IndexOf(_currentEditDeck);
                AssignCurrentDeck(false);
                ChangeTab(TAB.SELECT_OVERLORD_SKILL);
            }
            _buttonSelectOverlordContinue.interactable = true;
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
                Helpers.ExceptionReporter.LogException(exception);
                Debug.LogWarning(" Time out == " + exception);
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(exception, true);
            }
            catch (Client.RpcClientException exception)
            {
                Helpers.ExceptionReporter.LogException(exception);
                Debug.LogWarning(" RpcException == " + exception);
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(exception, true);
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogException(e);
                Debug.Log("Result === " + e);
                OpenAlertDialog($"Not able to Delete Deck {currentDeck.Id}: " + e.Message);
                return;
            }

            ChangeTab(TAB.SELECT_DECK);
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
            _buttonNewDeck = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_Content/Button_BuildNewDeck").GetComponent<Button>();
            _buttonNewDeck.onClick.AddListener(ButtonNewDeckHandler);
            _buttonNewDeck.onClick.AddListener(PlayClickSound);
            
            _buttonBack = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Frame/Image_ButtonBackTray/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);
            _buttonBack.onClick.AddListener(PlayClickSound);
            
            _buttonAuto = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Frame/Image_ButtonAutoTray/Button_Auto").GetComponent<Button>();
            _buttonAuto.onClick.AddListener(ButtonAutoHandler);
            _buttonAuto.onClick.AddListener(PlayClickSound);
            
            _buttonSelectDeckFilter = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_FrameComponents/Upper_Items/Button_Filter").GetComponent<Button>();
            _buttonSelectDeckFilter.onClick.AddListener(ButtonSelectDeckFilterHandler);
            _buttonSelectDeckFilter.onClick.AddListener(PlayClickSound);
            
            _buttonSelectDeckSearch = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_FrameComponents/Upper_Items/Button_SearchBar").GetComponent<Button>();
            _buttonSelectDeckSearch.onClick.AddListener(ButtonSelectDeckSearchHandler);
            _buttonSelectDeckSearch.onClick.AddListener(PlayClickSound);
            
            _buttonEditDeckFilter = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Upper_Items/Button_Filter").GetComponent<Button>();
            _buttonEditDeckFilter.onClick.AddListener(ButtonEditDeckFilterHandler);
            _buttonEditDeckFilter.onClick.AddListener(PlayClickSound);
            
            _buttonEditDeckSearch = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Upper_Items/Button_SearchBar").GetComponent<Button>();
            _buttonEditDeckSearch.onClick.AddListener(ButtonEditDeckSearchHandler);
            _buttonEditDeckSearch.onClick.AddListener(PlayClickSound);
            
            _buttonEditDeckUpperLeftArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Button_UpperLeftArrow").GetComponent<Button>();
            _buttonEditDeckUpperLeftArrow.onClick.AddListener(ButtonEditDeckUpperLeftArrowHandler);
            _buttonEditDeckUpperLeftArrow.onClick.AddListener(PlayClickSound);
            
            _buttonEditDeckUpperRightArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Button_UpperRightArrow").GetComponent<Button>();
            _buttonEditDeckUpperRightArrow.onClick.AddListener(ButtonEditDeckUpperRightArrowHandler);
            _buttonEditDeckUpperRightArrow.onClick.AddListener(PlayClickSound);
            
            _buttonEditDeckLowerLeftArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Button_LowerLeftArrow").GetComponent<Button>();
            _buttonEditDeckLowerLeftArrow.onClick.AddListener(ButtonEditDeckLowerLeftArrowHandler);
            _buttonEditDeckLowerLeftArrow.onClick.AddListener(PlayClickSound);
            
            _buttonEditDeckLowerRightArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Button_LowerRightArrow").GetComponent<Button>();
            _buttonEditDeckLowerRightArrow.onClick.AddListener(ButtonEditDeckLowerRightArrowHandler);
            _buttonEditDeckLowerRightArrow.onClick.AddListener(PlayClickSound);
            
            _buttonEdit = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Edit").GetComponent<Button>();
            _buttonEdit.onClick.AddListener(ButtonEditHandler);
            _buttonEdit.onClick.AddListener(PlayClickSound);
            
            _buttonDelete = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Delete").GetComponent<Button>();
            _buttonDelete.onClick.AddListener(ButtonDeleteHandler);
            _buttonDelete.onClick.AddListener(PlayClickSound);
            
            _buttonRename = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Rename").GetComponent<Button>();
            _buttonRename.onClick.AddListener(ButtonRenameHandler);
            _buttonRename.onClick.AddListener(PlayClickSound);
            
            _buttonSaveRenameDeck = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Rename/Panel_FrameComponents/Lower_Items/Button_Save").GetComponent<Button>();
            _buttonSaveRenameDeck.onClick.AddListener(ButtonSaveRenameDeckHandler);
            _buttonSaveRenameDeck.onClick.AddListener(PlayClickSound);
            
            _buttonSaveEditDeck = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Lower_Items/Button_SaveDeck").GetComponent<Button>();
            _buttonSaveEditDeck.onClick.AddListener(ButtonSaveEditDeckHandler);
            _buttonSaveEditDeck.onClick.AddListener(PlayClickSound);
            
            _buttonSelectOverlordLeftArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_Content/Button_LeftArrow").GetComponent<Button>();
            _buttonSelectOverlordLeftArrow.onClick.AddListener(ButtonSelectOverlordLeftArrowHandler);
            _buttonSelectOverlordLeftArrow.onClick.AddListener(PlayClickSound);
            
            _buttonSelectOverlordRightArrow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_Content/Button_RightArrow").GetComponent<Button>();
            _buttonSelectOverlordRightArrow.onClick.AddListener(ButtonSelectOverlordRightArrowHandler);
            _buttonSelectOverlordRightArrow.onClick.AddListener(PlayClickSound);
            
            _buttonSelectOverlordContinue = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_FrameComponents/Lower_Items/Button_Continue").GetComponent<Button>();
            _buttonSelectOverlordContinue.onClick.AddListener(ButtonSelectOverlordContinueHandler);
            _buttonSelectOverlordContinue.onClick.AddListener(PlayClickSound);
            
            _buttonSelectOverlordSkillContinue = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_FrameComponents/Lower_Items/Button_Continue").GetComponent<Button>();
            _buttonSelectOverlordSkillContinue.onClick.AddListener(ButtonSelectOverlordSkillContinueHandler);
            _buttonSelectOverlordSkillContinue.onClick.AddListener(PlayClickSound);
        }

        private void LoadObjects()
        {
            for(int i=0; i<6;++i)
            {
                Image overlordIcon = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_Content/Group_DeckIcon/Image_DeckIcon_" + i).GetComponent<Image>();
                Sprite sprite = _uiManager.GetPopup<DeckSelectionPopup>().GetDeckIconSprite
                (
                    _dataManager.CachedHeroesData.Heroes[i].HeroElement
                );
                overlordIcon.sprite = sprite;
                
                _selectOverlordIconList.Add
                (
                    overlordIcon.transform
                );
            }
            
            _deckInfoObjectList.Clear();
            for(int i=0; i<3; ++i)
            {
                DeckInfoObject deckInfoObject = new DeckInfoObject();
                string path = $"Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_Content/Image_DeckThumbnailNormal_{i + 1}";
                deckInfoObject._button = _selfPage.transform.Find(path).GetComponent<Button>();
                deckInfoObject._textDeckName = _selfPage.transform.Find(path+"/Text_DeckName").GetComponent<TextMeshProUGUI>();
                deckInfoObject._textCardsAmount = _selfPage.transform.Find(path+"/Text_CardsAmount").GetComponent<TextMeshProUGUI>();
                deckInfoObject._imageOverlordThumbnail = _selfPage.transform.Find(path+"/Image_DeckThumbnail").GetComponent<Image>();
                _deckInfoObjectList.Add(deckInfoObject);
            }

            _tabObjects = new GameObject[]
            {
                _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck").gameObject,
                _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Rename").gameObject,
                _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing").gameObject,
                _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord").gameObject,
                _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill").gameObject
            };
        }
        
        private void UpdateDeckInfoObjects()
        {
            List<Deck> deckList = GetDeckList();
            //TODO Add logic to display more than 3 decks
            for(int i=0; i<_deckInfoObjectList.Count; ++i)
            {
                DeckInfoObject deckInfoObject = _deckInfoObjectList[i];
                if(i>=deckList.Count)
                {
                    deckInfoObject._button.gameObject.SetActive(false);
                    continue;
                }

                deckInfoObject._button.gameObject.SetActive(true);
                Deck deck = deckList[i];
                
                string deckName = deck.Name;
                int cardsAmount = deck.GetNumCards();
                Enumerators.SetType heroElement = _dataManager.CachedHeroesData.Heroes[deck.HeroId].HeroElement;

                deckInfoObject._textDeckName.text = deckName;
                deckInfoObject._textCardsAmount.text = $"{cardsAmount}/{Constants.MaxDeckSize}";
                deckInfoObject._imageOverlordThumbnail.sprite = GetOverlordThumbnailSprite(heroElement);
                deckInfoObject._button.onClick.RemoveAllListeners();
                int index = i;
                deckInfoObject._button.onClick.AddListener(() =>
                {
                    ChangeDeckIndex(index);
                });
            }
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
        
        private void UpdateSelectedOverlordDisplay(int selectedOverlordIndex)
        {
            Hero hero = _dataManager.CachedHeroesData.Heroes[selectedOverlordIndex];
            _imageSelectOverlordGlow.transform.position = _selectOverlordIconList[selectedOverlordIndex].position;
            _imageSelectOverlordPortrait.sprite = GetOverlordPortraitSprite
            (
                hero.HeroElement
            );
            _imageSelectOverlordSkillPortrait.sprite = GetOverlordPortraitSprite
            (
                hero.HeroElement
            );
            _textSelectOverlordName.text = hero.FullName;
            _textSelectOverlordDescription.text = hero.ShortDescription;
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
        
        private Sprite GetOverlordPortraitSprite(Enumerators.SetType heroElement)
        {
            string path = "Images/UI/MyDecks/OverlordPortrait";
            switch(heroElement)
            {
                case Enumerators.SetType.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_air"); 
                case Enumerators.SetType.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_fire"); 
                case Enumerators.SetType.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_earth"); 
                case Enumerators.SetType.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_toxic"); 
                case Enumerators.SetType.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_water"); 
                case Enumerators.SetType.LIFE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_life"); 
                default:
                    Debug.Log($"No Overlord portrait found for setType {heroElement}");
                    return null;
            }        
        }

        //TODO Constructor
        private class DeckInfoObject
        {
            public Button _button;
            public TextMeshProUGUI _textDeckName;
            public Image _imageOverlordThumbnail;
            public TextMeshProUGUI _textCardsAmount;
        }
        
        private void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        #endregion

        #region Board Cards
        
        public List<Transform> CollectionsCardPositions,
                               DeckCardPositions;

        public GameObject CardCreaturePrefab,
                          CardItemPrefab,
                          CollectionsCardPlaceholdersPrefab,
                          DeckCardPlaceholdersPrefab;

        public GameObject CollectionsCardPlaceholders,
                          DeckCardPlaceholders;

        private List<BoardCard> _createdDeckBoardCards,
                                _createdCollectionsBoardCards;
                                
        private CardHighlightingVFXItem _highlightingVFXItem;
        
        private CollectionData _collectionData;

        private int _deckPageIndex;
        
        private bool _isDragging;
        
        private GameObject _draggingObject;
        
        private GameObject _dragAreaDeck, 
                           _dragAreaCollections;
        
        private void InitBoardCardPrefabsAndLists()
        {
            CardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            CardItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard");
            CollectionsCardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersMyDecksLower");
            DeckCardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersMyDecksUpper");            
            
            _createdDeckBoardCards = new List<BoardCard>();
            _createdCollectionsBoardCards = new List<BoardCard>();
            
            _collectionData = new CollectionData();
            _collectionData.Cards = new List<CollectionCardData>();
        }
        
        private void LoadBoardCardComponents()
        {
            _highlightingVFXItem = new CardHighlightingVFXItem(Object.Instantiate(
            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/UI/ArmyCardSelection"), _selfPage.transform, true));            
            
            DeckCardPlaceholders = Object.Instantiate(DeckCardPlaceholdersPrefab);
            Vector3 deckCardPlaceholdersPos = _locatorDeckCards.position;
            deckCardPlaceholdersPos.z = 0f;
            DeckCardPlaceholders.transform.position = deckCardPlaceholdersPos;
            
            DeckCardPositions = new List<Transform>();

            foreach (Transform placeholder in DeckCardPlaceholders.transform)
            {
                DeckCardPositions.Add(placeholder);
            }
            
            CollectionsCardPlaceholders = Object.Instantiate(CollectionsCardPlaceholdersPrefab);
            Vector3 collectionsCardPlaceholdersPos = _locatorCollectionCards.position;
            collectionsCardPlaceholdersPos.z = 0f;
            CollectionsCardPlaceholders.transform.position = collectionsCardPlaceholdersPos;
            
            CollectionsCardPositions = new List<Transform>();

            foreach (Transform placeholder in CollectionsCardPlaceholders.transform)
            {
                CollectionsCardPositions.Add(placeholder);
            }

            _deckPageIndex = 0;
            FillCollectionData();
        }
        
        private void FillCollectionData()
        {
            _collectionData.Cards.Clear();
            CollectionCardData cardData;

            List<CollectionCardData> data;
            if (_tutorialManager.IsTutorial)
            {
                data = _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.CardsForArmy;
            }
            else
            {
                data = _dataManager.CachedCollectionData.Cards;
            }

            foreach (CollectionCardData card in data)
            {
                cardData = new CollectionCardData();
                cardData.Amount = card.Amount;
                cardData.CardName = card.CardName;

                _collectionData.Cards.Add(cardData);
            }
        }

        public void LoadCollectionsCards(int page, Enumerators.SetType setType)
        {
            ResetCollectionsBoardCards();
            
            CardSet set = SetTypeUtility.GetCardSet(_dataManager, setType);
            List<Card> cards = set.Cards;
            int startIndex = page * CollectionsCardPositions.Count;
            int endIndex = Mathf.Min(startIndex + CollectionsCardPositions.Count, cards.Count);
            CollectionCardData collectionCardData = null;
            RectTransform rectContainer = _locatorCollectionCards.GetComponent<RectTransform>();

            for (int i = startIndex; i < endIndex; i++)
            {
                if (i >= cards.Count)
                    break;

                Card card = cards[i];
                CollectionCardData cardData = _dataManager.CachedCollectionData.GetCardData(card.Name);

                // hack !!!! CHECK IT!!!
                if (cardData == null)
                    continue;

                BoardCard boardCard = CreateBoardCard
                (
                    card, 
                    rectContainer,
                    CollectionsCardPositions[i % CollectionsCardPositions.Count].position, 
                    0.265f
                );
                _createdCollectionsBoardCards.Add(boardCard); 
                
                OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();

                eventHandler.DragBegan += BoardCardDragBeganHandler;
                eventHandler.DragEnded += BoardCardCollectionDragEndedHandler;
                eventHandler.DragUpdated += BoardCardDragUpdatedHandler;
                
                collectionCardData = _collectionData.GetCardData(card.Name);
                UpdateCollectionsCardAmount
                (
                    true, 
                    card.Name, 
                    collectionCardData.Amount
                );
            }
        }
        
        public void UpdateCollectionsCardAmount(bool init, string cardId, int amount)
        {
            foreach (BoardCard card in _createdCollectionsBoardCards)
            {
                if (card.LibraryCard.Name == cardId)
                {
                    card.SetAmountOfCardsInEditingPage
                    (
                        init, 
                        GetMaxCopiesValue(card.LibraryCard), 
                        amount, 
                        true
                    );
                    break;
                }
            }
        }

        public void LoadDeckCards(Deck deck)
        {
            ResetDeckBoardCards();

            RectTransform rectContainer = _locatorDeckCards.GetComponent<RectTransform>();
            foreach (DeckCardData card in deck.Cards)
            {
                Card libraryCard = _dataManager.CachedCardsLibraryData.GetCardFromName(card.CardName);

                bool itemFound = false;
                foreach (BoardCard item in _createdDeckBoardCards)
                {
                    if (item.LibraryCard.Name == card.CardName)
                    {
                        itemFound = true;
                        break;
                    }
                }

                if (!itemFound)
                {
                    BoardCard boardCard = CreateBoardCard
                    (
                        libraryCard, 
                        rectContainer,
                        Vector3.zero, 
                        0.3f
                    );
                    boardCard.Transform.Find("Amount").gameObject.SetActive(false);

                    _createdDeckBoardCards.Add(boardCard);
                    
                    boardCard.SetAmountOfCardsInEditingPage(true, GetMaxCopiesValue(libraryCard), card.Amount);
                    
                    OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();

                    eventHandler.DragBegan += BoardCardDragBeganHandler;
                    eventHandler.DragEnded += BoardCardDeckDragEndedHandler; 
                    eventHandler.DragUpdated += BoardCardDragUpdatedHandler;

                    _collectionData.GetCardData(card.CardName).Amount -= card.Amount;

                    UpdateEditDeckCardsAmount();
                }
            }
        }
        
        public void AddCardToDeck(IReadOnlyCard card)
        {
            if (_currentEditDeck == null)
                return;
            

            if (_setTypeAgainstDictionary[_currentEditHero.HeroElement] == card.CardSetType)
            {
                OpenAlertDialog(
                    "It's not possible to add cards to the deck \n from the faction from which the hero is weak against");
                return;
            }

            CollectionCardData collectionCardData = _collectionData.GetCardData(card.Name);
            if (collectionCardData.Amount == 0)
            {
                OpenAlertDialog(
                    "You don't have enough cards of this type. \n Buy or earn new packs to get more cards!");
                return;
            }

            DeckCardData existingCards = _currentEditDeck.Cards.Find(x => x.CardName == card.Name);

            uint maxCopies = GetMaxCopiesValue(card);

            if (existingCards != null && existingCards.Amount == maxCopies)
            {
                OpenAlertDialog("You cannot have more than " + maxCopies + " copies of the " +
                    card.CardRank.ToString().ToLowerInvariant() + " card in your deck.");
                return;
            }

            if (_currentEditDeck.GetNumCards() == Constants.DeckMaxSize)
            {
                OpenAlertDialog("You can not add more than " + Constants.DeckMaxSize + " Cards in a single Horde.");
                return;
            }

            bool itemFound = false;
            BoardCard foundItem = null;
            foreach (BoardCard item in _createdDeckBoardCards)
            {
                if (item.LibraryCard.MouldId == card.MouldId)
                {
                    foundItem = item;
                    itemFound = true;

                    break;
                }
            }

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_ADD_CARD,
                Constants.SfxSoundVolume, false, false, true);
                
            collectionCardData.Amount--;
            UpdateCollectionsCardAmount(false, card.Name, collectionCardData.Amount);

            
            if (!itemFound)
            {
                RectTransform rectContainer = _locatorDeckCards.GetComponent<RectTransform>();
                BoardCard boardCard = CreateBoardCard
                (
                    card, 
                    rectContainer,
                    Vector3.zero,
                    0.3f                  
                );
                boardCard.Transform.Find("Amount").gameObject.SetActive(false);
                foundItem = boardCard;
                
                OnBehaviourHandler eventHandler = boardCard.GameObject.GetComponent<OnBehaviourHandler>();

                eventHandler.DragBegan += BoardCardDragBeganHandler;
                eventHandler.DragEnded += BoardCardDeckDragEndedHandler; 
                eventHandler.DragUpdated += BoardCardDragUpdatedHandler;

                _createdDeckBoardCards.Add(boardCard);

                UpdateEditDeckCardsAmount();
                UpdateDeckCardPage();
            }

            _currentEditDeck.AddCard(card.Name);

            foundItem.SetAmountOfCardsInEditingPage(false, GetMaxCopiesValue(card),
                _currentEditDeck.Cards.Find(x => x.CardName == foundItem.LibraryCard.Name).Amount);

            UpdateDeckCardPage();

            if(_tutorialManager.IsTutorial && _currentEditDeck.GetNumCards() >= _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.HordeFilled);
            }      
        }

        private BoardCard CreateBoardCard(Card card, RectTransform root, Vector3 position, float scale)
        {
            GameObject go;
            BoardCard boardCard;
            int amount = _collectionData.GetCardData(card.Name).Amount;
            
            switch (card.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(CardCreaturePrefab);
                    boardCard = new UnitBoardCard(go);
                    break;
                case Enumerators.CardKind.SPELL:
                    go = Object.Instantiate(CardItemPrefab);
                    boardCard = new SpellBoardCard(go);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(card.CardKind), card.CardKind, null);
            }
            
            boardCard.Init(card, amount);
            boardCard.SetHighlightingEnabled(false);
            boardCard.Transform.position = position;
            boardCard.Transform.localScale = Vector3.one * scale;
            boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;
            boardCard.Transform.Find("Amount").gameObject.SetActive(false);
            
            boardCard.Transform.SetParent(_uiManager.Canvas.transform, true);
            RectTransform cardRectTransform = boardCard.GameObject.AddComponent<RectTransform>();

            if (root != null)
            {
                cardRectTransform.SetParent(root);
            }

            Vector3 anchoredPos = boardCard.Transform.localPosition;
            anchoredPos.z = 0;
            boardCard.Transform.localPosition = anchoredPos;

            return boardCard;
        }
        
        //TODO Refactor this
        private BoardCard CreateBoardCard(IReadOnlyCard card, RectTransform root, Vector3 position, float scale)
        {
            GameObject go;
            BoardCard boardCard;
            int amount = _collectionData.GetCardData(card.Name).Amount;
            
            switch (card.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(CardCreaturePrefab);
                    boardCard = new UnitBoardCard(go);
                    break;
                case Enumerators.CardKind.SPELL:
                    go = Object.Instantiate(CardItemPrefab);
                    boardCard = new SpellBoardCard(go);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(card.CardKind), card.CardKind, null);
            }
            
            boardCard.Init(card, amount);
            boardCard.SetHighlightingEnabled(false);
            boardCard.Transform.position = position;
            boardCard.Transform.localScale = Vector3.one * scale;
            boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI1;
            boardCard.Transform.Find("Amount").gameObject.SetActive(false);
            
            boardCard.Transform.SetParent(_uiManager.Canvas.transform, true);
            RectTransform cardRectTransform = boardCard.GameObject.AddComponent<RectTransform>();

            if (root != null)
            {
                cardRectTransform.SetParent(root);
            }

            Vector3 anchoredPos = boardCard.Transform.localPosition;
            anchoredPos.z = 0;
            boardCard.Transform.localPosition = anchoredPos;

            return boardCard;
        }

        #region Drag Handler

        private void BoardCardDragBeganHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (_isDragging || (GameClient.Get<ITutorialManager>().IsTutorial &&
                !GameClient.Get<ITutorialManager>().CurrentTutorial.IsGameplayTutorial() &&
                (GameClient.Get<ITutorialManager>().CurrentTutorialStep.ToMenuStep().CardsInteractingLocked ||
                !GameClient.Get<ITutorialManager>().CurrentTutorialStep.ToMenuStep().CanDragCards)))
                return;
            
            _draggingObject = Object.Instantiate(onOnject);
            _draggingObject.transform.localScale = Vector3.one * 0.3f;
            _draggingObject.transform.Find("Amount").gameObject.SetActive(false);
            _draggingObject.transform.Find("AmountForArmy").gameObject.SetActive(false);
            _draggingObject.transform.Find("DeckEditingGroupUI").gameObject.SetActive(false);
            _draggingObject.name = onOnject.GetInstanceID().ToString();
            _draggingObject.GetComponent<SortingGroup>().sortingOrder = 2;

            _isDragging = true;

            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.z = 0f;
            _draggingObject.transform.position = position;
        }
        
        private void BoardCardCollectionDragEndedHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (!_isDragging)
                return;            

            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, SRLayerMask.Default);

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject == _dragAreaDeck)
                    {
                        BoardCard armyCard = _createdCollectionsBoardCards.Find(x =>
                            x.GameObject.GetInstanceID().ToString() == _draggingObject.name);

                        AddCardToDeck(armyCard.LibraryCard);

                        GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.CardDragged);
                    }
                }
            }

            Object.Destroy(_draggingObject);
            _draggingObject = null;
            _isDragging = false;
        }
        
        private void BoardCardDeckDragEndedHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (!_isDragging)
                return;
            
            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, SRLayerMask.Default);

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject == _dragAreaCollections)
                    {
                        BoardCard hordeCard = _createdDeckBoardCards.Find(x =>
                            x.GameObject.GetInstanceID().ToString() == _draggingObject.name);

                        //RemoveCardFromDeck(null, hordeCard.LibraryCard);
                    }
                }
            }

            Object.Destroy(_draggingObject);
            _draggingObject = null;
            _isDragging = false;
        }
        
        private void BoardCardDragUpdatedHandler(PointerEventData eventData, GameObject onOnject)
        {
            if (!_isDragging)
                return;
            

            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.z = _draggingObject.transform.position.z;
            _draggingObject.transform.position = position;
        }
        
        #endregion

        private void UpdateEditDeckCardsAmount()
        {
            Deck currentDeck = _currentEditDeck;
            if (currentDeck != null)
            {
                if (_tutorialManager.IsTutorial)
                {
                    _textEditDeckCardsAmount.text = currentDeck.GetNumCards() + " / " + _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount;
                }
                else
                {
                    _textEditDeckCardsAmount.text = currentDeck.GetNumCards() + " / " + Constants.DeckMaxSize;
                }
            }
        }
        
        private void UpdateDeckCardPage()
        {
            int startIndex = _deckPageIndex * GetDeckCardAmountPerPage();
            int endIndex = (_deckPageIndex + 1) * GetDeckCardAmountPerPage();
            List<BoardCard> displayCardList = new List<BoardCard>();
            for( int i=0; i<_createdDeckBoardCards.Count; ++i)
            {   
                if(i >= startIndex && i < endIndex)
                {
                    _createdDeckBoardCards[i].GameObject.SetActive(true);
                    displayCardList.Add(_createdDeckBoardCards[i]);                    
                }
                else
                {
                    _createdDeckBoardCards[i].GameObject.SetActive(false);
                }
            }
            for(int i=0; i<displayCardList.Count; ++i)
            {
                displayCardList[i].Transform.position = DeckCardPositions[i].position;
            }
        }

        private void MoveDeckPageIndex(int direction)
        {
            _deckPageIndex = Mathf.Clamp(_deckPageIndex + direction, 0, GetDeckPageAmount() - 1);
        }
        
        private void ResetCollectionsBoardCards()
        {
            foreach (BoardCard item in _createdCollectionsBoardCards)
            {
                item.Dispose();
            }

            _createdCollectionsBoardCards.Clear();
        }
        
        private int GetDeckPageAmount()
        {
            return Mathf.CeilToInt((float) _createdDeckBoardCards.Count / GetDeckCardAmountPerPage());
        }
        
        private int GetDeckCardAmountPerPage()
        {
            return DeckCardPositions.Count;
        }

        private void ResetDeckBoardCards()
        {
            foreach (BoardCard item in _createdDeckBoardCards)
            {
                item.Dispose();
            }

            _createdDeckBoardCards.Clear();
        }
        
        private void DisposeBoardCards()
        {
            ResetCollectionsBoardCards();
            ResetDeckBoardCards();
            Object.Destroy(CollectionsCardPlaceholders);
            Object.Destroy(DeckCardPlaceholders);
        }
        
        public uint GetMaxCopiesValue(IReadOnlyCard card)
        {
            Enumerators.CardRank rank = card.CardRank;
            uint maxCopies;

            Enumerators.SetType setType = GameClient.Get<IGameplayManager>().GetController<CardsController>().GetSetOfCard(card);

            if (setType == Enumerators.SetType.ITEM)
            {
                maxCopies = Constants.CardItemMaxCopies;
                return maxCopies;
            }

            switch (rank)
            {
                case Enumerators.CardRank.MINION:
                    maxCopies = Constants.CardMinionMaxCopies;
                    break;
                case Enumerators.CardRank.OFFICER:
                    maxCopies = Constants.CardOfficerMaxCopies;
                    break;
                case Enumerators.CardRank.COMMANDER:
                    maxCopies = Constants.CardCommanderMaxCopies;
                    break;
                case Enumerators.CardRank.GENERAL:
                    maxCopies = Constants.CardGeneralMaxCopies;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            return maxCopies;
        }

        #endregion

        #region Util

        private void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        #endregion
    }
}