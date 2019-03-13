using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DrawCardAbility : AbilityBase
    {
        public Enumerators.Faction Faction { get; }
        public Enumerators.UnitStatusType UnitStatusType { get; }

        public DrawCardAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Faction = ability.Faction;
            UnitStatusType = ability.TargetUnitStatusType;
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

        public override void Action(object info = null)
        {
            base.Action(info);

            if (PlayerCallerOfAbility
                    .BoardCards.FindAll(x => x.Model.UnitStatus == UnitStatusType && x.Model != AbilityUnitOwner)
                    .Count <= 0)
                return;
            else if (PlayerCallerOfAbility.BoardCards
                    .FindAll(card => card.Model.Card.Prototype.Faction == Faction &&
                        card.Model != AbilityUnitOwner &&
                        card.Model.CurrentDefense > 0 &&
                        !card.Model.IsDead)
                    .Count <= 0)
                return;

            if (AbilityTargetTypes.Count > 0)
            {
                Enumerators.AbilityTarget abilityTargetType = AbilityTargetTypes[0];
                switch (abilityTargetType)
                {
                    case Enumerators.AbilityTarget.PLAYER:
                        CardsController.AddCardToHandFromOtherPlayerDeck(PlayerCallerOfAbility, PlayerCallerOfAbility);
                        break;
                    case Enumerators.AbilityTarget.OPPONENT:
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
                PlayerCallerOfAbility.PlayDrawCardVFX();
                CardsController.AddCardToHand(PlayerCallerOfAbility);
            }
        }
    }
}
