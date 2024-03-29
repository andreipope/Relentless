using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class PutRandomUnitFromDeckOnBoardAbility : AbilityBase
    {
        public int Count { get; }

        public PutRandomUnitFromDeckOnBoardAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
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
                IReadOnlyList<HandBoardCard> targets =
                    PredefinedTargets
                        .Select(x => x.BoardObject as CardModel)
                        .Select(x => BattlegroundController.CreateCustomHandBoardCard(x).HandBoardCard)
                        .ToList();

                foreach (HandBoardCard target in targets)
                {
                    PutCardFromDeckToBoard(target.OwnerPlayer, target.BoardCardView, ref targetEffects, ref boardCards, target.OwnerPlayer.IsLocalPlayer);
                }
            }
            else
            {
                BoardCardView boardCardView;

                foreach (Enumerators.Target targetType in AbilityData.Targets)
                {
                    Player playerOwner = null;
                    switch (targetType)
                    {
                        case Enumerators.Target.PLAYER:
                            playerOwner = PlayerCallerOfAbility;
                            break;
                        case Enumerators.Target.OPPONENT:
                            playerOwner = GetOpponentOverlord();
                            break;
                        default:
                            throw new NotImplementedException(nameof(targetType) + " not implemented!");
                    }

                    IReadOnlyList<CardModel> filteredCards = playerOwner.CardsInDeck.FindAll(x => x.Card.Prototype.Kind == Enumerators.CardKind.CREATURE);
                    filteredCards = InternalTools.GetRandomElementsFromList(filteredCards, Count).ToUniquePositionedList();
                    if (filteredCards.Count == 0)
                        continue;

                    if (playerOwner.CardsOnBoard.Count < Constants.MaxBoardUnits)
                    {
                        boardCardView = BattlegroundController.CreateCustomHandBoardCard(filteredCards[0]);
                        PutCardFromDeckToBoard(playerOwner, boardCardView, ref targetEffects, ref boardCards, true);
                    }
                }
            }

            InvokeUseAbilityEvent(
                boardCards
                    .Select(x => new ParametrizedAbilityBoardObject(x))
                    .ToList()
            );

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = AbilityUnitOwner,
                TargetEffects = targetEffects
            });
        }

        private void PutCardFromDeckToBoard(Player owner, BoardCardView boardCardView,
                                            ref List<PastActionsPopup.TargetEffectParam> targetEffects,
                                            ref List<HandBoardCard> cards, bool activateAbility)
        {
            if (!activateAbility && GameClient.Get<IGameplayManager>().IsLocalPlayerTurn()) {
                activateAbility = true;
            }
            
            owner.PlayerCardsController.RemoveCardFromDeck(boardCardView.Model);

            owner.PlayerCardsController.SummonUnitFromHand(boardCardView, activateAbility);

            cards.Add(boardCardView.HandBoardCard);

            targetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.PlayRandomCardOnBoardFromDeck,
                Target = boardCardView.Model,
            });
        }
    }
}
