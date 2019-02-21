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
                       _buttonSaveEditDeck;               
                       
        private TMP_InputField _inputFieldDeckName;

        private TextMeshProUGUI _textEditDeckName,
                                _textEditDeckCardsAmount;

        private List<DeckInfoObject> _deckInfoObjectList;

        private const int _numberOfDeckInfo = 3;

        #region Cache Data

        private enum TAB
        {
            NONE = -1,
            SELECT_DECK = 0,
            RENAME = 1,
            EDITING = 2,
        }
        
        private TAB _tab;
        
        private int _selectDeckIndex;

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
            _locatorCollectionCards.gameObject.SetActive(false);
            
            _locatorDeckCards = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_Content/Locator_DeckCards");
            _locatorDeckCards.gameObject.SetActive(false);

            _spriteDeckThumbnailNormal = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_Content/Sprite_deck_thumbnail_normal").GetComponent<Image>().sprite;
            _spriteDeckThumbnailSelected = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_Content/Sprite_deck_thumbnail_selected").GetComponent<Image>().sprite;

            _textEditDeckName = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();
            _textEditDeckCardsAmount = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing/Panel_FrameComponents/Lower_Items/Image_CardCounter/Text_CardsAmount").GetComponent<TextMeshProUGUI>();

            InitBoardCardComponents();
            InitButtons();
            InitObjects();            
            InitTabs();
            ChangeDeckIndex(0);            
        }
        
        public void Hide()
        {
            Dispose();
        
            if (_selfPage == null)
                return;
        
            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
            
            _deckInfoObjectList.Clear();
            
            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }
        
        public void Dispose()
        {
            DisposeBoardCards();   
        }

        #endregion

        #region UI Handlers

        private void ButtonNewDeckHandler()
        {
            _uiManager.SetPage<HordeSelectionPage>();
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
            ChangeTab(TAB.EDITING);
        }        
        
        private void ButtonDeleteHandler()
        {
            if (GetDeckList().Count <= 1)
            {
                OpenAlertDialog("Sorry, Not able to delete Last Deck.");
                return;
            }

            Deck deck = GetCurrentDeck();
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
            Deck deck = GetCurrentDeck();
            string newName = _inputFieldDeckName.text;
            ProcessRenameDeck(deck, newName);
        }

        private void ButtonSaveEditDeckHandler()
        {

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

        public void OnInputFieldEndedEdit(string value)
        {
            
        }
        
        private void ConfirmDeleteDeckReceivedHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmDeleteDeckReceivedHandler;

            if (!status)
                return;
                
            Deck deck = GetCurrentDeck();
            ProcessDeleteDeck(deck);

            _analyticsManager.SetEvent(AnalyticsManager.EventDeckDeleted);
        }

        #endregion

        #region Data and State

        private List<Deck> GetDeckList()
        {
            return _dataManager.CachedDecksData.Decks;
        }
        
        private Deck GetCurrentDeck()
        {
            List<Deck> deckList = GetDeckList();
            return deckList[_selectDeckIndex];
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
                newTab == TAB.RENAME
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
                    Deck deck = GetCurrentDeck();
                    _inputFieldDeckName.text = deck.Name;
                    break;
                case TAB.EDITING:
                    Deck deckEdit = GetCurrentDeck();
                    _textEditDeckName.text = deckEdit.Name;
                    _textEditDeckCardsAmount.text =  $"{deckEdit.Cards.Count}/{Constants.MaxDeckSize}";
                    LoadCollectionsCards(0,Enumerators.SetType.FIRE);
                    LoadDeckCards(deckEdit);
                    UpdateDeckCardPage();
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
        
        private void UpdateShowBackButton(bool isShow)
        {
            _trayButtonBack.gameObject.SetActive(isShow);
        }

        private void UpdateShowAutoButton(bool isShow)
        {
            _trayButtonAuto.gameObject.SetActive(isShow);
        }

        public async void ProcessRenameDeck(Deck currentDeck, string newName)
        {
            _buttonSaveRenameDeck.interactable = false;
            if (string.IsNullOrWhiteSpace(newName))
            {
                _buttonSaveRenameDeck.interactable = true;
                OpenAlertDialog("Saving Deck with an empty name is not allowed.");
                return;
            }

            List<Deck> deckList = GetDeckList();
            foreach (Deck deck in deckList)
            {
                if (currentDeck.Id != deck.Id &&
                    deck.Name.Trim().Equals(currentDeck.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    _buttonSaveRenameDeck.interactable = true;
                    OpenAlertDialog("Not able to Edit Deck: \n Deck Name already exists.");
                    return;
                }
            }
            
            currentDeck.Name = newName;
            bool success = true;
            try
            {
                await _backendFacade.EditDeck(_backendDataControlMediator.UserDataModel.UserId, currentDeck);

                for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count; i++)
                {
                    if (_dataManager.CachedDecksData.Decks[i].Id == currentDeck.Id)
                    {
                        _dataManager.CachedDecksData.Decks[i] = currentDeck;
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
                _dataManager.CachedUserLocalData.LastSelectedDeckId = (int)currentDeck.Id;
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                ChangeTab(TAB.SELECT_DECK);
                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.HordeSelection);
            }
            
            _buttonSaveRenameDeck.interactable = true;
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

        private void InitTabs()
        {
            _tab = TAB.NONE;
            ChangeTab(TAB.SELECT_DECK);
        }
        
        private void InitButtons()
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
        }

        private void InitObjects()
        {
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
                _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_Editing").gameObject
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

                Deck deck = deckList[i];
                
                string deckName = deck.Name;
                int cardsAmount = deck.Cards.Count;
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

        private Sprite GetOverlordThumbnailSprite(Enumerators.SetType heroElement)
        {
            string path = "Images/UI/MyDecks/OverlordPortrait/OverlordDeckThumbnail";
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
        
        private CollectionData _collectionDeckData;

        private int _deckPageIndex;
        
        private void InitBoardCardPrefabsAndLists()
        {
            CardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            CardItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard");
            CollectionsCardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersMyDecksLower");
            DeckCardPlaceholdersPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/CardPlaceholdersMyDecksUpper");            
            
            _createdDeckBoardCards = new List<BoardCard>();
            _createdCollectionsBoardCards = new List<BoardCard>();
            
            _collectionDeckData = new CollectionData();
            _collectionDeckData.Cards = new List<CollectionCardData>();
        }
        
        private void InitBoardCardComponents()
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
        }

        public void LoadCollectionsCards(int page, Enumerators.SetType setType)
        {
            CardSet set = SetTypeUtility.GetCardSet(_dataManager, setType);

            List<Card> cards = set.Cards;

            int startIndex = page * CollectionsCardPositions.Count;

            int endIndex = Mathf.Min(startIndex + CollectionsCardPositions.Count, cards.Count);

            ResetCollectionsBoardCards();
            _highlightingVFXItem.ChangeState(false);

            for (int i = startIndex; i < endIndex; i++)
            {
                if (i >= cards.Count)
                    break;

                Card card = cards[i];
                CollectionCardData cardData = _dataManager.CachedCollectionData.GetCardData(card.Name);

                // hack !!!! CHECK IT!!!
                if (cardData == null)
                    continue;

                BoardCard boardCard = CreateBoardCard(card, cardData.Amount, CollectionsCardPositions[i % CollectionsCardPositions.Count].position, 0.3f);
                _createdCollectionsBoardCards.Add(boardCard);                

                if (boardCard.LibraryCard.MouldId == _highlightingVFXItem.MouldId)
                {
                    _highlightingVFXItem.ChangeState(true);
                }
            }
        }
        
        private BoardCard CreateBoardCard(Card card, int amount, Vector3 position, float scale)
        {
            GameObject go;
            BoardCard boardCard;
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

            return boardCard;
        }

        public void LoadDeckCards(Deck deck)
        {
            ResetDeckBoardCards();

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
                    BoardCard boardCard = CreateBoardCard(libraryCard, 0, Vector3.zero, 0.3f);
                    boardCard.Transform.Find("Amount").gameObject.SetActive(false);

                    _createdDeckBoardCards.Add(boardCard);

                    //_collectionDeckData.GetCardData(card.CardName).Amount -= card.Amount;

                    Deck currentDeck = GetCurrentDeck();
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
            }

            //UpdateTopDeck();
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

        #endregion

        private void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
    }
}