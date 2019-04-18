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
        private List<CardModel> _affectedCards;

        public Enumerators.Stat StatType { get; }

        public Enumerators.CardKind TargetCardKind { get; }

        public int Attack { get; }

        public int Defense { get; }

        public int Cost { get; }

        public int Count { get;  }

        private bool _lastAuraActive;

        public ChangeStatsOfCardsInHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Attack = ability.Damage;
            Defense = ability.Defense;
            TargetCardKind = ability.TargetKind;
            Cost = ability.Cost;

            Count = Mathf.Clamp(ability.Count, 1, ability.Count);

            _affectedCards = new List<CardModel>();
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY || AbilityActivity != Enumerators.AbilityActivity.PASSIVE)
                return;

            CheckSubTriggers();
        }

        protected override void UnitAttackedHandler(IBoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            CheckSubTriggers();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            _affectedCards.ForEach(ResetStatsOfTargetCard);

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            CheckSubTriggers();
        }

        protected override void ChangeAuraStatusAction(bool status)
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            _lastAuraActive = status;
            if (status)
            {
                CheckSubTriggers();
            }
            else
            {
                _affectedCards?.ForEach(ResetStatsOfTargetCard);
            }
        }

        protected override void HandChangedHandler(int count)
        {

            if (_lastAuraActive) 
            {
                _affectedCards?.ForEach(ResetStatsOfTargetCard);
                CheckSubTriggers();
            }
        }

        private void CheckSubTriggers()
        {
            List<CardModel> cards = new List<CardModel>();
            List<CardModel> targetCards = new List<CardModel>();

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

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                cards = GetRandomElements(targetCards.FindAll(x => x.Prototype.Kind == TargetCardKind ||
                                                                    TargetCardKind == Enumerators.CardKind.UNDEFINED), Count);
            }
            else
            {
                cards = targetCards;
            }

            _affectedCards.Clear();

            foreach (CardModel card in cards)
            {
                SetStatOfTargetCard(card, AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.PermanentChanges);
            }

        }

        private void SetStatOfTargetCard(CardModel card, bool overrideStats = false)
        {
            _affectedCards.Add(card);
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


            BoardCardView boardCardView = BattlegroundController.GetCardViewByModel<BoardCardView>(card);
            boardCardView?.UpdateCardCost();
        }

        private void ResetStatsOfTargetCard(CardModel card)
        {
            card.InstanceCard.Damage = card.Prototype.Damage;
            card.InstanceCard.Defense = card.Prototype.Defense;
            card.InstanceCard.Cost = card.Prototype.Cost;

            BoardCardView boardCardView = BattlegroundController.GetCardViewByModel<BoardCardView>(card);
            boardCardView?.UpdateCardCost();
        }
    }
}
