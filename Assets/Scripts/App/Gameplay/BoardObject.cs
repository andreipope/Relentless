namespace Loom.ZombieBattleground
{
    public abstract class BoardObject
    {
    }

    public class OwnableBoardObject : BoardObject
    {
        public virtual Player OwnerPlayer { get; }
    }
}
