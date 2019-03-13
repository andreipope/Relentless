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

            InvokeUseAbilityEvent();

            if (AbilityCallType == Enumerators.AbilityCallType.ENTRY)
            {
                Action();
            }
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();
            if (AbilityCallType != Enumerators.AbilityCallType.END ||
          !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility) || (AbilityUnitOwner != null && AbilityUnitOwner.IsStun))
                return;
            Action();
        }

        protected override void UnitDiedHandler()
        {
            if (AbilityCallType != Enumerators.AbilityCallType.DEATH) {
                base.UnitDiedHandler();
                return;
            }

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardObject> possibleTargets = new List<BoardObject>();

            foreach (Enumerators.AbilityTargetType abilityTarget in AbilityData.AbilityTargetTypes)
            {
                switch (abilityTarget)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                        possibleTargets.AddRange(GetOpponentOverlord().BoardCards
                            .FindAll(unit => unit.Model.CurrentHp > 0)
                            .Select(unit => unit.Model));
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:
                    case Enumerators.AbilityTargetType.PLAYER_CARD:
                        possibleTargets.AddRange(PlayerCallerOfAbility.BoardCards
                            .FindAll(unit => unit.Model.CurrentHp > 0)
                            .Select(unit => unit.Model));
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        possibleTargets.Add(PlayerCallerOfAbility);
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT:
                        possibleTargets.Add(GetOpponentOverlord());
                        break;
                }
            }

            _targets = new List<BoardObject>();
            int count = Mathf.Max(1, Count);
            while (count > 0 && possibleTargets.Count > 0)
            {   
                int chosenIndex = MTwister.IRandom(0, possibleTargets.Count-1);
                _targets.Add(possibleTargets[chosenIndex]);
                possibleTargets.RemoveAt(chosenIndex);
                count--;
            }

            InvokeActionTriggered(_targets);      
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            int damageWas = -1;
            foreach (object target in _targets)
            {
                ActionCompleted(target, out damageWas);

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                    Target = target,
                    HasValue = true,
                    Value = -damageWas
                });
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });
        }

        private void ActionCompleted(object target, out int damageWas)
        {
            if (AbilityCallType == Enumerators.AbilityCallType.DEATH) {
                base.UnitDiedHandler();
            }
            
            int damageOverride = Mathf.Max(1, Damage);

            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.ForEachFactionOfUnitInHand)
            {
                damageOverride = PlayerCallerOfAbility.CardsInHand.FindAll(x => x.Prototype.CardSetType == SetType).Count;
            }

            damageWas = damageOverride;

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
