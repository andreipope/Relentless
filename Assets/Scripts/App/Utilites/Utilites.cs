using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client.Protobuf;
using Loom.Google.Protobuf.Reflection;
using Loom.ZombieBattleground.Protobuf;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
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

        public static string LimitStringLength(string str, int maxLength)
        {
            if (str.Length < maxLength)
                return str;

            return str.Substring(0, maxLength);
        }

        public static double GetTimestamp()
        {
            return new TimeSpan(DateTime.UtcNow.Ticks).TotalSeconds;
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static GameObject[] CollectAllSceneRootGameObjects(GameObject dontDestroyOnLoadGameObject)
        {
            Scene[] scenes = new Scene[SceneManager.sceneCount];
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                scenes[i] = SceneManager.GetSceneAt(i);
            }

            return 
                scenes
                    .Concat(new[]
                    {
                        dontDestroyOnLoadGameObject.scene
                    })
                    .Distinct()
                    .SelectMany(scene => scene.GetRootGameObjects())
                    .ToArray();
        }



        public static string FormatCallLogList<T>(IList<T> list)
        {
            return $"[({list.Count} items) {String.Join(", ", list)}]";
        }

        public static string FormatCallLogList<T>(IEnumerable<T> list)
        {
            return $"[({list.Count()} items) {String.Join(", ", list)}]";
        }

        public static bool SequenceEqual(this byte[] array1, byte[] array2)
        {
            if (array1 == null)
                throw new ArgumentNullException(nameof(array1));

            if (array2 == null)
                throw new ArgumentNullException(nameof(array2));

            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }

            return true;
        }
        
        /// <summary>
        /// Waits for the task to complete for up to <paramref name="timeoutMilliseconds"/>
        /// and returns whether it completed in time.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <returns>True if task timed out, false otherwise.</returns>
        public static async Task<bool> RunTaskWithTimeout(Task task, int timeoutMilliseconds)
        {
            if (timeoutMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds));

            if (timeoutMilliseconds == 0)
                return !task.IsCompleted;

#if UNITY_WEBGL
            // TODO: support timeout for WebGL
            await task;
            return false;
#else
            if (timeoutMilliseconds == Timeout.Infinite)
            {
                await task;
                return false;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            Task delayTask = Task.Delay(timeoutMilliseconds, cts.Token);
            Task firstTask = await Task.WhenAny(task, delayTask);
            if (firstTask == task) {
                cts.Cancel();
                await task;
                return false;
            }

            return true;
#endif
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

        #endregion asset bundles and cache

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
            catch (CryptographicException e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
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

        public static byte[] Base64UrlDecode(string input)
        {
            string output = input;
            output = output.Replace('-', '+');
            output = output.Replace('_', '/');
            switch (output.Length % 4)
            {
                case 0: 
                    break; 
                case 2: 
                    output += "=="; 
                    break;
                case 3: 
                    output += "="; 
                    break; 
                default: 
                    throw new Exception("Illegal base64url string!");
            }
            byte[] converted = Convert.FromBase64String(output); 
            return converted;
        }

        public static bool ValidateEmail(string email)
        {
            return email != null ? Regex.IsMatch(email, Constants.MatchEmailPattern) : false;
        }

        #endregion cryptography
    }
}
