using System;
using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class ActionsQueueController : IController
    {
        private Queue<GameAction<object>> _actionsToDo;

        private GameAction<object> _actionInProgress;

        public event Action<PastActionsPopup.PastActionParam> GotNewActionReportEvent;

        public List<PastActionsPopup.PastActionParam> ActionsReports { get; private set; }

        public void Init()
        {
            _actionsToDo = new Queue<GameAction<object>>();
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
        }

        /// <summary>
        ///     AddNewActionInToQueue
        /// </summary>
        /// <param name="actionToDo">action to do, parameter + callback action</param>
        /// <param name="parameter">parameters for action if ot needs</param>
        /// <param name="report">report that will be added into reports list</param>
        public void AddNewActionInToQueue(
            Action<object, Action> actionToDo, object parameter = null, PastActionsPopup.PastActionParam report = null)
        {
            GameAction<object> gameAction = new GameAction<object>(actionToDo, parameter, report);
            gameAction.OnActionDoneEvent += OnActionDoneEvent;
            _actionsToDo.Enqueue(gameAction);

            if (_actionInProgress == null && _actionsToDo.Count < 2)
            {
                TryCallNewActionFromQueue();
            }
        }

        public void StopAllActions()
        {
            _actionsToDo.Clear();
            _actionInProgress = null;
        }

        public void PostGameActionReport(PastActionsPopup.PastActionParam report)
        {
            if (report != null)
            {
                ActionsReports.Add(report);
                GotNewActionReportEvent?.Invoke(report);
            }
        }

        private void OnActionDoneEvent(GameAction<object> previousAction)
        {
            PostGameActionReport(previousAction.Report);

            TryCallNewActionFromQueue();
        }

        private void TryCallNewActionFromQueue()
        {
            if (_actionsToDo.Count > 0)
            {
                _actionInProgress = _actionsToDo.Dequeue();
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

        public PastActionsPopup.PastActionParam Report;

        private readonly ITimerManager _timerManager;

        private bool _actionDone;

        public GameAction(Action<T, Action> action, T parameter, PastActionsPopup.PastActionParam report)
        {
            _timerManager = GameClient.Get<ITimerManager>();

            Action = action;
            Parameter = parameter;
            Report = report;
        }

        public event Action<GameAction<T>> OnActionDoneEvent;

        public void DoAction()
        {
            try
            {
                Action?.Invoke(Parameter, ActionDoneCallback);
            }
            catch (Exception)
            {
                if (!_actionDone)
                {
                    ActionDoneCallback();
                }
            }
        }

        private void ActionDoneCallback()
        {
            _actionDone = true;

            InternalTools.DoActionDelayed(() =>
            {
                OnActionDoneEvent?.Invoke(this);
            }, Constants.DelayBetweenGameplayActions);
        }
    }

    public class GameActionReport
    {
        public Enumerators.ActionType ActionType;

        public object[] Parameters;

        public GameActionReport(Enumerators.ActionType actionType, object[] parameters)
        {
            ActionType = actionType;
            Parameters = parameters;
        }
    }
}
