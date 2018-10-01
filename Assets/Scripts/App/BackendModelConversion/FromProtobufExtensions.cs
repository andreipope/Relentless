using System.Linq;
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

        public static OpponentDeck FromProtobuf(this Protobuf.Deck deck)
        {
            return new OpponentDeck
            {
                Id = (int)deck.Id,
                HeroId = (int)deck.HeroId,
                Type = string.Empty,
                Cards = deck.Cards.Select(card => card.GetDeckCardData()).ToList()
            };
        }
        //TOTO: review does need this function at all
        private static DeckCardData GetDeckCardData(this CardCollection cardCollection)
        {
            return new DeckCardData
            {
                CardName = cardCollection.CardName,
                Amount = (int)cardCollection.Amount
            };
        }

        public static WorkingCard FromProtobuf(CardInstance cardInstance, Player player)
        {
            CardPrototype cardPrototype = cardInstance.Prototype;

            Card card = new Card
            {
                Id = cardPrototype.DataId,
                Kind = cardPrototype.Kind,
                Name = cardPrototype.Name,
                Cost = cardPrototype.GooCost,
                Description = cardPrototype.Description,
                FlavorText = cardPrototype.FlavorText,
                Picture = cardPrototype.Picture,
                Damage = cardPrototype.InitialDamage,
                Health = cardPrototype.InitialDefence,
                Rank = cardPrototype.Rank,
                Type = cardPrototype.Type,

                // TODO : Need Fill Ability
                //Abilities = cardPrototype.Abilities,
                CardViewInfo = new CardViewInfo
                {
                    Position = new FloatVector3 {
                        X = cardPrototype.CardViewInfo.Position.X,
                        Y = cardPrototype.CardViewInfo.Position.Y,
                        Z = cardPrototype.CardViewInfo.Position.Z
                    },
                    Scale = new FloatVector3 {
                        X = cardPrototype.CardViewInfo.Scale.X,
                        Y = cardPrototype.CardViewInfo.Scale.Y,
                        Z = cardPrototype.CardViewInfo.Scale.Z
                    }
                },
                Frame = cardPrototype.Frame,
                CardSetType = (Enumerators.SetType)cardPrototype.CardSetType,
                CardRank = (Enumerators.CardRank)cardPrototype.CreatureRank,
                CardType = (Enumerators.CardType)cardPrototype.CreatureType,
                CardKind = (Enumerators.CardKind)cardPrototype.CardKind
            };

            return new WorkingCard(card, player);
        }
    }
}
