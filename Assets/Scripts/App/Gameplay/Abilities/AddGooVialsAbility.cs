using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class AddGooVialsAbility : AbilityBase
    {
        public int Value = 1;

        public int Count;

        public AddGooVialsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
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

            if (PlayerCallerOfAbility.GooOnCurrentTurn == Constants.MaximumPlayerGoo)
            {
                for (int i = 0; i < Count; i++)
                {
                    CardsController.AddCardToHand(PlayerCallerOfAbility);
                }
            } else if (PlayerCallerOfAbility.GooOnCurrentTurn == Constants.MaximumPlayerGoo - 1)
            {
                for (int i = 0; i < Count - 1; i++)
                {
                    CardsController.AddCardToHand(PlayerCallerOfAbility);
                }
            }

            PlayerCallerOfAbility.GooOnCurrentTurn += Value;

            // playerCallerOfAbility.Goo = playerCallerOfAbility.GooOnCurrentTurn;
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }
    }
}
