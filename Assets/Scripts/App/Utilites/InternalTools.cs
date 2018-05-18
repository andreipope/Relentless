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

        public static void PlayCardSound(Enumerators.CardSound type, int cardId)
        {
            //  Play a card sound >>

            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(cardId);
            string soundFolder = type.ToString().Substring(0, 1) + type.ToString().Substring(1).ToLower();
            string soundPath = "Sounds/Cards/" + libraryCard.cardSetType.ToString().ToUpper() + "/" + libraryCard.name + "/" + soundFolder;
            var clips = Resources.LoadAll<AudioClip>(soundPath);

            if (clips.Length > 0)
                GameClient.Get<ISoundManager>().PlaySound(new List<AudioClip>() { clips[UnityEngine.Random.Range(0, clips.Length)] }, Enumerators.SoundType.OTHER);
            else
                Debug.Log("<color=yellow>Wanted to play a card sound: " + soundPath + ", but didn't find it.</color>");

            //  << play a card sound
        }
    }
}