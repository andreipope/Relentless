using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStatUntillEndOfTurnAbility : AbilityBase
    {
        public int Health { get; }

        public int Damage { get; }

        public ChangeStatUntillEndOfTurnAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Health = ability.Health;
            Damage = ability.Damage;
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Types.Enum.Character);

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            Player opponent = GetOpponentOverlord();

            foreach (BoardUnitView unit in opponent.BoardCards)
            {
                if (Damage != 0)
                {
                    unit.Model.DamageDebuffUntillEndOfTurn += Damage;
                    int buffresult = unit.Model.CurrentDamage + Damage;

                    if (buffresult < 0)
                    {
                        unit.Model.DamageDebuffUntillEndOfTurn -= buffresult;
                    }

                    unit.Model.CurrentDamage += Damage;
                }

                if (Health != 0)
                {
                    unit.Model.HpDebuffUntillEndOfTurn += Health;
                    unit.Model.CurrentHp += Health;
                }
            }
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            Player opponent = GetOpponentOverlord();

            foreach (BoardUnitView unit in opponent.BoardCards)
            {
                if (unit.Model.DamageDebuffUntillEndOfTurn != 0)
                {
                    unit.Model.CurrentDamage += Mathf.Abs(unit.Model.DamageDebuffUntillEndOfTurn);
                    unit.Model.DamageDebuffUntillEndOfTurn = 0;
                }

                if (unit.Model.HpDebuffUntillEndOfTurn != 0)
                {
                    unit.Model.CurrentHp += Mathf.Abs(unit.Model.HpDebuffUntillEndOfTurn);
                    unit.Model.HpDebuffUntillEndOfTurn = 0;
                }
            }

            AbilitiesController.DeactivateAbility(ActivityId);
        }
    }
}
