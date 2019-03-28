#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;

public class BuildProcessHandler : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private bool _wasStatusSplashScreen;

    public int callbackOrder { get { return 0; } }

    public void OnPostprocessBuild(BuildReport report)
    {
        PlayerSettings.SplashScreen.show = _wasStatusSplashScreen;
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        _wasStatusSplashScreen = PlayerSettings.SplashScreen.show;

       switch (report.summary.platformGroup)
       {
            case BuildTargetGroup.Android:
            case BuildTargetGroup.iOS:
                PlayerSettings.SplashScreen.show = false;
                break;
            case BuildTargetGroup.Standalone:
                PlayerSettings.SplashScreen.show = true;
                break;
       }
    }
}
#endif
