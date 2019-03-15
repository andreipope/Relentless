namespace Loom.Client
{
    /// <summary>
    /// Stores meta information about the initial call.
    /// </summary>
    public struct CallContext
    {
        public string Name { get; }
        
        public bool IsStatic { get; }

        public CallContext(string name, bool isStatic)
        {
            Name = name;
            IsStatic = isStatic;
        }
    }
}
