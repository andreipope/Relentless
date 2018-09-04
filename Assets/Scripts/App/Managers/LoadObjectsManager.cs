using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class LoadObjectsManager : IService, ILoadObjectsManager
    {
        private string[] _assetBundleNames = new string[] { "data", "testbundle.test" };
        private AssetBundle _loadedBundle;

#if UNITY_EDITOR
        private Dictionary<string, string> _assetsPaths;
#endif

        public T GetObjectByPath<T>(string path)
            where T : UnityEngine.Object
        {
            return LoadFromAssetBundle<T>(path);
        }

        public T[] GetObjectsByPath<T>(string[] paths)
            where T : UnityEngine.Object
        {
            return LoadAllFromAssetBundle<T>(paths);
        }

        public void Dispose()
        {
        }

        public void Init()
        {
#if UNITY_EDITOR
            PrepareAssetPaths();
#endif
            LoadAssetBundle();
        }

        public void Update()
        {
        }

#if UNITY_EDITOR
        private void PrepareAssetPaths()
        {
            _assetsPaths = new Dictionary<string, string>();

            string[] assetsPaths = AssetDatabase.FindAssets("", new[] { "Assets/Assets/LoadAtRuntime" });

            for (int i = 0; i < assetsPaths.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetsPaths[i]);
                string filename = Path.GetFileNameWithoutExtension(path).ToLower();

                Debug.Log("Load");
                Debug.Log(path);
                Debug.Log(filename);
                if (!_assetsPaths.ContainsKey(filename))
                {
                    _assetsPaths.Add(Path.GetFileNameWithoutExtension(path).ToLower(), path);
                }
            }
        }
#endif

        private void LoadAssetBundle()
        {
            _loadedBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + _assetBundleNames[0]); //need to refactor once we decide on a loading system of multiple asset bundles, for now, hard coded to use the first string in the array
        }

        private T[] LoadAllFromAssetBundle<T>(string[] paths)
            where T : UnityEngine.Object
        {
            T[] assets = null;
            if (_loadedBundle != null)
            {
                T[] loadedAssets = new T[paths.Length];

                int count = 0;
                for (int i = 0; i < loadedAssets.Length; i++)
                {
                    string filename = Path.GetFileName(paths[i]);

                    loadedAssets[count] = _loadedBundle.LoadAsset<T>(filename);

                    if (loadedAssets[count] != null)
                    {
                        count++;
                    }
                }

                assets = loadedAssets;
            }

#if UNITY_EDITOR
            if (assets == null)
            {
                assets = LoadAllFromAssets<T>(paths);
            }
#endif
            return assets;
        }

        private T LoadFromAssetBundle<T>(string path)
            where T : UnityEngine.Object
        {
            string filename = Path.GetFileName(path);

            T asset = null;
            if (_loadedBundle != null)
            {
                asset = _loadedBundle.LoadAsset<T>(filename);
            }

#if UNITY_EDITOR
            if (asset == null)
            {
                asset = LoadFromAssets<T>(filename);
            }
#endif
            return asset;
        }

#if UNITY_EDITOR
        private T[] LoadAllFromAssets<T>(string[] paths)
            where T : UnityEngine.Object
        {
            T[] loadedAssets = new T[paths.Length];

            int count = 0;
            for (int i = 0; i < loadedAssets.Length; i++)
            {
                loadedAssets[count] = AssetDatabase.LoadAssetAtPath<T>(_assetsPaths[Path.GetFileName(paths[i]).ToLower()]);

                if (loadedAssets[count] != null)
                {
                    count++;
                }
            }

            loadedAssets = loadedAssets.Where(x => x != null).ToArray();

            return loadedAssets;
        }

        private T LoadFromAssets<T>(string filename)
            where T : UnityEngine.Object
        {
            filename = filename.ToLower();
            Debug.Log(filename);
            string path = _assetsPaths[filename];
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif
    }
}
