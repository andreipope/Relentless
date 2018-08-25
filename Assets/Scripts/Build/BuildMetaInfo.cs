using System;
using System.Collections;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class BuildMetaInfo : ScriptableObject
    {
        public const string ResourcesPath = "BuildMetaInfo";
        
        public string GitBranchName = "";
        public string GitCommitHash = "";
        public string BuildDateTime = "";
        public int BuildDayOfYear = 0;
        public int CloudBuildBuildNumber;
        public string CloudBuildTargetName = "";
        public string CloudBuildGitBranchName = "";
        public string CloudBuildGitCommitHash = "";

        public string ShortVersionName => Constants.CURRENT_VERSION_BASE + "." + BuildDayOfYear;

        public string DisplayVersionName => Constants.CURRENT_VERSION_BASE + "." + BuildDayOfYear;
        
        public string FullVersionName
        {
            get
            {
                string text = DisplayVersionName;

#if UNITY_CLOUD_BUILD
                if (!String.IsNullOrEmpty(CloudBuildGitCommitHash))
                {
                    text += $" ({CloudBuildGitCommitHash})";
                }
#else
                if (!String.IsNullOrEmpty(GitCommitHash))
                {
                    text += $" ({GitCommitHash}/{GitBranchName})";
                }
#endif
                return text;
            }
        }

        public Version Version
        {
            get
            {
                Version baseVersion = Version.Parse(Constants.CURRENT_VERSION_BASE);
                return new Version(baseVersion.Major, baseVersion.Minor, baseVersion.Build, BuildDayOfYear);
            }
        }
        
        public bool CheckBackendVersionMatch(Version backendVersion)
        {
            Version localVersion = Version;
            return
                localVersion.Major == backendVersion.Major &&
                localVersion.Minor == backendVersion.Minor &&
                localVersion.Build == backendVersion.Build;
        }
        
        private static BuildMetaInfo _instance;

        public static BuildMetaInfo Instance
        {
            get
            {
                if (!_instance)
                    _instance = Resources.Load<BuildMetaInfo>(ResourcesPath);
                
                return _instance;
            }
        }
    }
}