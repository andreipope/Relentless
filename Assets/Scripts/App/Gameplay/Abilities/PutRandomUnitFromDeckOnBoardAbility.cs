using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

            WorkingCard card = InternalTools.GetRandomElementsFromList(
                PlayerCallerOfAbility.CardsInDeck.FindAll(x => x.LibraryCard.CardKind == Enumerators.CardKind.CREATURE),
                1).First(x => x != null && x != default(WorkingCard));

            if (card != null)
            {
                BoardCard boardCard = new UnitBoardCard(Object.Instantiate(CardsController.CreatureCardViewPrefab));
                boardCard.Init(card);
                boardCard.GameObject.transform.position = Constants.DefaultPositionOfBoardCard;
                boardCard.GameObject.transform.localScale = Vector3.one * .3f;
                boardCard.SetHighlightingEnabled(false);

                CardsController.SummonUnitFromHand(PlayerCallerOfAbility, boardCard);

                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                {
                    boardCard.HandBoardCard
                }, AbilityData.AbilityType, Protobuf.AffectObjectType.Card);

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = GetCaller(),
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.PlayRandomCardOnBoardFromDeck,
                            Target = boardCard,
                        }
                    }
                });
            }
        }
    }
}
