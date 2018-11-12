using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Newtonsoft.Json;
using Object = UnityEngine.Object;
using DG.Tweening;
using Loom.ZombieBattleground.BackendCommunication;

namespace Loom.ZombieBattleground
{
    public class TutorialManager : IService, ITutorialManager
    {
        public bool Paused;

        private IUIManager _uiManager;

        private ISoundManager _soundManager;

        private ILoadObjectsManager _loadObjectsManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private IDataManager _dataManager;

        private IGameplayManager _gameplayManager;

        private BattlegroundController _battlegroundController;

        private TutorialPopup _popup;

        private TutorialBoardArrow _targettingArrow;

        private GameObject _targettingArrowPrefab;

        private Sequence _helpArrowsDelay;

        private List<TutorialBoardArrow> _tutorialHelpBoardArrows;

        private IAnalyticsManager _analyticsManager;

        public bool IsTutorial { get; private set; }

        public bool IsBubbleShow { get; set; }

        private List<TutorialData> _tutorials;
        private List<TutorialDataStep> _tutorialSteps;
        private int _currentTutorialStepIndex;

        public TutorialData CurrentTutorial { get; private set; }
        public TutorialDataStep CurrentTutorialDataStep { get; private set; }

        public int TutorialsCount
        {
            get { return _tutorials.Count; }
        }

        public void Dispose()
        {
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            // card vs player
            _targettingArrowPrefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");

            _tutorials = JsonConvert.DeserializeObject<TutorialContentData>(_loadObjectsManager
                        .GetObjectByPath<TextAsset>("Data/tutorial_data").text).TutorialDatas;

            _tutorialHelpBoardArrows = new List<TutorialBoardArrow>();
        }

        public void Update()
        {
        }

        public void SetupTutorialById(int id)
        {
            CurrentTutorial = _tutorials.Find(tutor => tutor.TutorialId == id);
            _currentTutorialStepIndex = 0;
            _tutorialSteps = CurrentTutorial.TutorialDataSteps;
            CurrentTutorialDataStep = _tutorialSteps[_currentTutorialStepIndex];

            IsTutorial = false;
        }

        public void StartTutorial()
        {
            _battlegroundController.SetupBattlegroundAsSpecific(CurrentTutorial.SpecificBattlegroundInfo);

            IsBubbleShow = true;
            _uiManager.DrawPopup<TutorialPopup>();
            _popup = _uiManager.GetPopup<TutorialPopup>();
            UpdateTutorialVisual();
            _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, 0, CurrentTutorialDataStep.SoundName, Constants.TutorialSoundVolume, false);


            IsTutorial = true;

            _analyticsManager.SetEvent( AnalyticsManager.EventStartedTutorial);
        }

        public void StopTutorial()
        {
            if (!IsTutorial)
                return;

            _uiManager.HidePopup<TutorialPopup>();

            _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);


            if (_dataManager.CachedUserLocalData.CurrentTutorialId >= _tutorials.Count - 1)
            {
                _dataManager.CachedUserLocalData.CurrentTutorialId = 0;
                _gameplayManager.IsTutorial = false;
                _dataManager.CachedUserLocalData.Tutorial = false;
                _gameplayManager.IsSpecificGameplayBattleground = false;
            }

            _dataManager.CachedUserLocalData.CurrentTutorialId++;


