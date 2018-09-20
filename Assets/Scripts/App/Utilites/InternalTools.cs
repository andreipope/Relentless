using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Loom.ZombieBattleground.Helpers
{
    public class InternalTools
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

        public static void ShakeList<T>(ref List<T> list)
        {
            Random rnd = new Random();
            list = list.OrderBy(item => rnd.Next()).ToList();
        }

        public static List<T> ShakeList<T>(List<T> list)
        {
            Random rnd = new Random();
            return list.OrderBy(item => rnd.Next()).ToList();
        }

        public static void GroupHorizontalObjects(Transform root, float offset, float spacing)
        {
            int count = root.childCount;

            float width = spacing * count - 1;

            Vector3 pivot = new Vector3(offset, 0, 0);

            for (int i = 0; i < count; i++)
            {
                root.GetChild(i).localPosition = new Vector3(pivot.x - width / 2f, 0, 0);
                pivot.x += width / count;
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

        public static List<T> GetRandomElementsFromList<T>(List<T> root, int count)
        {
            List<T> list = new List<T>();

            if (root.Count < count)
            {
                list.AddRange(root);
            }
            else
            {
                T element;
                for (int i = 0; i < count; i++)
                {
                    element = ShakeList(root).First(x => !list.Contains(x));

                    if (element != null)
                    {
                        list.Add(element);
                    }
                }
            }

            return list;
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

            string newValue = string.Empty;

            char[] chars = origin.ToCharArray();

            newValue = chars[0].ToString().ToUpper();

            for(int i = 1; i < chars.Length; i++)
            {
                newValue += chars[i].ToString().ToLower();
            }
          
            return newValue;
        }
    }
}
