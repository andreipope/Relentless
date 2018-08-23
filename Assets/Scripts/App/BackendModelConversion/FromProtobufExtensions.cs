using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoomNetwork.CZB.Protobuf;
using LoomNetwork.CZB.Helpers;

namespace LoomNetwork.CZB.Data
{
    public static class FromProtobufExtensions
    {
        public static CollectionCardData FromProtobuf(this CardCollection cardCollection) {
            return new CollectionCardData
            {
                amount = (int) cardCollection.Amount,
                cardName = cardCollection.CardName
            };
        }

        public static CollectionData FromProtobuf(this GetCollectionResponse getCollectionResponse) {
            return new CollectionData
            {
                cards = getCollectionResponse.Cards
                    .Select(card => card.FromProtobuf())
                    .ToList()
            };
        }

        public static FloatVector3 FromProtobuf(this LoomNetwork.CZB.Protobuf.Coordinates coordinates) {
            return new FloatVector3(coordinates.X, coordinates.Y, coordinates.Z);
        }

        public static AbilityData FromProtobuf(this LoomNetwork.CZB.Protobuf.Ability ability) {
            return new AbilityData
            {
                buffType = ability.BuffType,
                type = ability.Type,
                activityType = ability.ActivityType,
                callType = ability.CallType,
                targetType = ability.TargetType,
                statType = ability.StatType,
                setType = ability.SetType,
                effectType = ability.EffectType,
                cardType = ability.CardType,
                unitStatus = ability.UnitStatus,
                unitType = ability.UnitType,
                value = ability.Value,
                damage = ability.Damage,
                health = ability.Health,
                attackInfo = ability.AttackInfo,
                name = ability.Name,
                turns = ability.Turns,
                count = ability.Count,
                delay = ability.Delay,
            };
        }

        public static CardViewInfo FromProtobuf(this LoomNetwork.CZB.Protobuf.CardViewInfo cardViewInfo) {
            if (cardViewInfo == null)
                return null;
            
            return new CardViewInfo
            {
                position = cardViewInfo.Position.FromProtobuf(),
                scale = cardViewInfo.Scale.FromProtobuf()
            };
        }

        public static Card FromProtobuf(this LoomNetwork.CZB.Protobuf.Card card) {
            return new Card
            {
                id = (int) card.Id,
                kind = card.Kind,
                name = card.Name,
                cost = card.Cost,
                description = card.Description,
                flavorText = card.FlavorText,
                picture = card.Picture,
                damage = card.Damage,
                health = card.Health,
                rank = card.Rank,
                type = card.Type,
                frame = card.Frame,
                abilities =
                    card.Abilities
                    .Select(x => x.FromProtobuf()).ToList(),
                cardViewInfo = card.CardViewInfo.FromProtobuf(),
            };
        }

        public static CardsLibraryData FromProtobuf(this ListCardLibraryResponse listCardLibraryResponse) {
            return new CardsLibraryData
            {
                sets = listCardLibraryResponse.Sets
                    .Select(set => set.FromProtobuf())
                    .ToList()
            };
        }

        public static CardSet FromProtobuf(this LoomNetwork.CZB.Protobuf.CardSet cardSet) {
            return new CardSet
            {
                name = !String.IsNullOrEmpty(cardSet.Name) ? cardSet.Name : "none",
                cards = cardSet.Cards
                    .Select(card => card.FromProtobuf())
                    .ToList()
            };
        }
    }
}