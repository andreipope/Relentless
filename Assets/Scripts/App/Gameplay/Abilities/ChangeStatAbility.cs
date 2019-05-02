using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStatAbility : AbilityBase
    {
        private List<CardStatInfo> _affectedUnits;
        public Enumerators.Stat StatType { get; }

        public int Value { get; }

        public int Attack { get; }

        public int Defense { get; }

        public ChangeStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.Stat;
            Value = ability.Value;
            Attack = ability.Damage;
            Defense = ability.Defense;

            _affectedUnits = new List<CardStatInfo>();
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityActivity == Enumerators.AbilityActivity.PASSIVE)
            {
                InvokeUseAbilityEvent();

                if (AbilityTrigger == Enumerators.AbilityTrigger.ENTRY)
                {
                    if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.AllOtherAllyUnitsInPlay)
                    {
                        if (AbilityTargets.Contains(Enumerators.Target.ITSELF))
                        {
                            ChangeStatsToItself();
                        }
                        else if (AbilityTargets.Contains(Enumerators.Target.PLAYER_ALL_CARDS))
                        {
                            GetParameters(out int defense, out int attack);
                            ChangeStatsOfPlayerAllyCards(defense, attack, false);
                        }
                    }
                    else if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.ForEachUnitInPlay ||
                             AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.ForEachEnemyUnitInPlay ||
                             AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.ForEachAllyUnitInPlay ||
                             AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.NumberOfUnspentGoo)
                    {
                        if (AbilityTargets.Contains(Enumerators.Target.ITSELF))
                        {
                            ChangeStatsToItself();
                        }
                    }
                }
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                GetParameters(out int defense, out int attack);
                ChangeStatsOfTarget(TargetUnit, defense, attack);

                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });
            }
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            ChangeStatsToItself();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            ChangeStatsToItself();
        }

		protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (!GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.NumberOfUnspentGoo)
            {
                ResetAffectedUnits();
            }

            if (AbilityTrigger != Enumerators.AbilityTrigger.END)
                return;

            ChangeStatsToItself();
        }

        protected override void ChangeAuraStatusAction(bool status)
        {
            base.ChangeAuraStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            if (status)
            {
                ChangeStatsOfPlayerAllyCards(Defense, Attack, false);
            }
            else
            {
                ChangeStatsOfPlayerAllyCards(-Defense, -Attack, false);
                _affectedUnits.Clear();
            }
        }

        protected override void BoardChangedHandler(int count)
        {
            base.BoardChangedHandler(count);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            ChangeStatsOfPlayerAllyCards(Defense, Attack, false, _affectedUnits);
        }

        protected override void PlayerCurrentGooChangedHandler(int goo)
        {
            base.PlayerCurrentGooChangedHandler(goo);

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.NumberOfUnspentGoo)
            {
                ResetAffectedUnits();
                ChangeStatsToItself();
            }
        }

        private void ChangeStatsToItself()
        {
            int defense;
            int attack;
            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.ForEachUnitInPlay)
            {
                int count = PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard.FindAll(
                                item => item != BoardUnitModel).Count +
                            GetOpponentOverlord().PlayerCardsController.CardsOnBoard.Count;

                defense = Defense * count;
                attack = Attack * count;
            }
            else if(AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.ForEachEnemyUnitInPlay)
            {
                int count = GetOpponentOverlord().PlayerCardsController.CardsOnBoard.Count;

                defense = Defense * count;
                attack = Attack * count;
            }
            else if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.ForEachAllyUnitInPlay)
            {
                int count = PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard.FindAll(item => item != AbilityUnitOwner).Count;

                defense = Defense * count;
                attack = Attack * count;
            }
            else if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.NumberOfUnspentGoo)
            {
                int unspentGoo = PlayerCallerOfAbility.CurrentGoo;
                defense = unspentGoo;
                attack =  unspentGoo;

                _affectedUnits.Add(new CardStatInfo()
                {
                    BoardUnitModel = AbilityUnitOwner,
                    ModifiedDamage = attack,
                    ModifiedDefense = defense
                });
            }
            else
            {
                GetParameters(out defense, out attack);
            }

            ChangeStatsOfTarget(AbilityUnitOwner, defense, attack);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = defense > 0 ? Enumerators.ActionEffectType.ShieldBuff : Enumerators.ActionEffectType.ShieldDebuff,
                        Target = AbilityUnitOwner
                    },
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = attack > 0 ? Enumerators.ActionEffectType.AttackBuff : Enumerators.ActionEffectType.AttackDebuff,
                        Target = AbilityUnitOwner
                    }
                }
            });
        }

        private void ChangeStatsOfPlayerAllyCards(int defense, int damage, bool withCaller = false, List<CardStatInfo> filterUnits = null)
        {
            List<BoardUnitModel> units = PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard.ToList();

            if(filterUnits != null)
            {
                filterUnits = filterUnits.Where(unit => !unit.BoardUnitModel.IsReanimated).ToList();

                filterUnits.ForEach((unit) => units.Remove(unit.BoardUnitModel));
            }

            foreach (BoardUnitModel unit in units)
            {
                if (!withCaller && unit == BoardUnitModel)
                    continue;

                ChangeStatsOfTarget(unit, defense, damage);

                _affectedUnits.Add(new CardStatInfo()
                {
                    BoardUnitModel = unit,
                    ModifiedDamage = damage,
                    ModifiedDefense = defense
                });
            }
        }
        
        private void ChangeStatsOfTarget(BoardUnitModel unit, int defense, int damage)
        {
            unit.BuffedDefense += defense;
            unit.AddToCurrentDefenseHistory(defense, Enumerators.ReasonForValueChange.AbilityBuff);
            unit.BuffedDamage += damage;
            unit.AddToCurrentDamageHistory(damage, Enumerators.ReasonForValueChange.AbilityBuff);
        }

        private void GetParameters(out int defense, out int attack)
        {
            if(StatType != Enumerators.Stat.UNDEFINED)
            {
                defense = Value;
                attack = Value;
            }
            else
            {
                defense = Defense;
                attack = Attack;
            }
        }

        private void ResetAffectedUnits()
        {
            foreach(CardStatInfo cardStat in _affectedUnits)
            {
                cardStat.BoardUnitModel.BuffedDefense =
                    Mathf.Clamp(cardStat.BoardUnitModel.BuffedDefense - cardStat.ModifiedDefense, 0, 999);
                cardStat.BoardUnitModel.AddToCurrentDefenseHistory(-cardStat.ModifiedDefense, Enumerators.ReasonForValueChange.AbilityBuff);

                cardStat.BoardUnitModel.BuffedDamage =
                    Mathf.Clamp(cardStat.BoardUnitModel.BuffedDamage - cardStat.ModifiedDamage, 0, 999);
                cardStat.BoardUnitModel.AddToCurrentDamageHistory(-cardStat.ModifiedDamage, Enumerators.ReasonForValueChange.AbilityBuff);
            }

            _affectedUnits.Clear();
        }

        class CardStatInfo
        {
            public BoardUnitModel BoardUnitModel;
            public int ModifiedDamage;
            public int ModifiedDefense;
        }
    }
}
