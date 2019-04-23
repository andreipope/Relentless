using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class DiscardCardFromHandAbility : AbilityBase
    {
        private int Count { get; }

        public DiscardCardFromHandAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardUnitModel> cards = new List<BoardUnitModel>();

            if (PredefinedTargets != null)
            {
                cards.AddRange(PredefinedTargets.Select(x => x.BoardObject).Cast<BoardUnitModel>());
            }
            else
            {
                foreach (Enumerators.Target target in AbilityTargets)
                {
                    switch (target)
                    {
                        case Enumerators.Target.PLAYER_CARD:
                            cards.AddRange(PlayerCallerOfAbility.PlayerCardsController.CardsInHand);
                            break;
                        case Enumerators.Target.OPPONENT_CARD:
                            cards.AddRange(GetOpponentOverlord().PlayerCardsController.CardsInHand);
                            break;
                    }
                }

                if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
                {
                    cards = GetRandomElements(cards, Count);
                }
            }

            if (cards.Count > 0)
            {
                List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

                foreach (BoardUnitModel boardUnit in cards)
                {
                    CardsController.DiscardCardFromHand(boardUnit);

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.CardDiscard,
                        Target = boardUnit
                    });
                }

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                    Caller = GetCaller(),
                    TargetEffects = TargetEffects
                });

                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>(cards.Select(item => new ParametrizedAbilityBoardObject(item))));
            }
        }
    }
}
