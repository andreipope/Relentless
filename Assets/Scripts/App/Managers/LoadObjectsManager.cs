// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using UnityEngine;

namespace LoomNetwork.CZB
{
    public class LoadObjectsManager : IService, ILoadObjectsManager
    {
        private readonly bool _loadFromResources = true;

        public T GetObjectByPath<T>(string path)
            where T : Object
        {
            if (_loadFromResources)
            {
                return LoadFromResources<T>(path);
            }

            return LoadFromResources<T>(path); // ToDo change into other load type
        }

        public T[] GetObjectsByPath<T>(string path)
            where T : Object
        {
            if (_loadFromResources)
            {
                return LoadAllFromResources<T>(path);
            }

            return LoadAllFromResources<T>(path); // ToDo change into other load type
        }

        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        private T LoadFromResources<T>(string path)
            where T : Object
        {
            return Resources.Load<T>(path);
        }

        private T[] LoadAllFromResources<T>(string path)
            where T : Object
        {
            return Resources.LoadAll<T>(path);
        }
    }
}
