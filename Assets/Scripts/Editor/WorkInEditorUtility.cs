#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class WorkInEditorUtility : MonoBehaviour
{
    [MenuItem("Utility/Editor/Prepare Editor To Work")]
    public static void PrepareEditorToWork()
    {
        BuildTargetGroup buildTarget = BuildTargetGroup.Unknown;

        switch(EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneOSX:
                buildTarget = BuildTargetGroup.Standalone;
                break;
            case BuildTarget.Android:
                buildTarget = BuildTargetGroup.Android;
                break;
            case BuildTarget.iOS:
                buildTarget = BuildTargetGroup.iOS;
                break;
        }

        string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);

        string stagingbackend = "USE_STAGING_BACKEND";

        if(!currentDefines.Contains(stagingbackend))
        {
            currentDefines += ";" + stagingbackend;
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, currentDefines);

        EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
    }
}
#endif
