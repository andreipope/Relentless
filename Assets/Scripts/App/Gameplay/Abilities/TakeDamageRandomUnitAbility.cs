// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class TakeDamageRandomUnitAbility : AbilityBase
    {
        public int value = 0;

        public TakeDamageRandomUnitAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
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

        protected override void UnitOnAttackEventHandler(object info)
        {
            base.UnitOnAttackEventHandler(info);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            var opponent = playerCallerOfAbility.Equals(_gameplayManager.CurrentPlayer) ? _gameplayManager.OpponentPlayer : _gameplayManager.CurrentPlayer;

            if (opponent.BoardCards.Count > 0)
            {
                var randomUnit = opponent.BoardCards[UnityEngine.Random.Range(0, opponent.BoardCards.Count)];

                _battleController.AttackCreatureByAbility(GetCaller(), abilityData, randomUnit);
            }
        }
    }
}
