using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;

namespace GrandDevs.CZB
{
    public class ActionsQueueController : IController
    {
        public event Action<GameActionReport> GotNewActionReportEvent;
        public event Action<List<GameActionReport>> ActionsReportsUpdatedEvent;

        private Queue<GameAction<object>> _actionsToDo;
        private List<GameActionReport> _actionsReports;

        private GameAction<object> _actionInProgress;

        public List<GameActionReport> ActionsReports
        {
            get
            {
                return _actionsReports;
            }
        }

        public ActionsQueueController()
        {
            _actionsToDo = new Queue<GameAction<object>>();
            _actionsReports = new List<GameActionReport>();
            _actionInProgress = null;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        /// <summary>
        /// AddNewActionInToQueue
        /// </summary>
        /// <param name="actionToDo">action to do</param>
        /// <param name="parameter">parameters for action if ot needs</param>
        /// <param name="report">report that will be added into reports list</param>
        public void AddNewActionInToQueue(Action<object> actionToDo, object parameter, GameActionReport report = null)
        {
            GameAction<object> gameAction = new GameAction<object>(actionToDo, parameter, report);
            gameAction.OnActionDoneEvent += OnActionDoneEvent;
            _actionsToDo.Enqueue(gameAction);

            if (_actionInProgress == null)
                TryCallNewActionFromQueue();
        }

        public void StopAllActions()
        {
            _actionsToDo.Clear();
            _actionInProgress = null;
        }

        //todo improve I guess
        public GameActionReport FormatGameActionReport(Enumerators.ActionType actionType, object[] parameters)
        {
            GameActionReport actionReport = new GameActionReport(actionType, parameters);

            return actionReport;
        }

        private void OnActionDoneEvent(GameAction<object> previousAction)
        {
            _actionsReports.Add(previousAction.report);

            GotNewActionReportEvent?.Invoke(previousAction.report);
            ActionsReportsUpdatedEvent?.Invoke(_actionsReports);

            TryCallNewActionFromQueue();
        }

        private void TryCallNewActionFromQueue()
        {
            if (_actionsToDo.Count > 0)
            {
                _actionInProgress = _actionsToDo.Dequeue();
                _actionInProgress.DoAction();
            }
            else _actionInProgress = null;
        }
    }

    public class GameAction<T>
    {
        public event Action<GameAction<T>> OnActionDoneEvent;

        private Action _localAction;

        public Action<T> action;
        public T parameter;
        public GameActionReport report;

        public GameAction(Action<T> action, T parameter, GameActionReport report)
        {
            this.action = action;
            this.parameter = parameter;
            this.report = report;
        }

        public void DoAction()
        {
            _localAction = () =>
            {
                action?.Invoke(parameter);
                OnActionDoneEvent?.Invoke(this);
            };
        }
    }

    public class GameActionReport
    {
        public Enumerators.ActionType actionType;
        public object[] parameters;

        public GameActionReport(Enumerators.ActionType actionType, object[] parameters)
        {
            this.actionType = actionType;
            this.parameters = parameters;
        }
    }
}
