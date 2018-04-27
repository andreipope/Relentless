using UnityEngine;
using System;
using System.Collections.Generic;
using GrandDevs.CZB.Gameplay;
using GrandDevs.CZB.Common;
using CCGKit;

namespace GrandDevs.CZB
{
    public class TutorialManager : IService, ITutorialManager
    {
        private IUIManager _uiManager;
        private IContentManager _contentManager;
        private TutorialPopup _popup;


        private int _currentStep = 0;

        private List<TutorialStep> _steps;

        private bool _tutorialStarted;

        private TutorialTargetingArrow _targettingArrow;

        private GameObject _targettingArrowPrefab;

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
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -6, 0), new Vector3(0, -1.5f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -1.3f, 0), new Vector3(0, 1.5f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -1.3f, 0), new Vector3(0, 4f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                              _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -1.3f, 0), new Vector3(0, 1.5f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, false));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -6, 0), new Vector3(0, -1.5f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                    _contentManager.TutorialInfo[i].Description, true, true, new Vector3(0, -1.3f, 0), new Vector3(0, 4f, 0)));
            _steps.Add(new TutorialStep(ref i,
                                 _contentManager.TutorialInfo[i].Description, true, true, new Vector3(1.8f, -3.5f, 0), new Vector3(0, 4f, 0)));
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
                    _currentStep == 7 ||
                    _currentStep == 11 ||
                    _currentStep == 12 ||
                    _currentStep == 16 ||
                    _currentStep == 20 ||
                    _currentStep == 21 ||
                    _currentStep == 25 ||
                    _currentStep == 26
                    )
                    NextStep();
            }
        }      
        
        public void NextStep()
        {
            if (_currentStep >= _steps.Count-1)
                return;

            _steps[_currentStep].finished = true;
            _currentStep++;
            GameManager.Instance.tutorialStep = _currentStep;
            UpdateTutorialVisual(/*_steps[_currentStep].description, _steps[_currentStep].focusPoints*/);

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
            }
            else
            {
                _popup.HideTutorialFocus();
            }
        }

        public void ReportAction(Enumerators.TutorialReportAction action)
        {
            Debug.Log(action + "_" + _currentStep);
            if(_tutorialStarted)
            switch(action)
            {
                case Enumerators.TutorialReportAction.MOVE_CARD:
                    if (_currentStep == 6 || _currentStep == 22)
                        NextStep();
                    break;
                case Enumerators.TutorialReportAction.END_TURN:
                    if (_currentStep == 8 || _currentStep == 9 || _currentStep == 13 || _currentStep == 14 || _currentStep == 17 || _currentStep == 18)
                        NextStep();
                    break;
                case Enumerators.TutorialReportAction.ATTACK_CARD_CARD:
					if (_currentStep == 10 || _currentStep == 19)
						NextStep();
					break;
                case Enumerators.TutorialReportAction.ATTACK_CARD_HERO:
                    if (_currentStep == 15 || _currentStep == 23)
                        NextStep();
                        break;
                case Enumerators.TutorialReportAction.USE_ABILITY:
                    if (_currentStep == 24)
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