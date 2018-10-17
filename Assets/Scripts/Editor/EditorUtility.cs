using UnityEditor;

namespace Loom.ZombieBattleground.Editor
{
    public static class EditorUtility
    {
        public static BuildAssetBundleOptions GetBuildAssetBundleOptions(BuildTarget buildTarget)
        {
            BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
            switch (buildTarget)
            {
                default:
                    options |= BuildAssetBundleOptions.ChunkBasedCompression;
                    break;
            }
            return options;
        }
    }
}