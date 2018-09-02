using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class TutorialManager : IService, ITutorialManager
    {
        public bool Paused;

        private IUIManager _uiManager;

        private IContentManager _contentManager;

        private ISoundManager _soundManager;

        private TutorialPopup _popup;

        private List<TutorialStep> _steps;

        private TutorialBoardArrow _targettingArrow;

        private GameObject _targettingArrowPrefab;

        public void Dispose()
        {
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _contentManager = GameClient.Get<IContentManager>();
            _soundManager = GameClient.Get<ISoundManager>();

            int i = 0;
            _steps = new List<TutorialStep>();
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBS_UP, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBS_UP, _contentManager.TutorialInfo[i].Description, true, true, new Vector3(5f, -6f, 0), new Vector3(0, -1.7f, 0)));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBS_UP, _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THINKING, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBS_UP, _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -1.5f, 0), new Vector3(0, 2f, 0)));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBS_UP, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL, _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THINKING, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL, _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -1.6f, 0), new Vector3(0, 5.55f, 0))); // card vs player
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBS_UP, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL, _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THINKING, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -1.5f, 0), new Vector3(0, 2f, 0)));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THINKING, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THINKING, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, true, true, new Vector3(7f, -6.5f, 0), new Vector3(0, -1.6f, 0)));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -1.5f, 0), new Vector3(0, 5.55f, 0)));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBS_UP, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING, _contentManager.TutorialInfo[i].Description, true, true, new Vector3(2.5f, -5.0f, 0), new Vector3(0f, 5.55f, 0)));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBS_UP, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBS_UP, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL, _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.KISS, _contentManager.TutorialInfo[i].Description, false));

            _targettingArrowPrefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");
        }

        public void Update()
        {
        }

        public int CurrentStep { get; private set; }

        public bool IsTutorial { get; private set; }

        public bool IsBubbleShow { get; set; }

        public void StartTutorial()
        {
            GameClient.Get<ITimerManager>().AddTimer(
                x =>
                {
                    CurrentStep = 0;
                    IsBubbleShow = true;
                    _uiManager.DrawPopup<TutorialPopup>();
                    _popup = _uiManager.GetPopup<TutorialPopup>();
                    UpdateTutorialVisual( /*_steps[_currentStep].description, _steps[_currentStep].focusPoints*/);
                    _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, 0, Constants.TutorialSoundVolume, false, false);
                },
                null,
                4f,
                false);

            IsTutorial = true;

            // GameObject.Find("Player/Avatar").GetComponent<PlayerAvatar>().SetupTutorial();
            // GameObject.Find("Opponent/Avatar").GetComponent<PlayerAvatar>().SetupTutorial();
        }

        public void StopTutorial()
        {
            _uiManager.HidePopup<TutorialPopup>();
            IsTutorial = false;
            GameClient.Get<IGameplayManager>().IsTutorial = false;
            GameClient.Get<IDataManager>().CachedUserLocalData.Tutorial = false;
            GameClient.Get<IDataManager>().SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
            _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);
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

            if ((CurrentStep == 0) || (CurrentStep == 1) || (CurrentStep == 2) || (CurrentStep == 3) || (CurrentStep == 4) || (CurrentStep == 5) || (CurrentStep == 6) || (CurrentStep == 7) || (CurrentStep == 9) || (CurrentStep == 14) || (CurrentStep == 15) || (CurrentStep == 18) || (CurrentStep == 20) || (CurrentStep == 22) || (CurrentStep == 24) || (CurrentStep == 25) || (CurrentStep == 26) || (CurrentStep == 29) || (CurrentStep == 30) || (CurrentStep == 31) || (CurrentStep == 33) || (CurrentStep == 34) || (CurrentStep == 35) || (CurrentStep == 36) || (CurrentStep == 37))
            {
                NextStep();
            }

            if ((CurrentStep == 11) && Paused)
            {
                NextStep();
            }
        }

        public void ReportAction(Enumerators.TutorialReportAction action)
        {
            if (IsTutorial)
            {
                switch (action)
                {
                    case Enumerators.TutorialReportAction.MOVE_CARD:
                        if ((CurrentStep == 8) || (CurrentStep == 27))
                        {
                            NextStep();
                        }

                        break;
                    case Enumerators.TutorialReportAction.END_TURN:
                        if ((CurrentStep == 10) || (CurrentStep == 12) || (CurrentStep == 16) || (CurrentStep == 17) || (CurrentStep == 21))
                        {
                            NextStep();
                        }

                        break;
                    case Enumerators.TutorialReportAction.ATTACK_CARD_CARD:
                        if ((CurrentStep == 13) || (CurrentStep == 23))
                        {
                            NextStep();
                        }

                        break;
                    case Enumerators.TutorialReportAction.ATTACK_CARD_HERO:
                        if ((CurrentStep == 19) || (CurrentStep == 28))
                        {
                            NextStep();
                        }

                        break;
                    case Enumerators.TutorialReportAction.USE_ABILITY:
                        if (CurrentStep == 32)
                        {
                            NextStep();
                        }

                        break;
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

            if (CurrentStep >= _steps.Count - 1)
            {
                GameClient.Get<IGameplayManager>().EndGame(Enumerators.EndGameType.WIN, 0);
                return;
            }

            if (CurrentStep == 11)
            {
                GameClient.Get<ITimerManager>().AddTimer(
                    x =>
                    {
                        GameClient.Get<IGameplayManager>().GetController<BattlegroundController>().StopTurn();
                    },
                    null,
                    5f,
                    false);
            }

            if (CurrentStep != 32)
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

        private void NextStepCommonEndActions()
        {
            _steps[CurrentStep].Finished = true;
            CurrentStep++;
            GameClient.Get<IGameplayManager>().TutorialStep = CurrentStep;
            UpdateTutorialVisual( /*_steps[_currentStep].description, _steps[_currentStep].focusPoints*/);
            _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);
            if (CurrentStep == 22)
            {
                GameClient.Get<ITimerManager>().AddTimer(
                    x =>
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, CurrentStep, Constants.TutorialSoundVolume, false, false);
                    },
                    null,
                    6f,
                    false);
            }
            else
            {
                _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, CurrentStep, Constants.TutorialSoundVolume, false, false);
            }
        }

        private void UpdateTutorialVisual( /*string text, Vector2[] positions*/)
        {
            DestroySelectTarget();
            _popup.Show(_steps[CurrentStep].Description);
            _popup.UpdatePose(_steps[CurrentStep].Pose);

            // _popup.SetPosition(positions[0]);
            if (_steps[CurrentStep].Focusing)
            {
                if (_steps[CurrentStep].IsArrowEnabled)
                {
                    CreateSelectTarget();
                }

                _popup.ShowTutorialFocus(CurrentStep);
                if ((CurrentStep == 5) || (CurrentStep == 9) || (CurrentStep == 14))
                {
                    _popup.ShowNextButton();
                }
            }
            else
            {
                _popup.HideTutorialFocus();
                if (CurrentStep == 3)
                {
                    _popup.ShowQuestion();
                }
                else if ((CurrentStep != 12) && (CurrentStep != 17))
                {
                    _popup.ShowNextButton();
                }
            }
        }

        private void CreateSelectTarget()
        {
            _targettingArrow = Object.Instantiate(_targettingArrowPrefab).AddComponent<TutorialBoardArrow>();
            _targettingArrow.Begin(_steps[CurrentStep].TutorialTargetingArrowInfo.StartPosition);
            _targettingArrow.UpdateTargetPosition(_steps[CurrentStep].TutorialTargetingArrowInfo.TargetPosition);
        }

        private void DestroySelectTarget()
        {
            if (_targettingArrow != null)
            {
                _targettingArrow.Dispose();
                _targettingArrow = null;
            }

            // _targettingArrow.Deactivate();
        }
    }

    public class TutorialStep
    {
        public Vector2[] FocusPoints;

        public string Description;

        public bool Focusing;

        public Enumerators.TutorialJanePoses Pose;

        // public Action _handler;
        public bool Finished;

        public bool IsArrowEnabled;

        public TutorialTargetingArrowInfo TutorialTargetingArrowInfo;

        private int _index;

        public TutorialStep(ref int index, Enumerators.TutorialJanePoses pose, string description, bool focusing)
        {
            _index = index;

            // this.focusPoints = Vector2.zero;
            Description = description;
            Focusing = focusing;
            Pose = pose;
            Finished = false;
            index++;
            TutorialTargetingArrowInfo = new TutorialTargetingArrowInfo();
            IsArrowEnabled = false;
        }

        public TutorialStep(
            ref int index,
            Enumerators.TutorialJanePoses pose,
            string description,
            bool focusing,
            bool isArrowEnabled,
            Vector3 startPosition,
            Vector3 targetPosition)
        {
            _index = index;

            // this.focusPoints = Vector2.zero;
            Description = description;
            Focusing = focusing;
            Pose = pose;

            Finished = false;
            index++;
            IsArrowEnabled = isArrowEnabled;
            TutorialTargetingArrowInfo = new TutorialTargetingArrowInfo();
            TutorialTargetingArrowInfo.StartPosition = startPosition;
            TutorialTargetingArrowInfo.TargetPosition = targetPosition;
        }

        public void Update()
        {
            if (Finished)
            {
                // _handler;
            }
        }
    }

    public class TutorialTargetingArrowInfo
    {
        public Vector3 StartPosition;

        public Vector3 TargetPosition;
    }
}
