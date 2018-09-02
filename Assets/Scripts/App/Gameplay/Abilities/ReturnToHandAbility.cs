using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ReturnToHandAbility : AbilityBase
    {

        public int Value { get; }

        public ReturnToHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PushVFX");
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            Player unitOwner = TargetUnit.OwnerPlayer;
            WorkingCard returningCard = TargetUnit.Card;

            returningCard.InitialCost = returningCard.LibraryCard.Cost;
            returningCard.RealCost = returningCard.InitialCost;

            Vector3 unitPosition = TargetUnit.Transform.position;

            CreateVfx(unitPosition, true, 3f, true);

            TimerManager.AddTimer(
                x =>
                {
                    // STEP 1 - REMOVE UNIT FROM BOARD
                    unitOwner.BoardCards.Remove(TargetUnit);

                    // STEP 2 - DESTROY UNIT ON THE BOARD OR ANIMATE
                    TargetUnit.Die(true);
                    Object.Destroy(TargetUnit.GameObject);

                    // STEP 3 - REMOVE WORKING CARD FROM BOARD
                    unitOwner.RemoveCardFromBoard(returningCard);

                    // STEP 4 - RETURN CARD TO HAND
                    CardsController.ReturnToHandBoardUnit(returningCard, unitOwner, unitPosition);

                    // STEP 4 - REARRANGE HANDS
                    GameplayManager.RearrangeHands();

                    ActionsQueueController.PostGameActionReport(ActionsQueueController.FormatGameActionReport(Enumerators.ActionType.RETURN_TO_HAND_CARD_ABILITY, new object[] { PlayerCallerOfAbility, AbilityData, TargetUnit }));
                },
                null,
                2f);
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }
    }
}
