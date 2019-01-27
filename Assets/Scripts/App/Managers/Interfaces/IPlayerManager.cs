namespace Loom.ZombieBattleground
{
    public interface IPlayerManager
    {
        User LocalUser { get; set; }

        UniqueList<BoardUnitView> PlayerGraveyardCards { get; set; }

        UniqueList<BoardUnitView> OpponentGraveyardCards { get; set; }

        void ChangeGoo(int value);

        int GetGoo();
    }
}
