namespace Loom.ZombieBattleground
{
    public interface IPlayerManager
    {
        User LocalUser { get; set; }
        void ChangeGoo(int value);

        int GetGoo();
    }
}
