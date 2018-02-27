// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Object pools are useful for optimization purposes. They allow retrieving game objects from a
/// pre-allocated pool, which avoids the need to instantiate them at runtime (a potentially costly
/// operation). They are particularly useful for handling sets consisting of many small objects
/// like particle systems or audio SFX.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public GameObject Prefab;
    public int InitialSize = 16;

    private List<GameObject> instances = new List<GameObject>();

    private void Start()
    {
        for (var i = 0; i < InitialSize; i++)
        {
            var clone = CreateInstance();
            clone.transform.parent = transform;
            clone.SetActive(false);
        }
    }

    private GameObject CreateInstance()
    {
        var clone = Instantiate(Prefab, Vector3.zero, Quaternion.identity) as GameObject;
        clone.transform.parent = transform;
        instances.Add(clone);
        return clone;
    }

    public GameObject GetObject()
    {
        foreach (var instance in instances)
        {
            if (instance.activeSelf != true)
            {
                instance.SetActive(true);
                return instance;
            }
        }
        return CreateInstance();
    }
}