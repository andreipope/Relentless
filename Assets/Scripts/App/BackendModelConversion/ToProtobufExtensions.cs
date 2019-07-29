using System;
using System.Linq;
using System.Numerics;
using Loom.Google.Protobuf;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground.Data
{
    public static class ToProtobufExtensions
    {
        public static Protobuf.CardKey ToProtobuf(this CardKey cardKey)
        {
            return new Protobuf.CardKey
            {
                MouldId = cardKey.MouldId.Id,
                Variant = (CardVariant.Types.Enum) cardKey.Variant
            };
        }
        public static DeckCard ToProtobuf(this DeckCardData deckCardData)
        {
            return new DeckCard
            {
                CardKey = deckCardData.CardKey.ToProtobuf(),
                Amount = deckCardData.Amount
            };
        }

        public static Protobuf.Deck ToProtobuf(this Deck deck)
        {
            return new Protobuf.Deck
            {
                Id = deck.Id.Id,
                OverlordId = deck.OverlordId.Id,
                Name = deck.Name,
                Cards =
                {
                    deck.Cards.Select(card => card.ToProtobuf())
                },
                PrimarySkill = (OverlordSkillType.Types.Enum)deck.PrimarySkill,
                SecondarySkill = (OverlordSkillType.Types.Enum)deck.SecondarySkill
            };
        }

        public static Protobuf.AbilityData ToProtobuf(this AbilityData ability) {
            Protobuf.AbilityData abilityData = new Protobuf.AbilityData {
                Ability = (Protobuf.AbilityType.Types.Enum) ability.Ability,
                Activity = (Protobuf.AbilityActivity.Types.Enum) ability.Activity,
                Trigger = (Protobuf.AbilityTrigger.Types.Enum) ability.Trigger,
                Targets =
                {
                    ability.Targets.Select(t => (Target.Types.Enum) t)
                },
                Stat = (Stat.Types.Enum) ability.Stat,
                Faction = (Protobuf.Faction.Types.Enum) ability.Faction,
                Effect = (AbilityEffect.Types.Enum) ability.Effect,
                AttackRestriction = (AttackRestriction.Types.Enum) ability.AttackRestriction,
                TargetCardType = (Protobuf.CardType.Types.Enum) ability.TargetCardType,
                TargetUnitSpecialStatus = (UnitSpecialStatus.Types.Enum) ability.TargetUnitSpecialStatus,
                TargetUnitType = (Protobuf.CardType.Types.Enum) ability.TargetUnitType,
                Value = ability.Value,
                Damage = ability.Damage,
                Defense = ability.Defense,
                Name = ability.Name,
                Turns = ability.Turns,
                Count = ability.Count,
                Delay = ability.Delay,
                VisualEffectsToPlay =
                {
                    ability.VisualEffectsToPlay.Select(v => v.ToProtobuf())
                },
                GameMechanicDescription = (GameMechanicDescription.Types.Enum) ability.GameMechanicDescription,
                TargetFaction = (Protobuf.Faction.Types.Enum) ability.TargetFaction,
                SubTrigger = (AbilitySubTrigger.Types.Enum) ability.SubTrigger,
                ChoosableAbilities =
                {
                    ability.ChoosableAbilities.Select(a => a.ToProtobuf())
                },
                Defense2 = ability.Defense2,
                Cost = ability.Cost,
                TargetCardKind = (CardKind.Types.Enum) ability.TargetKind,
                TargetGameMechanicDescriptionTypes =
                {
                    ability.TargetGameMechanicDescriptions.Select(m => (GameMechanicDescription.Types.Enum) m)
                }
            };

            return abilityData;
        }

        public static CardChoosableAbility ToProtobuf(this AbilityData.ChoosableAbility choosableAbility)
        {
            return new CardChoosableAbility {
                Description = choosableAbility.Description,
                AbilityData = choosableAbility.AbilityData.ToProtobuf()
            };
        }

        public static Protobuf.AbilityData.Types.VisualEffectInfo ToProtobuf(this AbilityData.VisualEffectInfo visualEffectInfo)
        {
            return new Protobuf.AbilityData.Types.VisualEffectInfo {
                Type = (Protobuf.AbilityData.Types.VisualEffectInfo.Types.VisualEffectType) visualEffectInfo.Type,
                Path = visualEffectInfo.Path
            };
        }

        public static CardInstance ToProtobuf(this WorkingCard workingCard)
        {
            CardInstance cardInstance = new CardInstance
            {
                InstanceId = workingCard.InstanceId.ToProtobuf(),
                Prototype = workingCard.Prototype.ToProtobuf(),
                Instance = workingCard.InstanceCard.ToProtobuf()
            };

            return cardInstance;
        }

        public static Protobuf.CardInstanceSpecificData ToProtobuf(this CardInstanceSpecificData data)
        {
            Protobuf.CardInstanceSpecificData protoData = new Protobuf.CardInstanceSpecificData
            {
                Cost = data.Cost,
                Damage = data.Damage,
                Defense = data.Defense,
                Faction = (Protobuf.Faction.Types.Enum) data.Faction,
                Type = (CardType.Types.Enum) data.CardType,
                Abilities =
                {
                    data.Abilities.Select(abilityData => abilityData.ToProtobuf())
                }
            };

            return protoData;
        }

        public static Protobuf.Card ToProtobuf(this IReadOnlyCard card)
        {
            Protobuf.Card protoCard = new Protobuf.Card
            {
                CardKey = card.CardKey.ToProtobuf(),
                Set = (CardSet.Types.Enum) card.Set,
                Name = card.Name,
                Cost = card.Cost,
                Description = card.Description,
                FlavorText = card.FlavorText,
                Picture = card.Picture,
                Damage = card.Damage,
                Defense = card.Defense,
                Faction = (Protobuf.Faction.Types.Enum) card.Faction,
                Frame = card.Frame,
                Kind = (CardKind.Types.Enum) card.Kind,
                Rank = (CreatureRank.Types.Enum) card.Rank,
                Type = (CardType.Types.Enum) card.Type,
                PictureTransforms = card.PictureTransforms.ToProtobuf(),
                Abilities =
                {
                    card.Abilities.Select(a => a.ToProtobuf())
                },
                UniqueAnimation = (Protobuf.UniqueAnimation.Types.Enum) card.UniqueAnimation,
                Hidden = card.Hidden,
                Overrides = card.Overrides?.ToProtobuf()
            };

            return protoCard;
        }

        public static Protobuf.CardOverrides ToProtobuf(this CardOverrideData card)
        {
            if (card == null)
                return null;

            Protobuf.CardOverrides protoCard = new Protobuf.CardOverrides
            {
                Set = card.Set == null ? null : new CardSetEnumValue { Value = (CardSet.Types.Enum) card.Set.Value },
                Name = card.Name == null ? null : new StringValue { Value = card.Name },
                Cost = card.Cost == null ? null : new Int32Value { Value = card.Cost.Value },
                Description = card.Description == null ? null : new StringValue { Value = card.Description },
                FlavorText = card.FlavorText == null ? null : new StringValue { Value = card.FlavorText },
                Picture = card.Picture == null ? null : new StringValue { Value = card.Picture },
                Damage = card.Damage == null ? null : new Int32Value { Value = card.Damage.Value },
                Defense = card.Defense == null ? null : new Int32Value { Value = card.Defense.Value },
                Faction = card.Faction == null ? null : new FactionEnumValue { Value = (Protobuf.Faction.Types.Enum) card.Faction.Value },
                Frame = card.Frame == null ? null : new StringValue { Value = card.Frame },
                Kind = card.Kind == null ? null : new CardKindEnumValue { Value = (CardKind.Types.Enum) card.Kind.Value },
                Rank = card.Rank == null ? null : new CreatureRankEnumValue { Value = (CreatureRank.Types.Enum) card.Rank.Value },
                Type = card.Type == null ? null : new CardTypeEnumValue { Value = (CardType.Types.Enum) card.Type.Value },
                PictureTransforms = card.PictureTransforms.ToProtobuf(),
                Abilities =
                {
                    card.Abilities.Select(a => a.ToProtobuf())
                },
                UniqueAnimation = card.UniqueAnimation == null ? null : new UniqueAnimationEnumValue { Value = (Protobuf.UniqueAnimation.Types.Enum) card.UniqueAnimation.Value },
                Hidden = card.Hidden == null ? null : new BoolValue { Value = card.Hidden.Value }
            };

            return protoCard;
        }

        public static Vector2Float ToProtobuf(this FloatVector2 vector)
        {
            return new Vector2Float
            {
                X = vector.X,
                Y = vector.Y
            };
        }

        public static Protobuf.PictureTransform ToProtobuf(this PictureTransform pictureTransform)
        {
            if (pictureTransform == null)
                return null;

            return new Protobuf.PictureTransform
            {
                Position = pictureTransform.Position.ToProtobuf(),
                Scale = pictureTransform.Scale
            };
        }

        public static Protobuf.CardPictureTransforms ToProtobuf(this Data.CardPictureTransforms pictureTransform)
        {
            if (pictureTransform == null)
                return null;

            return new Protobuf.CardPictureTransforms
            {
                Battleground = pictureTransform.Battleground.ToProtobuf(),
                DeckUI = pictureTransform.DeckUI.ToProtobuf(),
                PastAction = pictureTransform.PastAction.ToProtobuf(),
            };
        }

        public static Protobuf.InstanceId ToProtobuf(this InstanceId instanceId)
        {
            return new Protobuf.InstanceId
            {
                Id = instanceId.Id
            };
        }

        public static Protobuf.Parameter ToProtobuf(this ParametrizedAbilityParameters parameters)
        {
            return new Protobuf.Parameter
            {
                Damage = parameters.Attack,
                Defense = parameters.Defense,
                CardName = parameters.CardName
            };
        }

        public static Protobuf.DebugCheatsConfiguration ToProtobuf(this BackendCommunication.DebugCheatsConfiguration debugCheatsConfiguration)
        {
            if (debugCheatsConfiguration == null)
                return new Protobuf.DebugCheatsConfiguration();

            return new Protobuf.DebugCheatsConfiguration
            {
                Enabled = debugCheatsConfiguration.Enabled,

                UseCustomRandomSeed = debugCheatsConfiguration.CustomRandomSeed != null,
                CustomRandomSeed = debugCheatsConfiguration.CustomRandomSeed ?? 0,

                UseCustomDeck = debugCheatsConfiguration.UseCustomDeck,
                CustomDeck = debugCheatsConfiguration.UseCustomDeck ? debugCheatsConfiguration.CustomDeck?.ToProtobuf() : null,

                DisableDeckShuffle = debugCheatsConfiguration.DisableDeckShuffle,

                ForceFirstTurnUserId = debugCheatsConfiguration.ForceFirstTurnUserId ?? "",

                IgnoreGooRequirements = debugCheatsConfiguration.IgnoreGooRequirements,

                SkipMulligan = debugCheatsConfiguration.SkipMulligan
            };
        }

        public static Client.Protobuf.BigUInt ToProtobufUInt(this BigInteger bigInteger)
        {
            if (bigInteger.Sign < 0)
                throw new ArgumentException("Expected non-negative value");

            byte[] bytes = bigInteger.ToByteArray();

            // Swap endianness
            Array.Reverse(bytes, 0, bytes.Length);

            return new Client.Protobuf.BigUInt
            {
                Value = ByteString.CopyFrom(bytes)
            };
        }
    }
}

