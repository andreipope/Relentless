using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using DG.Tweening;
using log4net;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class DeckSelectionPopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(DeckSelectionPopup));

        public GameObject Self { get; private set; }

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private ISoundManager _soundManager;

        private IDataManager _dataManager;

        private TextMeshProUGUI _textDeckName;

        private GameObject _glowBorderVFX;

        private Button _buttonLeft, _buttonRight;

        private GameObject _deckIconPrefab;

        private List<GameObject> _createdDeckIconList;

        private Transform _deckIconGroup;

        private const float _deckIconScaleNormal = 0.7178f;

        private const float _deckIconScaleSelected = 1f;

        private List<Deck> _deckList;
        
        private int _selectDeckIndex;

        #region IUIPopup

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();

            LoginPopup.OnLoginSuccess += () =>
            {
                if (Self != null)
                {
                    ReloadDeckDataAndDisplay();
                }
            };
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

            DisposeCreatedObject();

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
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/DeckSelectionPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _deckIconPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/DeckSelection/Image_DeckIcon");

            _textDeckName = Self.transform.Find("Text_DeckName").GetComponent<TextMeshProUGUI>();
            _deckIconGroup = Self.transform.Find("Panel_DeckContent/Group");
            
            _glowBorderVFX = Self.transform.Find("Image_DeckIcon_Glow").gameObject;

            _buttonRight = Self.transform.Find("Button_Right").GetComponent<Button>();
            _buttonLeft = Self.transform.Find("Button_Left").GetComponent<Button>();
            _buttonRight.onClick.AddListener(ButtonRightHandler);
            _buttonLeft.onClick.AddListener(ButtonLeftHandler);

            ReloadDeckDataAndDisplay();                        
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        #endregion
        
        public void ReloadDeckDataAndDisplay()
        {
            LoadDefaultDeckData();
            LoadDeckObjects();
            UpdateSelectedDeckDisplay
            (
                GetSelectedDeck()
            );
        }

        #region Deck Data

        private void LoadDefaultDeckData()
        {
            int defaultSelectedDeckId = _dataManager.CachedUserLocalData.LastSelectedDeckId;

            if (defaultSelectedDeckId > _dataManager.CachedDecksData.Decks.Count)
            {
                defaultSelectedDeckId = 1;
            }
            if (_dataManager.CachedDecksData.Decks.Count > 0)
            {
                defaultSelectedDeckId = Mathf.Clamp(defaultSelectedDeckId, 1, defaultSelectedDeckId);
            }
            
            Deck selectedDeck = _dataManager.CachedDecksData.Decks.Find(x => x.Id == defaultSelectedDeckId);
            
            if(selectedDeck == null)
            {
                selectedDeck = _dataManager.CachedDecksData.Decks[0];
            }

            UpdateSelectedDeckData(selectedDeck);

            _deckList = _dataManager.CachedDecksData.Decks;            
        }       

        private void UpdateSelectedDeckData(Deck deck)
        {
            _dataManager.CachedUserLocalData.LastSelectedDeckId = (int)deck.Id;
            _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
            _selectDeckIndex = _dataManager.CachedDecksData.Decks.IndexOf(deck);            
        }

        private void UpdateSelectedDeckData(int deckId)
        {
            UpdateSelectedDeckData
            (
                _dataManager.CachedDecksData.Decks.Find(x => x.Id == deckId)
            );            
        }
        
        public Deck GetSelectedDeck()
        {
            return _dataManager.CachedDecksData.Decks.Find(x => x.Id == _dataManager.CachedUserLocalData.LastSelectedDeckId);            
        }
        
        public List<Deck> GetDeckList()
        {
            return _dataManager.CachedDecksData.Decks;
        }

        private OverlordModel GetOverlordDataFromDeck(Deck deck)
        {
            int overlordId = deck.OverlordId;
            OverlordModel overlord = _dataManager.CachedOverlordData.Overlords[overlordId];
            return overlord;
        }
        
        private void SetSelectedDeckIndex(int newIndex)
        {
            Deck selectedDeck = _deckList[newIndex];

            UpdateSelectedDeckData(selectedDeck);
            UpdateSelectedDeckDisplay(selectedDeck);
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
            
            int nextIndex = _selectDeckIndex + direction;
            if(nextIndex >= _deckList.Count)
            {
                nextIndex = 0;
            }else if(nextIndex < 0)
            {
                nextIndex = _deckList.Count - 1;
            }

            SetSelectedDeckIndex(nextIndex);
        }

        #endregion

        #region Deck Display

        private void LoadDeckObjects()
        {
            DisposeCreatedObject();
            _createdDeckIconList = new List<GameObject>();
            List<Vector3> positionList = GetIconPositionList(_dataManager.CachedDecksData.Decks.Count);
            
            for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count; i++)
            {
                GameObject deckIcon = Object.Instantiate(_deckIconPrefab);
                deckIcon.transform.SetParent(_deckIconGroup);
                deckIcon.transform.localPosition = positionList[i];
                deckIcon.transform.localScale = Vector3.one * _deckIconScaleNormal;

                Deck deck = _dataManager.CachedDecksData.Decks[i];
                deckIcon.GetComponent<Image>().sprite = GetDeckIconSprite
                ( 
                    GetOverlordDataFromDeck(deck).Faction
                );
                
                _createdDeckIconList.Add(deckIcon);

                int index = i;
                Button button = deckIcon.GetComponent<Button>();
                button.onClick.AddListener
                (()=>
                {
                    SetSelectedDeckIndex(index);
                });
            }
        }
        
        private List<Vector3> GetIconPositionList(int amount)
        {
            List<Vector3> positionList = new List<Vector3>();
            for (int i = 0; i < amount; ++i)
            {
                Vector3 position = new Vector3(0f, 0f, 0f);
                position.x += (i * 184f);
                positionList.Add(position);
            }
            return positionList;
        }

        private void UpdateSelectedDeckDisplay(Deck selectedDeck)
        {
            _textDeckName.text = selectedDeck.Name;
            OverlordModel selectedOverlord = GetOverlordDataFromDeck(selectedDeck);
            _uiManager.GetPage<MainMenuWithNavigationPage>().SetOverlordPortrait(selectedOverlord.Faction);

            for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count && i < _createdDeckIconList.Count; i++)
            {
                Deck deck = _dataManager.CachedDecksData.Decks[i];
                if(deck == selectedDeck)
                {
                    _createdDeckIconList[i].transform.localScale = Vector3.one * _deckIconScaleSelected;
                    _deckIconGroup.localPosition = -Vector3.right * (i * 184f);
                }
                else
                {
                    _createdDeckIconList[i].transform.localScale = Vector3.one * _deckIconScaleNormal;
                }
            }            
        }

        public Sprite GetDeckIconSprite(Enumerators.Faction faction)
        {
            switch(faction)
            {
                case Enumerators.Faction.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MainMenu/DeckIcons/icon_air");
                case Enumerators.Faction.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MainMenu/DeckIcons/icon_earth");
                case Enumerators.Faction.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MainMenu/DeckIcons/icon_fire");
                case Enumerators.Faction.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MainMenu/DeckIcons/icon_water");
                case Enumerators.Faction.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MainMenu/DeckIcons/icon_toxic");
                case Enumerators.Faction.LIFE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MainMenu/DeckIcons/icon_life");                 
                default:
                    return null;
            }
        }
        
        private void DisposeCreatedObject()
        {
            if(_createdDeckIconList != null)
            {
                foreach( GameObject icon in _createdDeckIconList)
                {
                    Object.Destroy(icon);
                }
                _createdDeckIconList.Clear();
                _createdDeckIconList = null;
            }
        }

        #endregion

        #region Buttons Handlers

        private void ButtonRightHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonRight.name))
                return;

            SwitchSelectedDeckIndex(1);
        }
        
        private void ButtonLeftHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonLeft.name))
                return;

            SwitchSelectedDeckIndex(-1);
        }

        #endregion
    }
}
