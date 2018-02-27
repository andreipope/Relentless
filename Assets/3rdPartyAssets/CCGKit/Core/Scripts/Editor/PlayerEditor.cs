// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// CCG Kit editor's 'Player' tab.
    /// </summary>
    public class PlayerEditor : EditorTab
    {
        private ReorderableList playerStatsList;
        private DefinitionStat currentPlayerStat;

        public PlayerEditor(GameConfiguration config) : base(config)
        {
            playerStatsList = EditorUtils.SetupReorderableList("Player stats", gameConfig.playerStats, ref currentPlayerStat, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.name);
            },
            (x) =>
            {
                currentPlayerStat = x;
            },
            () =>
            {
                var stat = new PlayerStat();
                gameConfig.playerStats.Add(stat);
            },
            (x) =>
            {
                currentPlayerStat = null;
            });
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (playerStatsList != null)
            {
                playerStatsList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentPlayerStat != null)
            {
                DrawDefinitionStat(currentPlayerStat);
            }

            GUILayout.EndHorizontal();
        }
    }
}
