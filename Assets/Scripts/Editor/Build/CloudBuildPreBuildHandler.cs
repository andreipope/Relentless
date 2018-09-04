using System.IO;
using UnityEditor;

namespace Loom.ZombieBattleground
{
    public static class CloudBuildPreBuildHandler
    {
        public static void PreCloudBuildExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
        {
            BuildMetaInfoGenerator.PreCloudBuildExport(manifest);

            BuildAssetBundles();
        }

        private static void BuildAssetBundles()
        {
            string outputPath = Utilites.GetAssetBundleLocalRoot();
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

            BuildPipeline.BuildAssetBundles(
                outputPath,
                Utilites.GetBuildAssetBundleOptions(),
                buildTarget);
        }
    }
}
