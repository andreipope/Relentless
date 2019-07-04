using DG.Tweening;
using Loom.ZombieBattleground.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Loom.ZombieBattleground.Helpers
{
    public static class InternalTools
    {
        public static void SetLayerRecursively(
            GameObject parent, int layer, List<string> ignoreNames = null, bool parentIgnored = false)
        {
            if (!parentIgnored)
            {
                parent.layer = layer;
            }

            bool ignored = false;
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                if (ignoreNames != null)
                {
                    ignored = ignoreNames.Contains(parent.transform.GetChild(i).gameObject.name);
                }

                if (!ignored && !parentIgnored || ignoreNames == null)
                {
                    parent.transform.GetChild(i).gameObject.layer = layer;
                }

                if (parent.transform.GetChild(i).childCount > 0)
                {
                    SetLayerRecursively(parent.transform.GetChild(i).gameObject, layer, ignoreNames, ignored);
                }
            }
        }

        public static void GroupHorizontalObjects(Transform root, float offset, float spacing, float offsetY, bool isReverse = false, float offsetZ = 0f)
        {
            int count = root.childCount;

            float width = spacing * (count - 1);

            Vector3 pivot = new Vector3(offset, 0, 0);

            for (int i = 0; i < count; i++)
            {
                root.GetChild(i).localPosition = new Vector3(pivot.x - width / 2f, offsetY, offsetZ);
                pivot.x += width / (count-1);
            }
        }

        public static void GroupVerticalObjects(
            Transform root, float spacing, float centerOffset = -7f, float height = 7.2f)
        {
            int count = root.childCount;
            float halfHeightOffset = height + spacing;

            float startPos = centerOffset + (count - 1) * halfHeightOffset / 2f;

            for (int i = 0; i < count; i++)
            {
                root.GetChild(i).localPosition = new Vector3(root.GetChild(i).localPosition.x,
                    startPos - halfHeightOffset * i, root.GetChild(i).localPosition.z);
            }
        }

        public static List<T> GetRandomElementsFromList<T>(IReadOnlyList<T> list, int count, bool deterministicRNG = false)
        {
            if (list == null || list.Count == 0)
                return list.ToList();

            if (deterministicRNG)
            {
                count = Mathf.Max(0, count);
                List<T> elements = list.ToList();
                List<T> export = new List<T>();

                while (count > 0 && elements.Count > 0)
                {
                    int chosenIndex = MTwister.IRandom(0, elements.Count - 1);
                    export.Add(elements[chosenIndex]);
                    elements.RemoveAt(chosenIndex);
                    count--;
                }
                return export;
            }
            else
            {
                return list.GetRandomElementsFromList(count);
            }
        }

        public static float DeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            return diagonalInches;
        }

        public static bool IsTabletScreen()
        {
#if FORCE_TABLET_UI
            return true;
#elif FORCE_PHONE_UI
            return false;
#else
            return DeviceDiagonalSizeInInches() > 6.5f;
#endif
        }

        public static string ProccesEnumToString(string origin)
        {
            if (string.IsNullOrEmpty(origin))
                return origin;

            char[] chars = origin.Replace("_", Constants.Space).ToCharArray();

            string newValue = chars[0].ToString().ToUpperInvariant();

            for(int i = 1; i < chars.Length; i++)
            {
                if (char.IsUpper(chars[i]))
                {
                    newValue += Constants.Space;
                }

                newValue += chars[i].ToString().ToLowerInvariant();
            }

            return newValue;
        }

        /// <returns>Whether the wait ended because of timeout, false if completed normally.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static async Task<bool> WaitWithTimeout(float timeout, Func<bool> isCompletedFunc)
        {
            if (isCompletedFunc == null)
                throw new ArgumentNullException(nameof(isCompletedFunc));

            if (timeout <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeout));

            if (isCompletedFunc())
                return false;

            double startTimestamp = Utilites.GetTimestamp();
            bool timedOut = false;
            await new WaitUntil(() =>
            {
                if (isCompletedFunc())
                    return true;

                if (Utilites.GetTimestamp() - startTimestamp > timeout)
                {
                    timedOut = true;
                    return true;
                }

                return false;
            });

            return timedOut;
        }

        public static Sequence DoActionDelayed(TweenCallback action, float delay = 0f)
        {
            if (action == null)
                return null;

            Sequence sequence = DOTween.Sequence();
            sequence.PrependInterval(delay);
            sequence.AppendCallback(action);

            return sequence;
        }

        public static string FormatStringToPascaleCase(string root)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(root.ToLower().Replace("_", " ")).Replace(" ", string.Empty);
        }

        public static ItemPosition GetSafePositionToInsert<T>(ItemPosition position, IReadOnlyCollection<T> list)
        {
            return new ItemPosition(Mathf.Clamp(position.GetIndex(list), 0, list.Count));
        }

        public static ItemPosition GetSafePositionToInsert<T>(int position, IReadOnlyCollection<T> list)
        {
            return new ItemPosition(Mathf.Clamp(position, 0, list.Count));
        }

        public static async global::System.Threading.Tasks.Task<T> GetJsonFromLink<T>(
            string uri,
            log4net.ILog log,
            JsonSerializerSettings jsonSerializerSettings)
        {
            T result = default(T);

            UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(uri);
            await request.SendWebRequest();

            while (!request.isDone) { await global::System.Threading.Tasks.Task.Delay(10); }

            if (!request.isNetworkError && !request.isHttpError)
            {
                try
                {
                    if (jsonSerializerSettings != null)
                    {
                        result = JsonConvert.DeserializeObject<T>(request.downloadHandler.text, jsonSerializerSettings);
                    }
                    else
                    {
                        result = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
                    }
                }
                catch (Exception e)
                {
                    log?.Warn($"Parse json to Type {typeof(T).Name} has error;", e);
                }
            }
            else
            {
                log?.Warn($"{uri} : request error: {request.downloadHandler?.text}");
            }

            return result;
        }
    }
}
