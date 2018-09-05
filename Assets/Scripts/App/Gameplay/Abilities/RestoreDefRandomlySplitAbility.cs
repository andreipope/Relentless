using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class RestoreDefRandomlySplitAbility : AbilityBase
    {
        private List<object> _targets;

        public int Value;
        public List<Enumerators.AbilityTargetType> TargetTypes;

        public RestoreDefRandomlySplitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            TargetTypes = ability.AbilityTargetTypes;

            _targets = new List<object>();
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
            foreach(var targetType in TargetTypes)
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
                        _targets.AddRange(GameplayManager.CurrentPlayer.BoardCards);
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                        _targets.AddRange(GameplayManager.OpponentPlayer.BoardCards);
                        break;
                    default: break;
                }
            }
        }

        private void SplitDefense()
        {
            if (_targets.Count == 0)
                return;

            int maxCount = Value;
            int defenseValue = 0;
            int blocksCount = _targets.Count;
            object currentTarget = null;

            while (maxCount > 0)
            {
                defenseValue = UnityEngine.Random.Range(1, blocksCount > Value ? Value + 1 : _targets.Count);
                currentTarget = _targets[UnityEngine.Random.Range(0, _targets.Count)];
                RestoreDefenseOfTarget(currentTarget, defenseValue);
                maxCount -= defenseValue;
                _targets.Remove(currentTarget);
            }
        }

        private void RestoreDefenseOfTarget(object target, int defenseValue)
        {
            if(target is BoardUnit)
            {
                BattleController.HealUnitByAbility(AbilityUnitOwner, AbilityData, target as BoardUnit, defenseValue);
            }
            else if(target is Player)
            {
                BattleController.HealPlayerByAbility(AbilityUnitOwner, AbilityData, target as Player, defenseValue);
            }
        }
    }
}
