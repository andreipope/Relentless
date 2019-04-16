using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class MassiveDamageAbility : AbilityBase
    {
        private int Damage;

        public event Action OnUpdateEvent;

        private List<BoardObject> _targets;

        public MassiveDamageAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Damage = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            TakeDamage(true);
        }

        public override void Update()
        {
            OnUpdateEvent?.Invoke();
        }

        protected override void UnitDiedHandler()
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH) {
                base.UnitDiedHandler();
                return;
            }

            TakeDamage();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.END)
                return;

            TakeDamage();
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            TakeDamage();
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            for (int i = _targets.Count-1; i >= 0; i--)
            {
                OneActionCompleted(_targets[i]);
            }

            if (TutorialManager.IsTutorial && AbilityTrigger == Enumerators.AbilityTrigger.DEATH)
            {
                TutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.DeathAbilityCompleted);
            }
        }

        private void TakeDamage(bool exceptCaller = false)
        {
            _targets = new List<BoardObject>();

            foreach (Enumerators.Target target in AbilityTargets)
            {
                switch (target)
                {
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                    case Enumerators.Target.OPPONENT_CARD:
                        _targets.AddRange(GetAliveUnits(GetOpponentOverlord().PlayerCardsController.CardsOnBoard));
                        break;
                    case Enumerators.Target.PLAYER_ALL_CARDS:
                    case Enumerators.Target.PLAYER_CARD:
                        _targets.AddRange(GetAliveUnits(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard));

                        if (exceptCaller && _targets.Contains(AbilityUnitOwner))
                        {
                            _targets.Remove(AbilityUnitOwner);
                        }
                        break;
                    case Enumerators.Target.OPPONENT:
                        _targets.Add(GetOpponentOverlord());
                        break;
                    case Enumerators.Target.PLAYER:
                        _targets.Add(PlayerCallerOfAbility);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }

            if(AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.EqualToUnitAttack)
            {
                Damage = BoardUnitModel.InstanceCard.Damage;
            }

            foreach(BoardUnitModel boardUnit in _targets)
            {
                boardUnit.HandleDefenseBuffer(Damage);
            }

            InvokeActionTriggered(_targets);
        }

        public void OneActionCompleted(BoardObject boardObject)
        {
            switch (boardObject)
            {
                case Player player:
                    BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, player, Damage);
                    break;
                case BoardUnitModel unit:
                    BattleController.AttackUnitByAbility(GetCaller(), AbilityData, unit, Damage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(boardObject), boardObject, null);
            }
            _targets.Remove(boardObject);
        }
    }
}
