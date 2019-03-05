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
        public Enumerators.Faction SetType { get; }

        private IGameplayManager _gameplayManager;
        private AbilitiesController _abilitiesController;

        public SummonFromHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Count = ability.Count;
            SetType = ability.AbilitySetType;

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
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            if (PredefinedTargets != null)
            {
                IEnumerable<HandBoardCard> boardCardsTargets = PredefinedTargets.Select(x => x.BoardObject as HandBoardCard);

                foreach (HandBoardCard target in boardCardsTargets)
                {
                    PutCardFromHandToBoard(target.OwnerPlayer, target.CardView, ref TargetEffects, ref boardCards, false);
                }
                return;
            }

            if (PlayerCallerOfAbility.BoardCards.Count >= Constants.MaxBoardUnits)
                return;

            IReadOnlyList<BoardCard> cards = BattlegroundController.PlayerHandCards.FindAll(
                x => x.WorkingCard.InstanceCard.Cost <= Value &&
                    x.LibraryCard.CardKind == Enumerators.CardKind.CREATURE);

            cards = cards.FindAll(x => x.LibraryCard.Faction == SetType);

            cards = InternalTools.GetRandomElementsFromList(cards, Count).ToUniqueList();

            if (cards.Count == 0)
                return;

            List<BoardObject> targets = new List<BoardObject>();

            for (int i = 0; i < cards.Count; i++)
            {
                if (PlayerCallerOfAbility.BoardCards.Count >= Constants.MaxBoardUnits)
                    break;

                PutCardFromHandToBoard(PlayerCallerOfAbility, cards[i], ref TargetEffects, ref boardCards, true);
            }

            InvokeUseAbilityEvent(
                boardCards
                    .Select(target => new ParametrizedAbilityBoardObject(target))
                    .ToList()
            );

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });
        }


        private void PutCardFromHandToBoard(Player owner, BoardCard boardCard,
            ref List<PastActionsPopup.TargetEffectParam> TargetEffects, ref List<HandBoardCard> cards, bool activateAbility)
        {
            CardsController.SummonUnitFromHand(owner, boardCard, activateAbility);
            cards.Add(boardCard.HandBoardCard);
            TargetEffects.Add(new PastActionsPopup.TargetEffectParam
            {
                ActionEffectType = Enumerators.ActionEffectType.PlayFromHand,
                Target = boardCard.HandBoardCard
            });
        }
    }
}
