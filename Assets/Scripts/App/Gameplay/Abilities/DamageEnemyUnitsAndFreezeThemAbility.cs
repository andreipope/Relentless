using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DamageEnemyUnitsAndFreezeThemAbility : AbilityBase
    {
        public int Value;

        public DamageEnemyUnitsAndFreezeThemAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
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

            Player opponent = PlayerCallerOfAbility.Equals(GameplayManager.CurrentPlayer)?GameplayManager.OpponentPlayer:GameplayManager.CurrentPlayer;

            foreach (Enumerators.AbilityTargetType target in AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OpponentAllCards:

                        foreach (BoardUnit unit in opponent.BoardCards)
                        {
                            BattleController.AttackUnitByAbility(GetCaller(), AbilityData, unit);
                        }

                        foreach (BoardUnit unit in opponent.BoardCards)
                        {
                            unit.Stun(Enumerators.StunType.Freeze, Value);
                        }

                        break;

                    case Enumerators.AbilityTargetType.Opponent:
                        BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, opponent);
                        opponent.Stun(Enumerators.StunType.Freeze, Value);
                        break;
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
    }
}
