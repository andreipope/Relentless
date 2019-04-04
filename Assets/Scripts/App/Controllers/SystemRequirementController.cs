using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SystemRequirementController : IController
    {
        public void Init()
        {
        }
        
        public void Update()
        {

        }

        public void Dispose()
        {

        }
        
        public void ResetAll()
        {

        }
        
        public bool CheckIfMeetMinimumSystemRequirement()
        {  
#if UNITY_EDITOR
            return true;
#endif
            
#if UNITY_ANDROID || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            if (SystemInfo.systemMemorySize < Constants.MinimumMemorySize)
            {
                return false;
            }
#endif

#if UNITY_ANDROID
            string apiNumberText = new string
            (
                SystemInfo.operatingSystem.Where(Char.IsDigit).ToArray()
            );
            int apiNumber;
            if(int.TryParse(apiNumberText, out apiNumber))
            {
                if(apiNumber < Constants.AndroidMinimumAPILevel)
                {
                    return false;
                }
            }
#endif

#if UNITY_IOS
            string[] textArray = Regex.Split(SystemInfo.operatingSystem, @"[^0-9\.]+")
                .Where(c => c != "." && c.Trim() != "").ToArray();
            if(textArray.Length > 0)
            {
                float iosVersion;
                if(float.TryParse(textArray[textArray.Length-1], out iosVersion))
                {
                    if(Mathf.FloorToInt(iosVersion) < Constants.IOSMinimumOSVersion)
                    {
                        return false;
                    }
                }
            }
#endif

#if UNITY_STANDALONE_OSX
            Regex pattern = new Regex(@"\d+(\.\d+)+");
            Match match = pattern.Match(SystemInfo.operatingSystem);
            string macOSVersion = match.Value;
            string[] versionNumberArray = macOSVersion.Split('.');
            
            int osVersion;
            int index = 0;
            foreach(string text in versionNumberArray)
            {
                if(int.TryParse(text,out osVersion))
                {
                    if(osVersion < Constants.MacOSMinimumOSVersion[index])
                    {
                        return false;
                    }
                }
                ++index;
            }
#endif

#if UNITY_STANDALONE_WIN
            string[] textArray = Regex.Split(SystemInfo.operatingSystem, @"[^0-9\.]+")
                .Where(c => c != "." && c.Trim() != "").ToArray();
            if (textArray.Length > 0)
            {
                float windowVersion;
                if(float.TryParse(textArray[0], out windowVersion))
                {
                    if(Mathf.FloorToInt(windowVersion) < Constants.WindowMinimumOSVersion)
                    {
                        return false;
                    }
                }
            }
#endif

            return true;
        }
    }
}