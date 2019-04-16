using System;
using log4net;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground {
    /// <summary>
    /// A game-specific variant of <see cref="ActionQueueAction"/>, for quick migration from the old system.
    /// </summary>
    public class GameplayActionQueueAction : ActionQueueAction
    {
        private static readonly ILog Log = Logging.GetLog(nameof(GameplayActionQueueAction));

        /// <summary>
        /// Delegate for the executed action.
        /// </summary>
        /// <param name="completedCallback">The callback that must be called by the delegate to mark the action as completed.</param>
        public delegate void ExecutedActionDelegate(Action completedCallback);

        /// <summary>
        /// Delegate to be executed when action is started.
        /// </summary>
        public ExecutedActionDelegate ExecutedAction { get; }

        /// <summary>
        /// Whether the action can only be completed by calling <see cref="TriggerActionManually"/>.
        /// </summary>
        public bool OnlyManualComplete { get; }

        /// <summary>
        /// Whether <see cref="TriggerActionManually"/> was already called on this action, and the action will be completed ASAP.
        /// </summary>
        public bool ManualCompleteTriggered { get; private set; }

        /// <summary>
        /// Information-only action type.
        /// </summary>
        public Enumerators.QueueActionType ActionType { get; }

        /// <summary>
        /// Incrementing ID. Only used as a debugging aid.
        /// </summary>
        public long Id { get; }

        public GameplayActionQueueAction(
            ExecutedActionDelegate executedAction,
            long id,
            Enumerators.QueueActionType actionType,
            bool onlyManualComplete)
        {
            ExecutedAction = executedAction;
            Id = id;
            ActionType = actionType;
            OnlyManualComplete = onlyManualComplete;
        }

        /// <summary>
        /// Schedules the action for to be executed, unconditionally.
        /// If the action is already started, will execute the action.
        /// If the action isn't already started, will schedule for it to be executed immediately after start.
        /// This a big hack and potentially dangerous.
        /// </summary>
        public void TriggerActionManually()
        {
            DebugLog(nameof(TriggerActionManually));
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

        public override string ToString()
        {
            return
                $"{nameof(ActionType)}: {ActionType}, " +
                $"{nameof(IsStarted)}: {IsStarted}, " +
                $"{nameof(IsCompleted)}: {IsCompleted}, " +
                $"{nameof(OnlyManualComplete)}: {OnlyManualComplete}, " +
                $"{nameof(ManualCompleteTriggered)}: {ManualCompleteTriggered}, " +
                $"{nameof(Id)}: {Id}";
        }

        protected override void Action(ActionQueue queue)
        {
            DebugLog($"{nameof(Action)}, Parent Queue: ({queue.Parent})");

            // Don't execute manually completed actions automatically, those will wait for ForceCompleteAction()
            if (!OnlyManualComplete || OnlyManualComplete && ManualCompleteTriggered)
            {
                ExecuteAction();
            }
        }

        private void ExecuteAction()
        {
            DebugLog(nameof(ExecuteAction));
            try
            {
                if (ExecutedAction == null)
                {
                    SetCompleted();
                }
                else
                {
                    ExecutedAction?.Invoke(SetCompleted);
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

        private void DebugLog(string text)
        {
            Log.Debug($"{text} ({this})");
        }
    }
}
