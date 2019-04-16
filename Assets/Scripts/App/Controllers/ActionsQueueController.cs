using System;
using log4net;
using Loom.ZombieBattleground.Common;

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
        /// <param name="actionToDo">action to do, callback action</param>
        public GameplayActionQueueAction AddNewActionInToQueue(
            GameplayActionQueueAction.ExecutedActionDelegate actionToDo, Enumerators.QueueActionType actionType, bool blockQueue = false)
        {
            Log.Debug($"{nameof(AddNewActionInToQueue)}(GameplayActionQueueAction.ExecutedActionDelegate actionToDo = {(actionToDo == null ? "null" : actionToDo.ToString())}, Enumerators.QueueActionType actionType = {actionType}, bool blockQueue = {blockQueue})");
            GameplayActionQueueAction action = GenerateActionForQueue(actionToDo, actionType, IsOnlyManualCompleteAction(actionType));
            if (IsUserInputAction(actionType))
            {
                RootQueue.Enqueue(action);
            }
            else
            {
                ActionQueue deepestQueue = RootQueue.GetCurrentlyExecutingAction();
                if (deepestQueue.Action is GameplayActionQueueAction gameplayActionQueueAction)
                {
                    if (IsStrictlyChildlessAction(gameplayActionQueueAction.ActionType))
                        throw new Exception($"Attempted to add action ({action}) to a strictly childless action ({deepestQueue.Action})");
                }

                deepestQueue.Enqueue(action);
            }

            return action;
        }

        public void ForceContinueAction(GameplayActionQueueAction modelActionForDying)
        {
            Log.Debug($"{nameof(ForceContinueAction)}(GameplayActionQueueAction modelActionForDying = {modelActionForDying})");
        }

        private GameplayActionQueueAction GenerateActionForQueue(
            GameplayActionQueueAction.ExecutedActionDelegate actionToDo, Enumerators.QueueActionType actionType, bool onlyManualComplete = false)
        {
            _nextActionId++;

            if (_isDebugMode)
            {
                Log.Warn(ActionsInQueue + " was actions; add <color=yellow>generated action " +
                                            actionType + " : " + _nextActionId + "; </color> from >>>> ");
            }

            return new GameplayActionQueueAction(actionToDo, _nextActionId, actionType, onlyManualComplete);
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

        /// <summary>
        /// Returns whether the action is an action that can happen anytime, such as user action.
        /// Those actions will be enqueued directly to the root queue.
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        private static bool IsUserInputAction(Enumerators.QueueActionType actionType)
        {
            return
                actionType == Enumerators.QueueActionType.StopTurn ||
                actionType == Enumerators.QueueActionType.EndMatch ||
                actionType == Enumerators.QueueActionType.CardPlay ||
                actionType == Enumerators.QueueActionType.OverlordSkillUsage ||
                actionType == Enumerators.QueueActionType.LeaveMatch;
        }

        /// <summary>
        /// Returns whether the action can only be completed by manually triggering the completion from code.
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        private static bool IsOnlyManualCompleteAction(Enumerators.QueueActionType actionType)
        {
            return actionType == Enumerators.QueueActionType.AbilityUsageBlocker;
        }

        /// <summary>
        /// Returns whether the action can't have child actions under any circumstances.
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        private static bool IsStrictlyChildlessAction(Enumerators.QueueActionType actionType)
        {
            return
                actionType == Enumerators.QueueActionType.StopTurn ||
                actionType == Enumerators.QueueActionType.EndMatch ||
                actionType == Enumerators.QueueActionType.LeaveMatch;
        }
    }
}
