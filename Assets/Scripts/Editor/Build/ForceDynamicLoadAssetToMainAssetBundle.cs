using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Editor.Tools;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    public class ForceDynamicLoadAssetToMainAssetBundle : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; } = 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            List<AssetImporter> modified = AssetBundleFolderValidator.ForceAssetBundleInFolder("Assets/Assets/DynamicLoad", "main");
            if (modified.Count > 0)
            {
                Debug.LogWarning("Assets forced to 'main' bundle:\n" + String.Join("\n", modified.Select(a => a.assetPath)));
            }
        }
    }
}
