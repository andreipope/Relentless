using System;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class BuildMetaInfo : ScriptableObject
    {
        public const string KResourcesPath = "BuildMetaInfo";

        private static BuildMetaInfo _instance;

        public string GitBranchName = "";

        public string GitCommitHash = "";

        public string BuildDateTime = "";

        public int BuildDayOfYear = 0;

        public int CloudBuildBuildNumber;

        public string CloudBuildTargetName = "";

        public string ShortVersionName => DisplayVersionName;

        public string DisplayVersionName => Constants.KCurrentVersionBase + "." + BuildDayOfYear;

        public string FullVersionName
        {
            get
            {
                string text = DisplayVersionName;

                if (!string.IsNullOrEmpty(GitCommitHash))
                {
#if UNITY_CLOUD_BUILD
                    text += $" ({GitCommitHash})";
#else
                    text += $" ({GitCommitHash}/{GitBranchName})";
#endif
                }

                return text;
            }
        }

        public Version Version
        {
            get
            {
                Version baseVersion = Version.Parse(Constants.KCurrentVersionBase);
                return new Version(baseVersion.Major, baseVersion.Minor, baseVersion.Build, BuildDayOfYear);
            }
        }

        public static BuildMetaInfo Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = Resources.Load<BuildMetaInfo>(KResourcesPath);
                }

                return _instance;
            }
        }

        public bool CheckBackendVersionMatch(Version backendVersion)
        {
            Version localVersion = Version;
            return (localVersion.Major == backendVersion.Major) && (localVersion.Minor == backendVersion.Minor) && (localVersion.Build == backendVersion.Build);
        }
    }
}
