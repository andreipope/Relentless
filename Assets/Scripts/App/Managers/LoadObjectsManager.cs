//#define DISABLE_EDITOR_ASSET_BUNDLE_SIMULATION

using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.Common;
#if UNITY_EDITOR && !DISABLE_EDITOR_ASSET_BUNDLE_SIMULATION
using System.Linq;
using UnityEditor;
#endif

namespace Loom.ZombieBattleground
{
    public class LoadObjectsManager : IService, ILoadObjectsManager
    {
        private static readonly ILog Log = Logging.GetLog(nameof(LoadObjectsManager));

        private readonly Dictionary<string, AssetBundle> _loadedAssetBundles = new Dictionary<string, AssetBundle>();
#if UNITY_EDITOR && !DISABLE_EDITOR_ASSET_BUNDLE_SIMULATION
        private const string DynamicLoadAssetsRoot = "Assets/Assets/DynamicLoad";
        private Dictionary<string, string> _assetsPaths;
#endif

        public void Dispose()
        {
        }

        public void Init()
        {
#if UNITY_EDITOR && !DISABLE_EDITOR_ASSET_BUNDLE_SIMULATION
            PrepareAssetPaths();
#endif
        }

        public void Update()
        {
        }

        public T GetObjectByPath<T>(string path, string bundleName = null)
            where T : UnityEngine.Object
        {
            AssetBundle assetBundle = GetAssetBundle(ref bundleName);

            string fileName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            T asset = Load<T>(fileName, assetBundle, bundleName);
            if (asset == null)
                Log.Warn($"Failed to load '{path}' from bundle '{bundleName}'");

            return asset;
        }

        public T[] GetObjectsByPath<T>(string[] paths, string bundleName = null)
            where T : UnityEngine.Object
        {
            AssetBundle assetBundle = GetAssetBundle(ref bundleName);

            T[] assets = new T[paths.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                string fileName = Path.GetFileNameWithoutExtension(paths[i]).ToLowerInvariant();
                assets[i] = Load<T>(fileName, assetBundle, bundleName);
                if (assets[i] == null)
                    Log.Warn($"Failed to load '{paths[i]}' from bundle '{bundleName}'");
            }

            return assets;
        }

        public void LoadAssetBundleFromFile(string name)
        {
            string bundleLocalPath = Utilites.GetAssetBundleLocalPath(name);
#if UNITY_EDITOR && !DISABLE_EDITOR_ASSET_BUNDLE_SIMULATION
            if (!File.Exists(bundleLocalPath))
                return;
#endif

            Stopwatch stopwatch = Stopwatch.StartNew();

            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundleLocalPath);
            if (assetBundle == null)
                throw new Exception($"Failed to load asset bundle '{name}'");

            _loadedAssetBundles.Add(name, assetBundle);

            stopwatch.Stop();
            Log.Info($"Loading '{name}' bundle took {stopwatch.ElapsedMilliseconds} ms");
        }

        public async Task LoadAssetBundleFromFileAsync(string name, IProgress<float> progress = null)
        {
            string bundleLocalPath = Utilites.GetAssetBundleLocalPath(name);
#if UNITY_EDITOR && !DISABLE_EDITOR_ASSET_BUNDLE_SIMULATION
            if (!File.Exists(bundleLocalPath))
                return;
#endif

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundleLocalPath);
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            request.completed += _ => tcs.TrySetResult(true);
            await tcs.Task;

            if (request.assetBundle == null)
                throw new Exception($"Failed to load asset bundle '{name}'");

            _loadedAssetBundles.Add(name, request.assetBundle);
        }

#if UNITY_EDITOR && !DISABLE_EDITOR_ASSET_BUNDLE_SIMULATION
        private void PrepareAssetPaths()
        {
            _assetsPaths = new Dictionary<string, string>();

            string[] assetsPaths =
                AssetDatabase
                    .FindAssets("", new[] { DynamicLoadAssetsRoot })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where((s, i) => !AssetDatabase.IsValidFolder(s))
                    .ToArray();

            for (int i = 0; i < assetsPaths.Length; i++)
            {
                string path = assetsPaths[i];
                string fileName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();

                if (_assetsPaths.ContainsKey(fileName))
                {
                    string conflictingPath = _assetsPaths[fileName];
                    if (conflictingPath != path)
                    {
                        throw new Exception($"Conflicting asset names:\n{conflictingPath}\n{path}");
                    }
                }
                else
                {
                    _assetsPaths.Add(fileName, path);
                }
            }
        }
#endif

        private AssetBundle GetAssetBundle(ref string bundleName)
        {
            if (String.IsNullOrEmpty(bundleName))
            {
                bundleName = Constants.AssetBundleMain;
            }

            bool bundleExists = _loadedAssetBundles.TryGetValue(bundleName, out AssetBundle assetBundle);
#if !(UNITY_EDITOR && !DISABLE_EDITOR_ASSET_BUNDLE_SIMULATION)
            if (!bundleExists)
                throw new Exception($"Asset bundle '{bundleName}' not loaded");
#endif

            return assetBundle;
        }

        private T Load<T>(string fileName, AssetBundle assetBundle, string bundleName)
            where T : UnityEngine.Object
        {
#if !(UNITY_EDITOR && SKIP_ASSET_BUNDLE_LOAD)
            if (assetBundle != null)
                return assetBundle.LoadAsset<T>(fileName);
#endif

#if UNITY_EDITOR && !DISABLE_EDITOR_ASSET_BUNDLE_SIMULATION
            fileName = fileName.ToLowerInvariant();

            if (_assetsPaths.TryGetValue(fileName, out string path))
                return AssetDatabase.LoadAssetAtPath<T>(path);
#endif

            return null;
        }


    }
}
