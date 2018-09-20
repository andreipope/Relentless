using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public interface IApplicationSettingsManager
    {
        List<ResolutionInfo> Resolutions { get; }

        Enumerators.QualityLevel CurrentQualityLevel { get; }

        Enumerators.ScreenMode CurrentScreenMode { get; }

        ResolutionInfo CurrentResolution { get; }

        void SetResolution(IntVector2 size);
        void SetScreenMode(Enumerators.ScreenMode screenMode);
        void SetQuality(Enumerators.QualityLevel qualityLevel);
    }
}
