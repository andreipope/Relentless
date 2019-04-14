using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Loom.ZombieBattleground
{
    public class ActionQueue
    {
        private readonly Queue<ActionQueue> _innerQueues = new Queue<ActionQueue>();

        public ActionQueueAction Action { get; }

        public ActionQueue Parent { get; }

        public IReadOnlyCollection<ActionQueue> InnerQueues => _innerQueues;

        public ActionQueue(ActionQueueAction action, ActionQueue parent)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Parent = parent;
        }

        public ActionQueue Enqueue(ActionQueueAction action)
        {
            ActionQueue actionQueue = new ActionQueue(action, this);
            _innerQueues.Enqueue(actionQueue);
            return actionQueue;
        }

        public void Traverse()
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
                    currentQueue.Action.Execute(currentQueue);
                }

                if (currentQueue.Action.IsCompleted)
                {
                    if (currentQueue._innerQueues.Count == 0)
                    {
                        currentQueue = currentQueue.Parent;
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
                return this;

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

        public override string ToString()
        {
            return ToString(DefaultFormat);
        }

        public virtual string ToString(Func<ActionQueue, string> actionFormatFunc)
        {
            if (actionFormatFunc == null)
                throw new ArgumentNullException(nameof(actionFormatFunc));

            StringBuilder stringBuilder = new StringBuilder();
            FormatActionQueue(this, 0, actionFormatFunc, stringBuilder);
            return stringBuilder.ToString();
        }

        private static string DefaultFormat(ActionQueue queue)
        {
            return $"Action: {queue.Action}, Parent: ({(queue.Parent == null ? "null" : queue.Parent.Action.ToString())})";
        }

        private static void FormatActionQueue(ActionQueue queue, int depth, Func<ActionQueue, string> actionFormatFunc, StringBuilder stringBuilder)
        {
            if (depth > 0)
            {
                stringBuilder.Append(new String(' ', depth * 4));
            }

            stringBuilder.Append(actionFormatFunc?.Invoke(queue) ?? queue.ToString());
            stringBuilder.AppendLine();
            depth++;
            foreach (ActionQueue subQueue in queue.InnerQueues)
            {
                FormatActionQueue(subQueue, depth, actionFormatFunc, stringBuilder);
            }
        }
    }
}
