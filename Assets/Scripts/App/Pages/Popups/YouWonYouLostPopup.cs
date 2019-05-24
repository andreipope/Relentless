using System;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using System.Collections;
using DG.Tweening;
using log4net;

namespace Loom.ZombieBattleground
{
    public class YouWonYouLostPopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(YouWonYouLostPopup));

        private const float ExperienceFillInterval = 1;
        private readonly WaitForSeconds _experienceFillWait = new WaitForSeconds(ExperienceFillInterval);

        public GameObject Self { get; private set; }

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;

        private ISoundManager _soundManager;
        
        private ITutorialManager _tutorialManager;

        private BackendDataControlMediator _backendDataControlMediator;

        private IMatchManager _matchManager;
        
        private IDataManager _dataManager;

        private IOverlordExperienceManager _overlordExperienceManager;

        private IAppStateManager _appStateManager;

        private INetworkActionManager _networkActionManager;

        private BackendFacade _backendFacade;

        private Button _buttonPlayAgain,
                       _buttonContinue;

        private GameObject _groupYouWin,
                           _groupYouLost;

        private Image _imageOverlordPortrait,
                      _imageExperienceBar,
                      _imageLock;

        private TextMeshProUGUI _textDeckName,
                                _textPlayerName,
                                _textLevel;

        private OverlordModel _currentPlayerOverlord;

        private Coroutine _fillExperienceBarCoroutine;

        private bool _isWin;

        private EndMatchResults _endMatchResults;

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
            _appStateManager = GameClient.Get<IAppStateManager>();
            _overlordExperienceManager = GameClient.Get<IOverlordExperienceManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _networkActionManager = GameClient.Get<INetworkActionManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _isWin = true;
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

            if (_fillExperienceBarCoroutine != null)
            {
                MainApp.Instance.StopCoroutine(_fillExperienceBarCoroutine);
            }

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public async void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YouWonYouLostPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);
            
            _groupYouWin = Self.transform.Find("Scaler/Image_Panel_Win").gameObject; 
            _groupYouLost = Self.transform.Find("Scaler/Image_Panel_Lose").gameObject;
            
            _buttonPlayAgain = Self.transform.Find("Scaler/Group_Buttons/Button_PlayAgain").GetComponent<Button>();                        
            _buttonPlayAgain.onClick.AddListener(ButtonPlayAgainHandler);
            
            _buttonContinue = Self.transform.Find("Scaler/Group_Buttons/Button_Continue").GetComponent<Button>();
            _buttonContinue.onClick.AddListener(ButtonContinueHandler);
            
            _groupYouWin.SetActive(_isWin);
            _groupYouLost.SetActive(!_isWin);

            Enumerators.SoundType soundType = _isWin ? Enumerators.SoundType.WON_POPUP : Enumerators.SoundType.LOST_POPUP;
            _soundManager.PlaySound(soundType, Constants.SfxSoundVolume, false, false, true);  


            _imageExperienceBar = Self.transform.Find("Scaler/Group_PlayerInfo/Image_Bar").GetComponent<Image>();

            _imageLock = Self.transform.Find("Scaler/Group_PlayerInfo/Image_Bar/Image_Lock").GetComponent<Image>();

            _imageOverlordPortrait = Self.transform.Find("Scaler/Image_OverlordPortrait").GetComponent<Image>();


            _textDeckName = Self.transform.Find("Scaler/Text_DeckName").GetComponent<TextMeshProUGUI>();
            _textPlayerName = Self.transform.Find("Scaler/Group_PlayerInfo/Text_PlayerName").GetComponent<TextMeshProUGUI>();
            _textLevel = Self.transform.Find("Scaler/Group_PlayerInfo/Image_Circle/Text_LevelNumber").GetComponent<TextMeshProUGUI>();

            _textPlayerName.text = _backendDataControlMediator.UserDataModel.UserId;

            _imageLock.gameObject.SetActive(_tutorialManager.IsTutorial);
            _buttonContinue.gameObject.SetActive(_tutorialManager.IsTutorial);
            _buttonPlayAgain.gameObject.SetActive(false);


            Deck deck;
            if (_tutorialManager.IsTutorial)
            {
                // For tutorial, the popup is deactivated, so no point in fetching data from server
                deck = _uiManager.GetPopup<DeckSelectionPopup>().GetSelectedDeck();
                _currentPlayerOverlord = _dataManager.CachedOverlordData.GetOverlordById(deck.OverlordId);

                _imageExperienceBar.fillAmount = 0;
                _textLevel.text = _currentPlayerOverlord.Level.ToString();

                _endMatchResults = new EndMatchResults(
                    deck.Id,
                    _currentPlayerOverlord.Id,
                    _currentPlayerOverlord.Level,
                    _currentPlayerOverlord.Experience,
                    _currentPlayerOverlord.Level,
                    _currentPlayerOverlord.Experience,
                    _isWin,
                    Array.Empty<LevelReward>()
                );
            }
            else
            {
                // Hide UI elements while fetching data
                _textLevel.text = "";
                _textDeckName.text = "";
                _imageOverlordPortrait.enabled = false;

                try
                {
                    (int? notificationId, EndMatchResults endMatchResults) =
                        await _overlordExperienceManager.GetEndMatchResultsFromEndMatchNotification();

                    _endMatchResults = endMatchResults;
                    if (_endMatchResults != null)
                    {
                        _currentPlayerOverlord = _dataManager.CachedOverlordData.GetOverlordById(_endMatchResults.OverlordId);
                        _currentPlayerOverlord.Level = _endMatchResults.CurrentLevel;
                        _currentPlayerOverlord.Experience = _endMatchResults.CurrentExperience;
                        deck = _dataManager.CachedDecksData.Decks.Find(x => x.Id == _endMatchResults.DeckId);

                        await _networkActionManager.EnqueueNetworkTask(
                            async () =>
                                await _backendFacade.ClearNotifications(
                                    _backendDataControlMediator.UserDataModel.UserId,
                                    new[]
                                    {
                                        notificationId.Value
                                    })
                        );
                    }
                    else
                    {
                        // We have no info to show, just close in shame silently
                        Log.Warn("No EndMatchNotification present on server, closing");
                        ButtonContinueHandler();
                        return;
                    }
                }
                catch
                {
                    ButtonContinueHandler();
                    return;
                }

                _fillExperienceBarCoroutine = MainApp.Instance.StartCoroutine(FillExperienceBar());
                _imageOverlordPortrait.enabled = true;
            }

            _imageOverlordPortrait.sprite = GetOverlordPortraitSprite
            (
                _currentPlayerOverlord.Faction
            );

            _textDeckName.text = deck?.Name ?? "";
        }

        public void Show(object data)
        {
            if (data is object[] param)
            {
                _isWin = (bool)param[0];
            }
            else
            {
                throw new ArgumentException(nameof(data));
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
            PlayClickSound();

            if (!_tutorialManager.IsTutorial && _appStateManager.AppState == Enumerators.AppState.GAMEPLAY)
            {
                MatchManager matchManager = (MatchManager)GameClient.Get<IMatchManager>();
                matchManager.AppStateWasLoaded += PlayAgainWhenAppStateLoaded;
            }
            
            if (_isWin)
            {
                ContinueOnWin();
            }
            else
            {
                ContinueOnLost();
            }
        }
        
        private void PlayAgainWhenAppStateLoaded()
        {
            MatchManager matchManager = (MatchManager)GameClient.Get<IMatchManager>();
            matchManager.AppStateWasLoaded -= PlayAgainWhenAppStateLoaded;
            _uiManager.GetPage<MainMenuWithNavigationPage>().StartMatch();
        }
        
        private void ButtonContinueHandler()
        {
            PlayClickSound();
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
            _soundManager.StopPlaying(Enumerators.SoundType.WON_POPUP);

            if (_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.YouWonPopupClosed);

                _uiManager.GetPopup<TutorialProgressInfoPopup>().PopupHiding += async () =>
                {
                    if (_appStateManager.AppState == Enumerators.AppState.GAMEPLAY)
                    {
                        _matchManager.FinishMatch(Enumerators.AppState.MAIN_MENU);
                    }
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.TutorialProgressInfoPopupClosed);
                    GameClient.Get<ITutorialManager>().StopTutorial();
                    //if (_tutorialManager.CurrentTutorial.Id == Constants.LastTutorialId && !_dataManager.CachedUserLocalData.TutorialRewardClaimed)
                    //{
                    //    await GameClient.Get<TutorialRewardManager>().CallRewardTutorialFlow();
                    //} 
                };
                _uiManager.DrawPopup<TutorialProgressInfoPopup>();
            }
            else
            {
                if (_appStateManager.AppState == Enumerators.AppState.GAMEPLAY)
                {
                    _matchManager.FinishMatch(Enumerators.AppState.MAIN_MENU);
                }
            }
        }
        
        private void ContinueOnLost()
        {
            if(_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.YouLosePopupClosed);
            }

            if (_appStateManager.AppState == Enumerators.AppState.GAMEPLAY)
            {
                _matchManager.FinishMatch(Enumerators.AppState.MAIN_MENU);
            }

            _uiManager.HidePopup<YouWonYouLostPopup>();
            _soundManager.StopPlaying(Enumerators.SoundType.LOST_POPUP);
        }
        
        public Sprite GetOverlordPortraitSprite(Enumerators.Faction overlordFaction)
        {
            string path = "Images/UI/WinLose/OverlordPortrait/results_overlord_"+overlordFaction.ToString().ToLower();
            return _loadObjectsManager.GetObjectByPath<Sprite>(path);       
        }
        
        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        private IEnumerator FillExperienceBar()
        {
            float CalculateProgressRation(long experience, int previousLevel, int currentLevel)
            {
                long experienceForPreviousLevel = _overlordExperienceManager.GetRequiredExperienceForLevel(previousLevel);
                long experienceForCurrentLevel = _overlordExperienceManager.GetRequiredExperienceForLevel(currentLevel);

                // When we have the exact experience for next level
                if (experience == experienceForCurrentLevel)
                    return 0;

                experience -= experienceForPreviousLevel;
                experienceForCurrentLevel -= experienceForPreviousLevel;

                return experience / (float) experienceForCurrentLevel;
            }

            float currentProgressRatio = CalculateProgressRation(
                _endMatchResults.PreviousExperience,
                _endMatchResults.PreviousLevel,
                _endMatchResults.PreviousLevel + 1
                );

            float targetProgressRatio = CalculateProgressRation(
                _endMatchResults.CurrentExperience,
                _endMatchResults.CurrentLevel,
                _endMatchResults.CurrentLevel + 1
            );

            _textLevel.text = _endMatchResults.PreviousLevel.ToString();
            _imageExperienceBar.fillAmount = currentProgressRatio;
            for (int level = _endMatchResults.PreviousLevel; level < _endMatchResults.CurrentLevel; level++)
            {
                _imageExperienceBar.DOFillAmount(1, ExperienceFillInterval);
                yield return _experienceFillWait;
                _imageExperienceBar.fillAmount = 0f;
                _textLevel.text = (level + 1).ToString();
            }

            _imageExperienceBar.DOFillAmount(targetProgressRatio, ExperienceFillInterval);

            yield return _experienceFillWait;
            _buttonContinue.gameObject.SetActive(true);
            _buttonPlayAgain.gameObject.SetActive(true);

            if (_endMatchResults.CurrentLevel > _endMatchResults.PreviousLevel)
            {
                _uiManager.DrawPopup<LevelUpPopup>(_endMatchResults);
            }
        }
    }
}
