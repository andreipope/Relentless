using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Localization;
using Loom.ZombieBattleground.Data;
using TMPro;
using log4net;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class DeckSelectionPopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(DeckSelectionPopup));

        public Action<Deck> SelectDeckEvent;

        public GameObject Self { get; private set; }

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IDataManager _dataManager;

        private ITutorialManager _tutorialManager;

        private TextMeshProUGUI _textDeckName;

        private Button _buttonLeft, _buttonRight;

        private GameObject _deckIconPrefab;

        private List<DeckIcon> _createdDeckIconList;

        private Transform _deckIconGroup;

        private const float DeckIconScaleNormal = 0.7178f;

        private const float DeckIconScaleSelected = 1f;

        private List<Deck> _deckList;

        private DeckId _selectedDeckId;

        private List<Vector3> _deckIconPositionList;

        #region IUIPopup

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            LoginPopup.OnLoginSuccess += () =>
            {
                if (Self != null)
                {
                    ReloadDeckDataAndDisplay();
                }
            };

            _deckIconPositionList = new List<Vector3>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

            DisposeCreatedObject();
            _deckIconPositionList.Clear();
            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/DeckSelectionPopup"),
                _uiManager.Canvas2.transform,
                false);

            _deckIconPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/DeckSelection/Image_DeckIcon");

            _textDeckName = Self.transform.Find("Text_DeckName").GetComponent<TextMeshProUGUI>();
            _deckIconGroup = Self.transform.Find("Panel_DeckContent/Group");

            _buttonRight = Self.transform.Find("Button_Right").GetComponent<Button>();
            _buttonLeft = Self.transform.Find("Button_Left").GetComponent<Button>();
            _buttonRight.onClick.AddListener(ButtonRightHandler);
            _buttonLeft.onClick.AddListener(ButtonLeftHandler);

            Transform frameGroup = Self.transform.Find("Frame_Group");
            _deckIconPositionList.Clear();
            foreach (Transform frame in frameGroup)
            {
                _deckIconPositionList.Add(frame.position);
            }

            ReloadDeckDataAndDisplay();

            ((AppStateManager)GameClient.Get<IAppStateManager>()).ConnectionStatusDidUpdate += ReloadDeckDataAndDisplay;
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        #endregion

        private void ReloadDeckDataAndDisplay()
        {
            LoadDefaultDeckData();
            LoadDeckObjects();
            UpdateSelectedDeckDisplay();
        }

        #region Deck Data

        private void LoadDefaultDeckData()
        {
            Deck selectedDeck = _dataManager.CachedDecksData.Decks.Find(x => x.Id == _dataManager.CachedUserLocalData.LastSelectedDeckId);

            if(selectedDeck == null && _dataManager.CachedDecksData.Decks.Count > 0)
            {
                selectedDeck = _dataManager.CachedDecksData.Decks[0];
            }

            _deckList = new List<Deck>();
            HordeSelectionWithNavigationPage hordeSelection = _uiManager.GetPage<HordeSelectionWithNavigationPage>();
            _deckList.AddRange(hordeSelection.GetDeckList());

            if (GameClient.Get<IGameplayManager>().IsTutorial && _dataManager.CachedDecksData.Decks.Count > 1 && _deckList.Count > 0)
            {
                selectedDeck = _deckList[_deckList.Count - 1];
            }

            SaveLastSelectedDeckId(selectedDeck);
        }

        private void SaveLastSelectedDeckId(Deck deck)
        {
            _selectedDeckId = deck.Id;
            SaveLastSelectedDeckId();
        }

        private void SaveLastSelectedDeckId()
        {
            if (_dataManager.CachedDecksData.Decks == null)
            {
                Log.Warn($"CachedDecksData.Decks: {_dataManager.CachedDecksData.Decks} is null! Data was loaded incorrectly!");
                return;
            }

            _dataManager.CachedUserLocalData.LastSelectedDeckId = _selectedDeckId;
            _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
        }

        private Deck GetSelectedDeck()
        {
            if (_deckList != null && _deckList.Count > 0)
            {
                return _deckList.Find(x => x.Id.Equals(_selectedDeckId));
            }

            return _dataManager.CachedDecksData.Decks.Find(x => x.Id.Equals(_selectedDeckId));
        }

        public Deck GetLastSelectedDeckFromCache()
        {
            if (_deckList != null && _deckList.Count > 0)
            {
                return _deckList.Find(x => x.Id == _dataManager.CachedUserLocalData.LastSelectedDeckId);
            }

            return _dataManager.CachedDecksData.Decks.Find(x => x.Id == _dataManager.CachedUserLocalData.LastSelectedDeckId);
        }

        private int GetSelectedDeckIndex()
        {
            return _deckList.FindIndex(x => x.Id.Equals(_selectedDeckId));
        }

        public Deck GetDefaultDeck()
        {
            return _dataManager.CachedDecksData.Decks[0];
        }

        public List<Deck> GetDeckList()
        {
            return _deckList;
        }

        private void SetSelectedDeckIdByIndex(int newIndex)
        {
            SaveLastSelectedDeckId(_deckList[newIndex]);
            UpdateSelectedDeckDisplay();

            SelectDeckEvent?.Invoke(GetSelectedDeck());
        }

        private void SwitchSelectedDeckIndex(int direction)
        {
            if (direction == 0)
                return;

            if (_deckList.Count <= 0)
            {
                Log.Info("No deck in list");
                return;
            }

            int nextIndex = GetSelectedDeckIndex() + direction;
            if(nextIndex >= _deckList.Count)
            {
                nextIndex = 0;
            }else if(nextIndex < 0)
            {
                nextIndex = _deckList.Count - 1;
            }

            SetSelectedDeckIdByIndex(nextIndex);
        }

        #endregion

        #region Deck Display

        private void LoadDeckObjects()
        {
            DisposeCreatedObject();
            _createdDeckIconList = new List<DeckIcon>();

            for (int i = 0; i < _deckList.Count; i++)
            {
                GameObject deckIconObj = Object.Instantiate(_deckIconPrefab, _deckIconGroup, false);
                deckIconObj.transform.localScale = Vector3.one * DeckIconScaleNormal;

                DeckIcon deckIcon = new DeckIcon(_deckList[i].Id);
                deckIcon.Init(deckIconObj);
                deckIcon.SetDeckIcon();

                _createdDeckIconList.Add(deckIcon);

                MultiPointerClickHandler multiPointerClickHandler = deckIconObj.AddComponent<MultiPointerClickHandler>();
                int index = i;
                multiPointerClickHandler.SingleClickReceived += ()=>
                {
                    if (_tutorialManager.IsTutorial)
                        return;

                    SetSelectedDeckIdByIndex(index);
                };
                multiPointerClickHandler.DoubleClickReceived += ()=>
                {
                    if (_tutorialManager.IsTutorial || _tutorialManager.BattleShouldBeWonBlocker)
                        return;

                    GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.HordeSelection);
                    HordeSelectionWithNavigationPage hordeSelection = _uiManager.GetPage<HordeSelectionWithNavigationPage>();

                    Deck selectedDeck = _deckList.Find(deck => deck.Id == deckIcon.DeckId);
                    hordeSelection.SelectedDeckId = (int)selectedDeck.Id.Id;
                    hordeSelection.CurrentEditDeck = selectedDeck;
                    hordeSelection.ChangeTab(HordeSelectionWithNavigationPage.Tab.Editing);
                };
            }

            AddNewDeckButton();
        }

        private void AddNewDeckButton()
        {
            GameObject deckIconObj = Object.Instantiate(_deckIconPrefab, _deckIconGroup, false);
            deckIconObj.transform.localScale = Vector3.one * DeckIconScaleNormal;

            DeckIcon deckIcon = new DeckIcon(null);
            deckIcon.Init(deckIconObj);
            deckIcon.SetDeckIcon();

            _createdDeckIconList.Add(deckIcon);

            deckIconObj.AddComponent<MultiPointerClickHandler>().SingleClickReceived += ()=>
            {
                if (_tutorialManager.IsTutorial)
                    return;

                if (_dataManager.CachedDecksData.Decks.Count >= Constants.MaxDecksCount)
                {
                    _uiManager.DrawPopup<WarningPopup>
                    (
                        LocalizationUtil.GetLocalizedString
                        (
                            LocalizationTerm.Warning_HordeSelection_MaxDeck
                        )
                        .Replace("{MAX_DECKS}", Constants.MaxDecksCount.ToString())
                    );
                    return;
                }

                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.HordeSelection);
                HordeSelectionWithNavigationPage deckPage = _uiManager.GetPage<HordeSelectionWithNavigationPage>();
                deckPage.HordeSelectDeckTab.OpenOverlordSelectionPopup();
            };
        }

        private void UpdateSelectedDeckDisplay()
        {
            Deck selectedDeck = GetSelectedDeck();
            int deckIndex = GetSelectedDeckIndex();
            _textDeckName.text = selectedDeck.Name;
            OverlordUserInstance selectedOverlord = DataUtilities.GetOverlordDataFromDeck(selectedDeck);
            _uiManager.GetPage<MainMenuWithNavigationPage>().SetOverlordPortrait(selectedOverlord.Prototype.Faction);

            int middleFrameIndex = _deckIconPositionList.Count / 2;
            int shiftIndex = deckIndex - middleFrameIndex;

            int frameIndex;
            GameObject deckIcon;
            float scale;
            for (int i = 0; i < _createdDeckIconList.Count; ++i)
            {
                deckIcon = _createdDeckIconList[i].GetGameObject();
                frameIndex = i - shiftIndex;
                if(frameIndex < 0 || frameIndex >= _deckIconPositionList.Count)
                {
                    deckIcon.SetActive(false);
                }
                else
                {
                    deckIcon.SetActive(true);
                    deckIcon.transform.position = _deckIconPositionList[frameIndex];
                    scale = i == deckIndex ? DeckIconScaleSelected : DeckIconScaleNormal;
                    deckIcon.transform.localScale = Vector3.one * scale;
                }
            }
        }

        private void DisposeCreatedObject()
        {
            if(_createdDeckIconList != null)
            {
                for(int i =_createdDeckIconList.Count-1; i>=0; i--)
                {
                    Object.Destroy(_createdDeckIconList[i].GetGameObject());
                }
                _createdDeckIconList.Clear();
                _createdDeckIconList = null;
            }
        }

        #endregion

        #region Buttons Handlers

        private void ButtonRightHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonRight.name))
                return;

            SwitchSelectedDeckIndex(1);
        }

        private void ButtonLeftHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonLeft.name))
                return;

            SwitchSelectedDeckIndex(-1);
        }

        #endregion
    }


    public class DeckIcon
    {
        public DeckId? DeckId;
        public Image OverlordImage;

        private GameObject Self;

        public DeckIcon(DeckId? deckId)
        {
            DeckId = deckId;
        }

        public void Init(GameObject obj)
        {
            Self = obj;
            OverlordImage = Self.GetComponent<Image>();
        }

        public void SetDeckIcon()
        {
            OverlordImage.gameObject.SetActive(DeckId != null);

            if (DeckId != null)
            {
                Enumerators.Faction faction = DataUtilities.GetOverlordDataFromDeck((DeckId) DeckId).Prototype.Faction;
                OverlordImage.sprite = DataUtilities.GetOverlordDeckIcon(faction);
            }
        }

        public GameObject GetGameObject()
        {
            return Self;
        }
    }
}
