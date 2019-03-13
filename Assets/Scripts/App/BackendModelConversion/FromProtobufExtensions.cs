using System.Collections.Generic;
using System.Linq;
using Loom.Google.Protobuf.Collections;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;

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
                    unit.Parameter.Attack,
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

        public static AbilityData FromProtobuf(this CardAbility ability)
        {
            return new AbilityData(
                (Enumerators.AbilityType) ability.Type,
                (Enumerators.AbilityActivity) ability.ActivityType,
                (Enumerators.AbilityTrigger) ability.Trigger,
                ability.TargetTypes.Select(t => (Enumerators.Target) t).ToList(),
                (Enumerators.Stat) ability.Stat,
                (Enumerators.Faction) ability.Set,
                (Enumerators.AbilityEffect) ability.Effect,
                (Enumerators.AttackRestriction) ability.AttackRestriction,
                (Enumerators.CardType) ability.TargetCardType,
                (Enumerators.UnitStatus) ability.TargetUnitSpecialStatus,
                (Enumerators.CardType) ability.TargetUnitType,
                ability.Value,
                ability.Attack,
                ability.Defense,
                ability.Name,
                ability.Turns,
                ability.Count,
                ability.Delay,
                ability.VisualEffectsToPlay.Select(v => v.FromProtobuf()).ToList(),
                (Enumerators.GameMechanicDescription) ability.GameMechanicDescriptionType,
                (Enumerators.Faction) ability.TargetSet,
                (Enumerators.AbilitySubTrigger) ability.SubTrigger,
                ability.ChoosableAbilities.Select(c => c.FromProtobuf()).ToList(),
                ability.Defense2,
                ability.Cost
            );
        }

        public static Hero FromProtobuf(this Protobuf.Hero hero)
        {
            return new Hero(
                (int) hero.HeroId,
                hero.Icon,
                hero.Name,
                hero.ShortDescription,
                hero.LongDescription,
                hero.Experience,
                (int) hero.Level,
                (Enumerators.Faction) hero.Element,
                hero.Skills.Select(skill => skill.FromProtobuf()).ToList(),
                (Enumerators.OverlordSkill)hero.PrimarySkill,
                (Enumerators.OverlordSkill)hero.SecondarySkill
            );
        }

        public static HeroSkill FromProtobuf(this Protobuf.Skill skill)
        {
            return new HeroSkill(
                (int)skill.Id,
                skill.Title,
                skill.IconPath,
                skill.Description,
                skill.Cooldown,
                skill.InitialCooldown,
                skill.Value,
                skill.Attack,
                skill.Count,
                (Enumerators.OverlordSkill) skill.Skill_,
                skill.SkillTargets.Select(t => (Enumerators.SkillTargetType) t).ToList(),
                (Enumerators.UnitStatus) skill.TargetUnitSpecialStatus,
                skill.ElementTargets.Select(t => (Enumerators.Faction) t).ToList(),
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
                (int) deck.HeroId,
                deck.Name,
                deck.Cards.Select(card => card.FromProtobuf()).ToList(),
                (Enumerators.OverlordSkill)deck.PrimarySkill,
                (Enumerators.OverlordSkill)deck.SecondarySkill
            );
        }

        public static DeckCardData FromProtobuf(this Protobuf.DeckCard card)
        {
            return new DeckCardData(
                card.CardName,
                (int) card.Amount
            );
        }

        public static AbilityData.VisualEffectInfo FromProtobuf(this CardAbility.Types.VisualEffectInfo visualEffectInfo)
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
                choosableAbility.AbilityData.FromProtobuf()
            );
        }

        public static PictureTransform FromProtobuf(this Protobuf.CardViewInfo cardViewInfo)
        {
            if (cardViewInfo == null)
                return null;

            return new PictureTransform(cardViewInfo.Position.FromProtobuf(), cardViewInfo.Scale.FromProtobuf());
        }

        public static Card FromProtobuf(this Protobuf.Card card)
        {
            return new Card(
                card.MouldId,
                card.Name,
                card.GooCost,
                card.Description,
                card.FlavorText,
                card.Picture,
                card.Attack,
                card.Defense,
                (Enumerators.Faction) card.Set,
                card.Frame,
                (Enumerators.CardKind) card.Kind,
                (Enumerators.CardRank) card.Rank,
                (Enumerators.CardType) card.Type,
                card.Abilities.Select(a => a.FromProtobuf()).ToList(),
                card.CardViewInfo.FromProtobuf(),
                (Enumerators.UniqueAnimation) card.UniqueAnimationType,
                card.Hidden
            );
        }

        public static CardInstanceSpecificData FromProtobuf(this Protobuf.CardInstanceSpecificData card)
        {
           return new CardInstanceSpecificData(
                card.Attack,
                card.Defense,
                (Enumerators.Faction) card.Set,
                (Enumerators.CardType) card.Type,
                card.GooCost
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
