using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DrawCardAbility : AbilityBase
    {
        public Enumerators.SetType SetType { get; }
        public Enumerators.UnitStatusType UnitStatusType { get; }

        public int Count { get; set; }

        public DrawCardAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
            UnitStatusType = ability.TargetUnitStatusType;
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Card);

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        protected override void UnitKilledUnitHandler(BoardUnitModel unit)
        {
            if (AbilityCallType != Enumerators.AbilityCallType.KILL_UNIT)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            Action();
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (UnitStatusType != Enumerators.UnitStatusType.NONE &&
                 PlayerCallerOfAbility
                    .BoardCards.FindAll(x => x.Model.UnitStatus == UnitStatusType && x.Model != AbilityUnitOwner)
                    .Count <= 0)
                return;
            else if (SetType != Enumerators.SetType.NONE &&
                (SetType == Enumerators.SetType.NONE ||
                    PlayerCallerOfAbility
                    .BoardCards.FindAll(x => x.Model.Card.LibraryCard.CardSetType == SetType && x.Model != AbilityUnitOwner)
                    .Count <= 0)
                )
                return;


            int cardsCount = Mathf.Clamp(Count, 1, Count);

            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.AllAllyUnitsInPlay)
            {
                cardsCount = PlayerCallerOfAbility.BoardCards.FindAll(x => x.Model.Card != MainWorkingCard).Count;
            }

            if (AbilityTargetTypes.Count > 0)
            {
                foreach (Enumerators.AbilityTargetType abilityTargetType in AbilityTargetTypes)
                {
                    switch (abilityTargetType)
                    {
                        case Enumerators.AbilityTargetType.PLAYER:
                            AddCardToHand(PlayerCallerOfAbility, PlayerCallerOfAbility, false, cardsCount);
                            break;
                        case Enumerators.AbilityTargetType.OPPONENT:
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
                CardsController.AddCardToHand(PlayerCallerOfAbility);
            }
        }

        private void AddCardToHand(Player from, Player to, bool fromOtherPlayerDeck = false, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (fromOtherPlayerDeck)
                {
                    CardsController.AddCardToHandFromOtherPlayerDeck(from,
                         from.Equals(GameplayManager.CurrentPlayer) ?
                             GameplayManager.OpponentPlayer :
                             GameplayManager.CurrentPlayer);
                }
                else
                {
                    CardsController.AddCardToHandFromOtherPlayerDeck(from, to);
                }
            }
        }
    }
}
