using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using Loom.ZombieBattleground.Editor.Tools;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground.Editor
{
    internal static class MenuItems
    {
        #region Misc

        [MenuItem("Utility/AOT/Update AOT Hint")]
        public static void GenerateZbAotHint()
        {
            AotHintFileUpdater.UpdateAotHint();
        }

        #endregion

        #region Bug Reporting

        [MenuItem("Utility/Bug Reporting/Throw Exception")]
        public static void ThrowException()
        {
            throw new Exception($"Awful exception {UnityEngine.Random.Range(10000, 99999)}!");
        }

        [MenuItem("Utility/Bug Reporting/Send Silent Report")]
        public static void SilentReport()
        {
            UserReportingScript.Instance.SummaryInput.text = "test silent report";
            UserReportingScript.Instance.CreateUserReport(
                true,
                false,
                "something awful happened somehow",
                "awful details: " + new Exception($"Awful exception {UnityEngine.Random.Range(10000, 99999)}!")
                );
        }

         #endregion

        #region Build

        [MenuItem("Utility/Build/Pre Cloud Build Test")]
        public static void PreCloudBuildExportTestRun()
        {
            CloudBuildPreBuildHandler.BuildAssetBundles();
        }

        #endregion

        #region project file hierarchy Utility

        [MenuItem("Utility/Editor/Delete Empty Folders")]
        public static void DeleteEmptyFolders()
        {
            ProcessDirectory(Directory.GetCurrentDirectory() + "/Assets");
            AssetDatabase.Refresh();
            Debug.Log("Delete Empty Folders Successful");
        }

        private static void ProcessDirectory(string startLocation)
        {
            foreach (string directory in Directory.GetDirectories(startLocation))
            {
                ProcessDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory);
                    File.Delete(directory + ".meta");
                    Debug.Log(directory + " deleted");
                }
            }
        }

        #endregion project file hierarchy Utility

        #region Auto Saving Scenes in Editor

        private static readonly int MinutesDelay = 2;

        private static bool _isStop;

        [MenuItem("Utility/AutoSaverScene/Init Auto Saving")]
        public static void Init()
        {
            Debug.Log(
                "Initialized Auto Saving! Be warning - if you hide editor, saving will stop automatically. You need to initialize auto saving again");
            _isStop = false;
            EditorCoroutine.Start(Save());
        }

        [MenuItem("Utility/AutoSaverScene/Stop Auto Saving")]
        public static void Stop()
        {
            Debug.Log("Stop Auto Saving");
            _isStop = true;
        }

        private static IEnumerator Save()
        {
            int iterations = 60 * 60 * MinutesDelay; // frames count * seconds per minute * minutes count
            for (float i = 0; i < iterations; i++)
            {
                yield return null;
            }

            if (!_isStop)
            {
                Debug.Log("Start Auto Save");
                if (EditorSceneManager.SaveOpenScenes())
                {
                    Debug.Log("All Opened scenes was saved successfull!");
                }
                else
                {
                    Debug.Log("Saving opened scenes failed");
                }

                EditorCoroutine.Start(Save());
            }
        }

        private class EditorCoroutine
        {
            private readonly IEnumerator _routine;

            private EditorCoroutine(IEnumerator routine)
            {
                _routine = routine;
            }

            public static EditorCoroutine Start(IEnumerator routine)
            {
                EditorCoroutine coroutine = new EditorCoroutine(routine);
                coroutine.Start();
                return coroutine;
            }

            public void Stop()
            {
                EditorApplication.update -= Update;
            }

            private void Start()
            {
                EditorApplication.update += Update;
            }

            private void Update()
            {
                if (!_routine.MoveNext())
                {
                    Stop();
                }
            }
        }

        #endregion Auto Saving Scenes in Editor

        #region cached data, player prefs, and data in persistent data path

        [MenuItem("Utility/Data/Delete PlayerPrefs")]
        public static void DeletePlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Delete Player Prefs Successful");
        }

        [MenuItem("Utility/Data/Clean Game Data (LocalLow)")]
        public static void CleanGameData()
        {
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/");
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.FullName == Logging.GetLogFilePath())
                    continue;

                file.Delete();
            }

            DirectoryInfo[] directories = dir.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                directory.Delete(true);
            }

            Debug.Log("Clean Game Data Successful");
        }

        [MenuItem("Utility/Data/Open Local Low Folder")]
        public static void OpenLocalLowFolder()
        {
            Process.Start(Application.persistentDataPath);
        }

        #endregion cached data, player prefs, and data in persistent data path

        #region asset bundles and cache

        [MenuItem("Utility/CacheAndBundles/Clean Cache")]
        public static void ClearCache()
        {
            if (Caching.ClearCache())
            {
                Debug.Log("Clean Cache Successful");
            }
            else
            {
                Debug.Log("Clean Cache Failed");
            }
        }

        [MenuItem("Utility/Build/Build AssetBundles + Game")]
        public static void BuildAssetBundlesAndGame()
        {
            BuildAssetBundlesAndGame(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("Utility/Build/Build Game")]
        public static void BuildGame()
        {
            BuildGame(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("Utility/Build/Build Asset Bundles")]
        public static void BuildAssetBundles()
        {
            BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("Utility/Build/Force DynamicLoad assets to 'main' AssetBundle")]
        public static void CheckDynamicLoadAssetBundle()
        {
            List<AssetImporter> modified = AssetBundleFolderValidator.ForceAssetBundleInFolder("Assets/Assets/DynamicLoad", "main");
            Debug.Log("Assets forced to 'main' bundle:\n" + String.Join("\n", modified.Select(a => a.assetPath)));
        }

        private static void BuildAssetBundlesAndGame(BuildTarget buildTarget)
        {
            BuildAssetBundles(buildTarget);
            BuildGame(EditorUserBuildSettings.activeBuildTarget);
        }

        private static void BuildGame(BuildTarget buildTarget)
        {
            string outputPath = Path.Combine("Builds", buildTarget.ToString());

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            outputPath = Path.Combine(outputPath, PlayerSettings.productName);
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    outputPath += ".exe";
                    break;
                case BuildTarget.Android:
                    outputPath += ".apk";
                    break;
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Select((scene, i) => scene.path).ToArray(),
                locationPathName = outputPath,
                target = buildTarget,
                targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget),
                options = BuildOptions.None
            };
            BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (buildReport.summary.result != BuildResult.Succeeded)
                throw new Exception("build failed");
        }

        private static void BuildAssetBundles(BuildTarget buildTarget)
        {
            string outputPath = GetAssetBundlesBuildPath(buildTarget);
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            BuildPipeline.BuildAssetBundles(
                outputPath,
                EditorUtility.GetBuildAssetBundleOptions(buildTarget),
                buildTarget);


            string assetBundleStreamingRoot = Utilites.GetAssetBundleLocalRoot();
            if (!Directory.Exists(assetBundleStreamingRoot))
            {
                Directory.CreateDirectory(assetBundleStreamingRoot);
            }

            // Delete existing StreamingAssets bundles
            string[] existingBundles = Directory.GetFiles(assetBundleStreamingRoot);
            foreach (string existingBundle in existingBundles)
            {
                if (Path.GetFileName(existingBundle) == ".gitkeep")
                    continue;

                File.Delete(existingBundle);
            }

            // Copy to StreamingAssets
            string[] files = Directory.GetFiles(outputPath);
            if (!Directory.Exists(assetBundleStreamingRoot))
            {
                Directory.CreateDirectory(assetBundleStreamingRoot);
            }

            foreach (string file in files)
            {
                string outPath = Path.Combine(assetBundleStreamingRoot, Path.GetFileName(file));
                File.Copy(file, outPath);
            }

            AssetDatabase.Refresh();
        }

        private static string GetAssetBundlesBuildPath(BuildTarget buildTarget)
        {
            return Path.Combine("AssetBundles", buildTarget.ToString());
        }

        #endregion asset bundles and cache

        #region scene hierarchy Utility

        [MenuItem("Utility/Editor/Select GOs With Missing Scripts")]
        public static void SelectMissing(MenuCommand command)
        {
            Transform[] ts = Object.FindObjectsOfType<Transform>();
            List<GameObject> selection = new List<GameObject>();
            foreach (Transform t in ts)
            {
                Component[] cs = t.gameObject.GetComponents<Component>();
                foreach (Component c in cs)
                {
                    if (c == null)
                    {
                        selection.Add(t.gameObject);
                    }
                }
            }

            Selection.objects = selection.ToArray();
        }

        [MenuItem("Utility/Editor/Apply Scale To Rect Transform")]
        public static void ApplyScaleToRectTransforms()
        {
            RectTransform[] rectTransforms = Selection.transforms.OfType<RectTransform>().ToArray();
            Undo.RecordObjects(rectTransforms, "Apply Scale To Rect Transform");
            foreach (RectTransform rectTransform in rectTransforms)
            {
                Vector2 sizeDelta = rectTransform.sizeDelta;
                sizeDelta.x *= rectTransform.localScale.x;
                sizeDelta.y *= rectTransform.localScale.y;
                rectTransform.sizeDelta = sizeDelta;

                rectTransform.localScale = Vector3.one;
            }
        }

        [MenuItem("Utility/Editor/Convert child SpriteRenderers to UI Image")]
        public static void ConvertSpriteRenderersToImage()
        {
            if (Selection.activeGameObject == null)
                return;

            Undo.RecordObject(Selection.activeGameObject, "Convert child SpriteRenderers to UI Image");
            SpriteRenderer[] spriteRenderers = Selection.activeGameObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                Image image = spriteRenderer.gameObject.AddComponent<Image>();
                image.sprite = spriteRenderer.sprite;
                image.raycastTarget = false;

                SpriteMask spriteMask = spriteRenderer.GetComponent<SpriteMask>();
                if (spriteMask != null)
                {
                    spriteRenderer.gameObject.AddComponent<Mask>();
                    Object.DestroyImmediate(spriteMask);
                }

                Object.DestroyImmediate(spriteRenderer);
            }
        }

        [MenuItem("Utility/Editor/Convert child Transform To RectTransform")]
        public static void ConvertTransformToRectTransform()
        {
            if (Selection.activeGameObject == null)
                return;

            Undo.RecordObject(Selection.activeGameObject, "Convert child Transform To RectTransform");
            Transform[] transforms = Selection.activeGameObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform transform in transforms)
            {
                if (transform is RectTransform)
                    continue;

                UnityEditor.EditorUtility.SetDirty(transform.gameObject);
                transform.gameObject.AddComponent<RectTransform>();
                Object.DestroyImmediate(transform);
            }

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                UnityEditor.EditorUtility.SetDirty(prefabStage.prefabContentsRoot);
            }
        }

        #endregion scene hierarchy Utility
    }
}
