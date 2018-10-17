using System.IO;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    internal static class CloudBuildPreBuildHandler
    {
        public static void PreCloudBuildExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
        {
            Debug.Log($"{nameof(PreCloudBuildExport)} invoked");
            BuildMetaInfoGenerator.PreCloudBuildExport(manifest);

            BuildAssetBundles();
        }

        public static void BuildAssetBundles()
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
                EditorUtility.GetBuildAssetBundleOptions(buildTarget) |
                BuildAssetBundleOptions.ForceRebuildAssetBundle,
                buildTarget
            );
        }
    }
}
