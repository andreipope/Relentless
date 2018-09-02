using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class AddGooVialsAbility : AbilityBase
    {
        public int value = 1;

        public int count;

        public AddGooVialsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
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

        public override void Action(object info = null)
        {
            base.Action(info);

            if (playerCallerOfAbility.GooOnCurrentTurn == Constants.MAXIMUM_PLAYER_GOO)
            {
                for (int i = 0; i < count; i++)
                {
                    _cardsController.AddCardToHand(playerCallerOfAbility);
                }
            } else if (playerCallerOfAbility.GooOnCurrentTurn == Constants.MAXIMUM_PLAYER_GOO - 1)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    _cardsController.AddCardToHand(playerCallerOfAbility);
                }
            }

            playerCallerOfAbility.GooOnCurrentTurn += value;

            // playerCallerOfAbility.Goo = playerCallerOfAbility.GooOnCurrentTurn;
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }
    }
}
