using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class ReanimateAbilityView : CardAbilityView
    {
        public override void DoVFXAction(IReadOnlyList<BoardObject> targets = null, IReadOnlyList<GenericParameter> genericParameters = null)
        {
            base.DoVFXAction(targets, genericParameters);
        }

        public override void EndVFXAction()
        {
            base.EndVFXAction();
        }
    }
}
