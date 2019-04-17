using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            if (AbilityTargets.Contains(Enumerators.Target.ITSELF))
            {
                ReturnTargetToHand(AbilityUnitOwner);
            }
        }

        private void ReturnDeadTargetToHand(BoardUnitModel unit)
        {
            unit.ResetToInitial();

            unit.Owner.PlayerCardsController.ReturnToHandBoardUnit(unit, new Vector3());               

            GameClient.Get<IGameplayManager>().RearrangeHands();
        }

        private void ReturnTargetToHand(BoardUnitModel unit)
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
            {
                Vector3 unitPosition = BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit).Transform.position;

                CreateVfx(unitPosition, true, 3f, true);

                CardsController.ReturnCardToHand(unit);
            }
            else
            {
                ReturnDeadTargetToHand(unit);
            }

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.HasChangesInParameters)
            {
                unit.Card.InstanceCard.Cost += Cost;
                
                if (PlayerCallerOfAbility.IsLocalPlayer)
                {
                    BattlegroundController.PlayerHandCards.FirstOrDefault(card => card.Model == BoardUnitModel).UpdateCardCost();
                }
            }

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(unit)
                }
            );

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
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
