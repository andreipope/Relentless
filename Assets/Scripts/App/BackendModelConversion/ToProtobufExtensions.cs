using System.Collections.Generic;
using System.Linq;
using Loom.Google.Protobuf.Collections;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground.Data
{
    public static class ToProtobufExtensions
    {
        public static Protobuf.Deck GetDeck(this Deck deck)
        {
            Protobuf.Deck protoDeck = new Protobuf.Deck
            {
                Id = deck.Id,
                Name = deck.Name,
                HeroId = deck.HeroId
            };

            protoDeck.Cards.AddRange(deck.Cards.Select(card => card.ToProtobuf()));

            return protoDeck;
        }

        public static DeckCard ToProtobuf(this DeckCardData deckCardData)
        {
            return new DeckCard
            {
                Amount = deckCardData.Amount,
                CardName = deckCardData.CardName
            };
        }

        public static Protobuf.Deck ToProtobuf(this Deck deck)
        {
            return new Protobuf.Deck
            {
                Id = deck.Id,
                HeroId = deck.HeroId,
                Name = deck.Name,
                Cards =
                {
                    deck.Cards.Select(card => card.ToProtobuf())
                },
                PrimarySkill = deck.PrimarySkill,
                SecondarySkill = deck.SecondarySkill
            };
        }

        public static CardAbility ToProtobuf(this AbilityData ability) {
            CardAbility cardAbility = new CardAbility {
                Type = (CardAbilityType.Types.Enum) ability.AbilityType,
                ActivityType = (CardAbilityActivityType.Types.Enum) ability.ActivityType,
                Trigger = (CardAbilityTrigger.Types.Enum) ability.CallType,
                TargetTypes =
                {
                    ability.AbilityTargetTypes.Select(t => (CardAbilityTarget.Types.Enum) t)
                },
                Stat = (StatType.Types.Enum) ability.AbilityStatType,
                Set = (CardSetType.Types.Enum) ability.AbilitySetType,
                Effect = (CardAbilityEffect.Types.Enum) ability.AbilityEffectType,
                AttackRestriction = (AttackRestriction.Types.Enum) ability.AttackRestriction,
                TargetCardType = (CreatureType.Types.Enum) ability.TargetCardType,
                TargetUnitSpecialStatus = (UnitSpecialStatus.Types.Enum) ability.TargetUnitStatusType,
                TargetUnitType = (CreatureType.Types.Enum) ability.TargetUnitType,
                Value = ability.Value,
                Attack = ability.Damage,
                Defense = ability.Health,
                Name = ability.Name,
                Turns = ability.Turns,
                Count = ability.Count,
                Delay = ability.Delay,
                VisualEffectsToPlay =
                {
                    ability.VisualEffectsToPlay.Select(v => v.ToProtobuf())
                },
                GameMechanicDescriptionType = (GameMechanicDescriptionType.Types.Enum) ability.GameMechanicDescriptionType,
                TargetSet = (CardSetType.Types.Enum) ability.TargetSetType,
                SubTrigger = (CardAbilitySubTrigger.Types.Enum) ability.AbilitySubTrigger,
                ChoosableAbilities =
                {
                    ability.ChoosableAbilities.Select(a => a.ToProtobuf())
                },
                Defense2 = ability.Defense,
                Cost = ability.Cost
            };

            return cardAbility;
        }

        public static Protobuf.Unit ToProtobuf(this Unit unit)
        {
            return new Protobuf.Unit
            {
                InstanceId = unit.InstanceId,
                AffectObjectType = (AffectObjectType.Types.Enum) unit.AffectObjectType,
            };
        }

        public static CardChoosableAbility ToProtobuf(this AbilityData.ChoosableAbility choosableAbility)
        {
            return new CardChoosableAbility {
                Description = choosableAbility.Description,
                AbilityData = choosableAbility.AbilityData.ToProtobuf()
            };
        }

        public static CardAbility.Types.VisualEffectInfo ToProtobuf(this AbilityData.VisualEffectInfo visualEffectInfo)
        {
            return new CardAbility.Types.VisualEffectInfo {
                Type = (CardAbility.Types.VisualEffectInfo.Types.VisualEffectType) visualEffectInfo.Type,
                Path = visualEffectInfo.Path
            };
        }

        public static CardInstance ToProtobuf(this WorkingCard workingCard)
        {
            CardInstance cardInstance = new CardInstance
            {
                InstanceId   = workingCard.InstanceId,
                Prototype = workingCard.LibraryCard.ToProtobuf(),
                Instance = workingCard.InstanceCard.ToProtobuf()
            };

            return cardInstance;
        }

        public static Protobuf.Card ToProtobuf(this IReadOnlyCard card)
        {
            Protobuf.Card protoCard = new Protobuf.Card
            {
                MouldId = card.MouldId,
                Name = card.Name,
                GooCost = card.Cost,
                Description = card.Description,
                FlavorText = card.FlavorText,
                Picture = card.Picture,
                Attack = card.Damage,
                Defense = card.Health,
                Set = (CardSetType.Types.Enum) card.CardSetType,
                Frame = card.Frame,
                Kind = (CardKind.Types.Enum) card.CardKind,
                Rank = (CreatureRank.Types.Enum) card.CardRank,
                Type = (CreatureType.Types.Enum) card.CardType,
                CardViewInfo = card.CardViewInfo.ToProtobuf(),
                Abilities =
                {
                    card.Abilities.Select(a => a.ToProtobuf())
                },
                UniqueAnimation = (UniqueAnimationType.Types.Enum) card.UniqueAnimationType
            };

            return protoCard;
        }

        public static Vector3Float ToProtobuf(this FloatVector3 vector)
        {
            return new Vector3Float
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z
            };
        }

        public static Protobuf.CardViewInfo ToProtobuf(this CardViewInfo cardViewInfo)
        {
            if (cardViewInfo == null)
                return null;

            return new Protobuf.CardViewInfo
            {
                Position = cardViewInfo.Position.ToProtobuf(),
                Scale = cardViewInfo.Scale.ToProtobuf()
            };
        }
    }
}

