using System;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class BuildMetaInfo : ScriptableObject
    {
        public const string ResourcesPath = "BuildMetaInfo";

        private static BuildMetaInfo _instance;

        public string GitBranchName = "";

        public string GitCommitHash = "";

        // ReSharper disable once NotAccessedField.Global
        public string BuildDateTime = "";

        public int BuildDayOfYear;

        // ReSharper disable once NotAccessedField.Global
        public int CloudBuildBuildNumber;

        // ReSharper disable once NotAccessedField.Global
        public string CloudBuildTargetName = "";

        public string ShortVersionName => DisplayVersionName;

        public string DisplayVersionName => Constants.CurrentVersionBase + "." + BuildDayOfYear;

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
                Version baseVersion = Version.Parse(Constants.CurrentVersionBase);
                return new Version(baseVersion.Major, baseVersion.Minor, baseVersion.Build, BuildDayOfYear);
            }
        }

        public static BuildMetaInfo Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = Resources.Load<BuildMetaInfo>(ResourcesPath);
                }

                return _instance;
            }
        }

        public bool CheckBackendVersionMatch(Version backendVersion)
        {
            Version localVersion = Version;
            return localVersion.Major == backendVersion.Major && localVersion.Minor == backendVersion.Minor &&
                localVersion.Build == backendVersion.Build;
        }
    }
}
