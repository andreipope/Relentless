namespace Loom.ZombieBattleground
{
    public abstract class BoardObject
    {
    }

    public class OwnableBoardObject : BoardObject
    {
        public Player OwnerPlayer { get; set; }
    }
}
