using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public interface IApplicationSettingsManager
    {
        List<ResolutionInfo> Resolutions { get; }

        Enumerators.QualityLevel CurrentQualityLevel { get; }

        Enumerators.ScreenMode CurrentScreenMode { get; }

        ResolutionInfo CurrentResolution { get; }

        void SetDefaults();
        void ApplySettings();
        void SetResolution(ResolutionInfo info);
        void SetScreenMode(Enumerators.ScreenMode screenMode);
        void SetQuality(Enumerators.QualityLevel qualityLevel);
    }
}
