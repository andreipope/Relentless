using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Loom.ZombieBattleground
{
    /// <summary>
    /// Implements a nested action queue. Each action is a queue in itself, that way, if during the action execution, new actions are added,
    /// they can be added to the parent queue, maintaining correct of execution.
    /// </summary>
    public class ActionQueue
    {
        private readonly Queue<ActionQueue> _innerQueues = new Queue<ActionQueue>();

        public ActionQueueAction Action { get; }

        public ActionQueue Parent { get; }

        public IReadOnlyCollection<ActionQueue> InnerQueues => _innerQueues;

        /// <summary>
        /// Invoked when the state of the queue or any of the child queue is changed.
        /// </summary>
        public event Action<ActionQueue> Changed;

        public ActionQueue(ActionQueueAction action, ActionQueue parent)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Parent = parent;
        }

        public ActionQueue Enqueue(ActionQueueAction action)
        {
            ActionQueue actionQueue = new ActionQueue(action, this);
            _innerQueues.Enqueue(actionQueue);
            InvokeStateChanged();
            return actionQueue;
        }

        /// <summary>
        /// Traverses the queue hierarchy, starts action execution, cleans up completed actions.
        /// </summary>
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
                    InvokeStateChanged();
                }

                if (currentQueue.Action.IsCompleted)
                {
                    if (currentQueue._innerQueues.Count == 0)
                    {
                        currentQueue = currentQueue.Parent;
                        currentQueue._innerQueues.Dequeue();
                        InvokeStateChanged();
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

        /// <summary>
        /// Gets the action that is currently being executed.
        /// </summary>
        public ActionQueue GetCurrentlyExecutingAction()
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

        private void InvokeStateChanged()
        {
            Changed?.Invoke(this);
            Parent?.InvokeStateChanged();
        }

        private static string DefaultFormat(ActionQueue queue)
        {
            return $"Action: {queue.Action}, Parent: ({(queue.Parent == null ? "null" : queue.Parent.Action.ToString())})";
        }

        private static void FormatActionQueue(ActionQueue queue, int depth, Func<ActionQueue, string> actionFormatFunc, StringBuilder stringBuilder)
        {
            if (depth > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append(new String(' ', depth * 4));
            }

            stringBuilder.Append(actionFormatFunc?.Invoke(queue) ?? queue.ToString());
            depth++;
            foreach (ActionQueue subQueue in queue.InnerQueues)
            {
                FormatActionQueue(subQueue, depth, actionFormatFunc, stringBuilder);
            }
        }
    }
}
