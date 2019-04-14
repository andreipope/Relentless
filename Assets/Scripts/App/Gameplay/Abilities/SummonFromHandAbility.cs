using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class SummonFromHandAbility : AbilityBase
    {
        public int Value { get; }
        public int Count { get; }
        public Enumerators.Faction Faction { get; }

        private IGameplayManager _gameplayManager;
        private AbilitiesController _abilitiesController;

        public SummonFromHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Count = ability.Count;
            Faction = ability.Faction;

            _gameplayManager = GameClient.Get<IGameplayManager>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }


        public override void Action(object info = null)
        {
            base.Action(info);

            List<HandBoardCard> boardCards = new List<HandBoardCard>();
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            if (PredefinedTargets != null)
            {
                IReadOnlyList<HandBoardCard> boardCardsTargets =
                    PredefinedTargets
                        .Select(x => x.BoardObject as CardModel)
                        .Select(x => BattlegroundController.CreateCustomHandBoardCard(x).HandBoardCard)
                        .ToList();

                foreach (HandBoardCard target in boardCardsTargets)
                {
                    PutCardFromHandToBoard(target.OwnerPlayer, target.BoardCardView, ref targetEffects, ref boardCards, false);
                }
                return;
            }

            if (PlayerCallerOfAbility.CardsOnBoard.Count >= Constants.MaxBoardUnits)
                return;

            IReadOnlyList<CardModel> cards = GameplayManager.CurrentPlayer.CardsInHand.FindAll(
                x => x.Card.InstanceCard.Cost <= Value &&
                    x.Card.Prototype.Kind == Enumerators.CardKind.CREATURE
            );

            if (Faction != Enumerators.Faction.Undefined)
            {
                cards = cards.FindAll(x => x.Card.Prototype.Faction == Faction);
            }

            cards = InternalTools.GetRandomElementsFromList(cards, Count).ToUniqueList();

            if (cards.Count == 0)
                return;

            List<IBoardObject> targets = new List<IBoardObject>();

            for (int i = 0; i < cards.Count; i++)
            {
                if (PlayerCallerOfAbility.CardsOnBoard.Count >= Constants.MaxBoardUnits)
                    break;

                BoardCardView cardView = BattlegroundController.GetCardViewByModel<BoardCardView>(cards[i]);
                PutCardFromHandToBoard(PlayerCallerOfAbility, cardView, ref targetEffects, ref boardCards, true);
            }

            InvokeUseAbilityEvent(
                boardCards
                    .Select(target => new ParametrizedAbilityBoardObject(target))
                    .ToList()
            );

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
