using System;

namespace Loom.ZombieBattleground
{
    public class ActionQueueException : Exception
    {
        public ActionQueueException() { }
        public ActionQueueException(string message) : base(message) { }
        public ActionQueueException(string message, Exception innerException) : base(message, innerException) { }
    }
}
