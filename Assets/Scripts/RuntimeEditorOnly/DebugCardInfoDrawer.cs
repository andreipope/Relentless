using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace ZombieBattleground.Editor.Runtime
{
    public static class DebugCardInfoDrawer
    {
        private static GUIStyle InfoBoxStyle;
        private static List<string> LinesCache = new List<string>();
        private static GUIContent GUIContentCache = new GUIContent();

        static DebugCardInfoDrawer()
        {
            InfoBoxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset
                {
                    left = 2,
                    right = 2,
                    bottom = 2,
                    top = 2
                },
                alignment = TextAnchor.UpperLeft,
                wordWrap = false
            };
        }

        public static void Draw(Vector3 position, int instanceId, string name = null)
        {
            position += new Vector3(0, 0.5f);
            GizmosGuiHelper.StartGizmosGui();
            {
                LinesCache.Clear();
                LinesCache.Add($"Id: {instanceId}");
                if (name != null)
                {
                    LinesCache.Add($"Name: {name}");
                }

                string joined = string.Join("\n", LinesCache);

                GUIContentCache.text = joined;
                Rect rect = HandleUtility.WorldPointToSizedRect(position, GUIContentCache, InfoBoxStyle);
                rect.x -= rect.width * 0.5f;
                rect = HandleTools.ClipRectToScreen(rect);

                float lineHeight = rect.height / LinesCache.Count;

                for (int i = 0; i < LinesCache.Count; i++)
                {
                    string line = LinesCache[i];
                    GUIContentCache.text = line;
                    Vector2 lineSize = InfoBoxStyle.CalcSize(GUIContentCache);
                    Rect lineRect = rect;
                    lineRect.height = lineHeight;
                    lineRect.y += i * lineHeight;
                    lineRect.x += lineRect.width / 2f - lineSize.x / 2f;
                    lineRect.width = lineSize.x;

                    GUI.Label(lineRect, GUIContentCache, InfoBoxStyle);
                }
            }
            GizmosGuiHelper.EndGizmosGui();
        }
    }
}
#endif
