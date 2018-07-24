// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections;
using System.Reflection;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace LoomNetwork.CZB.Helpers
{
    public class InternalTools
    {
        private static string LINE_BREAK = "%n%";

        public static void FixVerticalLayoutGroupFitting(UnityEngine.Object value)
        {
            VerticalLayoutGroup group = null;

            if (value is VerticalLayoutGroup)
                group = value as VerticalLayoutGroup;
            else if (value is GameObject)
                group = (value as GameObject).GetComponent<VerticalLayoutGroup>();
            else if (value is Transform)
                group = (value as Transform).GetComponent<VerticalLayoutGroup>();


            if (group == null)
                return;

            group.enabled = false;
            Canvas.ForceUpdateCanvases();
            group.SetLayoutVertical();
            group.CalculateLayoutInputVertical();
            group.enabled = true;
        }

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }


        // InternalTools.CallPhoneNumber("+############");
        public static void CallPhoneNumber(string phone)
        {
            Application.OpenURL("tel://" + phone);
        }

        public static string ReplaceLineBreaks(string data)
        {
            if (data == null)
                return "";

            return data.Replace(LINE_BREAK, "\n");
        }

        public static void SetLayerRecursively(GameObject parent, int layer, List<string> ignoreNames = null)
        {
            parent.layer = layer;

            for (int i = 0; i < parent.transform.childCount; i++)
            {
                if (ignoreNames == null || !ignoreNames.Contains(parent.transform.GetChild(i).gameObject.name))
                    parent.transform.GetChild(i).gameObject.layer = layer;

                if (parent.transform.GetChild(i).childCount > 0)
                    SetLayerRecursively(parent.transform.GetChild(i).gameObject, layer, ignoreNames);
            }
        }

        public static void ShakeList<T>(ref List<T> list)
        {
            var rnd = new System.Random();
            list = list.OrderBy(item => rnd.Next()).ToList();
        }

        public static void GroupHorizontalObjects(Transform root, float offset, float spacing)
        {
            int count = root.childCount;

            float handWidth = spacing * count - 1;

            var pivot = new Vector3(offset, 0, 0);

            for (var i = 0; i < count; i++)
            {
                root.GetChild(i).localPosition = new Vector3(pivot.x - handWidth / 2f, 0, 0);
                pivot.x += handWidth / count;
            }
        }
    }
}