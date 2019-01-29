using System;

namespace Loom.ZombieBattleground
{
    public class ActionSystemException : Exception
    {
        public ActionSystemException()
        {
        }

        public ActionSystemException(string message) : base(message)
        {
        }

        public ActionSystemException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
