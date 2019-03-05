using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class SideMenuPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;
        
        private IAppStateManager _stateManager;
        
        private ISoundManager _soundManager;
        
        private Button _buttonBattle, 
                       _buttonShop, 
                       _buttonMyDecks, 
                       _buttonMyPacks, 
                       _buttonMyCards;

        private List<Sprite> _selectedSpriteList;
        
        public enum MENU
        {
            NONE = -1,
            BATTLE = 0,
            SHOP = 1,
            MY_DECKS = 2,
            MY_PACKS = 3,
            MY_CARDS = 4            
        }

        private MENU _currentMenu = MENU.NONE;
        
        #region IUIPopup

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            
            _selectedSpriteList = new List<Sprite>();
            _selectedSpriteList.Add(_loadObjectsManager.GetObjectByPath<Sprite>
            (
                "Images/UI/MainMenu/Sidebar/sidebar_battle_selected"
            ));
            _selectedSpriteList.Add(_loadObjectsManager.GetObjectByPath<Sprite>
            (
                "Images/UI/MainMenu/Sidebar/sidebar_shop_selected"
            ));
            _selectedSpriteList.Add(_loadObjectsManager.GetObjectByPath<Sprite>
            (
                "Images/UI/MainMenu/Sidebar/sidebar_my_decks_selected"
            ));
            _selectedSpriteList.Add(_loadObjectsManager.GetObjectByPath<Sprite>
            (
                "Images/UI/MainMenu/Sidebar/sidebar_my_packs_selected"
            ));
            _selectedSpriteList.Add(_loadObjectsManager.GetObjectByPath<Sprite>
            (
                "Images/UI/MainMenu/Sidebar/sidebar_my_cards_selected"
            ));
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

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
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/SideMenuPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _buttonBattle = Self.transform.Find("Group/Button_Battle").GetComponent<Button>();
            _buttonShop = Self.transform.Find("Group/Button_Shop").GetComponent<Button>();           
            _buttonMyDecks = Self.transform.Find("Group/Button_MyDecks").GetComponent<Button>();
            _buttonMyPacks = Self.transform.Find("Group/Button_MyPacks").GetComponent<Button>();
            _buttonMyCards = Self.transform.Find("Group/Button_MyCards").GetComponent<Button>();
            
            _buttonBattle.onClick.AddListener(ButtonBattleHandler);
            _buttonShop.onClick.AddListener(ButtonShopHandler);
            _buttonMyDecks.onClick.AddListener(ButtonMyDecksHandler);
            _buttonMyPacks.onClick.AddListener(ButtonMyPacksHander);
            _buttonMyCards.onClick.AddListener(ButtonMyCardsHandler);

            UpdateButtonSprite();
        }

        public void Show(object data)
        {
            _currentMenu = (MENU)data;
            Show();
        }

        public void Update()
        {
        }

        #endregion
        
        private void UpdateButtonSprite()
        {
            switch(_currentMenu)
            {
                case MENU.BATTLE:
                    _buttonBattle.GetComponent<Image>().sprite = _selectedSpriteList[(int)_currentMenu];
                    break;
                case MENU.SHOP:
                    _buttonShop.GetComponent<Image>().sprite = _selectedSpriteList[(int)_currentMenu];
                    break;
                case MENU.MY_DECKS:
                    _buttonMyDecks.GetComponent<Image>().sprite = _selectedSpriteList[(int)_currentMenu];
                    break;
                case MENU.MY_PACKS:
                    _buttonMyPacks.GetComponent<Image>().sprite = _selectedSpriteList[(int)_currentMenu];
                    break;
                case MENU.MY_CARDS:
                    _buttonMyCards.GetComponent<Image>().sprite = _selectedSpriteList[(int)_currentMenu];
                    break;
            }
        }

        #region Buttons Handlers

        private void ButtonBattleHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            if(_stateManager.AppState != Enumerators.AppState.MAIN_MENU)
                _stateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }
        
        private void ButtonShopHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.SHOP);
        }
        
        private void ButtonMyDecksHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.HordeSelection);
        }
        
        private void ButtonMyPacksHander()
        { 
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.PACK_OPENER);
        }
        
        private void ButtonMyCardsHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.ARMY);
        }
        
        #endregion
    }
}