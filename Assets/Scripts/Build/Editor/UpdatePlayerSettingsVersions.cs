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

            PlayerSettings.bundleVersion = buildMetaInfo.ShortVersionName;
            PlayerSettings.macOS.buildNumber = buildMetaInfo.CloudBuildBuildNumber.ToString();
            PlayerSettings.iOS.buildNumber = buildMetaInfo.CloudBuildBuildNumber.ToString();
            PlayerSettings.Android.bundleVersionCode = buildMetaInfo.CloudBuildBuildNumber;
#endif
        }
    }
}