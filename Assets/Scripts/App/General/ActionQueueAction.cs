using System;
using System.Threading.Tasks;

namespace Loom.ZombieBattleground {
    public abstract class ActionQueueAction
    {
        public Task Execute(ActionQueue queue)
        {
            IsStarted = true;
            return Action(queue);
        }

        protected abstract Task Action(ActionQueue queue);

        protected void SetCompleted()
        {
            if (!IsStarted)
                throw new InvalidOperationException("Can't set state to completed before starting");

            IsCompleted = true;
        }

        public bool IsStarted { get; private set; }

        public bool IsCompleted { get; private set; }
    }
}