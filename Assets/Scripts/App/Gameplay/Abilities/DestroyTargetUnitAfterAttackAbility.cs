using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DestroyTargetUnitAfterAttackAbility : AbilityBase
    {
        public DestroyTargetUnitAfterAttackAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {

        }

        public override void Activate()
        {
            base.Activate();


            AbilityUnitOwner.AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.Destroy);

            InvokeUseAbilityEvent();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            switch (info)
            {
                case BoardUnitModel boardUnitModel:
                    BattlegroundController.DestroyBoardUnit(boardUnitModel);

                    ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                    {
                        ActionType = Enumerators.ActionType.CardAffectingCard,
                        Caller = GetCaller(),
                        TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        {
                            new PastActionsPopup.TargetEffectParam()
                            {
                                ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                                Target = boardUnitModel
                            }
                        }
                    });
                    break;
                case Player player:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info), info, null);
            }
        }

        protected override void UnitAttackedHandler(BoardObject from, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(from, damage, isAttacker);

            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK)
                return;

            Action(from);
        }
    }
}
