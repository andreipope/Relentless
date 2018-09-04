using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class LoadObjectsManager : IService, ILoadObjectsManager
    {
        private string _assetBundleName = "testbundle.test"; //move to a const? or could be helpful to load different bundles
        private string[] _assetsPaths;
        private AssetBundle _loadedBundle;

        public T GetObjectByPath<T>(string path)
            where T : Object
        {
            return LoadFromAssetBundle<T>(path);
        }

        public T[] GetObjectsByPath<T>(string[] paths)
            where T : Object
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
            _assetsPaths = AssetDatabase.FindAssets("", new[] { "Assets/Assets/LoadAtRuntime" });

            for (int i = 0; i < _assetsPaths.Length; i++)
            {
                _assetsPaths[i] = AssetDatabase.GUIDToAssetPath(_assetsPaths[i]);
            }
        }
#endif

        private void LoadAssetBundle()
        {
            _loadedBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + _assetBundleName);
        }

        private T[] LoadAllFromAssetBundle<T>(string[] paths)
            where T : Object
        {
            T[] assets = null;
            if (_loadedBundle != null)
            {
                T[] loadedAssets = new T[paths.Length];

                int count = 0;
                for (int i = 0; i < loadedAssets.Length; i++)
                {
                    string[] splitPath = paths[i].Split('/');
                    string filename = splitPath[splitPath.Length - 1];

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
            where T : Object
        {
            string[] splitPath = path.Split('/');
            string filename = splitPath[splitPath.Length - 1];

            T asset = null;
            if (_loadedBundle != null)
            {
                asset = _loadedBundle.LoadAsset<T>(filename);
            }

#if UNITY_EDITOR
            if (asset == null)
            {
                asset = LoadFromAssets<T>(path);
            }
#endif
            return asset;
        }

#if UNITY_EDITOR
        private T[] LoadAllFromAssets<T>(string[] paths)
            where T : Object
        {
            T[] loadedAssets = new T[paths.Length];

            int count = 0;
            for (int i = 0; i < loadedAssets.Length; i++)
            {
                int index = ArrayUtility.FindIndex(_assetsPaths, (x) =>
                {
                    string xLower = x.ToLower();
                    return xLower.Contains(paths[i].ToLower());
                });

                loadedAssets[count] = AssetDatabase.LoadAssetAtPath<T>(_assetsPaths[index]);

                if (loadedAssets[count] != null)
                {
                    count++;
                }
            }

            loadedAssets = loadedAssets.Where(x => x != null).ToArray();

            return loadedAssets;
        }

        private T LoadFromAssets<T>(string path)
            where T : Object
        {
            path = path.ToLower();
            int index = ArrayUtility.FindIndex(_assetsPaths, (x) =>
            {

                string xLower = x.ToLower();
                return xLower.Contains(path);
            });
            path = _assetsPaths[index];
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif
    }
}
