using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DrawCardAbility : AbilityBase
    {
        public Enumerators.SetType SetType { get; }

        public DrawCardAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Player);

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);
            if (SetType != Enumerators.SetType.NONE &&
                (SetType == Enumerators.SetType.NONE ||
                    PlayerCallerOfAbility
                    .BoardCards.FindAll(x => x.Model.Card.LibraryCard.CardSetType == SetType && x.Model != AbilityUnitOwner)
                    .Count <= 0)
                )
                return;

            if (AbilityTargetTypes.Count > 0)
            {
                Enumerators.AbilityTargetType abilityTargetType = AbilityTargetTypes[0];
                switch (abilityTargetType)
                {
                    case Enumerators.AbilityTargetType.PLAYER:
                        CardsController.AddCardToHandFromOtherPlayerDeck(PlayerCallerOfAbility, PlayerCallerOfAbility);
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT:
                        CardsController.AddCardToHandFromOtherPlayerDeck(PlayerCallerOfAbility,
                            PlayerCallerOfAbility.Equals(GameplayManager.CurrentPlayer) ?
                                GameplayManager.OpponentPlayer :
                                GameplayManager.CurrentPlayer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(abilityTargetType), abilityTargetType, null);
                }
            }
            else
            {
                CardsController.AddCardToHand(PlayerCallerOfAbility);
            }
        }
    }
}
