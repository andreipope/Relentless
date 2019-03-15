namespace Loom.Client
{
    /// <summary>
    /// Stores meta information about the initial call.
    /// </summary>
    public struct CallDescription
    {
        public string CalledMethodName { get; }
        
        public bool IsStatic { get; }

        public CallDescription(string calledMethodName, bool isStatic)
        {
            CalledMethodName = calledMethodName;
            IsStatic = isStatic;
        }
    }
}
