using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GainGooAbility : AbilityBase
    {
        public int Count;

        public GainGooAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (GameplayManager.CurrentTurnPlayer == PlayerCallerOfAbility)
            {
                PlayerCallerOfAbility.CurrentGoo = Mathf.Clamp(PlayerCallerOfAbility.CurrentGoo + Count, 0, Constants.MaximumPlayerGoo);
            }
            else
            {
                PlayerCallerOfAbility.CurrentGooModificator += Count;
            }
        }
    }
}
