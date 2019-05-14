using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class OverflowGooAbility : AbilityBase
    {
        public int Value;

        public OverflowGooAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            // FIXME: why are we hardcoding card names??
            if (CardOwnerOfAbility.Faction == PlayerCallerOfAbility.SelfOverlord.Prototype.Faction ||
                CardOwnerOfAbility.Name.Equals("Corrupted Goo") || CardOwnerOfAbility.Name.Equals("Tainted Goo"))
            {
                string clipTitle = CardOwnerOfAbility.Name.Replace(" ", "_");

                SoundManager.PlaySound(Enumerators.SoundType.SPELLS, clipTitle, Constants.SfxSoundVolume, Enumerators.CardSoundType.NONE);

                PlayerCallerOfAbility.CurrentGoo += Value;

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingOverlord,
                    Caller = GetCaller(),
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.Overflow,
                            Target = PlayerCallerOfAbility,
                            HasValue = true,
                            Value = Value
                        }
                    }
                });
            }
        }
    }
}
