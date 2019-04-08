using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class ChangeStatsOfCardsInHandAbility : AbilityBase
    {
        public Enumerators.Stat StatType { get; }

        public Enumerators.CardKind TargetCardKind { get; }

        public int Attack { get; }

        public int Defense { get; }

        public int Cost { get; }

        public int Count { get;  }

        public ChangeStatsOfCardsInHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Attack = ability.Damage;
            Defense = ability.Defense;
            TargetCardKind = ability.TargetCardKind;
            Cost = ability.Cost;

            Count = Mathf.Clamp(ability.Count, 1, ability.Count);
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY && AbilityActivity != Enumerators.AbilityActivity.PASSIVE)
                return;

            CheckSubTriggers();
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            CheckSubTriggers();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            CheckSubTriggers();
        }


        private void CheckSubTriggers()
        {

            List<BoardUnitModel> cards = new List<BoardUnitModel>();
            List<BoardUnitModel> targetCards = new List<BoardUnitModel>();
            List<ParametrizedAbilityBoardObject> parametrizedAbilityBoardObjects = new List<ParametrizedAbilityBoardObject>();

            foreach (Enumerators.Target type in AbilityData.Targets)
            {
                switch (type)
                {
                    case Enumerators.Target.OPPONENT:
                        targetCards.AddRange(GetOpponentOverlord().CardsInHand.ToList());
                        break;
                    case Enumerators.Target.PLAYER:
                        targetCards.AddRange(PlayerCallerOfAbility.CardsInHand.ToList());
                        break;
                }
            }

            if (PredefinedTargets != null)
            {
                foreach (ParametrizedAbilityBoardObject boardObject in PredefinedTargets)
                {
                    cards.Add(targetCards.Find(x => x.InstanceId.Id == Convert.ToInt32(boardObject.Parameters.CardName)));
                }
            }
            else
            {
                cards = InternalTools.GetRandomElementsFromList(targetCards.FindAll(x => x.Prototype.Kind == TargetCardKind), Count);
            }

            foreach (BoardUnitModel card in cards)
            {
                SetStatOfTargetCard(card.Card, AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.PermanentChanges);

                parametrizedAbilityBoardObjects.Add(new ParametrizedAbilityBoardObject(AbilityUnitOwner, new ParametrizedAbilityParameters()
                {
                    CardName = card.InstanceId.Id.ToString()
                }));
            }

            if (parametrizedAbilityBoardObjects.Count > 0)
            {
                InvokeUseAbilityEvent(parametrizedAbilityBoardObjects);
            }
        }

        private void SetStatOfTargetCard(WorkingCard card, bool overrideStats = false)
        {
            if (overrideStats)
            {
                card.InstanceCard.Damage = Attack;
                card.InstanceCard.Defense = Defense;
                card.InstanceCard.Cost = Cost;
            }
            else
            {
                card.InstanceCard.Damage += Attack;
                card.InstanceCard.Defense += Defense;
                card.InstanceCard.Cost += Cost;
            }
        }
    }
}
