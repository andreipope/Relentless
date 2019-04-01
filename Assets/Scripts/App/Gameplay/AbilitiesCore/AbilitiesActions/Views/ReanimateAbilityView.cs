using System;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class ReanimateAbilityView : CardAbilityView
    {
        public override event Action VFXBegan;

        public override event Action VFXEnded;

        public override void BeginVFX(IReadOnlyList<BoardObject> targets = null, IReadOnlyList<GenericParameter> genericParameters = null)
        {
            VFXBegan?.Invoke();

            VFXEnded?.Invoke();
        }
    }
}
