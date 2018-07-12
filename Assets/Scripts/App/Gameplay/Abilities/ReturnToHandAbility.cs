// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class ReturnToHandAbility : AbilityBase
    {
        public int value = 1;

        public ReturnToHandAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");
        }

        public override void Update() { }

        public override void Dispose() { }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                Action();
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            Player unitOwner = targetUnit.ownerPlayer;
            WorkingCard returningCard = targetUnit.Card;
            Vector3 unitPosition = targetUnit.transform.position;

            // STEP 1 - REMOVE UNIT FROM BOARD
            unitOwner.BoardCards.Remove(targetUnit);

            // STEP 2 - DESTROY UNIT ON THE BOARD OR ANIMATE
            CreateVFX(unitPosition);
            targetUnit.Die(true);
            MonoBehaviour.Destroy(targetUnit.gameObject);

            // STEP 3 - REMOVE WORKING CARD FROM BOARD
            unitOwner.RemoveCardFromBoard(returningCard);

            // STEP 4 - RETURN CARD TO HAND
            _cardsController.ReturnToHandBoardUnit(returningCard, unitOwner, unitPosition);

            // STEP 4 - REARRANGE HANDS
            _gameplayManager.RearrangeHands();

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.RETURN_TO_HAND_CARD_ABILITY, new object[]
            {
                playerCallerOfAbility,
                abilityData,
                targetUnit
            }));

            _gameplayManager.GetController<RanksController>().UpdateRanksBuffs(unitOwner);
        }
    }
}