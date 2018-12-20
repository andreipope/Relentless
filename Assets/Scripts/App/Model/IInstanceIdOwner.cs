namespace Loom.ZombieBattleground.Data
{
    /// <summary>
    /// Represents an object that has an <see cref="InstanceId"/>
    /// that uniquely identifies it in a match.
    /// </summary>
    public interface IInstanceIdOwner
    {
        InstanceId InstanceId { get; }
    }
}
