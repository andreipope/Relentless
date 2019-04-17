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

        public DamageAndDistractAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Damage = ability.Damage;
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            HandleSubTriggers();
        }

        private void HandleSubTriggers()
        {
            List<CardModel> units = new List<CardModel>();

            foreach(Enumerators.Target target in AbilityTargets)
            {
                switch(target)
                {
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                        units.AddRange(GetOpponentOverlord().PlayerCardsController.CardsOnBoard);
                        break;
                    case Enumerators.Target.PLAYER_ALL_CARDS:
                        units.AddRange(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard);
                        break;
                }
            }

            if (units.Count == 0)
                return;

            if(AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                units = GetRandomUnits(units, Count);
            }

            DamageAndDistract(units);

            InvokeUseAbilityEvent(units.Select(item => new ParametrizedAbilityBoardObject(item)).ToList());
        }

        private void DamageAndDistract(List<CardModel> units)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (CardModel boardUnit in units)
            {
                BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, boardUnit, Damage);

                BattlegroundController.DistractUnit(boardUnit);

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                    Target = boardUnit,
                    HasValue = true,
                    Value = -Damage
                });

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.Distract,
                    Target = boardUnit,
                });
            }

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = AbilityUnitOwner,
                TargetEffects = TargetEffects
            });
            AbilityProcessingAction?.TriggerActionExternally();
        }
    }
}
