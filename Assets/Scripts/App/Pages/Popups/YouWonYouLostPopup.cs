using System;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.Experimental.PlayerLoop;

namespace Loom.ZombieBattleground
{
    public class YouWonYouLostPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;

        private ISoundManager _soundManager;
        
        private ITutorialManager _tutorialManager;

        private IMatchManager _matchManager;
        
        private IDataManager _dataManager;

        private Button _buttonRematch,
                       _buttonContinue;

        private GameObject _groupYouWin,
                           _groupYouLost;

        private bool _isWin;
        
        #region IUIPopup
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _matchManager = GameClient.Get<IMatchManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _isWin = true;
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
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YouWonYouLostPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);
            
            _groupYouWin = Self.transform.Find("Scaler/Group_YouWin").GetComponent<GameObject>(); 
            _groupYouLost = Self.transform.Find("Scaler/Group_YouLost").GetComponent<GameObject>(); 
            
            _buttonRematch = Self.transform.Find("Scaler/Button_Rematch").GetComponent<Button>();                        
            _buttonRematch.onClick.AddListener(ButtonRematchHandler);
            _buttonContinue = Self.transform.Find("Scaler/Button_Continue").GetComponent<Button>();
            _buttonContinue.onClick.AddListener(ButtonContinueHandler);

            _groupYouWin.SetActive(_isWin);
            _groupYouLost.SetActive(!_isWin);
        }
        
        public void Show(object data)
        {
            bool isWin = (bool)data;
            _isWin = isWin;
            Show();
        }

        public void Update()
        {
        }

        #endregion

        #region Buttons Handlers

        private void ButtonRematchHandler()
        {
        }
        
        private void ButtonContinueHandler()
        {
            if(_isWin)
            {
                ContinueOnWin();
            }
            else
            {
                ContinueOnLost();
            }
        }

        #endregion
        
        private void ContinueOnWin()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _uiManager.HidePopup<YouWonYouLostPopup>();

            if (_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.YouWonPopupClosed);

                _uiManager.GetPopup<TutorialProgressInfoPopup>().PopupHiding += () =>
                {
                    _matchManager.FinishMatch(Enumerators.AppState.MAIN_MENU);
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.TutorialProgressInfoPopupClosed);
                    GameClient.Get<ITutorialManager>().StopTutorial();
                    if (_tutorialManager.CurrentTutorial.Id == Constants.LastTutorialId && !_dataManager.CachedUserLocalData.TutorialRewardClaimed)
                    {
                        GameClient.Get<TutorialRewardManager>().CallRewardTutorialFlow();
                    } 
                };
                _uiManager.DrawPopup<TutorialProgressInfoPopup>();
            }
            else
            {
                _matchManager.FinishMatch(Enumerators.AppState.HordeSelection);
            }
        }
        
        private void ContinueOnLost()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            if(_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.YouLosePopupClosed);
            }

            GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.HordeSelection);

            _uiManager.HidePopup<YouWonYouLostPopup>();
        }
    }
}