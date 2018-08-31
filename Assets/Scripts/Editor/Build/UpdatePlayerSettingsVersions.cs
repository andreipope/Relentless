using System;
using UnityEditor;
using UnityEditor.Build;

namespace LoomNetwork.CZB
{
    public class UpdatePlayerSettingsVersions : IPreprocessBuild
    {
        public int callbackOrder { get; } = 2;
        
        public void OnPreprocessBuild(BuildTarget target, string path)
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