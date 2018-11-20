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

            BoardCard boardCard;
            Player playerOwner;
            List<WorkingCard> filteredCards = null;

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

                if (PredefinedTargets != null)
                {
                    filteredCards = PredefinedTargets.Where(target => (target.BoardObject as HandBoardCard).OwnerPlayer == playerOwner)
                                                     .Select(x => (x.BoardObject as HandBoardCard).CardView.WorkingCard).ToList();
                }
                else
                {
                    filteredCards = playerOwner.CardsInDeck.FindAll(x => x.LibraryCard.CardKind == Enumerators.CardKind.CREATURE);
                }

                filteredCards = InternalTools.GetRandomElementsFromList(filteredCards, Count);

                if (filteredCards.Count == 0)
                    continue;

                if (PredefinedTargets != null)
                {
                    boardCard = (PredefinedTargets[0].BoardObject as HandBoardCard).CardView;
                }
                else
                { 
                    boardCard = BattlegroundController.CreateCustomHandBoardCard(filteredCards[0]);
                }

                InternalTools.DoActionDelayed(() =>
                {
                    CardsController.SummonUnitFromHand(filteredCards[0].Owner, boardCard);
                }, 0.25f);

                boardCards.Add(boardCard.HandBoardCard);

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.PlayRandomCardOnBoardFromDeck,
                    Target = boardCard,
                });
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, boardCards.Cast<BoardObject>().ToList(),
            AbilityData.AbilityType, Protobuf.AffectObjectType.Types.Enum.Card);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });
        }
    }
}
