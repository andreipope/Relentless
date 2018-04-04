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
        private TutorialPopup _popup;


        private int _currentStep = 0;

        private List<TutorialStep> _steps;

        private bool _tutorialStarted;

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
            
            int i = 0;
            _steps = new List<TutorialStep>();
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "<b><color=#FFC300>Hello dear friend!</color></b>\n We will teach you how to play \n *Press anykey to continue*", false));
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "Please take your card and drag to battleground", true));
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "Great, it was your first step to become a great Warrior. \n *Press anykey to continue*", false));
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "You creature need to rest before attack, end your turn!", true));
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "Wow enemy also call the creature, you should attack it. \n Wait for your turn.", false));
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "Drag your zombie to enemy to attack him and see what happens...", true));
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "Congrats! it was your first attack. Give your zombie to rest and end your turn", true));
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "Now Opponent Turn. \n You should wait...", false));
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "Try now attack the opponent hero!", true));
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "Perfect Attack! \n One step and you win.  \n *Press anykey to continue*", false));
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "Now end your turn.", true));
            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "Damn what is that??? \n He called the <b>Heavy</b> Monster. \n This creatures not give you chance to attack anybody else till they alive", false));
            _steps.Add(new TutorialStep(ref i,
                                   new Vector2[] { new Vector2(0, 0) },
                                   "You should kill him to get the chance to attack the Hero", true));
            _steps.Add(new TutorialStep(ref i,
                                  new Vector2[] { new Vector2(0, 0) },
                                  "I see you have <b>Ferral</b> zombie! Use It! \n This creatures can attack immediately after they was played", true));

            _steps.Add(new TutorialStep(ref i,
                                 new Vector2[] { new Vector2(0, 0) },
                                 "Now finish him! Do not let him escape! WOHAAA!!!", true));

            _steps.Add(new TutorialStep(ref i,
                                 new Vector2[] { new Vector2(0, 0) },
                                 "He still alive! \n Use your hero skill. \n BURN HIM!", true));

            _steps.Add(new TutorialStep(ref i,
                                    new Vector2[] { new Vector2(0, 0) },
                                    "Well Done! Good start warrior! Back to the deck selection and try real game", false));
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


        public void Update()
        {
            if (!_tutorialStarted)
                return;
            if(Input.anyKeyDown)
            {
                if (_currentStep == 0 || _currentStep == 2 || _currentStep == 9)
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
            _popup.Show(_steps[_currentStep].description);
            //_popup.SetPosition(positions[0]);
            if (_steps[_currentStep].focusing)
                _popup.ShowTutorialFocus(_currentStep);
            else
                _popup.HideTutorialFocus();
        }

        public void ReportAction(Enumerators.TutorialReportAction action)
        {
            //Debug.Log(action + "_" + _currentStep);
            if(_tutorialStarted)
            switch(action)
            {
                case Enumerators.TutorialReportAction.MOVE_CARD:
                    if (_currentStep == 1 || _currentStep == 13)
                        NextStep();
                    break;
                case Enumerators.TutorialReportAction.END_TURN:
                    if (_currentStep == 3 || _currentStep == 4 || _currentStep == 6 || _currentStep == 7 || _currentStep == 10 || _currentStep == 11)
                        NextStep();
                    break;
                case Enumerators.TutorialReportAction.ATTACK_CARD_CARD:
					if (_currentStep == 5 || _currentStep == 12)
						NextStep();
					break;
                case Enumerators.TutorialReportAction.ATTACK_CARD_HERO:
                    if (_currentStep == 8 || _currentStep == 14)
                        NextStep();
                        break;
                case Enumerators.TutorialReportAction.USE_ABILITY:
                    if (_currentStep == 15)
                        NextStep();
                        break;
                    default:
                    break;
            }
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


        public TutorialStep(ref int index, Vector2[] focusPoints, string description, bool focusing)
        {
            _index = index;
            this.focusPoints = focusPoints;
            this.description = description;
            this.focusing = focusing;
            finished = false;
            index++;
        }

        public void Update()
        {
            if(finished)
            {
               // _handler;
            }
            
        }
    }
}