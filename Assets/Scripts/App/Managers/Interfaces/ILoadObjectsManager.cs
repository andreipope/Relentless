using UnityEngine;

namespace LoomNetwork.CZB
{
    public interface ILoadObjectsManager
    {
        T GetObjectByPath<T>(string path)
            where T : Object;

        T[] GetObjectsByPath<T>(string path)
            where T : Object;
    }
}
