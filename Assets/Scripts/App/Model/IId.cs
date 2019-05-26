namespace Loom.ZombieBattleground
{
    public interface IId<out T> where T : struct
    {
        T Id { get; }
    }
}
