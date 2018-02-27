// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Reflection;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// Custom attribute for keyword types.
    /// </summary>
    public class KeywordTypeFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        public KeywordTypeFieldAttribute(string prefix) : base(prefix)
        {
            width = 100;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Draws this attribute.
        /// </summary>
        /// <param name="gameConfig">The configuration of the game.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="field">The field information.</param>
        public override void Draw(GameConfiguration gameConfig, object instance, ref FieldInfo field)
        {
            EditorGUILayout.PrefixLabel(prefix);
            var keywordTypes = gameConfig.keywords;
            var options = new string[keywordTypes.Count];
            for (var i = 0; i < keywordTypes.Count; i++)
            {
                options[i] = keywordTypes[i].name;
            }

            if (options.Length > 0)
            {
                var keywordTypeId = (int)field.GetValue(instance);
                if (keywordTypes.Find(x => x.id == keywordTypeId) == null)
                {
                    field.SetValue(instance, 0);
                }

                var type = keywordTypes.Find(x => x.id == keywordTypeId);
                var typeIndex = System.Array.FindIndex(options, x => x == type.name);

                var newTypeIndex = EditorGUILayout.Popup(typeIndex, options, GUILayout.MaxWidth(width));
                var newType = options[newTypeIndex];
                field.SetValue(instance, keywordTypes.Find(x => x.name == newType).id);
            }
        }

#endif
    }
}
