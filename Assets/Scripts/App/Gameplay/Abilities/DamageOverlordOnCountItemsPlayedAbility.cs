using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DamageOverlordOnCountItemsPlayedAbility : AbilityBase
    {
        private int _damage;

        public DamageOverlordOnCountItemsPlayedAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Player);

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }


        public override void Action(object info = null)
        {
            base.Action(info);

            _damage = PlayerCallerOfAbility.CardsInGraveyard.FindAll(x => x.LibraryCard.CardKind == Enumerators.CardKind.SPELL).Count;

            if (AbilityData.AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT))
            {
                DamageTarget(GetOpponentOverlord());
            }
        }

        private void DamageTarget(Player player)
        {
            object caller = AbilityUnitOwner != null ? AbilityUnitOwner : (object)BoardSpell;

            BattleController.AttackPlayerByAbility(caller, AbilityData, player, _damage);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingOverlord,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                        Target = player,
                        HasValue = true,
                        Value = -_damage
                    }
                }
            });
        }
    }
}
