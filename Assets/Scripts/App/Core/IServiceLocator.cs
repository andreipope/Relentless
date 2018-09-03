namespace LoomNetwork.CZB
{
    public interface IServiceLocator
    {
        T GetService<T>();

        void Update();
    }
}
