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
        public PutRandomUnitFromDeckOnBoardAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
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

            WorkingCard card;
            BoardCard boardCard;
            foreach (Enumerators.AbilityTargetType targetType in AbilityData.AbilityTargetTypes)
            {
                card = null;
                switch (targetType)
                {
                    case Enumerators.AbilityTargetType.PLAYER:
                        card = InternalTools.GetRandomElementsFromList(
                    PlayerCallerOfAbility.CardsInDeck.FindAll(x => x.LibraryCard.CardKind == Enumerators.CardKind.CREATURE),
                    1).First(x => x != null && x != default(WorkingCard));
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT:
                        card = InternalTools.GetRandomElementsFromList(
                      GetOpponentOverlord().CardsInDeck.FindAll(x => x.LibraryCard.CardKind == Enumerators.CardKind.CREATURE),
                      1).First(x => x != null && x != default(WorkingCard));
                        break;
                    default:
                        throw new NotImplementedException(nameof(targetType) + " not implemented!");
                }

                boardCard = PutCardOnBoard(card);

                boardCards.Add(boardCard.HandBoardCard);

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.PlayRandomCardOnBoardFromDeck,
                    Target = boardCard,
                });
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, boardCards.Cast<BoardObject>().ToList(),
            AbilityData.AbilityType, Protobuf.AffectObjectType.Card);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });

        }

        private BoardCard PutCardOnBoard(WorkingCard card)
        {
            BoardCard boardCard = new UnitBoardCard(Object.Instantiate(CardsController.CreatureCardViewPrefab));
            boardCard.Init(card);
            boardCard.GameObject.transform.position = Constants.DefaultPositionOfBoardCard;
            boardCard.GameObject.transform.localScale = Vector3.one * .3f;
            boardCard.SetHighlightingEnabled(false);

            CardsController.SummonUnitFromHand(PlayerCallerOfAbility, boardCard);

            return boardCard;
        }
    }
}
