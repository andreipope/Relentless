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

                InvokeActionTriggered(TargetUnit);
                
                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });
            }
        }

        protected override void UnitAttackedHandler(IBoardObject info, int damage, bool isAttacker)
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
                ResetAffectedUnits();
                _affectedUnits.Clear();
            }
        }

        protected override void BoardChangedHandler(int count)
        {
            base.BoardChangedHandler(count);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA || AbilityUnitOwner.IsDead || AbilityUnitOwner.CurrentDefense <= 0 || !LastAuraState)
                return;

            for (int i = 0; i < _affectedUnits.Count; ++i)
            {
                CardStatInfo unit = _affectedUnits[i];
                if (unit.CardModel.Card.Owner != AbilityUnitOwner.OwnerPlayer)
                {
                    RemoveBuffFromUnit(unit.ModifiedDamage, unit.ModifiedDefense, unit);
                    _affectedUnits.Remove(unit);
                    --i;
                }
            }

            ChangeStatsOfPlayerAllyCards(Defense, Attack, false, _affectedUnits);
        }

        protected override void PlayerCurrentGooChangedHandler(int goo)
        {
            base.PlayerCurrentGooChangedHandler(goo);

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.NumberOfUnspentGoo)
            {
                int oldAttack = 0;
                int oldDefense = 0;
                CardStatInfo cardStat = null;
                if (_affectedUnits.Count > 0) 
                {
                    oldAttack = _affectedUnits[0].ModifiedDamage;
                    oldDefense = _affectedUnits[0].ModifiedDefense;
                    cardStat = _affectedUnits[0];
                }
                _affectedUnits.Clear();
                ChangeStatsToItself();
                RemoveBuffFromUnit(oldAttack, oldDefense, cardStat);
            }
        }

        protected override void PlayerOwnerHasChanged(Player oldPlayer, Player newPlayer)
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA && !(AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.NumberOfUnspentGoo))
                return;

            ResetAffectedUnits();
            _affectedUnits.Clear();

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.NumberOfUnspentGoo)
            {
                PlayerCurrentGooChangedHandler(AbilityUnitOwner.OwnerPlayer.CurrentGoo);
            }
            else
            {
                ChangeStatsOfPlayerCards(newPlayer, Defense, Attack, false);
            }
        }

        private void ChangeStatsToItself()
        {
            int defense;
            int attack;
            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.ForEachUnitInPlay)
            {
                int count = GetAliveUnits(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard).ToList().FindAll(
                                item => item != CardModel).Count +
                            GetAliveUnits(GetOpponentOverlord().PlayerCardsController.CardsOnBoard).ToList().Count;

                defense = Defense * count;
                attack = Attack * count;
            }
            else if(AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.ForEachEnemyUnitInPlay)
            {
                int count = GetAliveUnits(GetOpponentOverlord().PlayerCardsController.CardsOnBoard).ToList().Count;

                defense = Defense * count;
                attack = Attack * count;
            }
            else if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.ForEachAllyUnitInPlay)
            {
                int count = PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard.FindAll(item => item != AbilityUnitOwner).Where(x => !x.IsDead && x.IsUnitActive && x.CurrentDefense > 0).ToList().Count;

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
                    CardModel = AbilityUnitOwner,
                    ModifiedDamage = attack,
                    ModifiedDefense = defense
                });
            }
            else
            {
                GetParameters(out defense, out attack);
            }

            ChangeStatsOfTarget(AbilityUnitOwner, defense, attack);

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = AbilityUnitOwner,
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

        private void ChangeStatsOfPlayerCards(Player player, int defense, int damage, bool withCaller = false, List<CardStatInfo> filterUnits = null)
        {
            List<CardModel> units = player.PlayerCardsController.CardsOnBoard.ToList();

            if(filterUnits != null)
            {
                filterUnits.ForEach((unit) => units.Remove(unit.CardModel));
            }

            foreach (CardModel unit in units)
            {
                if (!withCaller && unit == CardModel)
                    continue;

                ChangeStatsOfTarget(unit, defense, damage);

                _affectedUnits.Add(new CardStatInfo()
                {
                    CardModel = unit,
                    ModifiedDamage = damage,
                    ModifiedDefense = defense
                });
            }
        }
        
        private void ChangeStatsOfPlayerAllyCards(int defense, int damage, bool withCaller = false, List<CardStatInfo> filterUnits = null)
        {
            ChangeStatsOfPlayerCards
            (
                PlayerCallerOfAbility,
                defense,
                damage,
                withCaller,
                filterUnits
            );
        }
        
        private void ChangeStatsOfTarget(CardModel unit, int defense, int damage)
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

        private void RemoveBuffFromUnit(int attack, int defense, CardStatInfo cardStat)
        {
            if (cardStat == null)
                return;

            cardStat.CardModel.BuffedDefense =
                Mathf.Clamp(cardStat.CardModel.BuffedDefense - defense, 0, 999);
            cardStat.CardModel.AddToCurrentDefenseHistory(-defense, Enumerators.ReasonForValueChange.AbilityBuff);

            cardStat.CardModel.BuffedDamage =
                Mathf.Clamp(cardStat.CardModel.BuffedDamage - attack, 0, 999);
            cardStat.CardModel.AddToCurrentDamageHistory(-attack, Enumerators.ReasonForValueChange.AbilityBuff);
        }

        private void ResetAffectedUnits()
        {
            foreach(CardStatInfo cardStat in _affectedUnits)
            {
                if( cardStat.CardModel.OwnerPlayer.PlayerCardsController.CardsInHand.Contains(cardStat.CardModel) )
                {
                    continue;
                }
                RemoveBuffFromUnit(cardStat.ModifiedDamage, cardStat.ModifiedDefense, cardStat);
            }

            _affectedUnits.Clear();
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();
            TargetUnit.IsPlayable = true;
        }

        class CardStatInfo
        {
            public CardModel CardModel;
            public int ModifiedDamage;
            public int ModifiedDefense;
        }
    }
}
