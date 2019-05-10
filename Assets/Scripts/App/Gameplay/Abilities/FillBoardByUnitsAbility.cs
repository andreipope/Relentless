using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class FillBoardByUnitsAbility : AbilityBase
    {
        private int Cost { get; }

        public FillBoardByUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Cost = ability.Cost;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            foreach(Enumerators.Target target in AbilityData.Targets)
            {
                switch(target)
                {
                    case Enumerators.Target.OPPONENT:
                        FillBoard(GetOpponentOverlord());
                        break;
                    case Enumerators.Target.PLAYER:
                        FillBoard(PlayerCallerOfAbility);
                        break;
                }
            }
        }

        private void FillBoard(Player targetPlayer)
        {
            if (!HasEmptySpaceOnBoard(targetPlayer, out int maxUnits))
                return;

            if(AbilityUnitOwner.HasActiveMechanic(Enumerators.GameMechanicDescription.Reanimate))
            {
                if(!AbilityUnitOwner.IsAlive())
                {
                    maxUnits--;
                }
            }

            List<Card> cards = DataManager.CachedCardsLibraryData.Cards.FindAll(card => card.Cost == Cost && card.Kind == Enumerators.CardKind.CREATURE);

            cards = GetRandomElements(cards, maxUnits);

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            CardModel boardUnit;
            for (int i = 0; i < cards.Count; i++)
            {
                int CardOnBoard = GetCardOnBoard(targetPlayer);
                if (CardOnBoard >= targetPlayer.MaxCardsInPlay)
                    break;

                boardUnit = targetPlayer.PlayerCardsController.SpawnUnitOnBoard(cards[i].Name, ItemPosition.End, IsPVPAbility, null, false).Model;

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.None,
                    Target = boardUnit
                });
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

        private int GetCardOnBoard(Player targetPlayer)
        {
            int count = 0;
            for (int i = 0; i < targetPlayer.PlayerCardsController.CardsOnBoard.Count; i++)
            {
                if (targetPlayer.PlayerCardsController.CardsOnBoard[i].IsDead == false || targetPlayer.PlayerCardsController.CardsOnBoard[i].CurrentDefense <= 0)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
