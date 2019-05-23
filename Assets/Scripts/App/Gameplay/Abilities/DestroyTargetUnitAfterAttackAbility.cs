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


            AbilityUnitOwner.AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Destroy);

            InvokeUseAbilityEvent();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            switch (info)
            {
                case CardModel cardModel:
                    BattlegroundController.DestroyBoardUnit(cardModel, handleShield:true);

                    ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                    {
                        ActionType = Enumerators.ActionType.CardAffectingCard,
                        Caller = AbilityUnitOwner,
                        TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        {
                            new PastActionsPopup.TargetEffectParam()
                            {
                                ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                                Target = cardModel
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

        protected override void UnitAttackedHandler(IBoardObject from, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(from, damage, isAttacker);

            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || AbilityUnitOwner.CurrentDamage <= 0)
                return;

            Action(from);
        }
    }
}
