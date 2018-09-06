namespace Loom.ZombieBattleground
{
    public interface IServiceLocator
    {
        T GetService<T>();

        void Update();
    }
}
