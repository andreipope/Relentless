using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DamageAndDistractAbility : AbilityBase
    {
        public int Damage { get; }

        public int Count { get; }

        public event Action OnUpdateEvent;

        private List<BoardUnitModel> _units;

        private List<PastActionsPopup.TargetEffectParam> _targetEffects;

        public DamageAndDistractAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Damage = ability.Damage;
            Count = ability.Count;

            _targetEffects = new List<PastActionsPopup.TargetEffectParam>();
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            HandleSubTriggers();
        }

        public override void Update()
        {
            OnUpdateEvent?.Invoke();
        }

        private void HandleSubTriggers()
        {
            _units = new List<BoardUnitModel>();

            foreach(Enumerators.Target target in AbilityTargets)
            {
                switch(target)
                {
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                        _units.AddRange(GetOpponentOverlord().PlayerCardsController.CardsOnBoard);
                        break;
                    case Enumerators.Target.PLAYER_ALL_CARDS:
                        _units.AddRange(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard);
                        break;
                }
            }

            if (_units.Count == 0)
                return;

            if(AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                _units = GetRandomUnits(_units, Count);
            }

            InvokeActionTriggered(_units);

            InvokeUseAbilityEvent(_units.Select(item => new ParametrizedAbilityBoardObject(item)).ToList());
        }

        private void DamageAndDistract(List<BoardUnitModel> units)
        {
            foreach (BoardUnitModel boardUnit in units)
            {
                DamageAndDistractUnit(boardUnit);
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = GetCaller(),
                TargetEffects = _targetEffects
            });
        }

        private void DamageAndDistractUnit(BoardUnitModel boardUnit)
        {
            BattleController.AttackUnitByAbility(GetCaller(), AbilityData, boardUnit, Damage);

            boardUnit.HandleDefenseBuffer(Damage);

            if (boardUnit.IsUnitActive)
            {
                BattlegroundController.DistractUnit(boardUnit);
            }

            _targetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                Target = boardUnit,
                HasValue = true,
                Value = -Damage
            });

            _targetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.Distract,
                Target = boardUnit,
            });
        }

        public void OneActionCompleted(BoardUnitModel boardUnitModel)
        {
            DamageAndDistractUnit(boardUnitModel);
            _units.Remove(boardUnitModel);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            DamageAndDistract(_units);
        }
    }
}
