using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor.Tools
{
    public class FakeClientDataPreviewWindow : EditorWindow
    {
        private Vector2 _scrollPosition;

        public string Text;

        private void OnEnable()
        {
            titleContent = new GUIContent("Data View");
        }

        private void OnGUI()
        {
            GUIStyle guiStyle = new GUIStyle(EditorStyles.textField)
            {
                wordWrap = true
            };

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, false, false))
            {
                _scrollPosition = scrollView.scrollPosition;
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.TextArea(Text, guiStyle, GUILayout.ExpandHeight(true));
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
