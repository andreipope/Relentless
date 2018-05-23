using System.IO;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class LoadObjectsManager : IService, ILoadObjectsManager
    {
        private bool _loadFromResources = true;

        public void Dispose()
        {
          
        }

        public void Init()
        {

        }

        public void Update()
        {
            
        }


        public T GetObjectByPath<T>(string path) where T : UnityEngine.Object
        {
            if(_loadFromResources)
                return LoadFromResources<T>(path);
            else
                return LoadFromResources<T>(path); // ToDo change into other load type
        }

        private T LoadFromResources<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }

        public T[] GetObjectsByPath<T>(string path) where T : UnityEngine.Object
        {
            if (_loadFromResources)
                return LoadAllFromResources<T>(path);
            else
                return LoadAllFromResources<T>(path); // ToDo change into other load type
        }

        private T[] LoadAllFromResources<T>(string path) where T : UnityEngine.Object
        {
            return Resources.LoadAll<T>(path);
        }
    }
}