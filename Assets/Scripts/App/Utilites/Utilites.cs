using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Loom.Client.Protobuf;
using Loom.Google.Protobuf.Reflection;
using Loom.ZombieBattleground.Protobuf;
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

        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public static T CastStringTuEnum<T>(string data, bool isManual = false)
        {
            if (isManual)
            {
                return (T)Enum.Parse(typeof(T), data);
            }
            else
            {
                return (T)Enum.Parse(typeof(T), data.ToUpperInvariant());
            }
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

        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("input cannot be empty!");
            return input.First().ToString().ToUpperInvariant() + input.Substring(1).ToLowerInvariant();
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
