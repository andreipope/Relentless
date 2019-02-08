using System;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Loom.ZombieBattleground.BackendCommunication;

namespace Loom.ZombieBattleground
{
    public class MainMenuWithNavigationPage : IUIElement
    {
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;
        
        private ISoundManager _soundManager;
        
        private IPlayerManager _playerManager;
        
        private IDataManager _dataManager;
        
        private GameObject _selfPage;

        private Button _buttonPlay;

        private Image _imageOverlordPortrait;
        
        private bool _isReturnToTutorial;
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _dataManager = GameClient.Get<IDataManager>();            
        }
        
        public void Update()
        {            
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MainMenuWithNavigationPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);            
            
            _buttonPlay = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_BattleSwitch/Button_Battle").GetComponent<Button>();                        
            _buttonPlay.onClick.AddListener(OnClickPlay);

            _imageOverlordPortrait = _selfPage.transform.Find("Image_OverlordPortrait").GetComponent<Image>();
            
            _isReturnToTutorial = GameClient.Get<ITutorialManager>().UnfinishedTutorial;
            
            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.BATTLE);
            _uiManager.DrawPopup<AreaBarPopup>();

            SetOverlordPortrait(Enumerators.SetType.AIR);
        }
        
        public void Hide()
        {
            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;

            OnHide();
        }

        public void Dispose()
        {
        }

        private void PressedLoginHandler() 
        {
            //todo add ReportActivityAction
            //if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonBuy.name) || _isReturnToTutorial)
            //{
            //    GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
            //    return;
            //}

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            LoginPopup popup = _uiManager.GetPopup<LoginPopup>();
            popup.Show();
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }
        
        private void OnHide()
        {
            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }

        #region Buttons Handlers

        private void OnClickPlay()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonPlay.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }
            else if (_isReturnToTutorial)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.BattleStarted);

                GameClient.Get<IMatchManager>().FindMatch();
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _stateManager.ChangeAppState(Enumerators.AppState.PlaySelection);
        }

        #endregion
        
        private void SetOverlordPortrait(Enumerators.SetType setType)
        {
            switch(setType)
            {
                case Enumerators.SetType.AIR:
                    _imageOverlordPortrait.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/OverlordPortrait/main_portrait_air");                  
                    break;
                case Enumerators.SetType.FIRE:
                    _imageOverlordPortrait.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/OverlordPortrait/main_portrait_fire");
                    break;
                case Enumerators.SetType.EARTH:
                    _imageOverlordPortrait.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/OverlordPortrait/main_portrait_earth");
                    break;
                case Enumerators.SetType.TOXIC:
                    _imageOverlordPortrait.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/OverlordPortrait/main_portrait_toxic");
                    break;
                case Enumerators.SetType.WATER:
                    _imageOverlordPortrait.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/OverlordPortrait/main_portrait_water");
                    break;
                case Enumerators.SetType.LIFE:
                    _imageOverlordPortrait.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/OverlordPortrait/main_portrait_life");
                    break;
                default:
                    Debug.Log($"No OverlordPortrait found for setType {setType}");
                    return;
            }            
        }

    }
}
