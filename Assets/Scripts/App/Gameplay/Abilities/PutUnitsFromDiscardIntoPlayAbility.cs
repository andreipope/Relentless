using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class PutUnitsFromDiscardIntoPlayAbility : AbilityBase
    {
        public int Count { get; }

        public PutUnitsFromDiscardIntoPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardUnitModel> targets = new List<BoardUnitModel>();
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            if (PredefinedTargets != null)
            {
                targets.AddRange(PredefinedTargets.Select(x => x.BoardObject as BoardUnitModel).ToList());                
            }
            else
            {
                Player playerOwner = null;
                
                foreach (Enumerators.Target targetType in AbilityData.Targets)
                {
                    switch (targetType)
                    {
                        case Enumerators.Target.PLAYER:
                            playerOwner = PlayerCallerOfAbility;
                            break;
                        case Enumerators.Target.OPPONENT:
                            playerOwner = GetOpponentOverlord();
                            break;
                    }

                    List<BoardUnitModel> elements = playerOwner.PlayerCardsController.CardsInGraveyard.
                                        FindAll(card => card.Card.Prototype.Kind == Enumerators.CardKind.CREATURE);

                    if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
                    {
                        elements = GetRandomElements(elements, Count);
                    }
   
                    if (HasEmptySpaceOnBoard(playerOwner, out int emptyFields) && elements.Count > 0)
                    {
                        for (int i = 0; i < emptyFields; i++)
                        {
                            if (i >= elements.Count)
                                break;

                            targets.Add(elements[i]);
                        }
                    }
                }
            }

            if (targets.Count > 0)
            {
                foreach (BoardUnitModel target in targets)
                {
                    PutCardOnBoard(target.OwnerPlayer, target, ref targetEffects);
                }

                InvokeUseAbilityEvent(
                    targets
                        .Select(x => new ParametrizedAbilityBoardObject(x))
                        .ToList()
                );

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                    Caller = GetCaller(),
                    TargetEffects = targetEffects
                });
            }
        }

        private void PutCardOnBoard(Player owner, BoardUnitModel boardUnitModel, ref List<PastActionsPopup.TargetEffectParam> targetEffects)
        {
            owner.PlayerCardsController.RemoveCardFromGraveyard(boardUnitModel);
            owner.PlayerCardsController.SpawnUnitOnBoard(boardUnitModel, ItemPosition.End, IsPVPAbility);

            targetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.SpawnOnBoard,
                Target = boardUnitModel
            });
        }
    }
}
