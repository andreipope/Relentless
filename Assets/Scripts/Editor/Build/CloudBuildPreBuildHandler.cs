using System.IO;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public static class CloudBuildPreBuildHandler
    {
        [MenuItem("Utilites/Build/Pre Cloud Build Test")]
        public static void PreCloudBuildExportTestRun()
        {
            BuildAssetBundles();
        }

        public static void PreCloudBuildExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
        {
            Debug.Log($"{nameof(PreCloudBuildExport)} invoked");
            BuildMetaInfoGenerator.PreCloudBuildExport(manifest);

            BuildAssetBundles();
        }

        private static void BuildAssetBundles()
        {
            string outputPath = Utilites.GetAssetBundleLocalRoot();
            Debug.Log($"Building asset bundles, path: {outputPath}");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

            BuildPipeline.BuildAssetBundles(
                outputPath,
                Utilites.GetBuildAssetBundleOptions(buildTarget),
                buildTarget);
        }
    }
}
