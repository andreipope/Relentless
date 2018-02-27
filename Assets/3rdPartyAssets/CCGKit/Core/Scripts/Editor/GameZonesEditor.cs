// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// CCG Kit editor's 'Game zones' tab.
    /// </summary>
    public class GameZonesEditor : EditorTab
    {
        private ReorderableList gameZonesList;
        private GameZoneType currentGameZone;

        public GameZonesEditor(GameConfiguration config) : base(config)
        {
            gameZonesList = EditorUtils.SetupReorderableList("Game zones", gameConfig.gameZones, ref currentGameZone, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.name);
            },
            (x) =>
            {
                currentGameZone = x;
            },
            () =>
            {
                var zone = new GameZoneType();
                gameConfig.gameZones.Add(zone);
            },
            (x) =>
            {
                currentGameZone = null;
            });
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (gameZonesList != null)
            {
                gameZonesList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentGameZone != null)
            {
                DrawGameZone(currentGameZone);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawGameZone(GameZoneType zone)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorConfig.LargeLabelWidth;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            zone.name = EditorGUILayout.TextField(zone.name, GUILayout.MaxWidth(EditorConfig.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Owner");
            zone.owner = (ZoneOwner)EditorGUILayout.EnumPopup(zone.owner, GUILayout.MaxWidth(EditorConfig.RegularComboBoxWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            zone.type = (ZoneType)EditorGUILayout.EnumPopup(zone.type, GUILayout.MaxWidth(EditorConfig.RegularComboBoxWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Owner visibility");
            zone.ownerVisibility = (ZoneOwnerVisibility)EditorGUILayout.EnumPopup(zone.ownerVisibility, GUILayout.MaxWidth(EditorConfig.RegularComboBoxWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Opponent visibility");
            zone.opponentVisibility = (ZoneOpponentVisibility)EditorGUILayout.EnumPopup(zone.opponentVisibility, GUILayout.MaxWidth(EditorConfig.RegularComboBoxWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Has maximum size");
            zone.hasMaxSize = EditorGUILayout.Toggle(zone.hasMaxSize, GUILayout.MaxWidth(EditorConfig.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            if (zone.hasMaxSize)
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Maximum size");
                zone.maxSize = EditorGUILayout.IntField(zone.maxSize, GUILayout.MaxWidth(EditorConfig.RegularIntFieldWidth + 15));
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}
