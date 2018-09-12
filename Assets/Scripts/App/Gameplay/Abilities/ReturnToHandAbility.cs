using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
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

            Vector3 unitPosition = TargetUnit.Transform.position;

            CreateVfx(unitPosition, true, 3f, true);

            CardsController.ReturnCardToHand(TargetUnit);

            ActionsQueueController.PostGameActionReport(ActionsQueueController.FormatGameActionReport(
                Enumerators.ActionType.RETURN_TO_HAND_CARD_ABILITY, new object[]
                {
                            PlayerCallerOfAbility, AbilityData, TargetUnit
                }));
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }
    }
}
