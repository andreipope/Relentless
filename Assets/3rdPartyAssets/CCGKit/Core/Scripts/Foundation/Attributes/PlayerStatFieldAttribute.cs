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
    /// Custom attribute for player stats.
    /// </summary>
    public class PlayerStatFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        public PlayerStatFieldAttribute(string prefix) : base(prefix)
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
            var playerStats = gameConfig.playerStats;
            var options = new string[playerStats.Count];
            for (var i = 0; i < playerStats.Count; i++)
            {
                options[i] = playerStats[i].name;
            }

            if (options.Length > 0)
            {
                var playerStatId = (int)field.GetValue(instance);
                if (playerStats.Find(x => x.id == playerStatId) == null)
                {
                    field.SetValue(instance, 0);
                }

                var stat = playerStats.Find(x => x.id == playerStatId);
                var statIndex = System.Array.FindIndex(options, x => x == stat.name);

                var newStatIndex = EditorGUILayout.Popup(statIndex, options, GUILayout.MaxWidth(width));
                var newStat = options[newStatIndex];
                field.SetValue(instance, playerStats.Find(x => x.name == newStat).id);
            }
        }

#endif
    }
}
