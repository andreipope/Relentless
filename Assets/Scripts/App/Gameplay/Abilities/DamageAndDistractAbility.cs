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

        private List<CardModel> _units;

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
            _units = new List<CardModel>();

            foreach(Enumerators.Target target in AbilityTargets)
            {
                switch(target)
                {
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                        _units.AddRange(GetOpponentOverlord().PlayerCardsController.CardsOnBoard.Where(x => 
                            !x.IsDead &&
                            x.CurrentDefense > 0 &&
                            x != AbilityUnitOwner
                        ).ToList());
                        break;
                    case Enumerators.Target.PLAYER_ALL_CARDS:
                        _units.AddRange(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard.Where(x => 
                            !x.IsDead &&
                            x.CurrentDefense > 0 &&
                            x != AbilityUnitOwner
                        ).ToList());
                        break;
                }
            }

            if (_units.Count == 0)
                return;

            _units = _units.OrderByDescending(x => x.InstanceId.Id).ToList();

            if(AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                _units = GetRandomUnits(_units, Count);
            }

            foreach(CardModel cardModel in _units)
            {
                cardModel.HandleDefenseBuffer(Damage);
            }

            InvokeActionTriggered(_units);

            InvokeUseAbilityEvent(_units.Select(item => new ParametrizedAbilityBoardObject(item)).ToList());
        }

        private void DamageAndDistract(List<CardModel> units)
        {
            foreach (CardModel boardUnit in units)
            {
                DamageAndDistractUnit(boardUnit);
            }

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = AbilityUnitOwner,
                TargetEffects = _targetEffects
            });
            AbilityProcessingAction?.TriggerActionExternally();
        }

        private void DamageAndDistractUnit(CardModel boardUnit)
        {
            _targetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.Distract,
                Target = boardUnit,
            });

            _targetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                Target = boardUnit,
                HasValue = true,
                Value = -Damage
            });

            if (boardUnit.IsUnitActive)
            {
                BattlegroundController.DistractUnit(boardUnit);
            }

            BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, boardUnit, Damage);
        }

        public void OneActionCompleted(CardModel cardModel)
        {
            DamageAndDistractUnit(cardModel);
            _units.Remove(cardModel);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            DamageAndDistract(_units);
        }
    }
}
