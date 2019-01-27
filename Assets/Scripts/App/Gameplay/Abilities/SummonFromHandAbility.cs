using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class SummonFromHandAbility : AbilityBase
    {
        public int Value { get; }
        public int Count { get; }
        public Enumerators.SetType SetType { get; }

        public SummonFromHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Count = ability.Count;
            SetType = ability.AbilitySetType;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }


        public override void Action(object info = null)
        {
            base.Action(info);

            if (PlayerCallerOfAbility.BoardCards.Count >= Constants.MaxBoardUnits)
                return;

            UniqueList<BoardCard> cards = BattlegroundController.PlayerHandCards.FindAll(
                x => x.WorkingCard.InstanceCard.Cost <= Value &&
                x.LibraryCard.CardKind == Enumerators.CardKind.CREATURE);

            if(SetType != Enumerators.SetType.NONE)
            {
                cards = cards.FindAll(x => x.LibraryCard.CardSetType == SetType);
            }

            cards = InternalTools.GetRandomElementsFromList(cards, Count).ToUniqueList();

            if (cards.Count == 0)
                return;

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>(); 

            List<BoardObject> targets = new List<BoardObject>();

            for (int i = 0; i < cards.Count; i++)
            {
                if (PlayerCallerOfAbility.BoardCards.Count >= Constants.MaxBoardUnits)
                    break;

                CardsController.SummonUnitFromHand(PlayerCallerOfAbility, cards[i]);

                targets.Add(cards[i].HandBoardCard);

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.PlayFromHand,
                    Target = cards[i].HandBoardCard,
                });
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, targets, AbilityData.AbilityType, Enumerators.AffectObjectType.Character);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });
        }
    }
}
