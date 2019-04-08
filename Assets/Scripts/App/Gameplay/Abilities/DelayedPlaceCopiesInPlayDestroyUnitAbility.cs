using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;

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

            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            List<IBoardObject> targets = new List<IBoardObject>();

            CardModel cardModel;
            BoardUnitView boardUnitView;
            for (int i = 0; i < Count; i++)
            {
                if (PlayerCallerOfAbility.CardsOnBoard.Count >= PlayerCallerOfAbility.MaxCardsInPlay)
                    break;

                boardUnitView = PlayerCallerOfAbility.PlayerCardsController.SpawnUnitOnBoard(Name, ItemPosition.End);
                cardModel = boardUnitView.Model;

                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.SpawnOnBoard,
                    Target = cardModel,
                });

                targets.Add(cardModel);
            }

            targetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                Target = AbilityUnitOwner,
            });

            BattlegroundController.DestroyBoardUnit(AbilityUnitOwner, false, true);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = AbilityUnitOwner,
                TargetEffects = targetEffects
            });

            InvokeUseAbilityEvent(
                targets
                    .Select(boardObject => new ParametrizedAbilityBoardObject(boardObject))
                    .ToList()
            );
        }
    }
}
