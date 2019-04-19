using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SummonFromHandAbility : AbilityBase
    {
        public int Value { get; }
        public int Count { get; }
        public Enumerators.Faction Faction { get; }

        public SummonFromHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Count = ability.Count;
            Faction = ability.Faction;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();
            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            Action();
        }


        public override void Action(object info = null)
        {
            base.Action(info);

            List<HandBoardCard> boardCards = new List<HandBoardCard>();
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            if (!HasEmptySpaceOnBoard(PlayerCallerOfAbility, out int emptyFields))
                return;

            List<CardModel> cards = PlayerCallerOfAbility.PlayerCardsController.CardsInHand.
                FindAll(card => card.Prototype.Kind == Enumerators.CardKind.CREATURE);

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.HighestCost)
            {
                cards = cards.OrderByDescending(item => item.Card.InstanceCard.Cost).ToList();
                cards = cards.GetRange(0, Mathf.Clamp(Count, Mathf.Min(cards.Count, Count), cards.Count));
            }
            else
            {
                cards = cards.FindAll(x => x.Card.InstanceCard.Cost <= Value);

                if (Faction != Enumerators.Faction.Undefined)
                {
                    cards = cards.FindAll(x => x.Card.Prototype.Faction == Faction);
                }

                cards = GetRandomElements(cards, Count);
            }

            if (cards.Count == 0)
                return;

            List<IBoardObject> targets = new List<IBoardObject>();

            for (int i = 0; i < Mathf.Min(emptyFields, cards.Count); i++)
            {
                if (cards[i].Owner.IsLocalPlayer)
                {
                    BoardCardView cardView = BattlegroundController.GetCardViewByModel<BoardCardView>(cards[i]);
                    PutCardFromHandToBoard(PlayerCallerOfAbility, cardView, ref targetEffects, ref boardCards, true);
                }
                else
                {
                    HandBoardCard cardHand = BattlegroundController.CreateCustomHandBoardCard(cards[i]).HandBoardCard;
                    PutCardFromHandToBoard(PlayerCallerOfAbility, cardHand.BoardCardView, ref targetEffects, ref boardCards, false);
                }
            }

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = AbilityUnitOwner,
                TargetEffects = targetEffects
            });
        }


        private void PutCardFromHandToBoard(Player owner, BoardCardView boardCardView,
            ref List<PastActionsPopup.TargetEffectParam> targetEffects, ref List<HandBoardCard> cards, bool activateAbility)
        {
            owner.PlayerCardsController.SummonUnitFromHand(boardCardView, activateAbility);
            cards.Add(boardCardView.HandBoardCard);
            targetEffects.Add(new PastActionsPopup.TargetEffectParam
            {
                ActionEffectType = Enumerators.ActionEffectType.PlayFromHand,
                Target = boardCardView.HandBoardCard
            });
        }
    }
}
