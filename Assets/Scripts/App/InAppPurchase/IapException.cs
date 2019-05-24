using System;

namespace Loom.ZombieBattleground
{
    public class IapException : Exception
    {
        public IapException() { }
        public IapException(string message) : base(message) { }
        public IapException(string message, Exception innerException) : base(message, innerException) { }
    }
}
