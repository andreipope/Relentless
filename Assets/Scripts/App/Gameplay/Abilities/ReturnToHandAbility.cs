using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReturnToHandAbility : AbilityBase
    {
        public int Cost { get; }

        public ReturnToHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Cost = ability.Cost;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PushVFX");
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                ReturnTargetToHand(TargetUnit);
            }
        }

        private void ReturnTargetToHand(BoardUnitModel unit)
        {
            Vector3 unitPosition = BattlegroundController.GetBoardUnitViewByModel(unit).Transform.position;

            CreateVfx(unitPosition, true, 3f, true);

            CardsController.ReturnCardToHand(BattlegroundController.GetBoardUnitViewByModel(unit));

            if(AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.HasChangesInParameters)
            {
                unit.Card.InstanceCard.Cost += Cost;
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
            {
                unit
            }, AbilityData.AbilityType, Enumerators.AffectObjectType.Character);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.UseOverlordPowerOnCard,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.Push,
                        Target = unit
                    }
                }
            });
        }
    }
}
