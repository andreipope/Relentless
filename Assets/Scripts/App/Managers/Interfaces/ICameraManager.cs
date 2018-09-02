// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB.Gameplay
{
    public interface ICameraManager
    {
        bool IsFading { get; }

        Enumerators.FadeState CurrentFadeState { get; }

        void FadeIn(Action callback = null, int level = 0, bool isLastSibling = true);

        void FadeIn(float fadeTo, int level = 0, bool isLastSibling = true);

        void FadeOut(Action callback = null, int level = 0, bool immediately = false);
    }
}
