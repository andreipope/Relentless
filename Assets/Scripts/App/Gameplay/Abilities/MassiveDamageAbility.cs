using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class MassiveDamageAbility : AbilityBase
    {
        public int Value;

        public event Action OnUpdateEvent;

        private List<BoardObject> _targets;

        public MassiveDamageAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Update()
        {
            OnUpdateEvent?.Invoke();
        }

        protected override void UnitDiedHandler()
        {
            if (AbilityCallType != Enumerators.AbilityCallType.DEATH) {
                base.UnitDiedHandler();
                return;
            }

            Action();
        }

        protected override void VFXAnimationEndedHandler()
        {
            if (AbilityCallType == Enumerators.AbilityCallType.DEATH)
            {
                base.UnitDiedHandler();
            }
            base.VFXAnimationEndedHandler();

            for (int i = _targets.Count-1; i >= 0; i--)
            {
                OneActionCompleted(_targets[i]);
            }

            if (TutorialManager.IsTutorial && AbilityCallType == Enumerators.AbilityCallType.DEATH)
            {
                TutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.DeathAbilityCompleted);
            }
        }

        public override void Action(object info = null)
        {
            _targets = new List<BoardObject>();

            BoardObject caller = (BoardObject) AbilityUnitOwner ?? BoardSpell;

            Player opponent = PlayerCallerOfAbility == GameplayManager.CurrentPlayer ?
                GameplayManager.OpponentPlayer :
                GameplayManager.CurrentPlayer;
            foreach (Enumerators.AbilityTargetType target in AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                        _targets.AddRange(opponent.CardsOnBoard);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:
                        _targets.AddRange(PlayerCallerOfAbility.CardsOnBoard);
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT:
                        _targets.Add(opponent);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        _targets.Add(PlayerCallerOfAbility);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }

            InvokeActionTriggered(_targets);
        }

        public void OneActionCompleted(BoardObject boardObject)
        {
            switch (boardObject)
            {
                case Player player:
                    BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, player);
                    break;
                case BoardUnitModel unit:
                    BattleController.AttackUnitByAbility(GetCaller(), AbilityData, unit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(boardObject), boardObject, null);
            }
            _targets.Remove(boardObject);
        }
    }
}
