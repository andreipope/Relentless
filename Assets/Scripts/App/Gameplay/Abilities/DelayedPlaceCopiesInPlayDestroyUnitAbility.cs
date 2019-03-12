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

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            List<BoardObject> targets = new List<BoardObject>();

            BoardUnitModel boardUnitModel;
            BoardUnitView boardUnitView;
            for (int i = 0; i < Count; i++)
            {
                if (PlayerCallerOfAbility.CardsOnBoard.Count >= PlayerCallerOfAbility.MaxCardsInPlay)
                    break;

                boardUnitView = CardsController.SpawnUnitOnBoard(PlayerCallerOfAbility, Name, ItemPosition.End);
                boardUnitModel = boardUnitView.Model;

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.SpawnOnBoard,
                    Target = boardUnitModel,
                });

                if (AbilityUnitOwner.OwnerPlayer.IsLocalPlayer)
                {
                    GameplayManager.CurrentPlayer.BoardCards.Insert(ItemPosition.End, boardUnitView);
                }
                else
                {
                    GameplayManager.OpponentPlayer.BoardCards.Insert(ItemPosition.End, boardUnitView);
                }

                targets.Add(boardUnitModel);
            }

            TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                Target = AbilityUnitOwner,
            });

            BattlegroundController.DestroyBoardUnit(AbilityUnitOwner, false, true);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });

            InvokeUseAbilityEvent(
                targets
                    .Select(boardObject => new ParametrizedAbilityBoardObject(boardObject))
                    .ToList()
            );
        }
    }
}
