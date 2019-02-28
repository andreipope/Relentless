using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class DeckSelectionPopup : IUIPopup
    {
        public GameObject Self { get; private set; }

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private ISoundManager _soundManager;

        private IDataManager _dataManager;

        private TextMeshProUGUI _textDeckName;

        private GameObject _glowBorderVFX;

        private Button _buttonLeft, _buttonRight;

        private GameObject _deckIconPrefab,
                           _glowBorderPrefab;

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
            _glowBorderPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/DeckSelection/GlowBorder");

            _textDeckName = Self.transform.Find("Scaler/Text_DeckName").GetComponent<TextMeshProUGUI>();
            _deckIconGroup = Self.transform.Find("Scaler/Panel_DeckContent/Group");

            _buttonRight = Self.transform.Find("Scaler/Button_Right").GetComponent<Button>();
            _buttonLeft = Self.transform.Find("Scaler/Button_Left").GetComponent<Button>();
            _buttonRight.onClick.AddListener(ButtonRightHandler);
            _buttonLeft.onClick.AddListener(ButtonLeftHandler);

            LoadDefaultDeckData();
            LoadDeckObjects();
            UpdateSelectedDeckDisplay
            (
                GetSelectedDeck()
            );            
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        #endregion

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

        private Hero GetHeroDataFromDeck(Deck deck)
        {
            int heroId = deck.HeroId;
            Hero hero = _dataManager.CachedHeroesData.Heroes[heroId];
            return hero;
        }
        
        private void SwitchSelectedDeckIndex(int direction)
        {  
            if (direction == 0)
                return;
                
            if (_deckList.Count <= 0)
            {
                Debug.Log("No deck in list");
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

            Deck selectedDeck = _deckList[nextIndex];

            UpdateSelectedDeckData(selectedDeck);
            UpdateSelectedDeckDisplay(selectedDeck);
        }

        #endregion

        #region Deck Display
        
        private void LoadDeckObjects()
        {
            DisposeCreatedObject();
            _createdDeckIconList = new List<GameObject>();
            for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count; i++)
            {
                GameObject deckIcon = Object.Instantiate(_deckIconPrefab);
                deckIcon.transform.SetParent(_deckIconGroup);
                deckIcon.transform.localScale = Vector3.one * _deckIconScaleNormal;

                Deck deck = _dataManager.CachedDecksData.Decks[i];
                deckIcon.GetComponent<Image>().sprite = GetDeckIconSprite
                ( 
                    GetHeroDataFromDeck(deck).HeroElement
                );
                
                _createdDeckIconList.Add(deckIcon);
            }
        }

        private void UpdateSelectedDeckDisplay(Deck selectedDeck)
        {
            _textDeckName.text = selectedDeck.Name;
            Hero selectedHero = GetHeroDataFromDeck(selectedDeck);
            _uiManager.GetPage<MainMenuWithNavigationPage>().SetOverlordPortrait(selectedHero.HeroElement);

            for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count && i<_createdDeckIconList.Count; i++)
            {
                Deck deck = _dataManager.CachedDecksData.Decks[i];
                if(deck == selectedDeck)
                {
                    _createdDeckIconList[i].transform.localScale = Vector3.one * _deckIconScaleSelected;
                    if(_glowBorderVFX == null)
                    {
                        //_glowBorderVFX = Object.Instantiate(_glowBorderPrefab);
                        _glowBorderVFX = Self.transform.Find("Scaler/Panel_DeckContent/Image_DeckIcon_Glow").gameObject;
                    }
                    Transform deckIcon = _createdDeckIconList[i].transform;
                    Sequence sequence = DOTween.Sequence();
                    sequence.AppendInterval(0.1f);
                    sequence.OnComplete(()=> 
                    {
                        Vector3 glowBorderPosition = deckIcon.position;
                        glowBorderPosition.z = 0;
                        _glowBorderVFX.transform.position = glowBorderPosition;  
                    });   
                }
                else
                {
                    _createdDeckIconList[i].transform.localScale = Vector3.one * _deckIconScaleNormal;
                }
            }            
        }

        public Sprite GetDeckIconSprite(Enumerators.SetType setType)
        {
            switch(setType)
            {
                case Enumerators.SetType.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MainMenu/DeckIcons/icon_air");
                case Enumerators.SetType.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MainMenu/DeckIcons/icon_earth");
                case Enumerators.SetType.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MainMenu/DeckIcons/icon_fire");
                case Enumerators.SetType.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MainMenu/DeckIcons/icon_water");
                case Enumerators.SetType.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MainMenu/DeckIcons/icon_toxic");
                case Enumerators.SetType.LIFE:
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
            if(_glowBorderVFX != null)
            {
                Object.Destroy(_glowBorderVFX);
            }
        }

        #endregion

        #region Buttons Handlers

        private void ButtonRightHandler()
        {
            SwitchSelectedDeckIndex(1);
        }
        
        private void ButtonLeftHandler()
        {
            SwitchSelectedDeckIndex(-1);
        }

        #endregion
    }
}
