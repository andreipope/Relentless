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

        public Enumerators.Faction Faction;

        private List<BoardObject> _targets;

        private int _calculatedDamage;

        public TakeDamageRandomEnemyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Damage = ability.Damage;
            Count = ability.Count;
            Faction = ability.Faction;

            _targets = new List<BoardObject>();
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger == Enumerators.AbilityTrigger.ENTRY)
            {
                Action();
            }
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.TURN ||
        !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility) || (AbilityUnitOwner != null && AbilityUnitOwner.IsStun))
                return;
            Action();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();
            if (AbilityTrigger != Enumerators.AbilityTrigger.END ||
          !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility) || (AbilityUnitOwner != null && AbilityUnitOwner.IsStun) || (AbilityUnitOwner != null && AbilityUnitOwner.IsDead) || (AbilityUnitOwner != null && AbilityUnitOwner.CurrentDefense <= 0))
                return;
            Action();
        }

        protected override void UnitDiedHandler()
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH) 
            {
                base.UnitDiedHandler();
                return;
            }

            Action();
        }

        protected override void ChangeRageStatusAction(bool status)
        {
            base.ChangeRageStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.RAGE)
                return;

            if (status)
            {
                Action();
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardObject> possibleTargets = new List<BoardObject>();

            foreach (Enumerators.Target abilityTarget in AbilityData.Targets)
            {
                switch (abilityTarget)
                {
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                    case Enumerators.Target.OPPONENT_CARD:
                        possibleTargets.AddRange(GetOpponentOverlord().CardsOnBoard
                            .FindAll(unit => unit.CurrentDefense > 0 && !unit.IsDead && unit.IsUnitActive));
                        break;
                    case Enumerators.Target.PLAYER_ALL_CARDS:
                    case Enumerators.Target.PLAYER_CARD:
                        possibleTargets.AddRange(PlayerCallerOfAbility.CardsOnBoard
                            .FindAll(unit => unit.CurrentDefense > 0 && !unit.IsDead && unit.IsUnitActive));
                        break;
                    case Enumerators.Target.PLAYER:
                        possibleTargets.Add(PlayerCallerOfAbility);
                        break;
                    case Enumerators.Target.OPPONENT:
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

            _calculatedDamage  = Mathf.Max(1, Damage);

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.ForEachFactionOfUnitInHand)
            {
                _calculatedDamage = PlayerCallerOfAbility.CardsInHand.FindAll(x => x.Prototype.Faction == Faction).Count;
            }

            foreach (BoardObject target in _targets)
            {
                switch (target)
                {
                    case BoardUnitModel unit:
                        unit.HandleDefenseBuffer(_calculatedDamage);
                        break;
                }
            }

            InvokeActionTriggered(_targets);      
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            int damageWas = -1;
            foreach (object target in _targets)
            {
                ActionCompleted(target, out damageWas);

                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
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
                TargetEffects = targetEffects
            });

            if (AbilityTrigger == Enumerators.AbilityTrigger.DEATH) 
            {
                base.UnitDiedHandler();
            }
        }

        private void ActionCompleted(object target, out int damageWas)
        {
            if (AbilityTrigger == Enumerators.AbilityTrigger.DEATH) {
                base.UnitDiedHandler();
            }
            
            damageWas = _calculatedDamage;

            switch (target)
            {
                case Player player:
                    BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, player, _calculatedDamage);
                    break;
                case BoardUnitModel unit:
                    BattleController.AttackUnitByAbility(GetCaller(), AbilityData, unit, _calculatedDamage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }
    }
}
