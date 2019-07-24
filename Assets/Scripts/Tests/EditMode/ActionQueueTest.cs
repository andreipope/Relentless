using System;
using System.Linq;
using NUnit.Framework;

namespace Loom.ZombieBattleground.Test
{
    [Category("EditQuickSubset")]
    public class ActionQueueTest
    {
        [Test]
        public void Empty()
        {
            ActionQueue root = new ActionQueue(new TestActionQueueAction("Root"), null);
            Assert.AreEqual(null, root.Parent);
            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);

            root.Traverse();
            root.Traverse();
            root.Traverse();

            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);
            Assert.AreEqual(0, root.InnerQueues.Count);
            Assert.AreEqual(root, root.GetCurrentlyExecutingAction());
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

            Console.WriteLine(root.ToString(FormatTestActionQueue));
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

            Assert.AreEqual(null, root.Parent);
            Assert.AreEqual(root, action1.Parent);
            Assert.AreEqual(root, action2.Parent);
            Assert.AreEqual(action1, action1SubAction1.Parent);

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

            Assert.AreEqual(action1, root.GetCurrentlyExecutingAction());

            root.Traverse();

            Assert.False(root.InnerQueues.Contains(action1));
            Assert.False(root.InnerQueues.Contains(action2));

            Console.WriteLine(root.ToString(FormatTestActionQueue));
        }

        [Test]
        public void AddToRemovedActionMustFail()
        {
            void SetCompleted(ActionQueue queue, TestActionQueueAction action)
            {
                action.SetCompleted();
            }

            ActionQueue root = new ActionQueue(new TestActionQueueAction("Root"), null);
            ActionQueue action1 = root.Enqueue(new TestActionQueueAction("Action 1", SetCompleted));

            root.Traverse();
            Assert.False(root.InnerQueues.Contains(action1));
            Assert.Throws<ActionQueueException>(() => action1.Enqueue(new TestActionQueueAction("Action 1-1", SetCompleted)));
        }

        [Test]
        public void Basic1()
        {
            ActionQueue root = new ActionQueue(new TestActionQueueAction("Root"), null);
            ActionQueue action1 = root.Enqueue(new TestActionQueueAction("Action 1"));
            ActionQueue action2 = root.Enqueue(new TestActionQueueAction("Action 2"));
            ActionQueue action1SubAction1 = root.InnerQueues.Skip(0).First().Enqueue(new TestActionQueueAction("Action 1-1"));
            ActionQueue action1SubAction2 = root.InnerQueues.Skip(0).First().Enqueue(new TestActionQueueAction("Action 1-2"));

            Assert.AreEqual(null, root.Parent);
            Assert.AreEqual(root, action1.Parent);
            Assert.AreEqual(root, action2.Parent);
            Assert.AreEqual(action1, action1SubAction1.Parent);

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
            Assert.AreEqual(action1, root.GetCurrentlyExecutingAction());

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
            Assert.AreEqual(action1SubAction1, root.GetCurrentlyExecutingAction());

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
            Assert.AreEqual(action1SubAction2, root.GetCurrentlyExecutingAction());

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
            Assert.AreEqual(action2, root.GetCurrentlyExecutingAction());

            ((TestActionQueueAction) action2.Action).SetCompleted();
            root.Traverse();

            Assert.False(root.Action.IsStarted);
            Assert.False(root.Action.IsCompleted);
            Assert.True(action2.Action.IsStarted);
            Assert.True(action2.Action.IsCompleted);
            Assert.False(root.InnerQueues.Contains(action1));
            Assert.False(root.InnerQueues.Contains(action2));
            Assert.AreEqual(root, root.GetCurrentlyExecutingAction());

            Console.WriteLine(root.ToString(FormatTestActionQueue));
        }

        private static string FormatTestActionQueue(ActionQueue queue)
        {
            return $"{queue.Action}, Parent Queue: ({(queue.Parent == null ? "null" : ((TestActionQueueAction) queue.Parent.Action).Name)})";
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

            protected override void Action(ActionQueue queue)
            {
                _onAction?.Invoke(queue, this);
                Console.WriteLine("Executing: " + Name);
            }

            public string Name { get; }

            public override string ToString()
            {
                return $"Name: {Name}, IsStarted: {IsStarted}, IsCompleted: {IsCompleted}";
            }
        }
    }
}
