using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DrawCardAbility : AbilityBase
    {
        public Enumerators.Faction Faction { get; }
        public Enumerators.UnitSpecialStatus UnitSpecialStatusType { get; }
        public int Count { get; }

        public DrawCardAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Faction = ability.Faction;
            UnitSpecialStatusType = ability.TargetUnitSpecialStatus;
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        protected override void UnitKilledUnitHandler(BoardUnitModel unit)
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.KILL_UNIT)
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

        protected override void ChangeRageStatusAction(bool status)
        {
            base.ChangeRageStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.RAGE)
                return;

            if (status)
            {
                Action();
            }
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (UnitSpecialStatusType != Enumerators.UnitSpecialStatus.NONE && PlayerCallerOfAbility
                    .CardsOnBoard.FindAll(x => x.UnitSpecialStatus == UnitSpecialStatusType && x != AbilityUnitOwner)
                    .Count <= 0)
                return;
            else if (Faction != 0 && PlayerCallerOfAbility.CardsOnBoard
                    .FindAll(card => card.Card.Prototype.Faction == Faction &&
                        card != AbilityUnitOwner &&
                        card.CurrentDefense > 0 &&
                        !card.IsDead)
                    .Count <= 0)
                return;

            int cardsCount = Mathf.Clamp(Count, 1, Count);

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.AllAllyUnitsInPlay)
            {
                cardsCount = PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard.FindAll(model => model.Card != BoardUnitModel.Card).Count;
            }
            else if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.AllAllyUnitsByFactionInPlay)
            {
                cardsCount = PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard.FindAll(model => model.Card.InstanceCard.Faction == Faction).Count;
            }

            if (AbilityTargets.Count > 0)
            {
                foreach (Enumerators.Target abilityTargetType in AbilityTargets)
                {
                    switch (abilityTargetType)
                    {
                        case Enumerators.Target.PLAYER:
                            AddCardToHand(PlayerCallerOfAbility, PlayerCallerOfAbility, false, cardsCount);
                            break;
                        case Enumerators.Target.OPPONENT:
                            AddCardToHand(PlayerCallerOfAbility, PlayerCallerOfAbility, true, cardsCount);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(abilityTargetType), abilityTargetType, null);
                    }
                }
            }
            else
            {
                PlayerCallerOfAbility.PlayDrawCardVFX();
                PlayerCallerOfAbility.PlayerCardsController.AddCardFromDeckToHand();
            }
        }

        private void AddCardToHand(Player from, Player to, bool fromOtherPlayerDeck = false, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (fromOtherPlayerDeck)
                {
                    PlayerCallerOfAbility.PlayerCardsController.AddCardToHandFromOtherPlayerDeck();
                }
                else
                {
                    PlayerCallerOfAbility.PlayerCardsController.AddCardFromDeckToHand();
                }
            }
        }
    }
}
