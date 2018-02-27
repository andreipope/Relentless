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
    /// Custom attribute for card stats.
    /// </summary>
    public class CardStatFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        public CardStatFieldAttribute(string prefix) : base(prefix)
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

            var cardType = 0;

            var fields = instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < fields.Length; i++)
            {
                var cardTypeFieldAttribute = GetCustomAttribute(fields[i], typeof(CardTypeFieldAttribute)) as CardTypeFieldAttribute;
                if (cardTypeFieldAttribute != null)
                {
                    cardType = (int)fields[i].GetValue(instance);
                    break;
                }
            }

            var cardStats = gameConfig.cardTypes.Find(x => x.id == cardType).stats;
            var options = new string[cardStats.Count];
            for (var i = 0; i < cardStats.Count; i++)
            {
                options[i] = cardStats[i].name;
            }

            if (options.Length > 0)
            {
                var cardStatId = (int)field.GetValue(instance);
                if (cardStats.Find(x => x.id == cardStatId) == null)
                {
                    field.SetValue(instance, 0);
                }

                var stat = cardStats.Find(x => x.id == cardStatId);
                var statIndex = System.Array.FindIndex(options, x => x == stat.name);

                var newStatIndex = EditorGUILayout.Popup(statIndex, options, GUILayout.MaxWidth(width));
                var newStat = options[newStatIndex];
                field.SetValue(instance, cardStats.Find(x => x.name == newStat).id);
            }
        }

#endif
    }
}
