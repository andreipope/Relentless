using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class TakeDamageRandomEnemyAbility : AbilityBase
    {
        public int Damage { get; }

        public int Count { get; }

        public Enumerators.SetType SetType;

        private List<BoardObject> _targets;

        public TakeDamageRandomEnemyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Damage = ability.Damage;
            Count = ability.Count;
            SetType = ability.AbilitySetType;

            _targets = new List<BoardObject>();
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.END ||
          !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (PredefinedTargets != null)
            {
                _targets = PredefinedTargets.Select(x => x.BoardObject).ToList();
            }
            else
            {
                _targets = new List<BoardObject>();

                foreach (Enumerators.AbilityTargetType abilityTarget in AbilityData.AbilityTargetTypes)
                {
                    switch (abilityTarget)
                    {
                        case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                        case Enumerators.AbilityTargetType.OPPONENT_CARD:
                            _targets.AddRange(GetOpponentOverlord().BoardCards.Select(x => x.Model));
                            break;
                        case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:
                        case Enumerators.AbilityTargetType.PLAYER_CARD:
                            _targets.AddRange(PlayerCallerOfAbility.BoardCards.Select(x => x.Model));
                            break;
                        case Enumerators.AbilityTargetType.PLAYER:
                            _targets.Add(PlayerCallerOfAbility);
                            break;
                        case Enumerators.AbilityTargetType.OPPONENT:
                            _targets.Add(GetOpponentOverlord());
                            break;
                    }
                }

                _targets = InternalTools.GetRandomElementsFromList(_targets, Count);
            }

            InvokeActionTriggered(_targets);
            

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, _targets, AbilityData.AbilityType, Enumerators.AffectObjectType.Character);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            foreach (object target in _targets)
            {
                ActionCompleted(target);
            }
        }

        private void ActionCompleted(object target)
        {
            int damageOverride = Damage;

            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.ForEachFactionOfUnitInHand)
            {
                damageOverride = PlayerCallerOfAbility.CardsInHand.FindAll(x => x.LibraryCard.CardSetType == SetType).Count;
            }

            switch (target)
            {
                case Player player:
                    BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, player, damageOverride);
                    break;
                case BoardUnitModel unit:
                    BattleController.AttackUnitByAbility(GetCaller(), AbilityData, unit, damageOverride);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }
    }
}
