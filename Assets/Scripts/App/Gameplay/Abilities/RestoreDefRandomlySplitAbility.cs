using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class RestoreDefRandomlySplitAbility : AbilityBase
    {
        private List<BoardObject> _targets;

        public int Value;
        public List<Enumerators.AbilityTargetType> TargetTypes;

        public RestoreDefRandomlySplitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            TargetTypes = ability.AbilityTargetTypes;

            _targets = new List<BoardObject>();
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

            FillRandomTargets();
            SplitDefense();
        }

        private void FillRandomTargets()
        {
            foreach(Enumerators.AbilityTargetType targetType in TargetTypes)
            {
                switch(targetType)
                {
                    case Enumerators.AbilityTargetType.OPPONENT:
                        _targets.Add(GameplayManager.OpponentPlayer);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        _targets.Add(GameplayManager.CurrentPlayer);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_CARD:
                        _targets.AddRange(GameplayManager.CurrentPlayer.BoardCards.Select(x => x.Model));
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                        _targets.AddRange(GameplayManager.OpponentPlayer.BoardCards.Select(x => x.Model));
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

            List<ParametrizedAbilityBoardObject> abilityTargets = new List<ParametrizedAbilityBoardObject>();

            int maxCount = Value;
            int defenseValue = 0;
            int blocksCount = _targets.Count;
            BoardObject currentTarget = null;

            while (maxCount > 0)
            {
                defenseValue = UnityEngine.Random.Range(1, blocksCount > Value ? Value + 1 : _targets.Count);
                currentTarget = _targets[UnityEngine.Random.Range(0, _targets.Count)];
                RestoreDefenseOfTarget(currentTarget, defenseValue);
                maxCount -= defenseValue;
                _targets.Remove(currentTarget);

                abilityTargets.Add(new ParametrizedAbilityBoardObject()
                {
                    BoardObject = currentTarget,
                    Parameters = new ParametrizedAbilityBoardObject.AbilityParameters()
                    {
                        Defense = defenseValue
                    }
                });
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, abilityTargets, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }

        private void RestoreDefenseOfTarget(object target, int defenseValue)
        {
            switch (target)
            {
                case BoardUnitModel unit:
                    BattleController.HealUnitByAbility(AbilityUnitOwner, AbilityData, unit, defenseValue);
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
