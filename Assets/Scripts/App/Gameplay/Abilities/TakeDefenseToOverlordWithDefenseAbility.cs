using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class TakeDefenseToOverlordWithDefenseAbility : AbilityBase
    {
        public int AddedDefenseAboveThreshold { get; }

        public int AddedDefenseBelowThreshold { get; }

        public int DefenseThreshold { get; }

        public List<Enumerators.Target> TargetTypes { get; }

        public TakeDefenseToOverlordWithDefenseAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            AddedDefenseAboveThreshold = AbilityData.Value;
            AddedDefenseBelowThreshold = AbilityData.Defense;
            DefenseThreshold = AbilityData.Defense2;
            TargetTypes = AbilityData.Targets;
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
                int defenseToBuff = AddedDefenseAboveThreshold;

                if(PlayerCallerOfAbility.Defense <= DefenseThreshold)
                {
                    defenseToBuff = AddedDefenseBelowThreshold;
                }

                PlayerCallerOfAbility.BuffedDefense += defenseToBuff;
                PlayerCallerOfAbility.Defense += defenseToBuff;

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingOverlord,
                    Caller = AbilityUnitOwner,
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
