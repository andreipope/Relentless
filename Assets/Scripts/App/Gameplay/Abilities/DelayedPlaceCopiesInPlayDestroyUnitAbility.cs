using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class DelayedPlaceCopiesInPlayDestroyUnitAbility : DelayedAbilityBase
    {
        private int Count { get; }
        private string Name { get; }

        public DelayedPlaceCopiesInPlayDestroyUnitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
            Name = ability.Name;
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            List<BoardObject> targets = new List<BoardObject>();

            BoardUnitModel boardUnit;
            for (int i = 0; i < Count; i++)
            {
                boardUnit = CardsController.SpawnUnitOnBoard(PlayerCallerOfAbility, Name).Model;
                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.SpawnOnBoard,
                    Target = boardUnit,
                });

                targets.Add(boardUnit);
            }

            TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                Target = AbilityUnitOwner,
            });

            BattlegroundController.DestroyBoardUnit(AbilityUnitOwner);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, targets, AbilityData.AbilityType, Protobuf.AffectObjectType.Types.Enum.Character);
        }
    }
}
