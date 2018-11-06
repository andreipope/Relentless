using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class HealTargetAbility : AbilityBase
    {
        public int Value { get; }
        public int Count { get; }

        public HealTargetAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            if(AbilityCallType != Enumerators.AbilityCallType.ENTRY &&
               AbilityActivityType != Enumerators.AbilityActivityType.PASSIVE)
                return;

            HealRandomCountOfAllies();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                InvokeActionTriggered();
            }
        }

        private void HealSelectedTarget()
        {
            BoardObject boardObject = AffectObjectType == Enumerators.AffectObjectType.Player ? (BoardObject)TargetPlayer : TargetUnit;

            HealTarget(boardObject, Value);

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
            {
                boardObject
            }, AbilityData.AbilityType, Utilites.CastStringTuEnum<Protobuf.AffectObjectType>(AffectObjectType.ToString()));

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                        Target = boardObject,
                        HasValue = true,
                        Value = AbilityData.Value
                    }
                }
            });
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            HealSelectedTarget();
        }

        private void HealRandomCountOfAllies()
        {
            List<BoardObject> allies = new List<BoardObject>();

            if (PredefinedTargets != null)
            {
                allies = PredefinedTargets;
            }
            else
            {
                if (AbilityData.AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.PLAYER_CARD))
                {
                    allies.AddRange(PlayerCallerOfAbility.BoardCards.Select(x => x.Model));
                    allies.Remove(AbilityUnitOwner);
                }

                if (AbilityData.AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.PLAYER))
                {
                    allies.Add(PlayerCallerOfAbility);
                }

                allies = InternalTools.GetRandomElementsFromList(allies, Count);
            }

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            int value = 0;
            foreach (BoardObject boardObject in allies)
            {
                switch(boardObject)
                {
                    case BoardUnitModel unit:
                        value = unit.MaxCurrentHp - unit.CurrentHp;
                        break;
                    case Player player:
                        value = player.MaxCurrentHp - player.Defense;
                        break;
                }

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                    Target = boardObject,
                    HasValue = true,
                    Value = value
                });

                HealTarget(boardObject, value);
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, allies,
                AbilityData.AbilityType, Utilites.CastStringTuEnum<Protobuf.AffectObjectType>(AffectObjectType.ToString()));

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCardsWithOverlord,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });
        }

        private void HealTarget(BoardObject boardObject, int value)
        {
            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.Player:
                    BattleController.HealPlayerByAbility(GetCaller(), AbilityData, (Player)boardObject, value);
                    break;
                case Enumerators.AffectObjectType.Character:
                    BattleController.HealUnitByAbility(GetCaller(), AbilityData, (BoardUnitModel)boardObject, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(AffectObjectType), AffectObjectType, null);
            }
        }
    }
}
