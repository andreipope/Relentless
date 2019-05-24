using System;

namespace Loom.ZombieBattleground {
    /// <summary>
    /// Represents a completable action that can be put into a queue.
    /// </summary>
    public abstract class ActionQueueAction
    {
        public delegate void ActionCompletedHandler(ActionQueueAction action);

        public event ActionCompletedHandler Completed;

        public bool IsStarted { get; private set; }

        /// <summary>
        /// Whether the action is over and can be removed from the queue.
        /// Executing any logic after this is set to true is considered undefined behavior.
        /// </summary>
        public bool IsCompleted { get; private set; }

        public void Execute(ActionQueue queue)
        {
            IsStarted = true;
            Action(queue);
        }

        protected abstract void Action(ActionQueue queue);

        protected void SetCompleted()
        {
            if (!IsStarted)
                throw new InvalidOperationException($"Can't set state to completed before starting ({ToString()})");

            bool oldIsCompleted = IsCompleted;
            IsCompleted = true;

            if (!oldIsCompleted)
            {
                Completed?.Invoke(this);
            }
        }

        /// <summary>
        /// Creates an action that can be used as a root action queue.
        /// </summary>
        /// <returns></returns>
        public static ActionQueue CreateRootActionQueue()
        {
            return new ActionQueue(CreateRootActionQueueAction(), null);
        }

        private static ActionQueueAction CreateRootActionQueueAction()
        {
            return new RootActionQueueAction();
        }

        private class RootActionQueueAction : ActionQueueAction
        {
            protected override void Action(ActionQueue queue)
            {
            }

            public override string ToString()
            {
                return "Root Action";
            }
        }
    }
}
