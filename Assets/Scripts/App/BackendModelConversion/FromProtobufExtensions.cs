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
        public static CollectionCardData FromProtobuf(this CardCollection cardCollection)
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
            return new Unit(
                unit.InstanceId,
                (Enumerators.AffectObjectType) unit.AffectObjectType,
                new Unit.ParameterType(unit.Parameter.Attack, unit.Parameter.Defense, unit.Parameter.CardName)
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
                ability.AllowedTargetTypes.Select(t => (Enumerators.AbilityTargetType) t).ToList(),
                (Enumerators.StatType) ability.Stat,
                (Enumerators.SetType) ability.Set,
                (Enumerators.AbilityEffectType) ability.Effect,
                (Enumerators.AttackRestriction) ability.AttackRestriction,
                (Enumerators.CardType) ability.TargetCardType,
                (Enumerators.UnitStatusType) ability.TargetUnitStatusType,
                (Enumerators.CardType) ability.TargetUnitType,
                ability.Value,
                ability.Attack,
                ability.Defense,
                ability.Name,
                ability.Turns,
                ability.Count,
                ability.Delay,
                ability.VisualEffectsToPlay.Select(v => v.FromProtobuf()).ToList(),
                (Enumerators.MechanicPictureType) ability.MechanicPicture
            );
        }

        public static AbilityData.VisualEffectInfo FromProtobuf(this CardAbility.Types.VisualEffectInfo visualEffectInfo)
        {
            return new AbilityData.VisualEffectInfo(
                (Enumerators.VisualEffectType) visualEffectInfo.Type,
                visualEffectInfo.Path
            );
        }

        public static CardViewInfo FromProtobuf(this Protobuf.CardViewInfo cardViewInfo)
        {
            if (cardViewInfo == null)
                return null;

            return new CardViewInfo
            {
                Position = cardViewInfo.Position.FromProtobuf(),
                Scale = cardViewInfo.Scale.FromProtobuf()
            };
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
                card.CardViewInfo.FromProtobuf()
            );
        }

        public static CardsLibraryData FromProtobuf(this ListCardLibraryResponse listCardLibraryResponse)
        {
            return new CardsLibraryData
            {
                Sets = listCardLibraryResponse.Sets.Select(set => set.FromProtobuf()).ToList()
            };
        }

        public static CardSet FromProtobuf(this Protobuf.CardSet cardSet)
        {
            return new CardSet
            {
                Name = (Enumerators.SetType) cardSet.Name,
                Cards = cardSet.Cards.Select(card => card.FromProtobuf()).ToList()
            };
        }

        public static OpponentDeck FromProtobuf(this Protobuf.Deck deck)
        {
            return new OpponentDeck
            {
                Id = (int) deck.Id,
                HeroId = (int) deck.HeroId,
                Type = Enumerators.AiType.BLITZ_AI,
                Cards = deck.Cards.Select(card => card.GetDeckCardData()).ToList()
            };
        }

        //TOTO: review does need this function at all
        private static DeckCardData GetDeckCardData(this CardCollection cardCollection)
        {
            return new DeckCardData
            {
                CardName = cardCollection.CardName,
                Amount = (int) cardCollection.Amount
            };
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
