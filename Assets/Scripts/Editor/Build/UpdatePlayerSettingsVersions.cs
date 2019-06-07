using UnityEditor;
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

        public const string applicationIdentifierStaging = "games.loom.battleground-staging";

        public void OnPreprocessBuild(BuildReport report)
        {
            PlayerSettings.SplashScreen.showUnityLogo = false;

            #if USE_STAGING_BACKEND && !UNITY_IOS && !UNITY_ANDROID
                PlayerSettings.applicationIdentifier = applicationIdentifierStaging;
            #endif

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

            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.androidDebugMinification = AndroidMinification.None;
            EditorUserBuildSettings.androidReleaseMinification = AndroidMinification.Proguard;

#if UNITY_CLOUD_BUILD && ENABLE_DEBUG
            EditorUserBuildSettings.allowDebugging = true;
            EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Debug;
            EditorUserBuildSettings.androidBuildType = AndroidBuildType.Debug;
#endif
        }
    }
}
