using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DrawCardAbility : AbilityBase
    {
        public Enumerators.SetType SetType { get; }
        public Enumerators.UnitStatusType UnitStatusType { get; }

        public DrawCardAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
            UnitStatusType = ability.TargetUnitStatusType;
        }

        public override void Activate()
        {
            base.Activate();

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

            List<ParametrizedAbilityBoardObject> targets = null;

            WorkingCard card = null;

            if (AbilityTargetTypes.Count > 0)
            {
                Enumerators.AbilityTargetType abilityTargetType = AbilityTargetTypes[0];
                Player targetPlayer = null;
                switch (abilityTargetType)
                {
                    case Enumerators.AbilityTargetType.PLAYER:
                        targetPlayer = PlayerCallerOfAbility;
                        CardsController.AddCardToHandFromOtherPlayerDeck(PlayerCallerOfAbility, PlayerCallerOfAbility);
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT:
                        targetPlayer = PlayerCallerOfAbility.Equals(GameplayManager.CurrentPlayer) ?
                                GameplayManager.OpponentPlayer :
                                GameplayManager.CurrentPlayer;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(abilityTargetType), abilityTargetType, null);
                }

                if (PredefinedTargets != null && PredefinedTargets.Count > 0)
                {
                    card = PlayerCallerOfAbility.CardsInDeck.FirstOrDefault(cardInDeck => cardInDeck.InstanceId.Id.ToString() == PredefinedTargets[0].Parameters.CardName);
                }

                card = CardsController.AddCardToHandFromOtherPlayerDeck(PlayerCallerOfAbility, targetPlayer, card);
            }
            else
            {
                if (PredefinedTargets != null && PredefinedTargets.Count > 0)
                {
                    card = PlayerCallerOfAbility.CardsInDeck.FirstOrDefault(cardInDeck => cardInDeck.InstanceId.Id.ToString() == PredefinedTargets[0].Parameters.CardName);                    
                }
                PlayerCallerOfAbility.PlayDrawCardVFX();
                View.IView viewCard = CardsController.AddCardToHand(PlayerCallerOfAbility, card);

                if(card == null && viewCard is BoardCard boardCard)
                {
                    card = boardCard.WorkingCard;
                }
            }

            if (card != null)
            {
                targets = new List<ParametrizedAbilityBoardObject>();
                {
                    new ParametrizedAbilityBoardObject(PlayerCallerOfAbility,
                        new ParametrizedAbilityParameters()
                        {
                            CardName = card.InstanceId.Id.ToString()
                        });
                };
            }

            InvokeUseAbilityEvent(targets);
        }
    }
}
