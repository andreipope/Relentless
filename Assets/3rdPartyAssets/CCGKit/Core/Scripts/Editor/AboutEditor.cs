// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// CCG Kit editor's 'About' tab.
    /// </summary>
    public class AboutEditor : EditorTab
    {
        public AboutEditor(GameConfiguration config) : base(config)
        {
        }

        public override void Draw()
        {
            GUILayout.Space(20);

            GUILayout.BeginVertical();
            GUILayout.Label("Current version: " + CCGKitInfo.version);
            GUILayout.Label(CCGKitInfo.copyright);
            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Documentation", GUILayout.MaxWidth(100), GUILayout.MaxHeight(50)))
            {
                Application.OpenURL("https://www.ccgkit.com/wiki");
            }
            if (GUILayout.Button("Support", GUILayout.MaxWidth(100), GUILayout.MaxHeight(50)))
            {
                Application.OpenURL("mailto:support@spelltwinegames.com");
            }
            GUILayout.EndHorizontal();
        }
    }
}
