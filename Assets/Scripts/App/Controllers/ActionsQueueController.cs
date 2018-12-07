using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ActionsQueueController : IController
    {
        public event Action<PastActionsPopup.PastActionParam> GotNewActionReportEvent;

        private long _nextActionId = 0;

        private List<GameplayQueueAction<object>> _actionsToDo;

        private GameplayQueueAction<object> _actionInProgress;

        private bool _isDebugMode = false;

        public List<PastActionsPopup.PastActionParam> ActionsReports { get; private set; }

        public int ActionsInQueue { get { return _actionsToDo.Count + (_actionInProgress == null ? 0 : 1); } }

        public void Init()
        {
            _actionsToDo = new List<GameplayQueueAction<object>>();
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

        public void PostGameActionReport(PastActionsPopup.PastActionParam report)
        {
            if (report != null)
            {
                ActionsReports.Add(report);
                GotNewActionReportEvent?.Invoke(report);
            }
        }

        /// <summary>
        ///     AddNewActionInToQueue
        /// </summary>
        /// <param name="actionToDo">action to do, parameter + callback action</param>
        /// <param name="parameter">parameters for action if ot needs</param>
        /// <param name="report">report that will be added into reports list</param>
        public GameplayQueueAction<object> AddNewActionInToQueue(
            Action<object, Action> actionToDo, Enumerators.QueueActionType actionType, object parameter = null, bool blockQueue = false)
        {
            _nextActionId++;

            if (_isDebugMode)
            {
                UnityEngine.Debug.LogError(_actionsToDo.Count + " was actions; add <color=yellow>new action " +
                                            actionType + " : " + _nextActionId + "; </color> from >>>> ");
            }

            GameplayQueueAction<object> gameAction = new GameplayQueueAction<object>(actionToDo, parameter, _nextActionId, actionType, blockQueue);
            gameAction.OnActionDoneEvent += OnActionDoneEvent;
            _actionsToDo.Add(gameAction);

            if (_actionInProgress == null && _actionsToDo.Count < 2)
            {
                TryCallNewActionFromQueue();
            }

            return gameAction;
        }

        public void InsertActionAfterAction(GameplayQueueAction<object> actionToInsert, GameplayQueueAction<object> actionInsertAfter)
        {
            if (_actionsToDo.Contains(actionToInsert))
            {
                _actionsToDo.Remove(actionToInsert);
            }

            int position = _actionsToDo.IndexOf(actionInsertAfter);

            _actionsToDo.Insert(Mathf.Clamp(position, 0, _actionsToDo.Count), actionToInsert);
        }

        public void StopAllActions()
        {
            ClearActions();
        }

        public void ClearActions()
        {
            _actionsToDo.Clear();
            _actionInProgress = null;
        }

        public void ForceContinueAction(GameplayQueueAction<object> action)
        {
            if (action == null)
            {
                TryCallNewActionFromQueue();
            }
            else
            {
                if (_isDebugMode)
                {
                    UnityEngine.Debug.LogError(_actionsToDo.Count + " was actions; action <color=orange>" +
                    action.ActionType + " : " + action.Id + " force block disable and try run </color> from >>>> ");
                }

                action.BlockQueue = false;

                if (_actionsToDo.Count > 0)
                {
                    if (_actionsToDo.GetRange(0, 1)[0] == action && _actionInProgress == null)
                    {
                        TryCallNewActionFromQueue(true);
                    }
                }
            }
        }

        private void OnActionDoneEvent(GameplayQueueAction<object> previousAction)
        {
            if (_isDebugMode)
            {
                UnityEngine.Debug.LogError(_actionsToDo.Count + " was actions; action <color=cyan>" +
                previousAction.ActionType + " : " + previousAction.Id + " DONE </color> from >>>> ");
            }

            if (_actionsToDo.Contains(previousAction))
            {
                if(_actionInProgress == previousAction)
                {
                    TryCallNewActionFromQueue();
                }

                _actionsToDo.Remove(previousAction);
            }
            else
            {
                TryCallNewActionFromQueue();
            }
        }

        private void TryCallNewActionFromQueue(bool ignoreBlock = false)
        {
            if (_actionsToDo.Count > 0)
            {
                _actionInProgress = _actionsToDo.GetRange(0, 1)[0];

                if (_actionInProgress.BlockQueue && !ignoreBlock)
                {
                    _actionInProgress = null;
                    return;
                }

                _actionsToDo.Remove(_actionInProgress);

                if (_isDebugMode)
                {
                    UnityEngine.Debug.LogError(_actionsToDo.Count + " was actions; <color=white> Dooooooo action " +
                    _actionInProgress.ActionType + " : " + _actionInProgress.Id + ";  </color> from >>>> ");
                }

                _actionInProgress.DoAction();
            }
            else
            {
                _actionInProgress = null;
            }
        }
    }

    public class GameplayQueueAction<T>
    {
        private readonly ITimerManager _timerManager;

        private bool _actionDone;

        public Action<T, Action> Action;

        public T Parameter;

        public Enumerators.QueueActionType ActionType { get; }

        public long Id { get; }

        public bool BlockQueue { get; set; }

        public GameplayQueueAction(Action<T, Action> action, T parameter, long id, Enumerators.QueueActionType actionType, bool blockQueue)
        {
            _timerManager = GameClient.Get<ITimerManager>();

            Action = action;
            Parameter = parameter;
            Id = id;
            ActionType = actionType;
            BlockQueue = blockQueue;
        }

        public event Action<GameplayQueueAction<T>> OnActionDoneEvent;

        public void DoAction()
        {
            try
            {
                if (Action == null)
                {
                    ActionDoneCallback();
                }
                else
                {
                    Action?.Invoke(Parameter, ActionDoneCallback);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"<color=red>Action {ActionType} with id {Id} got error;</color> \n {ex.Message} ; {ex.StackTrace}");

                ActionDoneCallback();
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

            OnActionDoneEvent?.Invoke(this);
        }
    }
}
