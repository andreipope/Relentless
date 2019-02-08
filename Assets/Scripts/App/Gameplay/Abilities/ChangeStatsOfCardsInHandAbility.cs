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

        public int Attack { get; }

        public int Defense { get; }

        public int Count { get;  }

        public ChangeStatsOfCardsInHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Attack = ability.Damage;
            Defense = ability.Defense;
            Count = Mathf.Clamp(ability.Count, 1, ability.Count);
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY || AbilityActivityType != Enumerators.AbilityActivityType.PASSIVE)
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
            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
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
                    foreach(ParametrizedAbilityBoardObject boardObject in PredefinedTargets)
                    {
                        cards.Add(targetCards.Find(x => x.InstanceId.Id == Convert.ToInt32(boardObject.Parameters.CardName)));
                    }
                }
                else
                {
                    cards = InternalTools.GetRandomElementsFromList(targetCards.FindAll(x => x.LibraryCard.CardKind == Enumerators.CardKind.CREATURE), Count);
                }

                foreach(WorkingCard card in cards)
                {
                    SetStatOfTargetCard(card);

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
        }

        private void SetStatOfTargetCard(WorkingCard card)
        {
            card.InstanceCard.Damage = Attack;
            card.InstanceCard.Health = Defense;
        }
    }
}
