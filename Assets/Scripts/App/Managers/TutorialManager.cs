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

        private bool _tutorialStarted;

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
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true));
			_steps.Add(new TutorialStep(ref i,
									_contentManager.TutorialInfo[i].Description, false));
			_steps.Add(new TutorialStep(ref i,
						            _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -4.6f, 0), new Vector3(0, -3.0f, 0)));
			_steps.Add(new TutorialStep(ref i,
									_contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true));
			_steps.Add(new TutorialStep(ref i,
									_contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -0.3f, 0), new Vector3(0, 0.5f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
			_steps.Add(new TutorialStep(ref i,
									_contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -0.3f, 0), new Vector3(0, 2.5f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                              _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -0.3f, 0), new Vector3(0, 0.5f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
			_steps.Add(new TutorialStep(ref i,
									_contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -4.6f, 0), new Vector3(0, -3.0f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -0.3f, 0), new Vector3(0, 2.5f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                 _contentManager.TutorialInfo[i].Description, true, true, new Vector3(1.5f, -3.0f, 0), new Vector3(0.4f, 2.5f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
			_steps.Add(new TutorialStep(ref i,
									_contentManager.TutorialInfo[i].Description, false));
			_steps.Add(new TutorialStep(ref i,
									_contentManager.TutorialInfo[i].Description, false));

            _targettingArrowPrefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/Gameplay/TutorialTargetingArrow");
        }

        public void StartTutorial()
        {
            
            _uiManager.DrawPopup<TutorialPopup>();
            _popup = _uiManager.GetPopup<TutorialPopup>() as TutorialPopup;
            UpdateTutorialVisual(/*_steps[_currentStep].description, _steps[_currentStep].focusPoints*/);
            _soundManager.PlaySound(new List<AudioClip>(), Enumerators.SoundType.TUTORIAL, 0, 128, 1f, null, false, false, false);
            _tutorialStarted = true;            
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
            if (_currentStep >= _steps.Count - 1)
            {
				//var scene = GameObject.Find("GameScene").GetComponent<GameScene>();
                //scene.OpenPopup<PopupOneButton>("PopupOneButton", popup =>
                //{
                //	popup.text.text = "You win!";
                //	popup.buttonText.text = "Exit";
                //	popup.button.onClickEvent.AddListener(() =>
                //	{
                //		if (NetworkingUtils.GetLocalPlayer().isServer)
                //		{
                //			NetworkManager.singleton.StopHost();
                //		}
                //		else
                //		{
                //			NetworkManager.singleton.StopClient();
                //		}
                //		scene.ClosePopup();
                //		GameClient.Get<IAppStateManager>().ChangeAppState(GrandDevs.CZB.Common.Enumerators.AppState.DECK_SELECTION);
                //	});
                //});
                GameClient.Get<IUIManager>().DrawPopup<YouWonPopup>();
                GameClient.Get<ITutorialManager>().StopTutorial();
				return;
            }
            if (_currentStep == 11)
                GameClient.Get<ITimerManager>().AddTimer((x) => { DemoAIPlayer.Instance.StopTurn(); }, null, 5f, false);
			//if (_currentStep == 11)
			//	GameClient.Get<ITimerManager>().AddTimer((x) => { DemoAIPlayer.Instance.StopTurn(); }, null, 0.5f, false);

            _steps[_currentStep].finished = true;
            _currentStep++;
            GameManager.Instance.tutorialStep = _currentStep;
            UpdateTutorialVisual(/*_steps[_currentStep].description, _steps[_currentStep].focusPoints*/);
            _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);
            _soundManager.PlaySound(new List<AudioClip>(), Enumerators.SoundType.TUTORIAL, _currentStep, 128, 1f, null, false, false, false);
        }  

        private void UpdateTutorialVisual(/*string text, Vector2[] positions*/)
        {
            DestroySelectTarget();
            _popup.Show(_steps[_currentStep].description);
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
                else if(_currentStep != 12 && _currentStep != 17 && _currentStep != 22 )
                    _popup.ShowNextButton();
            }
        }

        public void ReportAction(Enumerators.TutorialReportAction action)
        {
            Debug.Log(action + "_" + _currentStep);
            if(_tutorialStarted)
            switch(action)
            {
                case Enumerators.TutorialReportAction.MOVE_CARD:
                    if (_currentStep == 8 || _currentStep == 27)
                        NextStep();
                    break;
                case Enumerators.TutorialReportAction.END_TURN:
                    if (_currentStep == 10 || _currentStep == 12 || _currentStep == 16 || _currentStep == 17 || _currentStep == 21 || _currentStep == 22)
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
        //public Action _handler;
        public bool finished;
        public bool isArrowEnabled;
        public TutorialTargetingArrowInfo tutorialTargetingArrowInfo;


        public TutorialStep(ref int index, string description, bool focusing)
        {
            _index = index;
           // this.focusPoints = Vector2.zero;
            this.description = description;
            this.focusing = focusing;
            finished = false;
            index++;
            tutorialTargetingArrowInfo = new TutorialTargetingArrowInfo();
            isArrowEnabled = false;
        }

        public TutorialStep(ref int index, string description, bool focusing, bool isArrowEnabled, Vector3 startPosition, Vector3 targetPosition)
        {
            _index = index;
           // this.focusPoints = Vector2.zero;
            this.description = description;
            this.focusing = focusing;
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