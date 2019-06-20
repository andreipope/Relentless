using System;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Gameplay
{
    public interface ICameraManager
    {
        bool IsFading { get; }

        Enumerators.FadeState CurrentFadeState { get; }

        void FadeIn(Action callback = null, int level = 0, bool isLastSibling = true);

        void FadeIn(float fadeTo, int level = 0, bool isLastSibling = true);

        void FadeOut(Action callback = null, int level = 0, bool immediately = false);

        void ShakeGameplay(Enumerators.ShakeType type);
    }
}
