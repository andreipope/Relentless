using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace ZombieBattleground.Editor.Runtime
{
    public static class DebugCardInfoDrawer
    {
        private static GUIStyle InfoBoxStyle;

        static DebugCardInfoDrawer()
        {
            InfoBoxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset
                {
                    left = 2,
                    right = 2,
                    bottom = 1,
                    top = 1
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
                string text = $"Id: {instanceId}";
                if (name != null)
                {
                    text += $"\nName: {name}";
                }
                GUIContent guiContent = new GUIContent(text);
                Rect rect = HandleUtility.WorldPointToSizedRect(position, guiContent, InfoBoxStyle);
                rect.x -= rect.width * 0.5f;
                GUI.Label(HandleTools.ClipRectToScreen(rect), guiContent, InfoBoxStyle);
            }
            GizmosGuiHelper.EndGizmosGui();
        }
    }
}
#endif
