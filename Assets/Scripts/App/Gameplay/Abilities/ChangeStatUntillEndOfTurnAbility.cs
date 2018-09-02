using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ChangeStatUntillEndOfTurnAbility : AbilityBase
    {
        public int Health;

        public int Damage;

        public ChangeStatUntillEndOfTurnAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Health = ability.Health;
            Damage = ability.Damage;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.Entry)

                return;

            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            Player opponent = GetOpponentOverlord();

            foreach (BoardUnit unit in opponent.BoardCards)
            {
                if (Damage != 0)
                {
                    unit.DamageDebuffUntillEndOfTurn += Damage;
                    int buffresult = unit.CurrentDamage + Damage;

                    if (buffresult < 0)
                    {
                        unit.DamageDebuffUntillEndOfTurn -= buffresult;
                    }

                    unit.CurrentDamage += Damage;
                }

                if (Health != 0)
                {
                    unit.HpDebuffUntillEndOfTurn += Health;
                    unit.CurrentHp += Health;
                }
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }

        protected override void OnEndTurnEventHandler()
        {
            base.OnEndTurnEventHandler();

            Player opponent = GetOpponentOverlord();

            foreach (BoardUnit unit in opponent.BoardCards)
            {
                if (unit.DamageDebuffUntillEndOfTurn != 0)
                {
                    unit.CurrentDamage += Mathf.Abs(unit.DamageDebuffUntillEndOfTurn);
                    unit.DamageDebuffUntillEndOfTurn = 0;
                }

                if (unit.HpDebuffUntillEndOfTurn != 0)
                {
                    unit.CurrentHp += Mathf.Abs(unit.HpDebuffUntillEndOfTurn);
                    unit.HpDebuffUntillEndOfTurn = 0;
                }
            }

            AbilitiesController.DeactivateAbility(ActivityId);
        }
    }
}
