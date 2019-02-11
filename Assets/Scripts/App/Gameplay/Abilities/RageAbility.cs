using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

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

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Character);
        }

        protected override void ChangeRageStatusAction(bool status)
        {
            base.ChangeRageStatusAction(status);

            if (!PvPManager.UseBackendGameLogic)
            {
                if (status)
                {
                    if (AbilityUnitOwner.CurrentHp < AbilityUnitOwner.MaxCurrentHp)
                    {
                        AbilityUnitOwner.BuffedDamage += Value;
                        AbilityUnitOwner.CurrentDamage += Value;
                        InvokeActionTriggered(true);
                    }
                }
                else
                {
                    if (AbilityUnitOwner.CurrentHp >= AbilityUnitOwner.MaxCurrentHp)
                    {
                        AbilityUnitOwner.BuffedDamage -= Value;
                        AbilityUnitOwner.CurrentDamage -= Value;
                        InvokeActionTriggered(false);
                    }
                }
            }
        }
    }
}
