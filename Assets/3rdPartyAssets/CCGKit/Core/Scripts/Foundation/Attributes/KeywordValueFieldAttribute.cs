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
    /// Custom attribute for keyword values.
    /// </summary>
    public class KeywordValueFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        public KeywordValueFieldAttribute(string prefix) : base(prefix)
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

            var keywordType = 0;

            var fields = instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < fields.Length; i++)
            {
                var keywordTypeFieldAttribute = GetCustomAttribute(fields[i], typeof(KeywordTypeFieldAttribute)) as KeywordTypeFieldAttribute;
                if (keywordTypeFieldAttribute != null)
                {
                    keywordType = (int)fields[i].GetValue(instance);
                    break;
                }
            }

            var keywordValues = gameConfig.keywords.Find(x => x.id == keywordType).values;
            var options = new string[keywordValues.Count];
            for (var i = 0; i < keywordValues.Count; i++)
            {
                options[i] = keywordValues[i].value;
            }

            if (options.Length > 0)
            {
                var keywordValue = (int)field.GetValue(instance);
                if (keywordValue >= keywordValues.Count)
                {
                    keywordValue = 0;
                    field.SetValue(instance, 0);
                }

                var newKeywordValue = EditorGUILayout.Popup(keywordValue, options, GUILayout.MaxWidth(width));
                field.SetValue(instance, newKeywordValue);
            }
        }

#endif
    }
}
