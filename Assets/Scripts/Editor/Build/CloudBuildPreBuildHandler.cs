using System.IO;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    public static class CloudBuildPreBuildHandler
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
#if ALTUNITYTESTER
               var altUnityRunner =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("AltUnityRunnerPrefab")[0]));
            var PreviousScenePath = SceneManager.GetActiveScene().path;
            var SceneWithAltUnityRunner = EditorSceneManager.OpenScene("Assets/Scenes/APP_INIT.unity");
            var AltUnityRunner = PrefabUtility.InstantiatePrefab(altUnityRunner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
#endif
        }
    }
}
