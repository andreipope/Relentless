// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

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

            if (abilityCallType != Enumerators.AbilityCallType.AT_START)
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

        public override void Action(object info = null)
        {
            base.Action(info);

            var opponent = GetOpponentOverlord();
                UnityEngine.Debug.Log("__" + opponent.BoardCards.Count);
            foreach(var item in opponent.BoardCards)
            {
                item.DebuffDamage(damage);
                item.DebuffHealth(health);
            }
        }
    }
}
