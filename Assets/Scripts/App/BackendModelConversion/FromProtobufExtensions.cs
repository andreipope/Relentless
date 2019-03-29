using System.Collections.Generic;
using System.Linq;
using Loom.Google.Protobuf.Collections;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;

namespace Loom.ZombieBattleground.Data
{
    public static class FromProtobufExtensions
    {
        public static CollectionCardData FromProtobuf(this CardCollectionCard cardCollection)
        {
            return new CollectionCardData
            {
                Amount = (int) cardCollection.Amount,
                CardName = cardCollection.CardName
            };
        }

        public static CollectionData FromProtobuf(this GetCollectionResponse getCollectionResponse)
        {
            return new CollectionData
            {
                Cards = getCollectionResponse.Cards.Select(card => card.FromProtobuf()).ToList()
            };
        }

        public static Unit FromProtobuf(this Protobuf.Unit unit)
        {
            Unit.ParameterType parameter = new Unit.ParameterType();

            if (unit.Parameter != null)
            {
                parameter = new Unit.ParameterType
                (
                    unit.Parameter.Damage,
                    unit.Parameter.Defense,
                    unit.Parameter.CardName
                );
            }

            return new Unit(
                unit.InstanceId.FromProtobuf(),
                parameter
            );
        }

        public static FloatVector3 FromProtobuf(this Vector3Float vector)
        {
            return new FloatVector3(vector.X, vector.Y, vector.Z);
        }

        public static AbilityData FromProtobuf(this Protobuf.AbilityData ability)
        {
            return new AbilityData(
                (Enumerators.AbilityType) ability.Ability,
                (Enumerators.AbilityTrigger) ability.Trigger,
                ability.Targets.Select(t => (Enumerators.Target) t).ToList(),
                (Enumerators.Stat) ability.Stat,
                (Enumerators.Faction) ability.Faction,
                (Enumerators.AttackRestriction) ability.AttackRestriction,
                (Enumerators.CardType) ability.TargetCardType,
                (Enumerators.UnitSpecialStatus) ability.TargetUnitSpecialStatus,
                (Enumerators.CardType) ability.TargetUnitType,
                ability.Value,
                ability.Damage,
                ability.Defense,
                ability.Name,
                ability.Turns,
                ability.Count,
                ability.Delay,
                ability.VisualEffectsToPlay.Select(v => v.FromProtobuf()).ToList(),
                (Enumerators.GameMechanicDescription) ability.GameMechanicDescription,
                (Enumerators.Faction) ability.TargetFaction,
                (Enumerators.AbilitySubTrigger) ability.SubTrigger,
                ability.ChoosableAbilities.Select(c => c.FromProtobuf()).ToList(),
                ability.Defense2,
                ability.Cost
            );
        }

        public static OverlordModel FromProtobuf(this Protobuf.Overlord overlord)
        {
            return new OverlordModel(
                (int) overlord.OverlordId,
                overlord.Icon,
                overlord.Name,
                overlord.ShortDescription,
                overlord.LongDescription,
                overlord.Experience,
                (int) overlord.Level,
                (Enumerators.Faction) overlord.Faction,
                overlord.Skills.Select(skill => skill.FromProtobuf()).ToList(),
                (Enumerators.Skill)overlord.PrimarySkill,
                (Enumerators.Skill)overlord.SecondarySkill
            );
        }

        public static OverlordSkill FromProtobuf(this Protobuf.Skill skill)
        {
            return new OverlordSkill(
                (int)skill.Id,
                skill.Title,
                skill.IconPath,
                skill.Description,
                skill.Cooldown,
                skill.InitialCooldown,
                skill.Value,
                skill.Damage,
                skill.Count,
                (Enumerators.Skill) skill.Skill_,
                skill.SkillTargets.Select(t => (Enumerators.SkillTarget) t).ToList(),
                (Enumerators.UnitSpecialStatus) skill.TargetUnitSpecialStatus,
                skill.TargetFactions.Select(t => (Enumerators.Faction) t).ToList(),
                skill.Unlocked,
                skill.CanSelectTarget,
                skill.SingleUse
            );
        }

