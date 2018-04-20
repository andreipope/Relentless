using System;
using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using UnityEngine;

namespace GrandDevs.CZB.Gameplay
{
    public interface ICameraManager
    {
        bool IsFading { get; }
        Enumerators.FadeState CurrentFadeState { get; }

		void FadeIn(Action callback = null);
		void FadeIn(float fadeTo);
        void FadeOut(Action callback = null);
    }
}