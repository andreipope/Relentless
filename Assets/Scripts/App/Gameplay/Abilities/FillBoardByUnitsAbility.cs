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
            long maxUnits = targetPlayer.MaxCardsInPlay - targetPlayer.PlayerCardsController.CardsOnBoard.Count;

            List<Card> cards = DataManager.CachedCardsLibraryData.Cards.FindAll(card => card.Cost == Cost);

            cards = cards.OrderByDescending(x => x.MouldId).ToList();

            cards = GetRandomElements(cards, (int)maxUnits);

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            CardModel boardUnit;
            for (int i = 0; i < cards.Count; i++)
            {
                boardUnit = targetPlayer.PlayerCardsController.SpawnUnitOnBoard(cards[i].Name, ItemPosition.End, IsPVPAbility).Model;

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
    }
}
