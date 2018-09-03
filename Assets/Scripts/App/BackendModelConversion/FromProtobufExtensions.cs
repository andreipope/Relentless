using System.Linq;
using LoomNetwork.CZB.Helpers;
using LoomNetwork.CZB.Protobuf;

namespace LoomNetwork.CZB.Data
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

        public static FloatVector3 FromProtobuf(this Coordinates coordinates)
        {
            return new FloatVector3(coordinates.X, coordinates.Y, coordinates.Z);
        }

        public static AbilityData FromProtobuf(this Ability ability)
        {
            return new AbilityData
            {
                BuffType = ability.BuffType,
                Type = ability.Type,
                ActivityType = ability.ActivityType,
                CallType = ability.CallType,
                TargetType = ability.TargetType,
                StatType = ability.StatType,
                SetType = ability.SetType,
                EffectType = ability.EffectType,
                CardType = ability.CardType,
                UnitStatus = ability.UnitStatus,
                UnitType = ability.UnitType,
                Value = ability.Value,
                Damage = ability.Damage,
                Health = ability.Health,
                AttackInfo = ability.AttackInfo,
                Name = ability.Name,
                Turns = ability.Turns,
                Count = ability.Count,
                Delay = ability.Delay
            };
        }

        public static CardViewInfo FromProtobuf(this Protobuf.CardViewInfo cardViewInfo)
        {
            if (cardViewInfo == null)
            {
                return null;
            }

            return new CardViewInfo
            {
                Position = cardViewInfo.Position.FromProtobuf(),
                Scale = cardViewInfo.Scale.FromProtobuf()
            };
        }

        public static Card FromProtobuf(this Protobuf.Card card)
        {
            return new Card
            {
                Id = (int) card.Id,
                Kind = card.Kind,
                Name = card.Name,
                Cost = card.Cost,
                Description = card.Description,
                FlavorText = card.FlavorText,
                Picture = card.Picture,
                Damage = card.Damage,
                Health = card.Health,
                Rank = card.Rank,
                Type = card.Type,
                Frame = card.Frame,
                Abilities = card.Abilities.Select(x => x.FromProtobuf()).ToList(),
                CardViewInfo = card.CardViewInfo.FromProtobuf()
            };
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
                Name = !string.IsNullOrEmpty(cardSet.Name) ? cardSet.Name : "none",
                Cards = cardSet.Cards.Select(card => card.FromProtobuf()).ToList()
            };
        }
    }
}
