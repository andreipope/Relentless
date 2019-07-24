using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    public class AotHintFileUpdateAssetPostprocessor : AssetPostprocessor
    {
        private const string MustRegenerateAotHintKey = "ZB_MustRegenerateAotHint";
        private static readonly string[] TargetFileNames =
        {
            "ZbCalls.cs",
            "ZbData.cs",
            "ZbEnums.cs",
            "ZbCustombase.cs",
        };

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (importedAssets.Any(s =>
            {
                s = Path.GetFileName(s);
                return TargetFileNames.Any(s.EndsWith);
            }))
            {
                EditorPrefs.SetBool(MustRegenerateAotHintKey, true);
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (EditorPrefs.GetBool(MustRegenerateAotHintKey))
            {
                EditorPrefs.SetBool(MustRegenerateAotHintKey, false);
                Debug.Log("Protobuf changed, updating AOT hint");
                AotHintFileUpdater.UpdateAotHint();
            }
        }
    }
}
