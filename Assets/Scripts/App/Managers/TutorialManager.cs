using UnityEngine;
using System;
using System.Collections.Generic;
using GrandDevs.CZB.Gameplay;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine.Networking;

namespace GrandDevs.CZB
{
    public class TutorialManager : IService, ITutorialManager
    {
        private IUIManager _uiManager;
        private IContentManager _contentManager;
        private ISoundManager _soundManager;
        private TutorialPopup _popup;


        private int _currentStep = 0;

        private List<TutorialStep> _steps;

        private bool _tutorialStarted,
                     _isBubbleShow;

        private TutorialTargetingArrow _targettingArrow;

        private GameObject _targettingArrowPrefab;

        public bool paused;

        public int CurrentStep
        {
            get { return _currentStep; }
        }

        public bool IsTutorial
        {
            get { return _tutorialStarted; }
        }

        public bool IsBubbleShow
        {
            get { return _isBubbleShow; }
            set { _isBubbleShow = value; }
        }

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
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBSUP,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
                                    _contentManager.TutorialInfo[i].Description, true));
			_steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
									_contentManager.TutorialInfo[i].Description, false));
			_steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
						            _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBSUP,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(5.9f, -7.1f, 0), new Vector3(1.5f, -3.2f, 0)));
			_steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
									_contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBSUP,
                                    _contentManager.TutorialInfo[i].Description, true));
			_steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
									_contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THINKING,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBSUP,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -0.3f, 0), new Vector3(0, 0.5f, 0)));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
                                    _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBSUP,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL,
                                    _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THINKING,
                                    _contentManager.TutorialInfo[i].Description, false));
			_steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
									_contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -1.6f, 0), new Vector3(0, 3.4f, 0)));//card vs player
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBSUP,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL,
                                    _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THINKING,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
                              _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -0.3f, 0), new Vector3(0, 0.5f, 0)));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THINKING,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THINKING,
                                    _contentManager.TutorialInfo[i].Description, false));
			_steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
									_contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(5.9f, -7.1f, 0), new Vector3(1.5f, -3.2f, 0)));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.POINTING,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -1.6f, 0), new Vector3(0, 3.4f, 0)));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBSUP,
                                 _contentManager.TutorialInfo[i].Description, true, true, new Vector3(2.5f, -5.0f, 0), new Vector3(0.5f, 3.2f, 0)));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBSUP,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.THUMBSUP,
                                    _contentManager.TutorialInfo[i].Description, false));
			_steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.NORMAL,
									_contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i, Enumerators.TutorialJanePoses.KISS,
									_contentManager.TutorialInfo[i].Description, false));

            _targettingArrowPrefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/Gameplay/TutorialTargetingArrow");
        }

        public void StartTutorial()
        {
            _isBubbleShow = true;
            _uiManager.DrawPopup<TutorialPopup>();
            _popup = _uiManager.GetPopup<TutorialPopup>() as TutorialPopup;
            UpdateTutorialVisual(/*_steps[_currentStep].description, _steps[_currentStep].focusPoints*/);
            _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL,0, Constants.TUTORIAL_SOUND_VOLUME, false, false);
            _tutorialStarted = true;

            GameObject.Find("Player/Avatar").GetComponent<PlayerAvatar>().SetupTutorial();
            GameObject.Find("Opponent/Avatar").GetComponent<PlayerAvatar>().SetupTutorial();
        }

        public void StopTutorial()
        {
            _uiManager.HidePopup<TutorialPopup>();
            _tutorialStarted = false;
            GameManager.Instance.tutorial = false;
            GameClient.Get<IDataManager>().CachedUserLocalData.tutorial = false;
        }

        public void CancelTutorial()
        {
            _uiManager.HidePopup<TutorialPopup>();
            _tutorialStarted = false;
            _currentStep = 0;
        }

        public void Update()
        {
            if (!_tutorialStarted)
                return;
            if(Input.anyKeyDown)
            {
                if (_currentStep == 0 ||
                    _currentStep == 1 ||
                    _currentStep == 2 ||
                    _currentStep == 3 ||
                    _currentStep == 4 ||
                    _currentStep == 5 ||
                    _currentStep == 6 ||
                    _currentStep == 7 ||
					_currentStep == 9 ||
                    _currentStep == 14 ||
                    _currentStep == 15 ||
                    _currentStep == 18 ||
                    _currentStep == 20 ||
                    _currentStep == 22 ||
                    _currentStep == 24 ||
					_currentStep == 25 ||
					_currentStep == 26 ||
                    _currentStep == 30 ||
                    _currentStep == 31 ||
                    _currentStep == 32 ||
                    _currentStep == 33 ||
                    _currentStep == 34
                    )
                    NextStep();
                if (_currentStep == 11 && paused)
                    NextStep();
            }
        }      
        
        public void NextStep()
        {
            if (!_isBubbleShow)
                return;

            if (_currentStep >= _steps.Count - 1)
            {
                GameClient.Get<IUIManager>().DrawPopup<YouWonPopup>();
                GameClient.Get<ITutorialManager>().StopTutorial();
				return;
            }
            if (_currentStep == 11)
                GameClient.Get<ITimerManager>().AddTimer((x) => { DemoAIPlayer.Instance.StopTurn(); }, null, 5f, false);

            if (_currentStep != 29)
                NextStepCommonEndActions();
            else
                GameClient.Get<ITimerManager>().AddTimer((x) => { NextStepCommonEndActions(); }, time:2f);
        }

        private void NextStepCommonEndActions()
        {
            _steps[_currentStep].finished = true;
            _currentStep++;
            GameManager.Instance.tutorialStep = _currentStep;
            UpdateTutorialVisual(/*_steps[_currentStep].description, _steps[_currentStep].focusPoints*/);
            _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);
            if (_currentStep == 22)
                GameClient.Get<ITimerManager>().AddTimer((x) => {
                    _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, _currentStep, Constants.TUTORIAL_SOUND_VOLUME, false, false);
                }, null, 6f, false);
            else
                _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, _currentStep, Constants.TUTORIAL_SOUND_VOLUME, false, false);
        }

        private void UpdateTutorialVisual(/*string text, Vector2[] positions*/)
        {
            DestroySelectTarget();
			_popup.Show(_steps[_currentStep].description);
			_popup.UpdatePose(_steps[_currentStep].pose);
            //_popup.SetPosition(positions[0]);
            if (_steps[_currentStep].focusing)
            {
                if (_steps[_currentStep].isArrowEnabled)
                    CreateSelectTarget();
                _popup.ShowTutorialFocus(_currentStep);
				if (_currentStep == 5 || _currentStep == 9 || _currentStep == 14)
					_popup.ShowNextButton();
			}
            else
            {
                _popup.HideTutorialFocus();
                if (_currentStep == 3)
                    _popup.ShowQuestion();
                else if(_currentStep != 12 && _currentStep != 17 )
                    _popup.ShowNextButton();
            }
        }

        public void ReportAction(Enumerators.TutorialReportAction action)
        {
            if(_tutorialStarted)
            switch(action)
            {
                case Enumerators.TutorialReportAction.MOVE_CARD:
                    if (_currentStep == 8 || _currentStep == 27)
                        NextStep();
                    break;
                case Enumerators.TutorialReportAction.END_TURN:
                    if (_currentStep == 10 || _currentStep == 12 || _currentStep == 16 || _currentStep == 17 || _currentStep == 21)
                        NextStep();
                    break;
                case Enumerators.TutorialReportAction.ATTACK_CARD_CARD:
					if (_currentStep == 13 || _currentStep == 23)
						NextStep();
					break;
                case Enumerators.TutorialReportAction.ATTACK_CARD_HERO:
                    if (_currentStep == 19 || _currentStep == 28)
                        NextStep();
                        break;
                case Enumerators.TutorialReportAction.USE_ABILITY:
                    if (_currentStep == 29)
                        NextStep();
                        break;
                    default:
                    break;
            }
        }

        public void ActivateSelectTarget()
        {
            if (_targettingArrow != null)
                _targettingArrow.Activate();
        }

        public void DeactivateSelectTarget()
        {
            if (_targettingArrow != null)
                _targettingArrow.Deactivate();
        }


        private void CreateSelectTarget()
        {
            _targettingArrow = MonoBehaviour.Instantiate(_targettingArrowPrefab).GetComponent<TutorialTargetingArrow>();
            _targettingArrow.Begin(_steps[_currentStep].tutorialTargetingArrowInfo.startPosition);
            _targettingArrow.UpdateTargetPosition(_steps[_currentStep].tutorialTargetingArrowInfo.targetPosition);
        }

        private void DestroySelectTarget()
        {
            if (_targettingArrow != null)
            {
                MonoBehaviour.Destroy(_targettingArrow.gameObject);
                _targettingArrow = null;
            }
            //_targettingArrow.Deactivate();
        }
    }

    public class TutorialStep
    {
        private int _index;
        public Vector2[] focusPoints;
        public string description;
        public bool focusing;
        public Enumerators.TutorialJanePoses pose;
        //public Action _handler;
        public bool finished;
        public bool isArrowEnabled;
        public TutorialTargetingArrowInfo tutorialTargetingArrowInfo;


        public TutorialStep(ref int index, Enumerators.TutorialJanePoses pose, string description, bool focusing)
        {
            _index = index;
           // this.focusPoints = Vector2.zero;
            this.description = description;
            this.focusing = focusing;
            this.pose = pose;
            finished = false;
            index++;
            tutorialTargetingArrowInfo = new TutorialTargetingArrowInfo();
            isArrowEnabled = false;
        }

        public TutorialStep(ref int index, Enumerators.TutorialJanePoses pose, string description, bool focusing, bool isArrowEnabled, Vector3 startPosition, Vector3 targetPosition)
        {
            _index = index;
           // this.focusPoints = Vector2.zero;
            this.description = description;
            this.focusing = focusing;
			this.pose = pose;

			finished = false;
            index++;
            this.isArrowEnabled = isArrowEnabled;
            tutorialTargetingArrowInfo = new TutorialTargetingArrowInfo();
            tutorialTargetingArrowInfo.startPosition = startPosition;
            tutorialTargetingArrowInfo.targetPosition = targetPosition;
        }

        public void Update()
        {
            if(finished)
            {
               // _handler;
            }
            
        }
    }

    public class TutorialTargetingArrowInfo
    {
        public Vector3 startPosition;
        public Vector3 targetPosition;
    }
}