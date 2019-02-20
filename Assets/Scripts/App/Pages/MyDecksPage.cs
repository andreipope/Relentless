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
        
        private GameObject _selfPage;

        private Transform _trayUpper;

        private Button _buttonNewDeck,
                       _buttonBack;

        private List<DeckInfoObject> _deckInfoObjectList;

        private const int _numberOfDeckInfo = 3;

        private const int _maxDeckCard = 30;
        
        #region IUIElement
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _deckInfoObjectList = new List<DeckInfoObject>();
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

            _trayUpper = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Frame/Image_ButtonTray");
            _trayUpper.gameObject.SetActive(false);

            _buttonNewDeck = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectDeck/Panel_Content/Button_BuildNewDeck").GetComponent<Button>();
            _buttonNewDeck.onClick.AddListener(ButtonNewDeckHandler);
            
            _buttonBack = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_Frame/Image_ButtonTray/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);

            InitObjects();
            UpdateDeckInfoObjects();  
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
        }

        #endregion

        #region Buttons Handlers

        private void ButtonNewDeckHandler()
        {
            _uiManager.SetPage<HordeSelectionPage>();
        }
        
        private void ButtonBackHandler()
        {
        
        }

        #endregion
        
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
        }
        
        private void UpdateDeckInfoObjects()
        {
            List<Deck> deckList = _dataManager.CachedDecksData.Decks;
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
                deckInfoObject._textCardsAmount.text = $"{cardsAmount}/{_maxDeckCard}";
                deckInfoObject._imageOverlordThumbnail.sprite = GetOverlordThumbnailSprite(heroElement);
                deckInfoObject._button.onClick.RemoveAllListeners();
                deckInfoObject._button.onClick.AddListener(() =>
                {
                    //TODO
                });
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
    }
}
