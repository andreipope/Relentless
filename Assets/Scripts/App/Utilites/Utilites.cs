// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_5_3_OR_NEWER && UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

namespace LoomNetwork.Internal
{
    public static class Utilites
    {
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

        [MenuItem("Utilites/CacheAndBundles/Build Asset Bundles")]
        public static void BuildAssetBundles()
        {
            string outputPath = Path.Combine("AssetBundles", GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget));

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            BuildPipeline.BuildAssetBundles(outputPath, 0, EditorUserBuildSettings.activeBuildTarget);
        }

        public static string GetPlatformFolderForAssetBundles(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
                default:
                    return null;
            }
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
            var files = dir.GetFiles();
            foreach(var file in files)
                file.Delete();
            files = null;

            var directories = dir.GetDirectories();
            foreach (var directory in directories)
                directory.Delete(true);
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
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                ProcessDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory);
                    File.Delete(directory + ".meta");
                    DebugLog(directory + " deleted");
                }
            }
        }

#endif

        #endregion project file hierarchy utilites

        #region scene hierarchy utilites

#if UNITY_EDITOR
        [MenuItem("Utilites/Editor/Select GOs With Missing Scripts")]
        public static void SelectMissing(MenuCommand command)
        {
            Transform[] ts = GameObject.FindObjectsOfType<Transform>();
            var selection = new System.Collections.Generic.List<GameObject>();
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

        #region Auto Saving Scenes in Editor

#if UNITY_EDITOR

        private static int MinutesDelay = 2;
        private static bool IsStop = false;


        [MenuItem("Utilites/AutoSaverScene/Init Auto Saving")]
        public static void Init()
        {
            DebugLog("Initialized Auto Saving! Be warning - if you hide editor, saving will stop automatically. You need to initialize auto saving again");
            IsStop = false;
            EditorCoroutine.Start(Save());
        }

        [MenuItem("Utilites/AutoSaverScene/Stop Auto Saving")]
        public static void Stop()
        {
            DebugLog("Stop Auto Saving");
            IsStop = true;
        }

        private static IEnumerator Save()
        {
            int iterations = 60 * 60 * MinutesDelay; // frames count * seconds per minute * minutes count
            for (float i = 0; i < iterations; i++)
                yield return null;

            if (!IsStop)
            {
                DebugLog("Start Auto Save");
#if UNITY_5_3_OR_NEWER
                if (EditorSceneManager.SaveOpenScenes())
                {
                    DebugLog("All Opened scenes was saved successfull!");
                }
                else
                {
                    DebugLog("Saving opened scenes failed");
                }
#elif UNITY_3 || UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
                if (EditorApplication.SaveScene())
                {
                    DebugLog("Opened scene was saved successfull!");
                }
                else
                {
                    DebugLog("Saving open scene failed");
                }
#endif
                EditorCoroutine.Start(Save());
            }
        }

        private class EditorCoroutine
        {
            public static EditorCoroutine Start(IEnumerator _routine)
            {
                EditorCoroutine coroutine = new EditorCoroutine(_routine);
                coroutine.Start();
                return coroutine;
            }

            private readonly IEnumerator routine;

            private EditorCoroutine(IEnumerator _routine)
            {
                routine = _routine;
            }

            private void Start()
            {
                EditorApplication.update += Update;
            }

            public void Stop()
            {
                EditorApplication.update -= Update;
            }

            private void Update()
            {
                if (!routine.MoveNext())
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
            UnityEngine.Debug.Log(message);
#else
            SaveLog(message);
#endif
        }

        public static void DebugError(object message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(message);
#else
            SaveLog("Error: " + message);
#endif
        }

        public static void DebugWarning(object message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(message);
#else
            SaveLog("Warning: " + message);
#endif
        }

        private static void SaveLog(object message)
        {
            File.AppendAllText(Path.Combine(Application.persistentDataPath, "Logs.txt"), message.ToString() + "\n");
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
            ICryptoTransform cryptoTransform = symmetricAlgorithm.CreateEncryptor((new Rfc2898DeriveBytes(value, new byte[16])).GetBytes(16), new byte[16]);
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
            ICryptoTransform cryptoTransform = symmetricAlgorithm.CreateDecryptor((new Rfc2898DeriveBytes(value, new byte[16])).GetBytes(16), new byte[16]);

            MemoryStream memoryStream = new MemoryStream(key);
            return new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
        }
		#endregion cryptography


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
            foreach (var target in targets)
            {
                list.Add(CastStringTuEnum<T>(target));
            }
            return list;
        }

        public static string FirstCharToUpper(string input)
        {
            if (!String.IsNullOrEmpty(input))
                input = input.First().ToString().ToUpper() + input.Substring(1);
            return input;

        }

        public static Vector3 CastVFXPosition(Vector3 position)
        {
            return new Vector3(position.x, position.z, position.y);
        }

        public static Color SetAlpha(this Color color, float alpha) {
            color.a = alpha;
            return color;
        }
    }
}