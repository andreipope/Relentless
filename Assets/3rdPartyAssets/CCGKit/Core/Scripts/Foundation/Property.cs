// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// The base class for properties.
    /// </summary>
    public class Property
    {
        /// <summary>
        /// The name of this property.
        /// </summary>
        public string name;

#if UNITY_EDITOR

        /// <summary>
        /// Draws this property in the editor.
        /// </summary>
        public virtual void Draw()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            name = EditorGUILayout.TextField(name, GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();
        }

#endif
    }
}
