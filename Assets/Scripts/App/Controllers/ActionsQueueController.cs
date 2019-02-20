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

        private bool _isDebugMode = false;

        public List<PastActionsPopup.PastActionParam> ActionsReports { get; private set; }

        public int ActionsInQueue { get { return _actionsToDo.Count + (ActionInProgress == null ? 0 : 1); } }

        public GameplayQueueAction<object> ActionInProgress { get; private set; }

        private List<PastActionsPopup.PastActionParam> _bufferActionsReports;

        public void Init()
        {
            _actionsToDo = new List<GameplayQueueAction<object>>();
            ActionsReports = new List<PastActionsPopup.PastActionParam>();
            _bufferActionsReports = new List<PastActionsPopup.PastActionParam>();
            ActionInProgress = null;
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
            _bufferActionsReports.Clear();

            _nextActionId = 0;
        }

        public void PostGameActionReport(PastActionsPopup.PastActionParam report)
        {
            if (report != null)
            {
                if (report.checkForCardOwner && !ActionsReports.Exists(x => x.workingCard == report.workingCard))
                {
                    _bufferActionsReports.Add(report);
                }
                else
                {
                    AddNewPostGameActionReport(report, !report.checkForCardOwner);
                }
            }
        }

        private void AddNewPostGameActionReport(PastActionsPopup.PastActionParam report, bool checkBuffer = false)
        {
            ActionsReports.Add(report);
            GotNewActionReportEvent?.Invoke(report);
            if(checkBuffer)
            {
                CheckReportsInBuffer(report);
            }
        }

        private void CheckReportsInBuffer(PastActionsPopup.PastActionParam report)
        {
            foreach (PastActionsPopup.PastActionParam sortingReport in _bufferActionsReports)
            {
                ActionsReports.Add(sortingReport);
                GotNewActionReportEvent?.Invoke(sortingReport);
            }
            _bufferActionsReports.Clear();
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
            GameplayQueueAction<object> gameAction = GenerateActionForQueue(actionToDo, actionType, parameter, blockQueue);
            gameAction.OnActionDoneEvent += OnActionDoneEvent;
            _actionsToDo.Add(gameAction);

            if (ActionInProgress == null && _actionsToDo.Count < 2)
            {
                TryCallNewActionFromQueue();
            }

            return gameAction;
        }

        public GameplayQueueAction<object> GenerateActionForQueue(
            Action<object, Action> actionToDo, Enumerators.QueueActionType actionType, object parameter = null, bool blockQueue = false)
        {
            _nextActionId++;

            if (_isDebugMode)
            {
                UnityEngine.Debug.LogWarning(_actionsToDo.Count + " was actions; add <color=yellow>generated action " +
                                            actionType + " : " + _nextActionId + "; </color> from >>>> ");
            }

            return new GameplayQueueAction<object>(actionToDo, parameter, _nextActionId, actionType, blockQueue);
        }

        public void MoveActionAfterAction(GameplayQueueAction<object> actionToInsert, GameplayQueueAction<object> actionInsertAfter)
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
            if (_isDebugMode)
            {
                UnityEngine.Debug.LogWarning(_actionsToDo.Count + " was actions; <color=black>clear whole list of actions;</color> from >>>> ");
            }

            if (ActionInProgress != null)
            {
                ActionInProgress.Action = null;
                ActionInProgress.ForceActionDone();
                ActionInProgress = null;
            }

            _actionsToDo.Clear();
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
                    UnityEngine.Debug.LogWarning(_actionsToDo.Count + " was actions; action <color=orange>" +
                    action.ActionType + " : " + action.Id + " force block disable and try run </color> from >>>> ");
                }

                action.BlockedInQueue = false;

                if (ActionInProgress == null)
                {
                    TryCallNewActionFromQueue();
                }
            }
        }

        private void OnActionDoneEvent(GameplayQueueAction<object> previousAction)
        {
            if (_isDebugMode)
            {
                UnityEngine.Debug.LogWarning(_actionsToDo.Count + " was actions; action <color=cyan>" +
                previousAction.ActionType + " : " + previousAction.Id + " DONE </color> from >>>> ");
            }

            if (_actionsToDo.Contains(previousAction))
            {
                _actionsToDo.Remove(previousAction);

                if (ActionInProgress == previousAction || ActionInProgress == null)
                {
                    TryCallNewActionFromQueue();
                }
                else
                {
                    ActionInProgress = null;
                }
            }
            else
            {
                TryCallNewActionFromQueue();
            }
        }

        private void TryCallNewActionFromQueue()
        {
            if (_actionsToDo.Count > 0)
            {
                GameplayQueueAction<object> actionToStart = _actionsToDo[0];

                if (actionToStart.BlockedInQueue)
                {
                    if (_isDebugMode)
                    {
                        UnityEngine.Debug.LogWarning(_actionsToDo.Count + " was actions; <color=brown> action blocked " +
                        actionToStart.ActionType + " : " + actionToStart.Id + ";  </color> from >>>> ");
                    }

                    ActionInProgress = null;

                    return;
                }

                _actionsToDo.Remove(actionToStart);

                if (_isDebugMode)
                {
                    UnityEngine.Debug.LogWarning(_actionsToDo.Count + " was actions; <color=white> Dooooooo action " +
                    actionToStart.ActionType + " : " + actionToStart.Id + ";  </color> from >>>> ");
                }

                ActionInProgress = actionToStart;
                ActionInProgress.DoAction();
            }
            else
            {
                ActionInProgress = null;
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

        public bool BlockedInQueue { get; set; }

        public GameplayQueueAction(Action<T, Action> action, T parameter, long id, Enumerators.QueueActionType actionType, bool blockQueue)
        {
            _timerManager = GameClient.Get<ITimerManager>();

            Action = action;
            Parameter = parameter;
            Id = id;
            ActionType = actionType;
            BlockedInQueue = blockQueue;
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
                ActionSystemException actionSystemException = new ActionSystemException($"[ACTION SYSTEM ISSUE REPORTER]: <color=red>Action {ActionType} with id {Id} got error;</color>", ex);
                Debug.LogException(actionSystemException);
                Helpers.ExceptionReporter.LogException(actionSystemException);

                ActionDoneCallback();
                throw actionSystemException;
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
