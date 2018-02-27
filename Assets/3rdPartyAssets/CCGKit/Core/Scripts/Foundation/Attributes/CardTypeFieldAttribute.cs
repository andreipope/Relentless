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
    /// Custom attribute for card types.
    /// </summary>
    public class CardTypeFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        public CardTypeFieldAttribute(string prefix) : base(prefix)
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
            var cardTypes = gameConfig.cardTypes;
            var options = new string[cardTypes.Count];
            for (var i = 0; i < cardTypes.Count; i++)
            {
                options[i] = cardTypes[i].name;
            }

            if (options.Length > 0)
            {
                var cardTypeId = (int)field.GetValue(instance);
                if (cardTypes.Find(x => x.id == cardTypeId) == null)
                {
                    field.SetValue(instance, 0);
                }

                var type = cardTypes.Find(x => x.id == cardTypeId);
                var typeIndex = System.Array.FindIndex(options, x => x == type.name);

                var newTypeIndex = EditorGUILayout.Popup(typeIndex, options, GUILayout.MaxWidth(width));
                var newType = options[newTypeIndex];
                field.SetValue(instance, cardTypes.Find(x => x.name == newType).id);
            }
        }

#endif
    }
}
