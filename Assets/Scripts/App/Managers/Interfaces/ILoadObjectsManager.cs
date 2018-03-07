namespace GrandDevs.CZB
{
    public interface ILoadObjectsManager
    {
        T GetObjectByPath<T>(string path) where T : UnityEngine.Object;
    }
}