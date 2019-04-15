using System;
using log4net;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground {
    public class GameplayActionQueueAction : ActionQueueAction
    {
        private static readonly ILog Log = Logging.GetLog(nameof(GameplayActionQueueAction));

        public delegate void ExecutedActionDelegate(Action completedCallback);

        public ExecutedActionDelegate ExecutedAction { get; }

        public bool ManualCompleteTriggered { get; private set; }

        public Enumerators.QueueActionType ActionType { get; }

        public long Id { get; }

        public bool OnlyManualComplete { get; }

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
