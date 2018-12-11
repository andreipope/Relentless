using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor.Tools
{
    public class MultiplayerDebugClientDataPreviewWindow : EditorWindow, IHasCustomMenu
    {
        private const int MaxTextLength = 16382;

        private Vector2 _scrollPosition;
        private List<string> _textChunks = new List<string>();
        private string _text;

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
                    foreach (string textChunk in _textChunks)
                    {
                        EditorGUILayout.TextArea(textChunk, guiStyle, GUILayout.ExpandHeight(true));
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        public void SetText(string text)
        {
            _text = text;
            _textChunks = SplitIntoChunks(text, MaxTextLength);
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Copy to Clipboard"),
                false,
                () =>
                {
                    EditorGUIUtility.systemCopyBuffer = _text;
                }
            );
        }

        private static List<string> SplitIntoChunks(string str, int chunkSize)
        {
            List<string> splitString = new List<string>();

            for (int index = 0; index < str.Length; index += chunkSize)
            {
                splitString.Add(str.Substring(index, Math.Min(chunkSize, str.Length - index)));
            }

            return splitString;
        }
    }
}
