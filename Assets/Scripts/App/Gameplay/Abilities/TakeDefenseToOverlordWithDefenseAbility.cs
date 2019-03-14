using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class TakeDefenseToOverlordWithDefenseAbility : AbilityBase
    {
        public int Value { get; }

        public int Defense { get; }

        public int Defense2 { get; }

        public List<Enumerators.Target> TargetTypes { get; }

        public TakeDefenseToOverlordWithDefenseAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = AbilityData.Value;
            Defense = AbilityData.Defense;
            Defense2 = AbilityData.Defense2;
            TargetTypes = AbilityData.AbilityTarget;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (TargetTypes.Contains(Enumerators.Target.PLAYER))
            {
                int defenseToBuff = Value;

                if(PlayerCallerOfAbility.Defense <= Defense2)
                {
                    defenseToBuff = Defense2;
                }

                PlayerCallerOfAbility.BuffedDefense += defenseToBuff;
                PlayerCallerOfAbility.Defense += defenseToBuff;

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingOverlord,
                    Caller = GetCaller(),
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                            Target = PlayerCallerOfAbility,
                            HasValue = true,
                            Value = defenseToBuff
                        }
                    }
                });
            }
        }
    }
}
