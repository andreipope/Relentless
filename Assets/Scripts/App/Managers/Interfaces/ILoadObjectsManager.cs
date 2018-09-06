using System;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public interface ILoadObjectsManager
    {
        T GetObjectByPath<T>(string path, string bundleName = null)
            where T : Object;

        T[] GetObjectsByPath<T>(string[] path, string bundleName = null)
            where T : Object;

        void LoadAssetBundleFromFile(string name);

        Task LoadAssetBundleFromFileAsync(string name, IProgress<float> progress = null);
    }
}
