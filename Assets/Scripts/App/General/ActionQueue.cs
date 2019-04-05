using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Loom.ZombieBattleground
{
    public class ActionQueue
    {
        private readonly Queue<ActionQueue> _innerQueues = new Queue<ActionQueue>();

        public ActionQueueAction Action { get; }

        public ActionQueue ParentQueue { get; }

        public IReadOnlyCollection<ActionQueue> InnerQueues => _innerQueues;

        public ActionQueue(ActionQueueAction action, ActionQueue parentQueue)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            ParentQueue = parentQueue;
        }

        public ActionQueue Enqueue(ActionQueueAction action)
        {
            ActionQueue actionQueue = new ActionQueue(action, this);
            _innerQueues.Enqueue(actionQueue);
            return actionQueue;
        }

        public async Task Traverse()
        {
            if (_innerQueues.Count == 0)
                return;

            ActionQueue currentQueue = _innerQueues.Peek();
            while (true)
            {
                if (currentQueue == this)
                {
                    if (_innerQueues.Count == 0)
                        break;

                    currentQueue = _innerQueues.Peek();
                }

                if (!currentQueue.Action.IsStarted)
                {
                    await currentQueue.Action.Execute(currentQueue);
                }

                if (currentQueue.Action.IsCompleted)
                {
                    if (currentQueue._innerQueues.Count == 0)
                    {
                        currentQueue = currentQueue.ParentQueue;
                        currentQueue._innerQueues.Dequeue();
                    }
                    else
                    {
                        currentQueue = currentQueue._innerQueues.Peek();
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public ActionQueue GetDeepestQueue()
        {
            if (_innerQueues.Count == 0)
                return null;

            ActionQueue currentQueue = _innerQueues.Peek();
            while (true)
            {
                if (currentQueue._innerQueues.Count == 0)
                    break;

                ActionQueue nextQueue = currentQueue._innerQueues.Peek();
                if (!nextQueue.Action.IsStarted)
                    break;

                currentQueue = nextQueue;
            }

            return currentQueue;
        }
    }

}
