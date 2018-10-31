using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class RestoreDefRandomlySplitAbility : AbilityBase
    {
        private List<object> _targets;

        public int Count;
        public List<Enumerators.AbilityTargetType> TargetTypes;

        public RestoreDefRandomlySplitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
            TargetTypes = ability.AbilityTargetTypes;

            _targets = new List<object>();
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Character);

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }


        public override void Action(object info = null)
        {
            base.Action(info);

            FillRandomTargets();
            SplitDefense();
        }

        private void FillRandomTargets()
        {
            foreach (Enumerators.AbilityTargetType targetType in TargetTypes)
            {
                switch (targetType)
                {
                    case Enumerators.AbilityTargetType.OPPONENT:
                        _targets.Add(GameplayManager.OpponentPlayer);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        _targets.Add(GameplayManager.CurrentPlayer);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_CARD:
                        _targets.AddRange(GameplayManager.CurrentPlayer.BoardCards);
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                        _targets.AddRange(GameplayManager.OpponentPlayer.BoardCards);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
                }
            }
        }

        private void SplitDefense()
        {
            if (_targets.Count == 0)
                return;

            int maxCount = Count;
            int defenseValue = 0;
            int blocksCount = _targets.Count;
            object currentTarget = null;

            while (maxCount > 0)
            {
                if (_targets.Count == 1)
                {
                    defenseValue = maxCount;
                }
                else
                {
                    defenseValue = UnityEngine.Random.Range(1, blocksCount > Count ? maxCount : _targets.Count + 1);
                }

                currentTarget = _targets[UnityEngine.Random.Range(0, _targets.Count)];
                maxCount -= defenseValue;

                RestoreDefenseOfTarget(currentTarget, defenseValue);
                _targets.Remove(currentTarget);
            }
        }

        private void RestoreDefenseOfTarget(object target, int defenseValue)
        {
            switch (target)
            {
                case BoardUnitView unit:
                    BattleController.HealUnitByAbility(AbilityUnitOwner, AbilityData, unit.Model, defenseValue);
                    break;
                case Player player:
                    BattleController.HealPlayerByAbility(AbilityUnitOwner, AbilityData, player, defenseValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }
    }
}
