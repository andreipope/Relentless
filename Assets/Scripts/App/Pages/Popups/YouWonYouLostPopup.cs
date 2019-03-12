using System;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
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

        private BackendDataControlMediator _backendDataControlMediator;

        private IMatchManager _matchManager;
        
        private IDataManager _dataManager;

        private Button _buttonPlayAgain,
                       _buttonContinue;

        private GameObject _groupYouWin,
                           _groupYouLost;

        private Image _imageOverlordPortrait;

        private TextMeshProUGUI _textDeckName,
                                _textPlayerName,
                                _textLevel;

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
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
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
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YouWonYouLostPopupEdited"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);
            
            _groupYouWin = Self.transform.Find("Scaler/Image_Panel_Win").gameObject; 
            _groupYouLost = Self.transform.Find("Scaler/Image_Panel_Lose").gameObject;
            
            _buttonPlayAgain = Self.transform.Find("Scaler/Button_PlayAgain").GetComponent<Button>();                        
            _buttonPlayAgain.onClick.AddListener(ButtonPlayAgainHandler);
            _buttonPlayAgain.onClick.AddListener(PlayClickSound);
            
            _buttonContinue = Self.transform.Find("Scaler/Button_Continue").GetComponent<Button>();
            _buttonContinue.onClick.AddListener(ButtonContinueHandler);
            _buttonContinue.onClick.AddListener(PlayClickSound);
            
            _groupYouWin.SetActive(_isWin);
            _groupYouLost.SetActive(!_isWin);

            Deck deck = _uiManager.GetPopup<DeckSelectionPopup>().GetSelectedDeck();
            
            Hero hero = _dataManager.CachedHeroesData.Heroes[deck.HeroId];
            
            _imageOverlordPortrait = Self.transform.Find("Scaler/Image_OverlordPortrait").GetComponent<Image>();
            _imageOverlordPortrait.sprite = GetOverlordPortraitSprite
            (
                hero.HeroElement
            );
            
            _textDeckName = Self.transform.Find("Scaler/Text_DeckName").GetComponent<TextMeshProUGUI>();
            _textPlayerName = Self.transform.Find("Scaler/Group_PlayerInfo/Text_PlayerName").GetComponent<TextMeshProUGUI>();
            _textLevel = Self.transform.Find("Scaler/Group_PlayerInfo/Image_Circle/Text_LevelNumber").GetComponent<TextMeshProUGUI>();

            _textPlayerName.text = _backendDataControlMediator.UserDataModel.UserId;
            _textDeckName.text = deck.Name;
            _textLevel.text = "1";
        }
        
        public void Show(object data)
        {
            if (data is object[] param)
            {
                _isWin = (bool)param[0];
            }
            Show();
        }

        public void Update()
        {
        }

        #endregion

        #region Buttons Handlers

        private void ButtonPlayAgainHandler()
        {
            //TODO Play Again
            if (_isWin)
            {
                ContinueOnWin();
            }
            else
            {
                ContinueOnLost();
            }
        }
        
        private void ButtonContinueHandler()
        {
            if (_isWin)
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
            _uiManager.HidePopup<YouWonYouLostPopup>();

            if (_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.YouWonPopupClosed);

                _uiManager.GetPopup<TutorialProgressInfoPopup>().PopupHiding += async () =>
                {
                    _matchManager.FinishMatch(Enumerators.AppState.MAIN_MENU);
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.TutorialProgressInfoPopupClosed);
                    GameClient.Get<ITutorialManager>().StopTutorial();
                    if (_tutorialManager.CurrentTutorial.Id == Constants.LastTutorialId && !_dataManager.CachedUserLocalData.TutorialRewardClaimed)
                    {
                        await GameClient.Get<TutorialRewardManager>().CallRewardTutorialFlow();
                    } 
                };
                _uiManager.DrawPopup<TutorialProgressInfoPopup>();
            }
            else
            {
                _matchManager.FinishMatch(Enumerators.AppState.MAIN_MENU);
            }
        }
        
        private void ContinueOnLost()
        {
            if(_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.YouLosePopupClosed);
            }

            GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU);

            _uiManager.HidePopup<YouWonYouLostPopup>();
        }
        
        public Sprite GetOverlordPortraitSprite(Enumerators.SetType heroElement)
        {
            string path = "Images/UI/WinLose/OverlordPortrait/results_overlord_"+heroElement.ToString().ToLower();
            return _loadObjectsManager.GetObjectByPath<Sprite>(path);       
        }
        
        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
    }
}
