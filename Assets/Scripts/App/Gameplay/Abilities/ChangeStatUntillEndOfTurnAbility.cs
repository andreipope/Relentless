using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
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

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
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

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

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
