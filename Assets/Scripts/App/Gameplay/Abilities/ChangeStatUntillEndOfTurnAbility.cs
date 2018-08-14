// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ChangeStatUntillEndOfTurnAbility : AbilityBase
    {
        public int health;
        public int damage;


        public ChangeStatUntillEndOfTurnAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            health = ability.health;
            damage = ability.damage;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.ENTRY)
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

            var opponent = GetOpponentOverlord();

            foreach (var unit in opponent.BoardCards)
            {
                if (unit.DamageDebuffUntillEndOfTurn != 0)
                {
                    unit.CurrentDamage += Mathf.Abs(unit.DamageDebuffUntillEndOfTurn);
                    unit.DamageDebuffUntillEndOfTurn = 0;
                }
                if (unit.HPDebuffUntillEndOfTurn != 0)
                {
                    unit.CurrentHP += Mathf.Abs(unit.HPDebuffUntillEndOfTurn);
                    unit.HPDebuffUntillEndOfTurn = 0;
                }
            }

            _abilitiesController.DeactivateAbility(activityId);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            var opponent = GetOpponentOverlord();

            foreach(var unit in opponent.BoardCards)
            {
                if(damage != 0)
                {
                    unit.DamageDebuffUntillEndOfTurn += damage;
                    var buffresult = unit.CurrentDamage + damage;

                    if (buffresult < 0)
                        unit.DamageDebuffUntillEndOfTurn -= buffresult;
                    unit.CurrentDamage += damage;
                }

                if (health != 0)
                {
                    unit.HPDebuffUntillEndOfTurn += health;
                    unit.CurrentHP += health;
                }
            }
        }
    }
}
