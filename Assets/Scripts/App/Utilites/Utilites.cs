using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
#endif

namespace Loom.ZombieBattleground
{
    public static class Utilites
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #region scene hierarchy utilites

#if UNITY_EDITOR
        [MenuItem("Utilites/Editor/Select GOs With Missing Scripts")]
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
#endif

        #endregion scene hierarchy utilites

        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public static T CastStringTuEnum<T>(string data)
        {
            return (T) Enum.Parse(typeof(T), data.ToUpper());
        }

        public static List<T> CastList<T>(string data, char separator = '|')
        {
            List<T> list = new List<T>();
            string[] targets = data.Split(separator);
            foreach (string target in targets)
            {
                list.Add(CastStringTuEnum<T>(target));
            }

            return list;
        }

        public static Vector3 CastVfxPosition(Vector3 position)
        {
            return new Vector3(position.x, position.z, position.y);
        }

        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static long GetCurrentUnixTimestampMillis()
        {
            DateTime localDateTime = DateTime.Now;
            DateTime universalDateTime = localDateTime.ToUniversalTime();
            return (long) (universalDateTime - UnixEpoch).TotalMilliseconds;
        }

        #region asset bundles and cache

        public static string GetAssetBundleLocalRoot()
        {
            return Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        }

        public static string GetAssetBundleLocalPath(string assetBundleName)
        {
            return Path.Combine(GetAssetBundleLocalRoot(), assetBundleName);
        }

#if UNITY_EDITOR

        [MenuItem("Utilites/CacheAndBundles/Clean Cache")]
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

        [MenuItem("Utilites/Build/Build AssetBundles + Game")]
        public static void BuildAssetBundlesAndGame()
        {
            BuildAssetBundlesAndGame(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("Utilites/Build/Build Game")]
        public static void BuildGame()
        {
            BuildGame(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("Utilites/Build/Build Asset Bundles")]
        public static void BuildAssetBundles()
        {
            BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        }

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
                GetBuildAssetBundleOptions(buildTarget),
                buildTarget);

            // Delete existing StreamingAssets bundles
            string assetBundleStreamingRoot = GetAssetBundleLocalRoot();
            string[] existingBundles = Directory.GetFiles(assetBundleStreamingRoot);
            foreach (string existingBundle in existingBundles)
            {
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
#endif

        #endregion asset bundles and cache

        #region cached data, player prefs, and data in persistent data path

#if UNITY_EDITOR
        [MenuItem("Utilites/Data/Delete PlayerPrefs")]
        public static void DeletePlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Delete Player Prefs Successful");
        }

        [MenuItem("Utilites/Data/Clean Game Data (LocalLow)")]
        public static void CleanGameData()
        {
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/");
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                file.Delete();
            }

            DirectoryInfo[] directories = dir.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                directory.Delete(true);
            }

            Debug.Log("Clean Game Data Successful");
        }

        [MenuItem("Utilites/Data/Open Local Low Folder")]
        public static void OpenLocalLowFolder()
        {
            Process.Start(Application.persistentDataPath);
        }

#endif

        #endregion cached data, player prefs, and data in persistent data path

        #region project file hierarchy utilites

#if UNITY_EDITOR
        [MenuItem("Utilites/Editor/Delete Empty Folders")]
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

#endif

        #endregion project file hierarchy utilites

        #region Auto Saving Scenes in Editor

#if UNITY_EDITOR

        private static readonly int MinutesDelay = 2;

        private static bool _isStop;

        [MenuItem("Utilites/AutoSaverScene/Init Auto Saving")]
        public static void Init()
        {
            Debug.Log(
                "Initialized Auto Saving! Be warning - if you hide editor, saving will stop automatically. You need to initialize auto saving again");
            _isStop = false;
            EditorCoroutine.Start(Save());
        }

        [MenuItem("Utilites/AutoSaverScene/Stop Auto Saving")]
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

#endif

        #endregion Auto Saving Scenes in Editor

        #region cryptography

        public static string Encrypt(string value, string key)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(value), key));
        }

        [DebuggerNonUserCode]
        public static string Decrypt(string value, string key)
        {
            string result;

            try
            {
                using (CryptoStream cryptoStream = InternalDecrypt(Convert.FromBase64String(value), key))
                {
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        result = streamReader.ReadToEnd();
                    }
                }
            }
            catch (CryptographicException)
            {
                return null;
            }

            return result;
        }

        private static byte[] Encrypt(byte[] key, string value)
        {
            SymmetricAlgorithm symmetricAlgorithm = Rijndael.Create();
            ICryptoTransform cryptoTransform =
                symmetricAlgorithm.CreateEncryptor(new Rfc2898DeriveBytes(value, new byte[16]).GetBytes(16), new byte[16]);

            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream =
                    new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(key, 0, key.Length);
                    cryptoStream.FlushFinalBlock();

                    result = memoryStream.ToArray();

                    memoryStream.Close();
                    memoryStream.Dispose();
                }
            }

            return result;
        }

        private static CryptoStream InternalDecrypt(byte[] key, string value)
        {
            SymmetricAlgorithm symmetricAlgorithm = Rijndael.Create();
            ICryptoTransform cryptoTransform =
                symmetricAlgorithm.CreateDecryptor(new Rfc2898DeriveBytes(value, new byte[16]).GetBytes(16),
                    new byte[16]);

            MemoryStream memoryStream = new MemoryStream(key);
            return new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
        }

        #endregion cryptography

    }
}
