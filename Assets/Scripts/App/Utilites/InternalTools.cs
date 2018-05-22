using System;
using System.Collections;
using System.Reflection;
using GrandDevs.CZB.Common;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GrandDevs.CZB.Helpers
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
    }
}