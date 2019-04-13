using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DamageTargetOnCountItemsPlayedAbility : AbilityBase
    {
        private int _damage;

        private List<IBoardObject> _targets;

        public DamageTargetOnCountItemsPlayedAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY || AbilityActivity != Enumerators.AbilityActivity.PASSIVE)
                return;

            PrepareToDamage();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                _targets = new List<IBoardObject> {
                    AffectObjectType == Enumerators.AffectObjectType.Player ? (IBoardObject)TargetPlayer : TargetUnit
                };
                DamageTargets();
            }
        }

        private void PrepareToDamage()
        {
            _targets = new List<IBoardObject>();

            if (PredefinedTargets != null)
            {
                foreach (ParametrizedAbilityBoardObject target in PredefinedTargets)
                {
                    _damage = target.Parameters.Attack;
                    _targets.Add(target.BoardObject);
                }
            }
            else
            {
                if (AbilityData.Targets.Contains(Enumerators.Target.OPPONENT))
                {
                    _targets.Add(GetOpponentOverlord());
                }

                _damage = PlayerCallerOfAbility.CardsInGraveyard.FindAll(x => x.Prototype.Kind == Enumerators.CardKind.ITEM && x != CardModel).Count;
            }

            InvokeActionTriggered(_targets);
        }

        private void DamageTargets()
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();
            foreach (IBoardObject target in _targets)
            {
                switch (target)
                {
                    case CardModel boardUnit:
                        break;
                    case Player player:
                        BattleController.AttackPlayerByAbility(AbilityUnitOwner, AbilityData, player, _damage);
                        break;
                }

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                    Target = target,
                    HasValue = true,
                    Value = -_damage
                });
            }

            if (_targets.Count > 0)
            {
                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingOverlord,
                    Caller = AbilityUnitOwner,
                    TargetEffects = TargetEffects
                });


                InvokeUseAbilityEvent(
                    new List<ParametrizedAbilityBoardObject>(_targets.Select(item => new ParametrizedAbilityBoardObject(item,
                     new ParametrizedAbilityParameters
                     {
                         Attack = _damage
                     })))
                );
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            DamageTargets();
        }
    }
}
