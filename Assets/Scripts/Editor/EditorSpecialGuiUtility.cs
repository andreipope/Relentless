using System;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    public static class EditorSpecialGuiUtility
    {
        public static string DrawPersistentFilePathField(string path, string label, string openWindowTitle, string extension, string prefsKey, int fieldWidth = 330)
        {
            return DrawPersistentPathField(path, label, openWindowTitle, prefsKey, s => UnityEditor.EditorUtility.OpenFilePanel(openWindowTitle, Application.dataPath, extension), fieldWidth);
        }

        public static string DrawPersistentFolderPathField(string path, string label, string openWindowTitle, string defaultName, string prefsKey, int fieldWidth = 330)
        {
            return DrawPersistentPathField(path, label, openWindowTitle, prefsKey, s => UnityEditor.EditorUtility.OpenFolderPanel(openWindowTitle, Application.dataPath, defaultName), fieldWidth);
        }

        public static string DrawPersistentPathField(string path, string label, string openWindowTitle, string prefsKey, Func<string, string> pathFiller, int fieldWidth = 330)
        {
            string originalPath = path;

            if (String.IsNullOrEmpty(path))
            {
                path = EditorPrefs.GetString(prefsKey);
            }

            EditorGUIUtility.labelWidth = fieldWidth;

            EditorGUILayout.BeginHorizontal();
            {
                path = EditorGUILayout.TextField(label, path);
                if (GUILayout.Button("Select...", GUILayout.Width(100)))
                {
                    path = pathFiller(path);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 0;
            if (path != originalPath)
            {
                EditorPrefs.SetString(prefsKey, path);
            }

            return path;
        }

        public static void DrawSeparator()
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, 2);
            rect.height = 1;
            EditorGUI.DrawRect(rect, Color.black);
            EditorGUILayout.Space();
        }
    }
}
