using System.Collections.Generic;
using System.Linq;
using Loom.Google.Protobuf.Collections;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground.Data
{
    public static class ToProtobufExtensions
    {
        public static CardAbility ToProtobuf(this AbilityData ability)
        {
            CardAbility cardAbility = new CardAbility{
                Type = (CardAbilityType) ability.AbilityType,
                ActivityType = (CardAbilityActivityType) ability.ActivityType,
                Trigger = (CardAbilityTrigger) ability.CallType,
                Stat = (StatType) ability.AbilityStatType,
                Set = (CardSetType) ability.AbilitySetType,
                Effect = (CardAbilityEffect) ability.AbilityEffectType,
                AttackRestriction = (AttackRestriction) ability.AttackRestriction,
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
            };

            cardAbility.AllowedTargetTypes.Add(ability.AbilityTargetTypes.Select(t => (AllowedTarget) t));
            cardAbility.VisualEffectsToPlay.Add(ability.VisualEffectsToPlay.Select(v => v.ToProtobuf()));

            return cardAbility;
        }

        public static Protobuf.Unit ToProtobuf(this Unit unit)
        {
            return new Protobuf.Unit
            {
                InstanceId = unit.InstanceId,
                AffectObjectType = (AffectObjectType) unit.AffectObjectType,
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
                Set = (CardSetType) card.CardSetType,
                Frame = card.Frame,
                Kind = (CardKind) card.CardKind,
                Rank = (CreatureRank) card.CardRank,
                Type = (CreatureType) card.CardType,
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

