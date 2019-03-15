using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DrawCardAbility : AbilityBase
    {
        public Enumerators.Faction Faction { get; }
        public Enumerators.UnitStatus UnitStatusType { get; }

        public DrawCardAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Faction = ability.Faction;
            UnitStatusType = ability.TargetUnitStatus;
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

            if (UnitStatusType != Enumerators.UnitStatus.NONE && PlayerCallerOfAbility
                    .CardsOnBoard.FindAll(x => x.UnitStatus == UnitStatusType && x != AbilityUnitOwner)
                    .Count <= 0)
                return;
            else if (Faction != 0 && PlayerCallerOfAbility.CardsOnBoard
                    .FindAll(card => card.Card.Prototype.Faction == Faction &&
                        card != AbilityUnitOwner &&
                        card.CurrentDefense > 0 &&
                        !card.IsDead)
                    .Count <= 0)
                return;

            if (AbilityTargetTypes.Count > 0)
            {
                Enumerators.Target abilityTargetType = AbilityTargetTypes[0];
                switch (abilityTargetType)
                {
                    case Enumerators.Target.PLAYER:
                        PlayerCallerOfAbility.PlayerCardsController.AddCardFromDeckToHand();
                        break;
                    case Enumerators.Target.OPPONENT:
                        PlayerCallerOfAbility.PlayerCardsController.AddCardToHandFromOtherPlayerDeck();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(abilityTargetType), abilityTargetType, null);
                }
            }
            else
            {
                PlayerCallerOfAbility.PlayDrawCardVFX();
                PlayerCallerOfAbility.PlayerCardsController.AddCardFromDeckToHand();
            }
        }
    }
}
