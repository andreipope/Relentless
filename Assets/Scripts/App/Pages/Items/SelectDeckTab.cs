
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class DeckInfoObject
    {
        public DeckId DeckId;
        public Button Button;
        public TextMeshProUGUI TextDeckName;
        public Image ImagePanel;
        public Image ImageOverlordThumbnail;
        public Image[] ImageAbilityIcons;
        public TextMeshProUGUI TextCardsAmount;
    }

    public class SelectDeckTab
    {
        private static readonly ILog Log = Logging.GetLog(nameof(SelectDeckTab));

        private TMP_InputField _inputFieldSearchDeckName;
        private Transform _paginationGroup;

        private Button _buttonNewDeck;
        private Button _buttonEdit;
        private Button _buttonDelete;
        private Button _buttonRename;
        private Button _buttonLeftArrow;
        private Button _buttonRightArrow;
        private Button _buttonSelectDeckFilter;

        private IDataManager _dataManager;
        private IAnalyticsManager _analyticsManager;
        private IUIManager _uiManager;
        private ITutorialManager _tutorialManager;
        private ILoadObjectsManager _loadObjectsManager;

        private HordeSelectionWithNavigationPage _myDeckPage;

        private List<Deck> _cacheDeckListToDisplay;
        private List<DeckInfoObject> _deckInfoObjectList;

        private int _deckPageIndex;
        private const int _deckInfoAmountPerPage = 4;

        private GameObject _imagePageDotNormal;
        private GameObject _imagePageDotSelected;

        private Sprite _spriteDeckThumbnailNormal;
        private Sprite _spriteDeckThumbnailSelected;

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _cacheDeckListToDisplay = new List<Deck>();
            _deckInfoObjectList = new List<DeckInfoObject>();
        }

        public void Show(GameObject selectDeckObj)
        {
            _inputFieldSearchDeckName = selectDeckObj.transform.Find("Panel_FrameComponents/Upper_Items/InputText_SearchDeckName").GetComponent<TMP_InputField>();
            _inputFieldSearchDeckName.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            _inputFieldSearchDeckName.text = "SEARCH";

            _paginationGroup = selectDeckObj.transform.Find("Panel_Content/Pagination_Group");

            _imagePageDotNormal = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/DeckSelection/Image_CircleDot_Normal");
            _imagePageDotSelected = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/DeckSelection/Image_CircleDot_Selected");

            _spriteDeckThumbnailNormal = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/deck_thumbnail_normal");
            _spriteDeckThumbnailSelected = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/deck_thumbnail_selected");

            _buttonNewDeck = selectDeckObj.transform.Find("Panel_Content/Button_BuildNewDeck").GetComponent<Button>();
            _buttonNewDeck.onClick.AddListener(ButtonNewDeckHandler);

            _buttonLeftArrow = selectDeckObj.transform.Find("Panel_Content/Button_LeftArrow").GetComponent<Button>();
            _buttonLeftArrow.onClick.AddListener(ButtonLeftArrowHandler);

            _buttonRightArrow = selectDeckObj.transform.Find("Panel_Content/Button_RightArrow").GetComponent<Button>();
            _buttonRightArrow.onClick.AddListener(ButtonRightArrowHandler);

            _buttonSelectDeckFilter = selectDeckObj.transform.Find("Panel_FrameComponents/Upper_Items/Button_Filter").GetComponent<Button>();
            _buttonSelectDeckFilter.onClick.AddListener(ButtonSelectDeckFilterHandler);

            _buttonEdit = selectDeckObj.transform.Find("Panel_FrameComponents/Lower_Items/Button_Edit").GetComponent<Button>();
            _buttonEdit.onClick.AddListener(ButtonEditHandler);

            _buttonDelete = selectDeckObj.transform.Find("Panel_FrameComponents/Lower_Items/Button_Delete").GetComponent<Button>();
            _buttonDelete.onClick.AddListener(ButtonDeleteHandler);

            _buttonRename = selectDeckObj.transform.Find("Panel_FrameComponents/Lower_Items/Button_Rename").GetComponent<Button>();
            _buttonRename.onClick.AddListener(ButtonRenameHandler);

            _myDeckPage = GameClient.Get<IUIManager>().GetPage<HordeSelectionWithNavigationPage>();

            LoadObjects(selectDeckObj);
        }

        public void Dispose()
        {
            _deckInfoObjectList.Clear();
            _cacheDeckListToDisplay.Clear();
        }

        private void LoadObjects(GameObject selectDeckObj)
        {
            _deckInfoObjectList.Clear();
            for (int i = 0; i < 4; ++i)
            {
                DeckInfoObject deckInfoObject = new DeckInfoObject();

                string path = $"Panel_Content/Button_DeckSelect_{i}";
                deckInfoObject.Button = selectDeckObj.transform.Find(path).GetComponent<Button>();
                deckInfoObject.TextDeckName = selectDeckObj.transform.Find(path+"/Text_DeckName").GetComponent<TextMeshProUGUI>();
                deckInfoObject.TextCardsAmount = selectDeckObj.transform.Find(path+"/Text_CardsAmount").GetComponent<TextMeshProUGUI>();
                deckInfoObject.ImagePanel = selectDeckObj.transform.Find(path+"/Image_DeckThumbnailNormal").GetComponent<Image>();
                deckInfoObject.ImageOverlordThumbnail = selectDeckObj.transform.Find(path+"/Image_DeckThumbnail").GetComponent<Image>();
                deckInfoObject.ImageAbilityIcons = new Image[]
                {
                    selectDeckObj.transform.Find(path+"/Image_SkillIcon_1").GetComponent<Image>(),
                    selectDeckObj.transform.Find(path+"/Image_SkillIcon_2").GetComponent<Image>()
                };
                _deckInfoObjectList.Add(deckInfoObject);
            }
        }

        public void ChangeSelectedDeckName(string newDeckName)
        {
            DeckInfoObject deckUI = _deckInfoObjectList.Find(deckObj => deckObj.DeckId == _myDeckPage.CurrentEditDeck.Id);
            deckUI.TextDeckName.text = newDeckName;
        }

        public void InputFieldApplyFilter()
        {
            _inputFieldSearchDeckName.text = "";
            ApplyDeckByLastSelected();
            if (!_tutorialManager.IsTutorial)
                ApplyFilter();
        }

        private void ApplyDeckByLastSelected()
        {
            _cacheDeckListToDisplay = _myDeckPage.GetDeckList();

            int indexInPage = 0;
            if(_myDeckPage.SelectDeckIndex < _deckInfoAmountPerPage-1)
            {
                _deckPageIndex = 0;
                indexInPage = _myDeckPage.SelectDeckIndex + 1;
            }
            else
            {
                int deckIndexAfterSubtractFistPage = _myDeckPage.SelectDeckIndex - (_deckInfoAmountPerPage - 1);
                _deckPageIndex = (deckIndexAfterSubtractFistPage / _deckInfoAmountPerPage) + 1;
                indexInPage = deckIndexAfterSubtractFistPage % _deckInfoAmountPerPage;
            }

            UpdateDeckInfoObjects();
            ChangeSelectDeckIndex(indexInPage);
        }

        private void OnInputFieldSearchEndedEdit(string value)
        {
            ApplyDeckSearch();
        }

        private void ApplyDeckSearch()
        {
            _cacheDeckListToDisplay = GetDeckListBySearchKeywordToDisplay();
            _deckPageIndex = 0;
            UpdateDeckInfoObjects();
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

        private void UpdateDeckInfoObjects()
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
                MultiPointerClickHandler multiPointerClickHandler = deckInfoObject.Button.gameObject.GetComponent<MultiPointerClickHandler>();
                if (multiPointerClickHandler == null)
                {
                    multiPointerClickHandler = deckInfoObject.Button.gameObject.AddComponent<MultiPointerClickHandler>();
                    multiPointerClickHandler.DoubleClickReceived += ()=>
                    {
                        ChangeSelectDeckIndex(index);
                        ButtonEditHandler();
                        PlayClickSound();
                    };
                }

#endif

                Deck deck = deckListToDisplay[deckDataIndex];

                string deckName = deck.Name;
                int cardsAmount = deck.GetNumCards();
                OverlordUserInstance overlord = _dataManager.CachedOverlordData.GetOverlordById(deck.OverlordId);

                deckInfoObject.DeckId = deck.Id;
                deckInfoObject.TextDeckName.text = deckName;
                if (_tutorialManager.IsTutorial)
                {
                    deckInfoObject.TextCardsAmount.text = $"{cardsAmount}/{_tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount}";
                }
                else
                {
                    deckInfoObject.TextCardsAmount.text = $"{cardsAmount}/{Constants.MaxDeckSize}";
                }
                deckInfoObject.ImageOverlordThumbnail.sprite = GetOverlordThumbnailSprite(overlord.Prototype.Faction);

                if(deck.PrimarySkill == Enumerators.Skill.NONE)
                {
                    deckInfoObject.ImageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
                }
                else
                {
                    string iconPath = overlord.GetSkill(deck.PrimarySkill).Prototype.IconPath;
                    deckInfoObject.ImageAbilityIcons[0].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
                }

                if(deck.SecondarySkill == Enumerators.Skill.NONE)
                {
                    deckInfoObject.ImageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
                }
                else
                {
                    string iconPath = overlord.GetSkill(deck.SecondarySkill).Prototype.IconPath;
                    deckInfoObject.ImageAbilityIcons[1].sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
                }

                deckInfoObject.Button.onClick.RemoveAllListeners();
                deckInfoObject.Button.onClick.AddListener(() =>
                {
                    _myDeckPage.SelectedDeckId = (int) deck.Id.Id;
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

        private int GetDefaultDeckIndex()
        {
            return _deckPageIndex == 0 ? 1 : 0;
        }

        private int GetDeckPageAmount(List<Deck> deckList)
        {
            if(deckList.Count <= _deckInfoAmountPerPage-1)
            {
                return 1;
            }

            return (deckList.Count - _deckInfoAmountPerPage) / _deckInfoAmountPerPage + 2;
        }

        private void ChangeSelectDeckIndex(int newIndexInPage)
        {
            UpdateSelectedDeckDisplay(newIndexInPage);
            if(_deckPageIndex == 0)
            {
                _myDeckPage.SelectDeckIndex = newIndexInPage-1;
            }
            else
            {
                _myDeckPage.SelectDeckIndex = newIndexInPage + (_deckPageIndex-1) * _deckInfoAmountPerPage + (_deckInfoAmountPerPage-1);
            }

            if (_tutorialManager.IsTutorial && _dataManager.CachedDecksData.Decks.Count > 1)
            {
                _myDeckPage.SelectDeckIndex = 1;
            }

            _myDeckPage.AssignCurrentDeck();
        }

        public Deck GetSelectedDeck()
        {
            List<Deck> deckList = _myDeckPage.GetDeckList();
            return deckList.Find(deck => deck.Id.Id == _myDeckPage.SelectedDeckId);
        }

        private List<Deck> GetDeckListBySearchKeywordToDisplay()
        {
            List<Deck> deckList = _myDeckPage.GetDeckList();
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

            OpenOverlordSelectionPopup();

            if (_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.HordeTabChanged);
            }
        }

        public void OpenOverlordSelectionPopup()
        {
            _myDeckPage.SelectedDeckId = -1;
            _uiManager.DrawPopup<OverlordSelectionPopup>();
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

        private void MoveDeckPageIndex(int direction)
        {
            _deckPageIndex = Mathf.Clamp(_deckPageIndex + direction, 0, GetDeckPageAmount(_cacheDeckListToDisplay) - 1);
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
            ApplyDeckFilter();
        }

        private void ApplyFilter()
        {
            if(CheckAvailableDeckExist())
            {
                ApplyDeckFilter();
            }
            else
            {
                _uiManager.DrawPopup<WarningPopup>("No decks found for the selected faction.");
                _uiManager.DrawPopup<ElementFilterPopup>();
            }

            ElementFilterPopup popup = _uiManager.GetPopup<ElementFilterPopup>();
            popup.ActionPopupHiding -= FilterPopupHidingHandler;
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

        private List<Deck> GetDeckListByElementToDisplay(Enumerators.Faction faction)
        {
            List<Deck> deckList = _myDeckPage.GetDeckList();

            List<Deck> deckListToDisplay = new List<Deck>();
            for (int i = 0; i < deckList.Count; ++i)
            {
                OverlordUserInstance overlord = _dataManager.CachedOverlordData.GetOverlordById(deckList[i].OverlordId);
                if (faction == overlord.Prototype.Faction)
                {
                    deckListToDisplay.Add(deckList[i]);
                }
            }

            return deckListToDisplay;
        }

        public void ApplyDeckFilter()
        {
            _inputFieldSearchDeckName.text = "";

            ElementFilterPopup elementFilterPopup = _uiManager.GetPopup<ElementFilterPopup>();
            if(elementFilterPopup.SelectedFactionList.Count == elementFilterPopup.AvailableFactionList.Count)
            {
                _cacheDeckListToDisplay = _myDeckPage.GetDeckList();
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

        private void ButtonEditHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonEdit.name))
                return;

            PlayClickSound();
            _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.Editing);
        }

        private void ButtonDeleteHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonDelete.name))
                return;

            PlayClickSound();
            if (_myDeckPage.GetDeckList().Count <= 1)
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

        private async void ConfirmDeleteDeckReceivedHandler(bool status)
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
            await deckGeneratorController.ProcessDeleteDeck(deck);

            _analyticsManager.SetEvent(AnalyticsManager.EventDeckDeleted);
        }

        private void FinishDeleteDeck(bool success, Deck deck)
        {
            _buttonDelete.enabled = true;

            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().FinishDeleteDeck -= FinishDeleteDeck;

            _cacheDeckListToDisplay = _myDeckPage.GetDeckList();
            _myDeckPage.SelectDeckIndex = Mathf.Min(_myDeckPage.SelectDeckIndex, _cacheDeckListToDisplay.Count-1);

            _myDeckPage.SelectedDeckId = (int)_cacheDeckListToDisplay[_myDeckPage.SelectDeckIndex].Id.Id;
            _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SelectDeck);
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

        private void ButtonRenameHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonRename.name))
                return;

            PlayClickSound();

            _uiManager.DrawPopup<RenamePopup>(new object[] { _myDeckPage.CurrentEditDeck, false});
        }

        private void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        private void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
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
    }
}
