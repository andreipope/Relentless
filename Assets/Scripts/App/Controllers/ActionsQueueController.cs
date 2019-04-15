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
        public GameplayActionQueueAction AddNewActionInToQueue(
            GameplayActionQueueAction.ExecutedActionDelegate actionToDo, Enumerators.QueueActionType actionType, bool blockQueue = false)
        {
            GameplayActionQueueAction action = GenerateActionForQueue(actionToDo, actionType, IsOnlyManualCompleteAction(actionType));
            if (IsUserInputAction(actionType))
            {
                RootQueue.Enqueue(action);
            }
            else
            {
                ActionQueue deepestQueue = RootQueue.GetDeepestQueue();
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
            // does nothing now?
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

        private static bool IsUserInputAction(Enumerators.QueueActionType actionType)
        {
            return
                actionType == Enumerators.QueueActionType.StopTurn ||
                actionType == Enumerators.QueueActionType.EndMatch ||
                actionType == Enumerators.QueueActionType.CardPlay ||
                actionType == Enumerators.QueueActionType.OverlordSkillUsage ||
                actionType == Enumerators.QueueActionType.LeaveMatch;
        }

        private static bool IsOnlyManualCompleteAction(Enumerators.QueueActionType actionType)
        {
            return actionType == Enumerators.QueueActionType.AbilityUsageBlocker;
        }

        private static bool IsStrictlyChildlessAction(Enumerators.QueueActionType actionType)
        {
            return
                actionType == Enumerators.QueueActionType.StopTurn ||
                actionType == Enumerators.QueueActionType.EndMatch ||
                actionType == Enumerators.QueueActionType.LeaveMatch;
        }
    }
}
