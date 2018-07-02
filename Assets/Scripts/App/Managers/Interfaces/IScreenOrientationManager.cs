// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public interface IScreenOrientationManager
    {
        Enumerators.ScreenOrientationMode CurrentOrientation { get; }

        void SwitchOrientation(Enumerators.ScreenOrientationMode mode);
    }
}