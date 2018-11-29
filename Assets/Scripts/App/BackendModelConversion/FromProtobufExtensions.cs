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
            Unit.ParameterType Parameter = new Unit.ParameterType();

            if (unit.Parameter != null)
            {
                Parameter = new Unit.ParameterType
                (
                    unit.Parameter.Attack,
                    unit.Parameter.Defense,
                    unit.Parameter.CardName
                );
            }

            return new Unit(
                unit.InstanceId,
                (Enumerators.AffectObjectType) unit.AffectObjectType,
                Parameter
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
                (Enumerators.AbilityActivityType) ability.ActivityType,
                (Enumerators.AbilityCallType) ability.Trigger,
                ability.TargetTypes.Select(t => (Enumerators.AbilityTargetType) t).ToList(),
                (Enumerators.StatType) ability.Stat,
                (Enumerators.SetType) ability.Set,
                (Enumerators.AbilityEffectType) ability.Effect,
                (Enumerators.AttackRestriction) ability.AttackRestriction,
                (Enumerators.CardType) ability.TargetCardType,
                (Enumerators.UnitStatusType) ability.TargetUnitSpecialStatus,
                (Enumerators.CardType) ability.TargetUnitType,
                ability.Value,
                ability.Attack,
                ability.Defense,
                ability.Name,
                ability.Turns,
                ability.Count,
                ability.Delay,
                ability.VisualEffectsToPlay.Select(v => v.FromProtobuf()).ToList(),
                (Enumerators.GameMechanicDescriptionType) ability.GameMechanicDescriptionType,
                (Enumerators.SetType) ability.TargetSet,
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
                (Enumerators.SetType) hero.Element,
                hero.Skills.Select(skill => skill.FromProtobuf()).ToList(),
<<<<<<< HEAD
                hero.PrimarySkill,
                hero.SecondarySkill
=======
                (Enumerators.OverlordSkill)hero.PrimarySkill,
                (Enumerators.OverlordSkill)hero.SecondarySkill
>>>>>>> content-development
            );
        }

        public static HeroSkill FromProtobuf(this Protobuf.Skill skill)
        {
            return new HeroSkill(
<<<<<<< HEAD
=======
                (int)skill.Id,
>>>>>>> content-development
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
                (Enumerators.UnitStatusType) skill.TargetUnitSpecialStatus,
<<<<<<< HEAD
                skill.ElementTargets.Select(t => (Enumerators.SetType) t).ToList()
=======
                skill.ElementTargets.Select(t => (Enumerators.SetType) t).ToList(),
                skill.Unlocked,
                skill.CanSelectTarget
>>>>>>> content-development
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
                deck.Cards.Select(skill => skill.FromProtobuf()).ToList(),
<<<<<<< HEAD
                deck.PrimarySkill,
                deck.SecondarySkill
=======
                (Enumerators.OverlordSkill)deck.PrimarySkill,
                (Enumerators.OverlordSkill)deck.SecondarySkill
>>>>>>> content-development
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

        public static CardViewInfo FromProtobuf(this Protobuf.CardViewInfo cardViewInfo)
        {
            if (cardViewInfo == null)
                return null;

            return new CardViewInfo(cardViewInfo.Position.FromProtobuf(), cardViewInfo.Scale.FromProtobuf());
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
                (Enumerators.SetType) card.Set,
                card.Frame,
                (Enumerators.CardKind) card.Kind,
                (Enumerators.CardRank) card.Rank,
                (Enumerators.CardType) card.Type,
                card.Abilities.Select(a => a.FromProtobuf()).ToList(),
                card.CardViewInfo.FromProtobuf(),
                (Enumerators.UniqueAnimationType) card.UniqueAnimation
            );
        }

        public static List<CardInstance> FromProtobuf(this RepeatedField<CardInstance> repeatedFieldCardInstance)
        {
            List<CardInstance> cardInstances = new List<CardInstance>();
            cardInstances.AddRange(repeatedFieldCardInstance);
            return cardInstances;
        }

        public static WorkingCard FromProtobuf(this CardInstance cardInstance, Player player)
        {
            return
                new WorkingCard(
                    cardInstance.Prototype.FromProtobuf(),
                    cardInstance.Instance.FromProtobuf(),
                    player,
                    cardInstance.InstanceId
                );
        }
    }
}
