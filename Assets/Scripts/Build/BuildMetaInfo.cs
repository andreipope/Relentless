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
        public string GitHash = "";
        public string BuildDateTime = "";
        public string CloudBuildBuildNumber;
        public string CloudBuildTargetName = "";
        public string CloudBuildGitBranchName = "";

        public string ShortVersionName
        {
            get
            {
                string text = Constants.CURRENT_VERSION_BASE;

#if UNITY_CLOUD_BUILD
                text += "b" + CloudBuildBuildNumber;
#endif
                return text;
            }
        }
        
        public string DisplayVersionName
        {
            get
            {
                string text = Constants.CURRENT_VERSION_BASE;

#if UNITY_CLOUD_BUILD
                text += "b" + CloudBuildBuildNumber;
#endif
                return text;
            }
        }
        
        public string FullVersionName
        {
            get
            {
                string text = Constants.CURRENT_VERSION_BASE;

#if UNITY_CLOUD_BUILD
                text += "b" + CloudBuildBuildNumber;
#else
                if (!String.IsNullOrEmpty(GitHash))
                {
                    text += $"@{GitHash}/{GitBranchName}";
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
                
#if UNITY_CLOUD_BUILD
                int buildNumber = int.Parse(CloudBuildBuildNumber);
#else
                int buildNumber = 0;
#endif
                return new Version(baseVersion.Major, baseVersion.Minor, baseVersion.Build, buildNumber);
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