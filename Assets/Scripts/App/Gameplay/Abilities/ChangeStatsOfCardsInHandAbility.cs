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
        public Enumerators.StatType StatType { get; }

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

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY && AbilityActivityType != Enumerators.AbilityActivityType.PASSIVE)
                return;

            CheckSubTriggers();
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            CheckSubTriggers();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            CheckSubTriggers();
        }


        private void CheckSubTriggers()
        {

            List<WorkingCard> cards = new List<WorkingCard>();
            List<WorkingCard> targetCards = new List<WorkingCard>();
            List<ParametrizedAbilityBoardObject> parametrizedAbilityBoardObjects = new List<ParametrizedAbilityBoardObject>();

            foreach (Enumerators.AbilityTargetType type in AbilityData.AbilityTargetTypes)
            {
                switch (type)
                {
                    case Enumerators.AbilityTargetType.OPPONENT:
                        targetCards.AddRange(GetOpponentOverlord().CardsInHand.ToList());
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
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
                cards = InternalTools.GetRandomElementsFromList(targetCards.FindAll(x => x.LibraryCard.CardKind == TargetCardKind), Count);
            }

            foreach (WorkingCard card in cards)
            {
                SetStatOfTargetCard(card, AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.PermanentChanges);

                parametrizedAbilityBoardObjects.Add(new ParametrizedAbilityBoardObject(AbilityUnitOwner, new ParametrizedAbilityParameters()
                {
                    CardName = card.InstanceId.Id.ToString()
                }));
            }

            if (parametrizedAbilityBoardObjects.Count > 0)
            {
                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, parametrizedAbilityBoardObjects, AbilityData.AbilityType, Enumerators.AffectObjectType.Character);
            }

        }

        private void SetStatOfTargetCard(WorkingCard card, bool overrideStats = false)
        {
            if (overrideStats)
            {
                card.InstanceCard.Damage = Attack;
                card.InstanceCard.Health = Defense;
                card.InstanceCard.Cost = Cost;
            }
            else
            {
                card.InstanceCard.Damage += Attack;
                card.InstanceCard.Health += Defense;
                card.InstanceCard.Cost += Cost;
            }
        }
    }
}
