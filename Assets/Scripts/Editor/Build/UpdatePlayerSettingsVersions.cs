using UnityEditor.Build;
using UnityEditor.Build.Reporting;

#if UNITY_CLOUD_BUILD
using UnityEditor;
#if UNITY_IOS
using System;
#endif
#endif

namespace Loom.ZombieBattleground.Editor
{
    public class UpdatePlayerSettingsVersions : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; } = 2;

        public void OnPreprocessBuild(BuildReport report)
        {
#if UNITY_CLOUD_BUILD
            BuildMetaInfo buildMetaInfo = BuildMetaInfo.Instance;

#if UNITY_IOS
            Version version = buildMetaInfo.Version;
            PlayerSettings.bundleVersion = $"{version.Major}.{version.Minor}.{version.Build}";
#else
            PlayerSettings.bundleVersion = buildMetaInfo.ShortVersionName;
#endif            
            PlayerSettings.macOS.buildNumber = buildMetaInfo.CloudBuildBuildNumber.ToString();
            PlayerSettings.iOS.buildNumber = buildMetaInfo.CloudBuildBuildNumber.ToString();
            PlayerSettings.Android.bundleVersionCode = buildMetaInfo.CloudBuildBuildNumber;
#endif
        }
    }
}
