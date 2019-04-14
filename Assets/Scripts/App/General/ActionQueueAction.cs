using System;
using System.Threading.Tasks;

namespace Loom.ZombieBattleground {
    public abstract class ActionQueueAction
    {
        public delegate void ActionCompletedHandler(ActionQueueAction action);

        public event ActionCompletedHandler Completed;

        public bool IsStarted { get; private set; }

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

        public static ActionQueue CreateRootActionQueue()
        {
            return new ActionQueue(CreateRootActionQueueAction(), null);
        }

        public static ActionQueueAction CreateRootActionQueueAction()
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
