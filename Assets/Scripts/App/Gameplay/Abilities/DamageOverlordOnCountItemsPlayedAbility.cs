using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DamageOverlordOnCountItemsPlayedAbility : AbilityBase
    {
        private int _damage;

        private Player _targetPlayer;

        public DamageOverlordOnCountItemsPlayedAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (PredefinedTargets != null)
            {
                foreach (ParametrizedAbilityBoardObject target in PredefinedTargets)
                {
                    if (target.BoardObject is Player player)
                    {
                        _damage = target.Parameters.Attack;
                        _targetPlayer = player;
                        break;
                    }
                }
            }
            else
            {
                if (AbilityData.Targets.Contains(Enumerators.Target.OPPONENT))
                {
                    _targetPlayer = GetOpponentOverlord();
                    _damage = PlayerCallerOfAbility.CardsInGraveyard.FindAll(x => x.Prototype.Kind == Enumerators.CardKind.ITEM && x != CardModel).Count;
                }
            }

            if (_targetPlayer != null)
            {
                InvokeActionTriggered(_targetPlayer);
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            DamageTarget(_targetPlayer);
        }

        private void DamageTarget(Player player)
        {
            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(
                        player,
                        new ParametrizedAbilityParameters
                        {
                            Attack = _damage
                        }
                    )
                }
            );

            BattleController.AttackPlayerByAbility(AbilityUnitOwner, AbilityData, player, _damage);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingOverlord,
                Caller = AbilityUnitOwner,
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
