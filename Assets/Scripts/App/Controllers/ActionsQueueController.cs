using System;
using System.Collections.Generic;

namespace GrandDevs.CZB
{
    public class ActionsQueueController : IController
    {
        private Queue<GameAction<object>> _actionsToDo;
        private GameAction<object> _actionInProgress;

        public ActionsQueueController()
        {
            _actionsToDo = new Queue<GameAction<object>>();
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public void AddNewActionInToQueue(Action<object> actionToDo, object parameter)
        {
            GameAction<object> gameAction = new GameAction<object>(actionToDo, parameter);
            gameAction.OnActionDoneEvent += OnActionDoneEvent;
            _actionsToDo.Enqueue(gameAction);

            if (_actionInProgress == null)
                TryCallNewActionFromQueue();
        }

        public void StopAllActions()
        {
            _actionsToDo.Clear();
        }

        private void OnActionDoneEvent(GameAction<object> previousAction)
        {
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

        public GameAction(Action<T> action, T parameter)
        {
            this.action = action;
            this.parameter = parameter;
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
}
