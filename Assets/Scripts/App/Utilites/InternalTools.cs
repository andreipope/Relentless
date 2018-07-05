// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections;
using System.Reflection;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

        public static void SetLayerRecursively(GameObject parent, int layer)
        {
            parent.layer = layer;

            for (int i = 0; i < parent.transform.childCount; i++)
            {
                parent.transform.GetChild(i).gameObject.layer = layer;

                if (parent.transform.GetChild(i).childCount > 0)
                    SetLayerRecursively(parent.transform.GetChild(i).gameObject, layer);
            }
        }
    }
}