#if UNITY_EDITOR
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace LoomNetwork.Internal
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
            return (T)Enum.Parse(typeof(T), data.ToUpper());
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

        public static string FirstCharToUpper(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                input = input.First().ToString().ToUpper() + input.Substring(1);
            }

            return input;
        }

        public static string GetStringFromByteArray(byte[] byteArr)
        {
            return Convert.ToBase64String(byteArr);
        }

        public static byte[] GetByteArrFromString(string str)
        {
            return string.IsNullOrEmpty(str)?null:Convert.FromBase64String(str);
        }

        public static T CreateFromJson<T>(string jsonString)
        {
            return JsonUtility.FromJson<T>(jsonString);
        }

        public static string SaveToString(object obj)
        {
            return JsonUtility.ToJson(obj);
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
            return (long)(universalDateTime - UnixEpoch).TotalMilliseconds;
        }

        #region asset bundles and cache

#if UNITY_EDITOR

        [MenuItem("Utilites/CacheAndBundles/Clean Cache")]
        public static void ClearCache()
        {
            if (Caching.ClearCache())
            {
                DebugLog("Clean Cache Successful");
            }
            else
            {
                DebugLog("Clean Cache Failed");
            }
        }

        [MenuItem("Utilites/Build/Build AssetBundles + Game")]
        public static void BuildAssetBundlesAndGame()
        {
            BuildAssetBundlesAndGame(EditorUserBuildSettings.activeBuildTarget);
        }

        private static void BuildAssetBundlesAndGame(BuildTarget buildTarget)
        {
            BuildAssetBundles(buildTarget);
            BuildGame(EditorUserBuildSettings.activeBuildTarget);

            // Delete existing StreamingAssets bundles
            string assetBundleStreamingRoot = Application.streamingAssetsPath + "/AssetBundles";
            string[] existingBundles = Directory.GetFiles(assetBundleStreamingRoot);
            foreach (string existingBundle in existingBundles)
            {
                File.Delete(existingBundle);
            }

            // Copy to StreamingAssets
            string[] files = Directory.GetFiles(GetAssetBundlesBuildPath(buildTarget));
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

        [MenuItem("Utilites/Build/Build Asset Bundles")]
        public static void BuildAssetBundles()
        {
            BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
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
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = EditorBuildSettings.scenes.Select((scene, i) => scene.path).ToArray();
            buildPlayerOptions.locationPathName = outputPath;
            buildPlayerOptions.target = buildTarget;
            buildPlayerOptions.targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
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

            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.UncompressedAssetBundle, buildTarget);
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
            DebugLog("Delete Player Prefs Successful");
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

            files = null;

            DirectoryInfo[] directories = dir.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                directory.Delete(true);
            }

            directories = null;

            DebugLog("Clean Game Data Successful");
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
            DebugLog("Delete Empty Folders Successful");
        }

        private static void ProcessDirectory(string startLocation)
        {
            foreach (string directory in Directory.GetDirectories(startLocation))
            {
                ProcessDirectory(directory);
                if ((Directory.GetFiles(directory).Length == 0) && (Directory.GetDirectories(directory).Length == 0))
                {
                    Directory.Delete(directory);
                    File.Delete(directory + ".meta");
                    DebugLog(directory + " deleted");
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
            DebugLog("Initialized Auto Saving! Be warning - if you hide editor, saving will stop automatically. You need to initialize auto saving again");
            _isStop = false;
            EditorCoroutine.Start(Save());
        }

        [MenuItem("Utilites/AutoSaverScene/Stop Auto Saving")]
        public static void Stop()
        {
            DebugLog("Stop Auto Saving");
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
                DebugLog("Start Auto Save");
                if (EditorSceneManager.SaveOpenScenes())
                {
                    DebugLog("All Opened scenes was saved successfull!");
                }
                else
                {
                    DebugLog("Saving opened scenes failed");
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

        #region debugger

        public static void DebugLog(object message)
        {
#if UNITY_EDITOR
            Debug.Log(message);
#else
            SaveLog(message);
#endif
        }

        public static void DebugError(object message)
        {
#if UNITY_EDITOR
            Debug.LogError(message);
#else
            SaveLog("Error: " + message);
#endif
        }

        public static void DebugWarning(object message)
        {
#if UNITY_EDITOR
            Debug.LogWarning(message);
#else
            SaveLog("Warning: " + message);
#endif
        }

        private static void SaveLog(object message)
        {
            File.AppendAllText(Path.Combine(Application.persistentDataPath, "Logs.txt"), message + "\n");
        }

        #endregion debugger

        #region cryptography

        public static string Encrypt(string value, string key)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(value), key));
        }

        [DebuggerNonUserCode]
        public static string Decrypt(string value, string key)
        {
            string result = string.Empty;

            try
            {
                using (CryptoStream cryptoStream = InternalDecrypt(Convert.FromBase64String(value), key))
                {
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        result = streamReader.ReadToEnd();
                    }
                }
            } catch (CryptographicException)
            {
                return null;
            }

            return result;
        }

        private static byte[] Encrypt(byte[] key, string value)
        {
            SymmetricAlgorithm symmetricAlgorithm = Rijndael.Create();
            ICryptoTransform cryptoTransform = symmetricAlgorithm.CreateEncryptor(new Rfc2898DeriveBytes(value, new byte[16]).GetBytes(16), new byte[16]);
            byte[] result = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
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
            ICryptoTransform cryptoTransform = symmetricAlgorithm.CreateDecryptor(new Rfc2898DeriveBytes(value, new byte[16]).GetBytes(16), new byte[16]);

            MemoryStream memoryStream = new MemoryStream(key);
            return new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
        }

        #endregion cryptography
    }
}
