// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// Custom attribute for values.
    /// </summary>
    public class ValueFieldAttribute : FieldAttribute
    {
        private int selectedValueType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        public ValueFieldAttribute(string prefix) : base(prefix)
        {
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
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Value");
            var valueTypes = new List<Type>(AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(Value)));
            var valueNames = new string[valueTypes.Count];
            for (var i = 0; i < valueTypes.Count; i++)
            {
                valueNames[i] = StringUtils.DisplayCamelCaseString(valueTypes[i].Name);
            }

            var currentValue = field.GetValue(instance);
            int currentValueType = 0;
            if (currentValue != null)
            {
                currentValueType = valueTypes.FindIndex(x => x == currentValue.GetType());
                selectedValueType = currentValueType;
            }
            selectedValueType = EditorGUILayout.Popup(currentValueType, valueNames, GUILayout.MaxWidth(150));
            if (selectedValueType != currentValueType || currentValue == null)
            {
                var newValueType = Activator.CreateInstance(valueTypes[selectedValueType]);
                field.SetValue(instance, newValueType);
            }
            GUILayout.EndHorizontal();

            currentValue = field.GetValue(instance);
            if (currentValue != null)
            {
                var fields = currentValue.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                for (var i = 0; i < fields.Length; i++)
                {
                    var attribute = GetCustomAttribute(fields[i], typeof(FieldAttribute)) as FieldAttribute;
                    if (attribute != null)
                    {
                        GUILayout.BeginHorizontal();
                        attribute.Draw(gameConfig, currentValue, ref fields[i]);
                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.EndVertical();
        }

#endif
    }
}
