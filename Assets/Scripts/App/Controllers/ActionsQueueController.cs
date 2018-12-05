using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class ActionsQueueController : IController
    {
        private long _nextActionId = 0;

        private Queue<GameAction<object>> _actionsToDo;

        private Queue<Action<Action>> _updateBoardActions;

        private GameAction<object> _actionInProgress;

        private Action<Action> _updateBoardActionInProgress;

        public event Action<PastActionsPopup.PastActionParam> GotNewActionReportEvent;

        public List<PastActionsPopup.PastActionParam> ActionsReports { get; private set; }

        public int ActionsInQueue { get { return _actionsToDo.Count + (_actionInProgress == null ? 0 : 1); } }

        private bool _isDebugMode = false;

        public void Init()
        {
            _actionsToDo = new Queue<GameAction<object>>();
            _updateBoardActions = new Queue<Action<Action>>();
            ActionsReports = new List<PastActionsPopup.PastActionParam>();
            _actionInProgress = null;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
            StopAllActions();

            ActionsReports.Clear();

            _nextActionId = 0;
        }

        /// <summary>
        ///     AddNewActionInToQueue
        /// </summary>
        /// <param name="actionToDo">action to do, parameter + callback action</param>
        /// <param name="parameter">parameters for action if ot needs</param>
        /// <param name="report">report that will be added into reports list</param>
        public GameAction<object> AddNewActionInToQueue(
            Action<object, Action> actionToDo, string ACTION_NAME, object parameter = null)
        {
            _nextActionId++;

            if (_isDebugMode)
            {
                UnityEngine.Debug.LogError(_actionsToDo.Count + " was actions; add <color=yellow>new action " + ACTION_NAME + " : " + _nextActionId + "; </color> from >>>> ");
            }

            GameAction<object> gameAction = new GameAction<object>(actionToDo, parameter, _nextActionId, ACTION_NAME);
            gameAction.OnActionDoneEvent += OnActionDoneEvent;
            _actionsToDo.Enqueue(gameAction);

            if (_actionInProgress == null && _actionsToDo.Count < 2)
            {
                TryCallNewActionFromQueue();
            }

            return gameAction;
        }

        public void StopAllActions()
        {
            ClearActions();
        }

        public void ClearActions()
        {
            _actionsToDo.Clear();
            _actionInProgress = null;
            _updateBoardActionInProgress = null;

            _updateBoardActions.Clear();
        }

        public void PostGameActionReport(PastActionsPopup.PastActionParam report)
        {
            if (report != null)
            {
                ActionsReports.Add(report);
                GotNewActionReportEvent?.Invoke(report);
            }
        }

        public void AddUpdateBoardAction(Action<Action> action)
        {
            _updateBoardActions.Enqueue(action);

            if (_updateBoardActions.Count == 1 && _updateBoardActionInProgress == null)
            {
                StartUpdateBoardAction();
            }
        }

        private void UpdateBoardActionEndCallback()
        {
            _updateBoardActionInProgress = null;
            StartUpdateBoardAction();
        }

        private void StartUpdateBoardAction()
        {
            if (_updateBoardActions.Count > 0)
            {
                _updateBoardActionInProgress = _updateBoardActions.Dequeue();
                _updateBoardActionInProgress.Invoke(UpdateBoardActionEndCallback);
            }
        }

        private void OnActionDoneEvent(GameAction<object> previousAction)
        {
            if (_isDebugMode)
            {
                UnityEngine.Debug.LogError(_actionsToDo.Count + " was actions; action <color=cyan>" + previousAction.Name + " : " + previousAction.Id + " DONE </color> from >>>> ");
            }

            TryCallNewActionFromQueue();
        }

        private void TryCallNewActionFromQueue()
        {
            if (_actionsToDo.Count > 0)
            {
                _actionInProgress = _actionsToDo.Dequeue();

                if (_isDebugMode)
                {
                    UnityEngine.Debug.LogError(_actionsToDo.Count + " was actions; <color=red> Dooooooo action " + _actionInProgress.Name + " : " + _actionInProgress.Id + ";  </color> from >>>> ");
                }

                _actionInProgress.DoAction();
            }
            else
            {
                _actionInProgress = null;
            }
        }
    }

    public class GameAction<T>
    {
        public Action<T, Action> Action;

        public T Parameter;

        private readonly ITimerManager _timerManager;

        private bool _actionDone;

        public string Name;

        public long Id;

        public GameAction(Action<T, Action> action, T parameter, long id, string name)
        {
            _timerManager = GameClient.Get<ITimerManager>();

            Action = action;
            Parameter = parameter;
            Id = id;

            Name = name;
        }

        public event Action<GameAction<T>> OnActionDoneEvent;

        public void DoAction()
        {
            try
            {
                Action?.Invoke(Parameter, ActionDoneCallback);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.Message + "; " + ex.StackTrace);

                if (!_actionDone)
                {
                    ActionDoneCallback();
                }
            }
        }

        public void ForceActionDone()
        {
            if (_actionDone)
                return;

            _actionDone = true;
            OnActionDoneEvent?.Invoke(this);
        }

        private void ActionDoneCallback()
        {
            if (_actionDone)
                return;

            _actionDone = true;

          //  InternalTools.DoActionDelayed(() =>
        //    {
                OnActionDoneEvent?.Invoke(this);
         //   }, Constants.DelayBetweenGameplayActions);
        }
    }
}
