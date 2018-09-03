using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
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

        public override void Action(object info = null)
        {
            base.Action(info);

            if (PlayerCallerOfAbility.GooOnCurrentTurn == Constants.MaximumPlayerGoo)
            {
                for (int i = 0; i < Count; i++)
                {
                    CardsController.AddCardToHand(PlayerCallerOfAbility);
                }
            }
            else if (PlayerCallerOfAbility.GooOnCurrentTurn == Constants.MaximumPlayerGoo - 1)
            {
                for (int i = 0; i < Count - 1; i++)
                {
                    CardsController.AddCardToHand(PlayerCallerOfAbility);
                }
            }

            PlayerCallerOfAbility.GooOnCurrentTurn += Value;
        }
    }
}
