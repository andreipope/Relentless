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
            Protobuf.Deck newdeck = new Protobuf.Deck
            {
                Id = deck.Id,
                Name = deck.Name,
                HeroId = deck.HeroId,
            };
            for (int i = 0; i < deck.Cards.Count; i++)
            {
                CardCollection card = new CardCollection
                {
                    CardName = deck.Cards[i].CardName,
                    Amount = deck.Cards[i].Amount
                };
                newdeck.Cards.Add(card);
            }

            return newdeck;
        }

        public static CardAbility ToProtobuf(this AbilityData ability)        {
            CardAbility cardAbility = new CardAbility{
                Type = (CardAbilityType.Types.Enum) ability.AbilityType,
                ActivityType = (CardAbilityActivityType.Types.Enum) ability.ActivityType,
                Trigger = (CardAbilityTrigger.Types.Enum) ability.CallType,
                Stat = (StatType.Types.Enum) ability.AbilityStatType,
                Set = (CardSetType.Types.Enum) ability.AbilitySetType,
                Effect = (CardAbilityEffect.Types.Enum) ability.AbilityEffectType,
                AttackRestriction = (AttackRestriction.Types.Enum) ability.AttackRestriction,
                TargetCardType = (CardKind) ability.TargetCardType,
                TargetUnitStatusType = (UnitSpecialStatus) ability.TargetUnitStatusType,
                TargetUnitType = (CardKind) ability.TargetUnitType,
                Value = ability.Value,
                Attack = ability.Damage,
                Defense = ability.Health,
                Name = ability.Name,
                Turns = ability.Turns,
                Count = ability.Count,
                Delay = ability.Delay,
                MechanicPicture = (MechanicPictureType.Types.Enum) ability.MechanicPicture
            };

            cardAbility.AllowedTargetTypes.Add(ability.AbilityTargetTypes.Select(t => (AllowedTarget.Types.Enum) t));
            cardAbility.VisualEffectsToPlay.Add(ability.VisualEffectsToPlay.Select(v => v.ToProtobuf()));

            return cardAbility;
        }

        public static Protobuf.Unit ToProtobuf(this Unit unit)
        {
            return new Protobuf.Unit
            {
                InstanceId = unit.InstanceId,
                AffectObjectType = (AffectObjectType.Types.Enum) unit.AffectObjectType,
                Parameter = new Parameter
                {
                    Attack = unit.Parameter.Attack,
                    Defense = unit.Parameter.Defense,
                    CardName = unit.Parameter.CardName
                }
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
                Kind = (CardKind) card.CardKind,
                Rank = (CreatureRank.Types.Enum) card.CardRank,
                Type = (CreatureType.Types.Enum) card.CardType,
                CardViewInfo = card.CardViewInfo.ToProtobuf()
            };

            protoCard.Abilities.Add(card.Abilities.Select(a => a.ToProtobuf()));
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

        /*private static RepeatedField<CardInstance> GetMulliganCards(this List<WorkingCard> cards)
        {
            RepeatedField<CardInstance> cardInstances = new RepeatedField<CardInstance>();
            for (int i = 0; i < cards.Count; i++)
            {
                cardInstances[i] = new CardInstance
                {
                    InstanceId = cards[i].InstanceId,
                    Prototype = cards[i].LibraryCard.GetCardPrototype(),
                    Defense = cards[i].Health,
                    Attack = cards[i].Damage
                };
            }

            return cardInstances;
        }*/
    }
}

