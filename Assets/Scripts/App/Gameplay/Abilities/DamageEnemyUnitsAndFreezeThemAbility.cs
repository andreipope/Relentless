using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DamageEnemyUnitsAndFreezeThemAbility : AbilityBase
    {
        public int Value { get; }

        public DamageEnemyUnitsAndFreezeThemAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
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

            Player opponent = PlayerCallerOfAbility.Equals(GameplayManager.CurrentPlayer) ?
                GameplayManager.OpponentPlayer :
                GameplayManager.CurrentPlayer;

            foreach (Enumerators.AbilityTargetType target in AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:

                        foreach (BoardUnitView unit in opponent.BoardCards)
                        {
                            BattleController.AttackUnitByAbility(GetCaller(), AbilityData, unit.Model);
                        }

                        foreach (BoardUnitView unit in opponent.BoardCards)
                        {
                            unit.Model.Stun(Enumerators.StunType.FREEZE, Value);
                        }

                        break;

                    case Enumerators.AbilityTargetType.OPPONENT:
                        BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, opponent);
                        opponent.Stun(Enumerators.StunType.FREEZE, Value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }
    }
}
