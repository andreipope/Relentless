// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

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
