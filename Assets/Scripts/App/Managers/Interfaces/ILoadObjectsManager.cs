namespace GrandDevs.CZB
{
    public interface ILoadObjectsManager
    {
        T GetObjectByPath<T>(string path) where T : UnityEngine.Object;
        T[] GetObjectsByPath<T>(string path) where T : UnityEngine.Object;       
    }
}