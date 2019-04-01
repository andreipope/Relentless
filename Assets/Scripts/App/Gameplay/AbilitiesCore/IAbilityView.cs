using System;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public interface ICardAbilityView
    {
        event Action VFXBegan;

        event Action VFXEnded;

        void BeginVFX(IReadOnlyList<BoardObject> targets = null, IReadOnlyList<GenericParameter> genericParameters = null);
    }

    public abstract class CardAbilityView : ICardAbilityView
    {
        public abstract event Action VFXBegan;

        public abstract event Action VFXEnded;

        public abstract void BeginVFX(IReadOnlyList<BoardObject> targets = null, IReadOnlyList<GenericParameter> genericParameters = null);
    }
}
