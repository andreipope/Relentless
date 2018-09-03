using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class LoadObjectsManager : IService, ILoadObjectsManager
    {
        private readonly bool _loadFromResources = true;
        private string _assetBundleName = "testbundle.test"; //move to a const? or could be helpful to load different bundles?
        private AssetBundle _loadedBundle;

        public T GetObjectByPath<T>(string path)
            where T : Object
        {
            if (_loadFromResources)
            {
                return LoadFromAssetBundle<T>(path);
            }

            return LoadFromAssetBundle<T>(path); // ToDo change into other load type
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
            LoadAssetBundle();
        }

        public void Update()
        {
        }

        private void LoadAssetBundle()
        {
            _loadedBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + _assetBundleName);
        }

        private T LoadFromAssetBundle<T>(string path)
            where T : Object
        {
            if (_loadedBundle == null) {
                LoadAssetBundle();
            }

            string[] splitPath = path.Split('/');
            string filename = splitPath[splitPath.Length - 1];
            T asset = _loadedBundle.LoadAsset<T>(filename);
            if (asset == null) {
                asset = LoadFromResources<T>(path);
            }
            return asset;
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
