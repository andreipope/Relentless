using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class RestoreDefRandomlySplitAbility : AbilityBase
    {
        private List<BoardObject> _targets;

        public int Count;
        public List<Enumerators.Target> TargetTypes;

        public RestoreDefRandomlySplitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
            TargetTypes = ability.AbilityTarget;

            _targets = new List<BoardObject>();
        }

        public override void Activate()
        {
            base.Activate();

            AbilityUnitOwner.AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.Restore);

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }


        public override void Action(object info = null)
        {
            base.Action(info);

            FillRandomTargets();
            InvokeActionTriggered(_targets);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            SplitDefense();
        }

        private void FillRandomTargets()
        {
            if (PredefinedTargets != null)
            {
                _targets = PredefinedTargets.Select(x => x.BoardObject).ToList();
            }
            else
            {
                _targets = new List<BoardObject>();
                foreach (Enumerators.Target targetType in TargetTypes)
                {
                    switch (targetType)
                    {
                        case Enumerators.Target.OPPONENT:
                            _targets.Add(GetOpponentOverlord());
                            break;
                        case Enumerators.Target.PLAYER:
                            _targets.Add(PlayerCallerOfAbility);
                            break;
                        case Enumerators.Target.PLAYER_CARD:
                            _targets.AddRange(PlayerCallerOfAbility.BoardCards.Select(x => x.Model));
                            break;
                        case Enumerators.Target.OPPONENT_CARD:
                            _targets.AddRange(GetOpponentOverlord().BoardCards.Select(x => x.Model));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
                    }
                }
            }
        }

        private void SplitDefense()
        {
            if (_targets.Count == 0)
                return;

            if (PredefinedTargets != null)
            {
                foreach (var target in PredefinedTargets)
                {
                    RestoreDefenseOfTarget(target.BoardObject, target.Parameters.Defense);
                }

                return;
            }

            List<ParametrizedAbilityBoardObject> abilityTargets = new List<ParametrizedAbilityBoardObject>();

            int maxCount = Count;
            int defenseValue = 0;
            int blocksCount = _targets.Count;
            BoardObject currentTarget = null;

            int deltaDefense = 0;

            while (maxCount > 0)
            {
                currentTarget = _targets[UnityEngine.Random.Range(0, _targets.Count)];
               
                switch (currentTarget)
                {
                    case BoardUnitModel unit:
                        deltaDefense = unit.MaxCurrentDefense - unit.CurrentDefense;
                        break;
                    case Player player:
                        deltaDefense = player.MaxCurrentDefense - player.Defense;
                        break;
                }

                defenseValue = _targets.Count == 1 ?  maxCount : UnityEngine.Random.Range(1, maxCount);
                defenseValue = Mathf.Clamp(defenseValue, 0, deltaDefense);

                maxCount -= defenseValue;

                if (defenseValue > 0)
                {
                    RestoreDefenseOfTarget(currentTarget, defenseValue);
                }

                if (defenseValue == deltaDefense)
                {
                    _targets.Remove(currentTarget);
                }

                abilityTargets.Add(new ParametrizedAbilityBoardObject(
                    currentTarget,
                    new ParametrizedAbilityParameters
                    {
                        Defense = defenseValue
                    }
                ));

                if (_targets.Count == 0)
                {
                    maxCount = 0;
                }
            }

            InvokeUseAbilityEvent(abilityTargets);
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
