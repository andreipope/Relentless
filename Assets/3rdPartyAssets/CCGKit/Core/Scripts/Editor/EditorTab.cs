// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// CCG Kit editor's base tab class.
    /// </summary>
    public class EditorTab
    {
        public string name { get; protected set; }

        protected GameConfiguration gameConfig;

        public EditorTab(GameConfiguration config)
        {
            gameConfig = config;
        }

        public virtual void OnTabSelected()
        {
        }

        public virtual void Draw()
        {
        }

        protected void DrawDefinitionStat(DefinitionStat stat)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorConfig.LargeLabelWidth;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            stat.name = EditorGUILayout.TextField(stat.name, GUILayout.MaxWidth(EditorConfig.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Base value");
            stat.baseValue = EditorGUILayout.IntField(stat.baseValue, GUILayout.MaxWidth(EditorConfig.RegularIntFieldWidth));
            stat.originalValue = stat.baseValue;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Minimum value");
            stat.minValue = EditorGUILayout.IntField(stat.minValue, GUILayout.MaxWidth(EditorConfig.RegularIntFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Maximum value");
            stat.maxValue = EditorGUILayout.IntField(stat.maxValue, GUILayout.MaxWidth(EditorConfig.RegularIntFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        protected void DrawStat(Stat stat)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorConfig.LargeLabelWidth;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            stat.name = EditorGUILayout.TextField(stat.name, GUILayout.MaxWidth(EditorConfig.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Base value");
            stat.baseValue = EditorGUILayout.IntField(stat.baseValue, GUILayout.MaxWidth(EditorConfig.RegularIntFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Minimum value");
            stat.minValue = EditorGUILayout.IntField(stat.minValue, GUILayout.MaxWidth(EditorConfig.RegularIntFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Maximum value");
            stat.maxValue = EditorGUILayout.IntField(stat.maxValue, GUILayout.MaxWidth(EditorConfig.RegularIntFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}