        public static AIDeck FromProtobuf(this Protobuf.AIDeck aiDeck)
        {
            return new AIDeck(
                aiDeck.Deck.FromProtobuf(),
                (Enumerators.AIType) aiDeck.Type
            );
        }

        public static Deck FromProtobuf(this Protobuf.Deck deck)
        {
            return new Deck(
                deck.Id,
                (int) deck.OverlordId,
                deck.Name,
                deck.Cards.Select(card => card.FromProtobuf()).ToList(),
                (Enumerators.Skill)deck.PrimarySkill,
                (Enumerators.Skill)deck.SecondarySkill
            );
        }

        public static DeckCardData FromProtobuf(this Protobuf.DeckCard card)
        {
            return new DeckCardData(
                card.CardName,
                (int) card.Amount
            );
        }

        public static AbilityData.VisualEffectInfo FromProtobuf(this Protobuf.AbilityData.Types.VisualEffectInfo visualEffectInfo)
        {
            return new AbilityData.VisualEffectInfo(
                (Enumerators.VisualEffectType) visualEffectInfo.Type,
                visualEffectInfo.Path
            );
        }

        public static AbilityData.ChoosableAbility FromProtobuf(this CardChoosableAbility choosableAbility)
        {
            return new AbilityData.ChoosableAbility(
                choosableAbility.Description,
                choosableAbility.AbilityData.FromProtobuf(),
                choosableAbility.Attribute
            );
        }

        public static PictureTransform FromProtobuf(this Protobuf.PictureTransform pictureTransform)
        {
            if (pictureTransform == null)
                return null;

            return new PictureTransform(pictureTransform.Position.FromProtobuf(), pictureTransform.Scale.FromProtobuf());
        }

        public static Card FromProtobuf(this Protobuf.Card card)
        {
            return new Card(
                card.MouldId,
                card.Name,
                card.Cost,
                card.Description,
                card.FlavorText,
                card.Picture,
                card.Damage,
                card.Defense,
                (Enumerators.Faction) card.Faction,
                card.Frame,
                (Enumerators.CardKind) card.Kind,
                (Enumerators.CardRank) card.Rank,
                (Enumerators.CardType) card.Type,
                new CardAbilities(new List<GenericParameter>(), new List<CardAbilityData>()
                {
                    new CardAbilityData(Enumerators.AbilityType.Blitz, Enumerators.GameMechanicDescription.Blitz, new List<Enumerators.AbilityTrigger>()
                    {
                        Enumerators.AbilityTrigger.Entry
                    }, new List<Enumerators.Target>()
                    {
                        Enumerators.Target.ItSelf
                    }, new List<GenericParameter>())
                }),
                card.PictureTransform.FromProtobuf(),
                (Enumerators.UniqueAnimation) card.UniqueAnimation,
                card.Hidden
            );
        }

        public static CardInstanceSpecificData FromProtobuf(this Protobuf.CardInstanceSpecificData card)
        {
           return new CardInstanceSpecificData(
                card.Damage,
                card.Defense,
                (Enumerators.Faction) card.Faction,
                (Enumerators.CardType) card.Type,
                card.Cost,
                new CardAbilities(new List<GenericParameter>(), new List<CardAbilityData>()
                {
                    new CardAbilityData(Enumerators.AbilityType.Blitz, Enumerators.GameMechanicDescription.Blitz, new List<Enumerators.AbilityTrigger>()
                    {
                        Enumerators.AbilityTrigger.Entry
                    }, new List<Enumerators.Target>()
                    {
                        Enumerators.Target.ItSelf
                    }, new List<GenericParameter>())
                })
            );
        }

        public static WorkingCard FromProtobuf(this CardInstance cardInstance, Player player)
        {
            return
                new WorkingCard(
                    cardInstance.Prototype.FromProtobuf(),
                    cardInstance.Instance.FromProtobuf(),
                    player,
                    cardInstance.InstanceId.FromProtobuf()
                );
        }

        public static InstanceId FromProtobuf(this Protobuf.InstanceId cardInstance)
        {
            return new InstanceId(cardInstance.Id);
        }
    }
}
