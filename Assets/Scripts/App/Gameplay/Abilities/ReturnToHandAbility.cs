using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
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

            Vector3 unitPosition = BattlegroundController.GetBoardUnitViewByModel(TargetUnit).Transform.position;

            CreateVfx(unitPosition, true, 3f, true);

            CardsController.ReturnCardToHand(BattlegroundController.GetBoardUnitViewByModel(TargetUnit));

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
            {
                TargetUnit
            }, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
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
