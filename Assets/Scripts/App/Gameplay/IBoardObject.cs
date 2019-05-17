namespace Loom.ZombieBattleground
{
    public interface IBoardObject
    {
    }

    public interface IOwnableBoardObject : IBoardObject
    {
        Player OwnerPlayer { get; }
    }
}
