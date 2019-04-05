using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

#pragma warning disable 4014

namespace Loom.ZombieBattleground.Test
{
    public class ActionQueueTest
    {
        [Test]
        public void Empty()
        {
            ActionQueue root = new ActionQueue(new TestActionQueueAction("Root"), null);
            Assert.AreEqual(null, root.ParentQueue);
            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);

            root.Traverse();
            root.Traverse();
            root.Traverse();

            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);
            Assert.AreEqual(0, root.InnerQueues.Count);
            Assert.AreEqual(null, root.GetDeepestQueue());
        }

        [Test]
        public void DynamicAdd()
        {
            void SetCompleted(ActionQueue queue, TestActionQueueAction action)
            {
                action.SetCompleted();
            }

            ActionQueue AddAction(ActionQueue queue, ActionQueueAction currentAction, string name)
            {
                return queue.Enqueue(new TestActionQueueAction(name));
            }

            ActionQueue root = new ActionQueue(new TestActionQueueAction("Root"), null);

            ActionQueue action1 = root.Enqueue(
                new TestActionQueueAction(
                    "Action 1",
                    (queue, action) =>
                    {
                        SetCompleted(queue, action);
                        AddAction(queue, action, "Action 1-1");
                    })
                );
            root.Traverse();

            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);
            Assert.True(action1.Action.IsStarted);
            Assert.True(action1.Action.IsCompleted);
            Assert.True(root.InnerQueues.Contains(action1));

            ActionQueue action1SubAction1 = action1.InnerQueues.First();
            Assert.True(action1SubAction1.Action.IsStarted);
            Assert.False(action1SubAction1.Action.IsCompleted);

            ((TestActionQueueAction) action1SubAction1.Action).SetCompleted();
            root.Traverse();

            Assert.False(root.InnerQueues.Contains(action1));

            Console.WriteLine(FormatMultiQueue(root));
        }

        [Test]
        public void MultiComplete()
        {
            void SetCompleted(ActionQueue queue, TestActionQueueAction action)
            {
                action.SetCompleted();
            }

            ActionQueue root = new ActionQueue(new TestActionQueueAction("Root"), null);
            ActionQueue action1 = root.Enqueue(new TestActionQueueAction("Action 1", SetCompleted));
            ActionQueue action2 = root.Enqueue(new TestActionQueueAction("Action 2", SetCompleted));
            ActionQueue action1SubAction1 = root.InnerQueues.Skip(0).First().Enqueue(new TestActionQueueAction("Action 1-1", SetCompleted));
            ActionQueue action1SubAction2 = root.InnerQueues.Skip(0).First().Enqueue(new TestActionQueueAction("Action 1-2", SetCompleted));

            Assert.AreEqual(null, root.ParentQueue);
            Assert.AreEqual(root, action1.ParentQueue);
            Assert.AreEqual(root, action2.ParentQueue);
            Assert.AreEqual(action1, action1SubAction1.ParentQueue);

            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);
            Assert.False(action1.Action.IsStarted);
            Assert.False(action1.Action.IsCompleted);
            Assert.False(action1SubAction1.Action.IsStarted);
            Assert.False(action1SubAction1.Action.IsCompleted);
            Assert.False(action1SubAction2.Action.IsStarted);
            Assert.False(action1SubAction2.Action.IsCompleted);
            Assert.False(action2.Action.IsStarted);
            Assert.False(action2.Action.IsCompleted);
            Assert.True(root.InnerQueues.Contains(action1));
            Assert.True(root.InnerQueues.Contains(action2));

            Assert.AreEqual(action1, root.GetDeepestQueue());

            root.Traverse();

            Assert.False(root.InnerQueues.Contains(action1));
            Assert.False(root.InnerQueues.Contains(action2));

            Console.WriteLine(FormatMultiQueue(root));
        }

        [Test]
        public void Basic1()
        {
            ActionQueue root = new ActionQueue(new TestActionQueueAction("Root"), null);
            ActionQueue action1 = root.Enqueue(new TestActionQueueAction("Action 1"));
            ActionQueue action2 = root.Enqueue(new TestActionQueueAction("Action 2"));
            ActionQueue action1SubAction1 = root.InnerQueues.Skip(0).First().Enqueue(new TestActionQueueAction("Action 1-1"));
            ActionQueue action1SubAction2 = root.InnerQueues.Skip(0).First().Enqueue(new TestActionQueueAction("Action 1-2"));

            Assert.AreEqual(null, root.ParentQueue);
            Assert.AreEqual(root, action1.ParentQueue);
            Assert.AreEqual(root, action2.ParentQueue);
            Assert.AreEqual(action1, action1SubAction1.ParentQueue);

            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);
            Assert.False(action1.Action.IsStarted);
            Assert.False(action1.Action.IsCompleted);
            Assert.False(action1SubAction1.Action.IsStarted);
            Assert.False(action1SubAction1.Action.IsCompleted);
            Assert.False(action1SubAction2.Action.IsStarted);
            Assert.False(action1SubAction2.Action.IsCompleted);
            Assert.False(action2.Action.IsStarted);
            Assert.False(action2.Action.IsCompleted);
            Assert.True(root.InnerQueues.Contains(action1));

            root.Traverse();

            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);
            Assert.True(action1.Action.IsStarted);
            Assert.False(action1.Action.IsCompleted);
            Assert.False(action1SubAction1.Action.IsStarted);
            Assert.False(action1SubAction1.Action.IsCompleted);
            Assert.False(action1SubAction2.Action.IsStarted);
            Assert.False(action1SubAction2.Action.IsCompleted);
            Assert.False(action2.Action.IsStarted);
            Assert.False(action2.Action.IsCompleted);
            Assert.True(root.InnerQueues.Contains(action1));
            Assert.True(root.InnerQueues.Contains(action2));
            Assert.AreEqual(action1, root.GetDeepestQueue());

            ((TestActionQueueAction) action1.Action).SetCompleted();
            root.Traverse();

            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);
            Assert.True(action1.Action.IsStarted);
            Assert.True(action1.Action.IsCompleted);
            Assert.True(action1SubAction1.Action.IsStarted);
            Assert.False(action1SubAction1.Action.IsCompleted);
            Assert.False(action1SubAction2.Action.IsStarted);
            Assert.False(action1SubAction2.Action.IsCompleted);
            Assert.False(action2.Action.IsStarted);
            Assert.False(action2.Action.IsCompleted);
            Assert.True(root.InnerQueues.Contains(action1));
            Assert.AreEqual(action1SubAction1, root.GetDeepestQueue());

            ((TestActionQueueAction) action1SubAction1.Action).SetCompleted();
            root.Traverse();

            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);
            Assert.True(action1.Action.IsStarted);
            Assert.True(action1.Action.IsCompleted);
            Assert.True(action1SubAction2.Action.IsStarted);
            Assert.False(action1SubAction2.Action.IsCompleted);
            Assert.False(action2.Action.IsStarted);
            Assert.False(action2.Action.IsCompleted);
            Assert.False(action1.InnerQueues.Contains(action1SubAction1));
            Assert.True(action1.InnerQueues.Contains(action1SubAction2));
            Assert.True(root.InnerQueues.Contains(action1));
            Assert.AreEqual(action1SubAction2, root.GetDeepestQueue());

            ((TestActionQueueAction) action1SubAction2.Action).SetCompleted();
            root.Traverse();

            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);
            Assert.True(action1.Action.IsStarted);
            Assert.True(action1.Action.IsCompleted);
            Assert.True(action2.Action.IsStarted);
            Assert.False(action2.Action.IsCompleted);
            Assert.False(action1.InnerQueues.Contains(action1SubAction1));
            Assert.False(action1.InnerQueues.Contains(action1SubAction2));
            Assert.False(root.InnerQueues.Contains(action1));
            Assert.True(root.InnerQueues.Contains(action2));
            Assert.AreEqual(action2, root.GetDeepestQueue());

            ((TestActionQueueAction) action2.Action).SetCompleted();
            root.Traverse();

            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);
            Assert.True(action2.Action.IsStarted);
            Assert.True(action2.Action.IsCompleted);
            Assert.False(root.InnerQueues.Contains(action1));
            Assert.False(root.InnerQueues.Contains(action2));
            Assert.AreEqual(null, root.GetDeepestQueue());

            Console.WriteLine(FormatMultiQueue(root));
        }

        private static string FormatMultiQueue(ActionQueue queue, int depth = 0)
        {
            StringBuilder sb = new StringBuilder();
            if (depth > 0)
            {
                sb.Append(new String(' ', depth * 4));

                //sb.Append('-');
                sb.Append(" ");
            }

            sb.Append($"{queue.Action}, Parent: {(queue.ParentQueue == null ? "null" : ((TestActionQueueAction) queue.ParentQueue.Action).Name)}");
            sb.AppendLine();
            depth++;
            foreach (ActionQueue subQueue in queue.InnerQueues)
            {
                sb.Append(FormatMultiQueue(subQueue, depth));
            }

            return sb.ToString();
        }

        public class TestActionQueueAction : ActionQueueAction
        {
            private readonly Action<ActionQueue, TestActionQueueAction> _onAction;

            public TestActionQueueAction(string name, Action<ActionQueue, TestActionQueueAction> onAction = null)
            {
                Name = name;
                _onAction = onAction;
            }

            public new void SetCompleted()
            {
                Console.WriteLine("Completed: " + Name);
                base.SetCompleted();
            }

            protected override Task Action(ActionQueue queue)
            {
                _onAction?.Invoke(queue, this);
                Console.WriteLine("Executing: " + Name);
                return Task.CompletedTask;
            }

            public string Name { get; }

            public override string ToString()
            {
                return $"Name: {Name}, IsStarted: {IsStarted}, IsCompleted: {IsCompleted}";
            }
        }
    }
}
