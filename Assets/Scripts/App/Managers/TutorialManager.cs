using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class TutorialManager : IService, ITutorialManager
    {
        public bool Paused;

        private IUIManager _uiManager;

        private ISoundManager _soundManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private IGameplayManager _gameplayManager;

        private BattlegroundController _battlegroundController;

        private TutorialPopup _popup;

        private TutorialBoardArrow _targettingArrow;

        private GameObject _targettingArrowPrefab;

        public bool IsTutorial { get; private set; }

        public bool IsBubbleShow { get; set; }

        private List<TutorialData> _tutorials;
        private List<TutorialDataStep> _tutorialSteps;
        private int _currentTutorialStepIndex;

        public TutorialData CurrentTutorial { get; private set; }
        public TutorialDataStep CurrentTutorialDataStep { get; private set; }

        public int LatestTutorialId { get; private set; } = 0;

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

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            // card vs player
            _targettingArrowPrefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");

            _tutorials = JsonConvert.DeserializeObject<TutorialContentData>(_loadObjectsManager
                        .GetObjectByPath<TextAsset>("Data/tutorial_data").text).TutorialDatas;
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
            //  GameClient.Get<ITimerManager>().AddTimer(
            //      x =>
            //      {
            _battlegroundController.SetupBattlegroundAsSpecific(CurrentTutorial.SpecificBattlegroundInfo);

            IsBubbleShow = true;
            _uiManager.DrawPopup<TutorialPopup>();
            _popup = _uiManager.GetPopup<TutorialPopup>();
            UpdateTutorialVisual();
            _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, CurrentTutorialDataStep.SoundName, Constants.TutorialSoundVolume, false);
            //        },
            //      null,
            //       4f);

            IsTutorial = true;
        }

        public void StopTutorial()
        {
            _uiManager.HidePopup<TutorialPopup>();

            _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);

            LatestTutorialId++;

            if (LatestTutorialId >= 2)
            {
                LatestTutorialId = 0;
                IsTutorial = false;
                GameClient.Get<IGameplayManager>().IsTutorial = false;
                GameClient.Get<IDataManager>().CachedUserLocalData.Tutorial = false;
                GameClient.Get<IDataManager>().SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
            }
        }

        public void SkipTutorial(Enumerators.AppState state)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            Action callback = () =>
            {
                GameClient.Get<IGameplayManager>().EndGame(Enumerators.EndGameType.CANCEL);
                GameClient.Get<IMatchManager>().FinishMatch(state);
            };
            _uiManager.DrawPopup<ConfirmationPopup>(callback);
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
                if (CurrentTutorialDataStep.RequiredAction == action)
                {
                    NextStep();
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
                GameClient.Get<ITimerManager>().AddTimer(
                    x =>
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, CurrentTutorialDataStep.SoundName,
                            Constants.TutorialSoundVolume, false);
                    },
                    null,
                    CurrentTutorialDataStep.DelayToPlaySound);
            }
            else
            {
                _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, CurrentTutorialDataStep.SoundName, Constants.TutorialSoundVolume,
                    false);
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
