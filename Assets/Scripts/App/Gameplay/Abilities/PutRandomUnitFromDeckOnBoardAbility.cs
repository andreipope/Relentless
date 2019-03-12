using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

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

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
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
                IEnumerable<HandBoardCard> targets = PredefinedTargets.Select(x => x.BoardObject as HandBoardCard);

                foreach (HandBoardCard target in targets)
                {
                    PutCardFromDeckToBoard(target.OwnerPlayer, target.CardView, ref TargetEffects, ref boardCards, target.OwnerPlayer.IsLocalPlayer);
                }
            }
            else
            {
                BoardCardView boardCardView;
                Player playerOwner;
                UniquePositionedList<BoardUnitModel> filteredCards = null;

                foreach (Enumerators.AbilityTargetType targetType in AbilityData.AbilityTargetTypes)
                {
                    playerOwner = null;
                    switch (targetType)
                    {
                        case Enumerators.AbilityTargetType.PLAYER:
                            playerOwner = PlayerCallerOfAbility;
                            break;
                        case Enumerators.AbilityTargetType.OPPONENT:
                            playerOwner = GetOpponentOverlord();
                            break;
                        default:
                            throw new NotImplementedException(nameof(targetType) + " not implemented!");
                    }

                    filteredCards = playerOwner.CardsInDeck.FindAll(x => x.Card.Prototype.CardKind == Enumerators.CardKind.CREATURE);
                    filteredCards = InternalTools.GetRandomElementsFromList(filteredCards, Count).ToUniquePositionedList();
                    if (filteredCards.Count == 0)
                        continue;

                    if (playerOwner.BoardCards.Count < Constants.MaxBoardUnits) 
                    {
                        boardCardView = BattlegroundController.CreateCustomHandBoardCard(filteredCards[0]);
                        PutCardFromDeckToBoard(playerOwner, boardCardView, ref TargetEffects, ref boardCards, true);
                    }
                }
            }

            InvokeUseAbilityEvent(
                boardCards
                    .Select(x => new ParametrizedAbilityBoardObject(x))
                    .ToList()
            );

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });
        }

        private void PutCardFromDeckToBoard(Player owner, BoardCardView boardCardView,
                                            ref List<PastActionsPopup.TargetEffectParam> TargetEffects,
                                            ref List<HandBoardCard> cards, bool activateAbility)
        {
            owner.RemoveCardFromDeck(boardCardView.BoardUnitModel);

            CardsController.SummonUnitFromHand(owner, boardCardView, activateAbility);

            cards.Add(boardCardView.HandBoardCard);

            TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.PlayRandomCardOnBoardFromDeck,
                Target = boardCardView,
            });
        }
    }
}
