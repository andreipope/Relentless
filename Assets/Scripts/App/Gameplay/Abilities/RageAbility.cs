using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class RageAbility : AbilityBase
    {
        public int Value;

        public RageAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
        }

        protected override void ChangeRageStatusAction(bool status)
        {
            base.ChangeRageStatusAction(status);

            if (!PvPManager.UseBackendGameLogic)
            {
                if (status)
                {
                    AbilityUnitOwner.BuffedDamage += Value;
                    AbilityUnitOwner.AddToCurrentDamageHistory(Value, Enumerators.ReasonForValueChange.AbilityBuff);
                    InvokeActionTriggered(true);
                }
                else
                {
                    AbilityUnitOwner.BuffedDamage -= Value;
                    AbilityUnitOwner.AddToCurrentDamageHistory(-Value, Enumerators.ReasonForValueChange.AbilityBuff);
                    InvokeActionTriggered(false);
                }
            }
        }
    }
}