            IsTutorial = false;
            _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);

            _analyticsManager.SetEvent(AnalyticsManager.EventCompletedTutorial);
        }

        public void SkipTutorial(Enumerators.AppState state)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            string tutorialSkipQuestion = "Are you sure you want to go back to Main Menu?";
            QuestionPopup questionPopup = _uiManager.GetPopup<QuestionPopup>();
            if (state == Enumerators.AppState.MAIN_MENU)
            {
                questionPopup.ConfirmationReceived += ConfirmQuitReceivedHandler;
            }
            else
            {
                tutorialSkipQuestion = "Do you really want to skip \nBasic Tutorial?";
                if (_dataManager.CachedUserLocalData.CurrentTutorialId > 0)
                    tutorialSkipQuestion = "Do you really want to skip \nAdvanced Tutorial?";
                questionPopup.ConfirmationReceived += ConfirmSkipReceivedHandler;
            }

            _uiManager.DrawPopup<QuestionPopup>(new object[]{tutorialSkipQuestion, false});
            GameClient.Get<IAppStateManager>().SetPausingApp(true);
        }

        private void ConfirmSkipReceivedHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmSkipReceivedHandler;
            if (status)
            {
                _gameplayManager.EndGame(Enumerators.EndGameType.CANCEL);
                GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.PlaySelection);
            }
            GameClient.Get<IAppStateManager>().SetPausingApp(false);
        }

        private void ConfirmQuitReceivedHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmQuitReceivedHandler;
            if (status)
            {
                _dataManager.CachedUserLocalData.CurrentTutorialId = _tutorials.Count;
                _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                _gameplayManager.EndGame(Enumerators.EndGameType.CANCEL);
                GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU);
            }
            GameClient.Get<IAppStateManager>().SetPausingApp(false);
        }


        public void NextButtonClickHandler()
        {
            if (!IsTutorial)
                return;

            if (CurrentTutorialDataStep.CanMoveToNextStepByClick)
            {
                NextStep();
            }

            if (CurrentTutorialDataStep.CanMoveToNextStepByClickInPaused && Paused)
            {
                NextStep();
            }
        }

        public void ReportAction(Enumerators.TutorialReportAction action)
        {
            if (IsTutorial)
            {
                if (CurrentTutorialDataStep != null)
                {
                    if (CurrentTutorialDataStep.RequiredAction == action)
                    {
                        if(action == Enumerators.TutorialReportAction.START_TURN &&
                          !string.IsNullOrEmpty(CurrentTutorialDataStep.WaiterOfWhichTurn))
                        {
                            if ((_gameplayManager.IsLocalPlayerTurn() &&
                                 CurrentTutorialDataStep.WaiterOfWhichTurn == Constants.Player) ||
                               (!_gameplayManager.IsLocalPlayerTurn() &&
                                CurrentTutorialDataStep.WaiterOfWhichTurn == Constants.Opponent))
                            {
                                NextStep();
                            }
                        }
                        else
                        {
                            NextStep();
                        }
                    }

                    if(action == Enumerators.TutorialReportAction.USE_ABILITY)
                    {
                        DestroySelectTarget();
                    }

                    ResetHelpArrows();
                }
            }
        }

        public void ActivateSelectTarget()
        {
            if (_targettingArrow != null)
            {
                _targettingArrow.Activate();
            }
        }

        public void DeactivateSelectTarget()
        {
            if (_targettingArrow != null)
            {
                _targettingArrow.Deactivate();
            }
        }

        public void NextStep()
        {
            if (!IsBubbleShow)
                return;

            if (_tutorialSteps.IndexOf(CurrentTutorialDataStep) >= _tutorialSteps.Count - 1)
            {
                GameClient.Get<IGameplayManager>().EndGame(Enumerators.EndGameType.WIN, 0);
                return;
            }

            if (CurrentTutorialDataStep.ShouldStopTurn)
            {
                GameClient.Get<ITimerManager>().AddTimer(
                    x =>
                    {
                        GameClient.Get<IGameplayManager>().GetController<BattlegroundController>().StopTurn();
                    },
                    null,
                    5f);
            }

            if (CurrentTutorialDataStep.CanProceedWithEndStepManually)
            {
                NextStepCommonEndActions();
            }
            else
            {
                GameClient.Get<ITimerManager>().AddTimer(
                    x =>
                    {
                        NextStepCommonEndActions();
                    },
                    time: 2f);
            }
        }

        private async void NextStepCommonEndActions()
        {
            _currentTutorialStepIndex++;

            CurrentTutorialDataStep = _tutorialSteps[_currentTutorialStepIndex];

            UpdateTutorialVisual();
            _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);
            if (CurrentTutorialDataStep.HasDelayToPlaySound)
            {
                if (!string.IsNullOrEmpty(CurrentTutorialDataStep.SoundName))
                {
                    GameClient.Get<ITimerManager>().AddTimer(
                    x =>
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, 0, CurrentTutorialDataStep.SoundName,
                            Constants.TutorialSoundVolume, false);
                    },
                    null,
                    CurrentTutorialDataStep.DelayToPlaySound);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(CurrentTutorialDataStep.SoundName))
                {
                    _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, 0, CurrentTutorialDataStep.SoundName, Constants.TutorialSoundVolume,
                        false);
                }
            }

            if (CurrentTutorialDataStep.IsLaunchAIBrain)
               await _gameplayManager.GetController<AIController>().LaunchAIBrain();
        }

        private void UpdateTutorialVisual()
        {
            DestroySelectTarget();

            _popup.Show(CurrentTutorialDataStep.JaneText);
            _popup.UpdatePose(CurrentTutorialDataStep.JanePose);

            if (CurrentTutorialDataStep.IsFocusing)
            {
                if (CurrentTutorialDataStep.IsArrowEnabled)
                {
                    CreateSelectTarget();
                }

                _popup.ShowTutorialFocus(_currentTutorialStepIndex);

                if (CurrentTutorialDataStep.IsShowNextButtonFocusing)
                {
                    _popup.ShowNextButton();
                }
            }
            else
            {
                _popup.HideTutorialFocus();

                if (CurrentTutorialDataStep.IsShowQuestion)
                {
                    _popup.ShowQuestion();
                }
                else if (CurrentTutorialDataStep.IsShowNextButton)
                {
                    _popup.ShowNextButton();
                }
            }

            ResetHelpArrows();

            if (CurrentTutorialDataStep.HasHelpArrowsAfterDelay)
            {
                _helpArrowsDelay = DOTween.Sequence();
                _helpArrowsDelay.PrependInterval(CurrentTutorialDataStep.DelayToShowHelpArrows);
                _helpArrowsDelay.OnComplete(ShowHelpArrowsDelay);
            }
        }

        private void ShowHelpArrowsDelay()
        {
           List<BoardUnitView> availableUnits = _gameplayManager.CurrentPlayer.BoardCards.FindAll(x => x.Model.UnitCanBeUsable());

            TutorialBoardArrow tutorialBoardArrow;
            foreach (BoardUnitView unit in availableUnits)
            {
                tutorialBoardArrow = Object.Instantiate(_targettingArrowPrefab).AddComponent<TutorialBoardArrow>();
                tutorialBoardArrow.Begin(unit.Transform.position);
                tutorialBoardArrow.UpdateTargetPosition((Vector3)CurrentTutorialDataStep.ArrowEndPosition);
                _tutorialHelpBoardArrows.Add(tutorialBoardArrow);
            }
        }

        private void ResetHelpArrows()
        {
            if (_helpArrowsDelay != null)
            {
                _helpArrowsDelay.Kill();
            }

            foreach (TutorialBoardArrow arrow in _tutorialHelpBoardArrows)
            {
                arrow.Dispose();
            }
            _tutorialHelpBoardArrows.Clear();
        }

        private void CreateSelectTarget()
        {
            _targettingArrow = Object.Instantiate(_targettingArrowPrefab).AddComponent<TutorialBoardArrow>();
            _targettingArrow.Begin((Vector3)CurrentTutorialDataStep.ArrowStartPosition);
            _targettingArrow.UpdateTargetPosition((Vector3)CurrentTutorialDataStep.ArrowEndPosition);
        }

        private void DestroySelectTarget()
        {
            if (_targettingArrow != null)
            {
                _targettingArrow.Dispose();
                _targettingArrow = null;
            }
        }
    }
}
