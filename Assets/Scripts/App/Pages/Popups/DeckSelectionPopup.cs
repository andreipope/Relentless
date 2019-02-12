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

        private Image _imageDeckIconGrow;

        private Button _buttonLeft, _buttonRight;

        private GameObject _deckIconPrefab;

        private List<GameObject> _createdDeckIconList;

        private Transform _deckIconGroup;
        
        private Deck _selectedDeck;
        
        private int _defaultSelectedDeck = 1;

        private const float _deckIconScaleNormal = 0.7178f;
        
        private const float _deckIconScaleSelected = 1f;
        
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
            
            _textDeckName = Self.transform.Find("Scaler/Text_DeckName").GetComponent<TextMeshProUGUI>();
            _imageDeckIconGrow = Self.transform.Find("Scaler/Panel_DeckContent/Image_DeckIcon_Glow").GetComponent<Image>();
            _deckIconGroup = Self.transform.Find("Scaler/Panel_DeckContent/Group");
            
            _buttonRight = Self.transform.Find("Scaler/Button_Right").GetComponent<Button>();
            _buttonLeft = Self.transform.Find("Scaler/Button_Left").GetComponent<Button>();
            _buttonRight.onClick.AddListener(ButtonRightHandler);
            _buttonLeft.onClick.AddListener(ButtonLeftHandler);

            LoadDeckData();
            if (_selectedDeck != null)
            {
                UpdateSelectedDeckDisplay();
            }
        }
        
        public void Show(object data)
        {
            Show();
        }
        
        public void Update()
        {
        }
        
        private void LoadDeckData()
        {
            _defaultSelectedDeck = _dataManager.CachedUserLocalData.LastSelectedDeckId;
            
            if (_defaultSelectedDeck > _dataManager.CachedDecksData.Decks.Count)
            {
                _defaultSelectedDeck = 1;
            }
            if (_dataManager.CachedDecksData.Decks.Count > 1)
            {
                _defaultSelectedDeck = Mathf.Clamp(_defaultSelectedDeck, 1, _defaultSelectedDeck);
            }

            _selectedDeck = _dataManager.CachedDecksData.Decks.Find(x => x.Id == _defaultSelectedDeck);

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

        private Hero GetHeroDataFromDeck(Deck deck)
        {
            int heroId = deck.HeroId;
            Hero hero = _dataManager.CachedHeroesData.Heroes[heroId];
            return hero;
        }
        
        private void UpdateSelectedDeckDisplay()
        {
            _textDeckName.text = _selectedDeck.Name;
            Hero selectedHero = GetHeroDataFromDeck(_selectedDeck);
            _uiManager.GetPage<MainMenuWithNavigationPage>().SetOverlordPortrait(selectedHero.HeroElement);

            for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count && i<_createdDeckIconList.Count; i++)
            {
                Deck deck = _dataManager.CachedDecksData.Decks[i];
                if(deck == _selectedDeck)
                {
                    _createdDeckIconList[i].transform.localScale = Vector3.one * _deckIconScaleSelected;
                    //_imageDeckIconGrow.GetComponent<RectTransform>().localPosition = _createdDeckIconList[i].GetComponent<RectTransform>().localPosition;                   
                }
                else
                {
                    _createdDeckIconList[i].transform.localScale = Vector3.one * _deckIconScaleNormal;
                }
            }            
        }

        private Sprite GetDeckIconSprite(Enumerators.SetType setType)
        {
            switch(setType)
            {
                case Enumerators.SetType.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/DeckIcons/icon_air");
                case Enumerators.SetType.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/DeckIcons/icon_earth");
                case Enumerators.SetType.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/DeckIcons/icon_fire");
                case Enumerators.SetType.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/DeckIcons/icon_water");
                case Enumerators.SetType.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/DeckIcons/icon_toxic");
                case Enumerators.SetType.LIFE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/DeckIcons/icon_life");                 
                default:
                    return null;
            }
        }
        
        private void ButtonRightHandler()
        {

        }
        
        private void ButtonLeftHandler()
        {

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
    }
}
