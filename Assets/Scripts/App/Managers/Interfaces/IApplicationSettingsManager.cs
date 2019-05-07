using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public interface IApplicationSettingsManager
    {
        List<ResolutionInfo> Resolutions { get; }

        Enumerators.ScreenMode CurrentScreenMode { get; }

        ResolutionInfo CurrentResolution { get; }
        
        event Action OnResolutionChanged;

        void SetDefaults();
        void ApplySettings();
#if !UNITY_ANDROID && !UNITY_IOS
        void SetResolution(ResolutionInfo info);
        void SetScreenMode(Enumerators.ScreenMode screenMode);
#endif
    }
}
