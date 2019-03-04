using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class TakeDefenseToOverlordWithDefenseAbility : AbilityBase
    {
        public int Value { get; }

        public int Health { get; }

        public int Defense { get; }

        public List<Enumerators.AbilityTargetType> TargetTypes { get; }

        public TakeDefenseToOverlordWithDefenseAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = AbilityData.Value;
            Health = AbilityData.Health;
            Defense = AbilityData.Defense;
            TargetTypes = AbilityData.AbilityTargetTypes;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (TargetTypes.Contains(Enumerators.AbilityTargetType.PLAYER))
            {
                int defenseToBuff = Value;

                if(PlayerCallerOfAbility.Defense <= Defense)
                {
                    defenseToBuff = Health;
                }

                PlayerCallerOfAbility.BuffedHp += defenseToBuff;
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
