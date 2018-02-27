// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace CCGKit
{
    public class StringProperty : Property
    {
        public string value;

#if UNITY_EDITOR

        public override void Draw()
        {
            base.Draw();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Default value");
            value = EditorGUILayout.TextField(value, GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();
        }

#endif
    }
}