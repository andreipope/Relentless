// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// The CCG Kit editor accessible from Unity's menu. This editor provides an intuitive way to define
    /// the fundamental properties of a collectible card game.
    /// </summary>
    public class CCGKitEditor : EditorWindow
    {
        private GameConfiguration gameConfig;

        private string gameConfigPath;

        private int selectedTabIndex = -1;
        private int prevSelectedTabIndex = -1;

        private List<EditorTab> tabs = new List<EditorTab>();

        private Vector2 scrollPos;

        [MenuItem("Window/CCG Kit Editor")]
        private static void Init()
        {
            var window = GetWindow(typeof(CCGKitEditor));
            window.titleContent = new GUIContent("CCG Kit Editor");
        }

        private void OnEnable()
        {
            if (EditorPrefs.HasKey("GameConfigurationPath"))
            {
                gameConfigPath = EditorPrefs.GetString("GameConfigurationPath");
                gameConfig = new GameConfiguration();
                gameConfig.LoadGameConfiguration(gameConfigPath);
                ResetEditorTabs();
                selectedTabIndex = 0;
            }
        }

        private void ResetGameConfiguration()
        {
            gameConfig = new GameConfiguration();
            selectedTabIndex = 0;
            ResetEditorTabs();
            ResetIds();
        }

        private void ResetEditorTabs()
        {
            tabs.Clear();
            tabs.Add(new GameConfigurationEditor(gameConfig));
            tabs.Add(new GameZonesEditor(gameConfig));
            tabs.Add(new PlayerEditor(gameConfig));
            tabs.Add(new CardTypesEditor(gameConfig));
            tabs.Add(new KeywordEditor(gameConfig));
            tabs.Add(new CardCollectionEditor(gameConfig));
            tabs.Add(new AboutEditor(gameConfig));
        }

        private void ResetIds()
        {
            GameZoneType.currentId = 0;
            PlayerStat.currentId = 0;
            CardType.currentId = 0;
            CardStat.currentId = 0;
            Card.currentId = 0;
            Keyword.currentId = 0;
        }

        private void OpenGameConfiguration()
        {
            var path = EditorUtility.OpenFolderPanel("Select game configuration folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                gameConfigPath = path;
                gameConfig = new GameConfiguration();
                gameConfig.LoadGameConfiguration(gameConfigPath);
                ResetEditorTabs();
                selectedTabIndex = 0;
                EditorPrefs.SetString("GameConfigurationPath", gameConfigPath);
            }
        }

        private void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, false);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("New", GUILayout.MaxWidth(60)))
            {
                ResetGameConfiguration();
            }
            if (GUILayout.Button("Open", GUILayout.MaxWidth(60)))
            {
                OpenGameConfiguration();
            }
            if (GUILayout.Button("Save", GUILayout.MaxWidth(60)))
            {
                gameConfig.SaveGameConfiguration(gameConfigPath);
            }
            if (GUILayout.Button("Save as", GUILayout.MaxWidth(60)))
            {
                gameConfig.SaveGameConfigurationAs();
                gameConfigPath = EditorPrefs.GetString("GameConfigurationPath");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Current path: ", GUILayout.MaxWidth(90));
            GUILayout.Label(gameConfigPath);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (gameConfig == null)
            {
                return;
            }

            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, new string[] { "Game configuration", "Game zones", "Player", "Card types", "Keywords", "Card collection", "About CCG Kit" });
            if (selectedTabIndex >= 0 && selectedTabIndex < tabs.Count)
            {
                var selectedEditor = tabs[selectedTabIndex];
                if (selectedTabIndex != prevSelectedTabIndex)
                {
                    selectedEditor.OnTabSelected();
                }
                selectedEditor.Draw();

                prevSelectedTabIndex = selectedTabIndex;
            }

            GUILayout.EndScrollView();
        }
    }
}
