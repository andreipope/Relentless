// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class AddGooVialsAbility : AbilityBase
    {
        public int value = 1;
        public int count = 0;

        public AddGooVialsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
            count = ability.count;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            int currentVials = Mathf.Clamp(playerCallerOfAbility.GooOnCurrentTurn, 0, Constants.MAXIMUM_PLAYER_GOO);

            if (currentVials == Constants.MAXIMUM_PLAYER_GOO && (playerCallerOfAbility.Goo + cardOwnerOfAbility.cost) == Constants.MAXIMUM_PLAYER_GOO)
            {
                for (int i = 0; i < count; i++)
                    _cardsController.AddCardToHand(playerCallerOfAbility);
            }
            else if (currentVials == Constants.MAXIMUM_PLAYER_GOO - 1 && (playerCallerOfAbility.Goo + cardOwnerOfAbility.cost) == Constants.MAXIMUM_PLAYER_GOO - 1)
            {
                for (int i = 0; i < count - 1; i++)
                    _cardsController.AddCardToHand(playerCallerOfAbility);
            }

            playerCallerOfAbility.GooOnCurrentTurn += value;
            //playerCallerOfAbility.Goo = playerCallerOfAbility.GooOnCurrentTurn;
        }
    }
}