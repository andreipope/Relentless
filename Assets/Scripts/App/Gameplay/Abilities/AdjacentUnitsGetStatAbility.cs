using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class AdjacentUnitsGetStatAbility : AbilityBase
    {
        public int Defense { get; }

        public int Damage { get; }

        private List<CardModel> _affectedUnits;

        public AdjacentUnitsGetStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Damage = ability.Damage;
            Defense = ability.Defense;
            _affectedUnits = new List<CardModel>();
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityActivity == Enumerators.AbilityActivity.PASSIVE)
            {
                InvokeUseAbilityEvent();
            }

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            ChangeStats(BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner), Defense, Damage);
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                ChangeStats(BattlegroundController.GetAdjacentUnitsToUnit(TargetUnit), Defense, Damage);

                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });
            }
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            ChangeStats(BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner), Defense, Damage);
        }

        protected override void ChangeAuraStatusAction(bool status)
        {
            base.ChangeAuraStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            if (status)
            {
                if (AbilityUnitOwner.OwnerPlayer.PlayerCardsController.CardsOnBoard.Contains(AbilityUnitOwner))
                {
                    ChangeStats(BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner), Defense, Damage);
                }
            }
            else
            {
                ResetStats();
            }
        }

        protected override void BoardChangedHandler(int count)
        {
            base.BoardChangedHandler(count);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            if(!AbilityUnitOwner.IsDead && AbilityUnitOwner.CurrentDefense > 0 && LastAuraState)
            {
                ResetStats();
                ChangeStats(BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner), Defense, Damage);
            }
        }

        protected override void PlayerOwnerHasChanged(Player oldPlayer, Player newPlayer)
        {
            ResetStats();
            ChangeStats(BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner), Defense, Damage);
        }

        private void ResetStats()
        {
            for (int i = _affectedUnits.Count-1; i >= 0; i--)
            {
                _affectedUnits[i].BuffedDefense -= Defense;
                _affectedUnits[i].AddToCurrentDefenseHistory(-Defense, Enumerators.ReasonForValueChange.AbilityBuff);

                _affectedUnits[i].BuffedDamage -= Damage;
                _affectedUnits[i].AddToCurrentDamageHistory(-Damage, Enumerators.ReasonForValueChange.AbilityBuff);

                _affectedUnits.Remove(_affectedUnits[i]);
            }
        }

        private void ChangeStats(List<CardModel> units, int defense, int damage)
        {
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (CardModel unit in units)
            {
                _affectedUnits.Add(unit);
                unit.UnitDistracted += () =>
                {
                    if (_affectedUnits != null && _affectedUnits.Contains(unit))
                    {
                        _affectedUnits.Remove(unit);
                    }
                };
                if (defense != 0)
                {
                    unit.BuffedDefense += defense;
                    unit.AddToCurrentDefenseHistory(defense, Enumerators.ReasonForValueChange.AbilityBuff);

                    targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = defense >= 0 ? Enumerators.ActionEffectType.ShieldBuff : Enumerators.ActionEffectType.ShieldDebuff,
                        Target = unit,
                    });
                }

                if (damage != 0)
                {
                    unit.BuffedDamage += damage;
                    unit.AddToCurrentDamageHistory(damage, Enumerators.ReasonForValueChange.AbilityBuff);

                    targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = damage >= 0 ? Enumerators.ActionEffectType.AttackBuff : Enumerators.ActionEffectType.AttackDebuff,
                        Target = unit
                    });
                }
            }

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = AbilityUnitOwner,
                TargetEffects = targetEffects
            });
        }
    }
}
