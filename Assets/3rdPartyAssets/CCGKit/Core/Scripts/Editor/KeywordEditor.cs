// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// CCG Kit editor's 'Keyword' tab.
    /// </summary>
    public class KeywordEditor : EditorTab
    {
        private ReorderableList keywordList;
        private Keyword currentKeyword;

        private ReorderableList currentKeywordValuesList;
        private KeywordValue currentKeywordValue;

        public KeywordEditor(GameConfiguration config) : base(config)
        {
            keywordList = EditorUtils.SetupReorderableList("Keywords", gameConfig.keywords, ref currentKeyword, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.name);
            },
            (x) =>
            {
                currentKeyword = x;
                currentKeywordValue = null;
                CreateCurrentKeywordValuesList();
            },
            () =>
            {
                var keyword = new Keyword();
                gameConfig.keywords.Add(keyword);
            },
            (x) =>
            {
                currentKeyword = null;
                currentKeywordValue = null;
            });
        }

        private void CreateCurrentKeywordValuesList()
        {
            currentKeywordValuesList = EditorUtils.SetupReorderableList("Values", currentKeyword.values, ref currentKeywordValue, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.value);
            },
            (x) =>
            {
                currentKeywordValue = x;
            },
            () =>
            {
                var value = new KeywordValue();
                currentKeyword.values.Add(value);
            },
            (x) =>
            {
                currentKeywordValue = null;
            });
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (keywordList != null)
            {
                keywordList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentKeyword != null)
            {
                DrawKeyword(currentKeyword);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawKeyword(Keyword keyword)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorConfig.RegularLabelWidth;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            keyword.name = EditorGUILayout.TextField(keyword.name, GUILayout.MaxWidth(EditorConfig.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (currentKeywordValuesList != null)
            {
                currentKeywordValuesList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentKeywordValue != null)
            {
                DrawKeywordValue(currentKeywordValue);
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        private void DrawKeywordValue(KeywordValue value)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            value.value = EditorGUILayout.TextField(value.value, GUILayout.MaxWidth(EditorConfig.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
