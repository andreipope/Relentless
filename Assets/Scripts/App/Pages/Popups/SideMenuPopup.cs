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

        private List<Transform> _locatorList;
        
        private Button _buttonBattle, _buttonShop, _buttonMyDecks, _buttonMyPacks, _buttonMyCards;

        private List<Sprite> _selectedSpriteList;
        
        private List<Sprite> _normalSpriteList;
        
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

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
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

            _locatorList = new List<Transform>();
            _locatorList.Add
            (
                Self.transform.Find("Locator/Button_Battle")
            );
            _locatorList.Add
            (
                Self.transform.Find("Locator/Button_Shop")
            );
            _locatorList.Add
            (
                Self.transform.Find("Locator/Button_MyDecks")
            );
            _locatorList.Add
            (
                Self.transform.Find("Locator/Button_MyPacks")
            );
            _locatorList.Add
            (
                Self.transform.Find("Locator/Button_MyCards")
            );
            _selectedSpriteList = new List<Sprite>();
            for(int i=0; i<_locatorList.Count;++i)
            {
                _selectedSpriteList.Add
                (
                    _locatorList[i].GetComponent<Image>().sprite
                );
            }
            Self.transform.Find("Locator").gameObject.SetActive(false);

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

            List<Transform> buttonTransformList = new List<Transform>();
            buttonTransformList.Add(_buttonBattle.transform);
            buttonTransformList.Add(_buttonShop.transform);
            buttonTransformList.Add(_buttonMyDecks.transform);
            buttonTransformList.Add(_buttonMyPacks.transform);
            buttonTransformList.Add(_buttonMyCards.transform);
            _normalSpriteList = new List<Sprite>();
            for(int i=0; i<buttonTransformList.Count;++i)
            {
                _normalSpriteList.Add
                (
                    buttonTransformList[i].GetComponent<Image>().sprite
                );
            }
            
            for(int i=0;i<_locatorList.Count;++i)
            {
                Transform locator = _locatorList[i];
                Transform button = buttonTransformList[i];
                button.position = locator.position - Vector3.right * 3.5f;
                
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval( 0.1f * i );
                sequence.Append(button.DOMove(locator.position, .5f).SetEase(Ease.OutQuad));
            }
            
            if(_currentMenu != MENU.NONE)
            {
                buttonTransformList[(int)_currentMenu].GetComponent<Image>().sprite = _selectedSpriteList[(int)_currentMenu];
            }
        }

        public void Show(object data)
        {
            _currentMenu = (MENU)data;
            Show();
        }

        public void Update()
        {
        }
        
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
    }
}