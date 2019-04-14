using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ActionsQueueController : IController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(ActionsQueueController));

        private long _nextActionId;

        private ActionQueue _rootQueue;

        private bool _isDebugMode = false;

        public int ActionsInQueue => _rootQueue.InnerQueues.Count;

        public ActionQueue RootQueue => _rootQueue;

        public void Init()
        {
            _rootQueue = ActionQueueAction.CreateRootActionQueue();
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            _rootQueue.Traverse();
        }

        public void ResetAll()
        {
            StopAllActions();
            _nextActionId = 0;
        }

        /// <summary>
        ///     AddNewActionInToQueue
        /// </summary>
        /// <param name="actionToDo">action to do, parameter + callback action</param>
        /// <param name="parameter">parameters for action if ot needs</param>
        /// <param name="report">report that will be added into reports list</param>
        public GameplayActionQueueAction<object> AddNewActionInToQueue(
            GameplayActionQueueAction<object>.ExecutedActionDelegate actionToDo, Enumerators.QueueActionType actionType, object parameter = null, bool blockQueue = false)
        {
            bool onlyManualComplete = actionType == Enumerators.QueueActionType.AbilityUsageBlocker;
            bool isUserInputAction =
                actionType == Enumerators.QueueActionType.StopTurn ||
                actionType == Enumerators.QueueActionType.EndMatch ||
                actionType == Enumerators.QueueActionType.CardPlay ||
                actionType == Enumerators.QueueActionType.OverlordSkillUsage ||
                actionType == Enumerators.QueueActionType.LeaveMatch;

            GameplayActionQueueAction<object> gameAction = GenerateActionForQueue(actionToDo, actionType, parameter, blockQueue, onlyManualComplete);
            if (isUserInputAction)
            {
                RootQueue.Enqueue(gameAction);
            }
            else
            {
                RootQueue.GetDeepestQueue().Enqueue(gameAction);
            }

            return gameAction;
        }

        private GameplayActionQueueAction<object> GenerateActionForQueue(
            GameplayActionQueueAction<object>.ExecutedActionDelegate actionToDo, Enumerators.QueueActionType actionType, object parameter = null, bool blockQueue = false, bool onlyManualComplete = false)
        {
            _nextActionId++;

            if (_isDebugMode)
            {
                Log.Warn(ActionsInQueue + " was actions; add <color=yellow>generated action " +
                                            actionType + " : " + _nextActionId + "; </color> from >>>> ");
            }

            return new GameplayActionQueueAction<object>(actionToDo, parameter, _nextActionId, actionType, blockQueue, onlyManualComplete);
        }

        private void StopAllActions()
        {
            ClearActions();
        }

        public void ClearActions()
        {
            if (_isDebugMode)
            {
                Log.Warn(ActionsInQueue + " was actions; <color=black>clear whole list of actions;</color> from >>>> ");
            }

            _rootQueue = ActionQueueAction.CreateRootActionQueue();
        }

        public void ForceContinueAction(GameplayActionQueueAction<object> modelActionForDying)
        {
            // does nothing now?
        }
    }

    public class GameplayActionQueueAction<T> : ActionQueueAction
    {
        private static readonly ILog Log = Logging.GetLog(nameof(GameplayActionQueueAction<T>));

        private bool _actionDone;

        public delegate void ExecutedActionDelegate(T parameter, Action completedCallback);

        public ExecutedActionDelegate ExecutedAction;

        public T Parameter;

        public bool ManualCompleteTriggered { get; private set; }

        public Enumerators.QueueActionType ActionType { get; }

        public long Id { get; }

        public bool BlockedInQueue { get; }

        public bool OnlyManualComplete { get; }

        public GameplayActionQueueAction(
            ExecutedActionDelegate executedAction,
            T parameter,
            long id,
            Enumerators.QueueActionType actionType,
            bool blockQueue,
            bool onlyManualComplete)
        {
            ExecutedAction = executedAction;
            Parameter = parameter;
            Id = id;
            ActionType = actionType;
            BlockedInQueue = blockQueue;
            OnlyManualComplete = onlyManualComplete;
        }

        protected override void Action(ActionQueue queue)
        {
            DebugLog($"{nameof(Action)}, Parent Queue: ({queue.Parent})");
            if (!OnlyManualComplete || OnlyManualComplete && ManualCompleteTriggered)
            {
                ExecuteAction();
            }
        }

        private void ExecuteAction()
        {
            DebugLog(nameof(ExecutedAction));
            try
            {
                if (ExecutedAction == null)
                {
                    SetCompleted();
                }
                else
                {
                    ExecutedAction?.Invoke(Parameter, SetCompleted);
                }
            }
            catch (Exception ex)
            {
                ActionSystemException actionSystemException =
                    new ActionSystemException($"[ACTION SYSTEM ISSUE REPORTER]: <color=red>Action {ActionType} with id {Id} got error;</color>", ex);
                Log.Error(actionSystemException.ToString());
                Helpers.ExceptionReporter.SilentReportException(actionSystemException);

                SetCompleted();
                throw actionSystemException;
            }
        }

        public override string ToString()
        {
            return
                $"{nameof(ActionType)}: {ActionType}, " +
                $"{nameof(IsStarted)}: {IsStarted}, " +
                $"{nameof(IsCompleted)}: {IsCompleted}, " +
                $"{nameof(OnlyManualComplete)}: {OnlyManualComplete}, " +
                $"{nameof(ManualCompleteTriggered)}: {ManualCompleteTriggered}, " +
                $"{nameof(Parameter)}: {(Parameter == null ? "null" : Parameter.ToString())}, " +
                $"{nameof(Id)}: {Id}, " +
                $"{nameof(BlockedInQueue)}: {BlockedInQueue}";
        }

        public void ForceActionDone()
        {
            DebugLog(nameof(ForceActionDone));
            if (IsCompleted || ManualCompleteTriggered)
                return;

            if (IsStarted)
            {
                ExecuteAction();
            }
            else
            {
                ManualCompleteTriggered = true;
            }
        }

        private void DebugLog(string text)
        {
            Log.Debug($"{text} ({this})");
        }
    }
}
