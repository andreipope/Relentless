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

        private MyDecksEditTab _myDecksEditTab; 
        
        private GameObject _selfPage;

        private GameObject[] _tabObjects;

        private Sprite _spriteDeckThumbnailNormal,
                       _spriteDeckThumbnailSelected;

        private Transform _trayButtonBack,
                          _trayButtonAuto;
                          
        public Transform LocatorCollectionCards,
                         LocatorDeckCards;                          

        private Button _buttonNewDeck,
                       _buttonBack,
                       _buttonSelectDeckFilter,
                       _buttonSelectDeckSearch,
                       _buttonEdit,
                       _buttonDelete,
                       _buttonRename,
                       _buttonAuto,                       
                       _buttonSelectOverlordLeftArrow,
                       _buttonSelectOverlordRightArrow,
                       _buttonSelectOverlordContinue,
                       _buttonSelectOverlordSkillContinue;

        public Button ButtonSaveRenameDeck;                           
                       
        private TMP_InputField _inputFieldDeckName;

        private TextMeshProUGUI _textSelectOverlordName,
                                _textSelectOverlordDescription,
                                _textSelectOverlordDeckName,
                                _textSelectOverlordSkillDeckname;

        private List<DeckInfoObject> _deckInfoObjectList;

        private Image _imageSelectOverlordGlow,
                      _imageSelectOverlordPortrait,
                      _imageSelectOverlordSkillPortrait;

        private List<Transform> _selectOverlordIconList;    
        
        public GameObject DragAreaDeck, 
                          DragAreaCollections; 
                         
        private CardHighlightingVFXItem _highlightingVFXItem;   
        
        public event Action<TAB> EventChangeTab;

        #region Cache Data
        
        private const int _numberOfDeckInfo = 3;

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
        
        private int _selectDeckIndex;

        private int _selectOverlordIndex;

        public Deck CurrentEditDeck;

        public Hero CurrentEditHero;

        public bool IsEditingNewDeck;

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

            _myDecksEditTab = new MyDecksEditTab();
            _myDecksEditTab.Init();
        }

        public void Update()
        {
            _myDecksEditTab.Update();
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
            
            LocatorCollectionCards = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Locator_CollectionCards");            
            LocatorDeckCards = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Locator_DeckCards");
            
            _spriteDeckThumbnailNormal = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_Content/Sprite_deck_thumbnail_normal").GetComponent<Image>().sprite;
            _spriteDeckThumbnailSelected = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_Content/Sprite_deck_thumbnail_selected").GetComponent<Image>().sprite;            

            _textSelectOverlordName = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_Content/Text_SelectOverlord").GetComponent<TextMeshProUGUI>();
            _textSelectOverlordDescription = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_Content/Text_Desc").GetComponent<TextMeshProUGUI>();
            _textSelectOverlordDeckName = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();
            _textSelectOverlordSkillDeckname = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();
            
            _imageSelectOverlordGlow = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_Content/Image_Glow").GetComponent<Image>();
            _imageSelectOverlordPortrait = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlord/Panel_Content/Image_OverlordPortrait").GetComponent<Image>();
            _imageSelectOverlordSkillPortrait = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_Content/Image_OverlordPortrait").GetComponent<Image>();
            
            DragAreaDeck = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/DragArea_Deck").gameObject;
            DragAreaCollections = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/DragArea_Collections").gameObject;
            
            _highlightingVFXItem = new CardHighlightingVFXItem(Object.Instantiate(
            _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/UI/ArmyCardSelection"), _selfPage.transform, true));

            _myDecksEditTab.Show(_selfPage);
            
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
            _myDecksEditTab.Dispose();
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
            _myDecksEditTab.ProcessRenameDeck(deck, newName);
        }
        
        private void ButtonAutoHandler()
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
            CurrentEditHero = _dataManager.CachedHeroesData.Heroes[_selectOverlordIndex];
            AssignCurrentDeck(true);
            ProcessAddDeck();            
        }
        
        private async void ButtonSelectOverlordSkillContinueHandler()
        {
            _buttonSelectOverlordSkillContinue.interactable = false;
            Deck deck = CurrentEditDeck;
        
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

        public List<Deck> GetDeckList()
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
                    UpdateDeckInfoObjects();
                    break;
                case TAB.RENAME:
                    _inputFieldDeckName.text = GetSelectedDeck().Name;
                    break;
                case TAB.EDITING:                                      
                    break;
                case TAB.SELECT_OVERLORD:
                    _textSelectOverlordDeckName.text = "NEW DECK";
                    ChangeOverlordIndex(0);
                    break;
                case TAB.SELECT_OVERLORD_SKILL:
                    _textSelectOverlordSkillDeckname.text = CurrentEditDeck.Name;
                    break;
                default:
                    break;
            }            
            
            EventChangeTab?.Invoke(_tab);
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
        
        private async void ProcessAddDeck()
        {
            bool success = true;
            CurrentEditDeck.HeroId = CurrentEditHero.HeroId;
            CurrentEditDeck.PrimarySkill = CurrentEditHero.PrimarySkill;
            CurrentEditDeck.SecondarySkill = CurrentEditHero.SecondarySkill;

            try
            {
                long newDeckId = await _backendFacade.AddDeck(_backendDataControlMediator.UserDataModel.UserId, CurrentEditDeck);
                CurrentEditDeck.Id = newDeckId;
                _dataManager.CachedDecksData.Decks.Add(CurrentEditDeck);
                _analyticsManager.SetEvent(AnalyticsManager.EventDeckCreated);
                Debug.Log(" ====== Add Deck " + newDeckId + " Successfully ==== ");

                if(_tutorialManager.IsTutorial)
                {
                    _dataManager.CachedUserLocalData.TutorialSavedDeck = CurrentEditDeck;
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
                _dataManager.CachedUserLocalData.LastSelectedDeckId = (int)CurrentEditDeck.Id;
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);                

                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.HordeSaved);

                _selectDeckIndex = GetDeckList().IndexOf(CurrentEditDeck);
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
            
            _buttonEdit = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Edit").GetComponent<Button>();
            _buttonEdit.onClick.AddListener(ButtonEditHandler);
            _buttonEdit.onClick.AddListener(PlayClickSound);
            
            _buttonDelete = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Delete").GetComponent<Button>();
            _buttonDelete.onClick.AddListener(ButtonDeleteHandler);
            _buttonDelete.onClick.AddListener(PlayClickSound);
            
            _buttonRename = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_FrameComponents/Lower_Items/Button_Rename").GetComponent<Button>();
            _buttonRename.onClick.AddListener(ButtonRenameHandler);
            _buttonRename.onClick.AddListener(PlayClickSound);
            
            ButtonSaveRenameDeck = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Rename/Panel_FrameComponents/Lower_Items/Button_Save").GetComponent<Button>();
            ButtonSaveRenameDeck.onClick.AddListener(ButtonSaveRenameDeckHandler);
            ButtonSaveRenameDeck.onClick.AddListener(PlayClickSound);
            
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