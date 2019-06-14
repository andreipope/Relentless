using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class ChangeStatThisTurnAbility : AbilityBase
    {
        private List<ChangedStatInfo> _affectedUnits;
        private int Attack { get; }

        private int Defense { get; }

        public ChangeStatThisTurnAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Attack = ability.Damage;
            Defense = ability.Defense;

            _affectedUnits = new List<ChangedStatInfo>();
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            HandleTargets();
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            RestoreAffectedUnits();
        }

        private void HandleTargets()
        {
            List<CardModel> targets = new List<CardModel>();

            foreach (Enumerators.Target target in AbilityTargets)
            {
                switch (target)
                {
                    case Enumerators.Target.PLAYER_CARD:
                    case Enumerators.Target.PLAYER_ALL_CARDS:
                        targets.AddRange(GetAliveUnits(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard));
                        break;
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                    case Enumerators.Target.OPPONENT_CARD:
                        targets.AddRange(GetAliveUnits(GetOpponentOverlord().PlayerCardsController.CardsOnBoard));
                        break;
                }
            }

            SetStatsOfUnits(targets, Defense, Attack);
        }

        private void RestoreAffectedUnits()
        {
            foreach(ChangedStatInfo changedStatInfo in _affectedUnits)
            {
                changedStatInfo.ForcedValue.Enabled = false;

                if (changedStatInfo.ChangedAttack)
                {
                    changedStatInfo.CardModel.InvokeDamageChanged(changedStatInfo.ForcedValue.ValueInteger, changedStatInfo.CardModel.CurrentDamage);
                }

                if (changedStatInfo.ChangedDefense)
                {
                    changedStatInfo.CardModel.InvokeDamageChanged(changedStatInfo.ForcedValue.ValueInteger, changedStatInfo.CardModel.CurrentDefense);
                }
            }

            _affectedUnits.Clear();
        }

        private void SetStatsOfUnits(List<CardModel> units, int defense, int damage)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            ChangedStatInfo changedStatInfo;
            foreach (CardModel unit in units)
            {
                if (defense != 0)
                {
                    unit.AddToCurrentDefenseHistory(defense, Enumerators.ReasonForValueChange.AbilityBuff, forced: true);

                    changedStatInfo = new ChangedStatInfo()
                    {
                        CardModel = unit,
                        ForcedValue = unit.FindFirstForcedValueInValueHistory(unit.CurrentDefenseHistory),
                        ChangedDefense = true
                    };

                    _affectedUnits.Add(changedStatInfo);

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = defense > 0 ? Enumerators.ActionEffectType.ShieldBuff : Enumerators.ActionEffectType.ShieldDebuff,
                        Target = unit,
                        HasValue = true,
                        Value = defense
                    });
                }

                if (damage != 0)
                {
                    unit.AddToCurrentDamageHistory(damage, Enumerators.ReasonForValueChange.AbilityBuff, forced: true);

                    changedStatInfo = new ChangedStatInfo()
                    {
                        CardModel = unit,
                        ForcedValue = unit.FindFirstForcedValueInValueHistory(unit.CurrentDamageHistory),
                        ChangedAttack = true
                    };

                    _affectedUnits.Add(changedStatInfo);

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = damage > 0 ? Enumerators.ActionEffectType.AttackBuff : Enumerators.ActionEffectType.AttackDebuff,
                        Target = unit,
                        HasValue = true,
                        Value = damage
                    });
                }
            }

            if (TargetEffects.Count > 0)
            {
                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                    Caller = AbilityUnitOwner,
                    TargetEffects = TargetEffects
                });
            }
        }

        class ChangedStatInfo
        {
            public CardModel CardModel;

            public ValueHistory ForcedValue;

            public bool ChangedDefense;

            public bool ChangedAttack;
        }
    }
}
