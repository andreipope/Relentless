using System;
using System.Collections.Generic;
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

            InvokeUseAbilityEvent();

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
                    .CardsOnBoard.FindAll(x => x.UnitStatus == UnitStatusType && x != AbilityUnitOwner)
                    .Count <= 0)
                return;
            else if (SetType != Enumerators.SetType.NONE &&
                (PlayerCallerOfAbility.CardsOnBoard
                    .FindAll(card => card.Card.Prototype.CardSetType == SetType &&
                        card != AbilityUnitOwner &&
                        card.CurrentHp > 0 &&
                        !card.IsDead)
                    .Count <= 0)
                )
                return;

            if (AbilityTargetTypes.Count > 0)
            {
                Enumerators.AbilityTargetType abilityTargetType = AbilityTargetTypes[0];
                switch (abilityTargetType)
                {
                    case Enumerators.AbilityTargetType.PLAYER:
                        PlayerCallerOfAbility.LocalCardsController.AddCardToHand3();
                        CardsController.AddCardToHandFromOtherPlayerDeck(PlayerCallerOfAbility, PlayerCallerOfAbility);
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT:
                        PlayerCallerOfAbility.LocalCardsController.AddCardToHandFromOpponentDeck();
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
