using System.Collections.Generic;
using Loom.Google.Protobuf.Collections;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground.Data
{
    public static class ToProtobufExtensions
    {
        public static CardPrototype GetCardPrototype(this WorkingCard workingCard)
        {
            Card card = workingCard.LibraryCard;
            CardPrototype cardPrototype = new CardPrototype
            {
                DataId = card.Id,
                CardSetType = (ElementKind)card.CardSetType,
                Name = card.Name,
                GooCost = card.Cost,
                Description = card.Description,
                FlavorText = card.FlavorText,
                Picture = card.Picture,
                Frame = card.Frame,
                InitialDamage = card.Damage,
                InitialDefence = card.Health,
                Rank = card.Rank,
                Type = card.Type,

                // TODO : Fill ability later, some problem on backend side now
                //Abilities = new RepeatedField<CardAbility>(){ },

                CardViewInfo = new Protobuf.CardViewInfo
                {
                    Position = new Coordinates { X = card.CardViewInfo.Position.X, Y = card.CardViewInfo.Position.Y, Z = card.CardViewInfo.Position.Z },
                    Scale = new Coordinates { X = card.CardViewInfo.Scale.X, Y = card.CardViewInfo.Scale.Y, Z = card.CardViewInfo.Scale.Z }
                },
                CreatureRank = (CreatureRank)card.CardRank,
                CreatureType = (CreatureType)card.CardType,
                CardKind = (CardKind)card.CardKind,
                Kind = card.Kind
            };

            return cardPrototype;
        }

        private static RepeatedField<CardInstance> GetMulliganCards(this List<WorkingCard> cards)
        {
            RepeatedField<CardInstance> cardInstances = new RepeatedField<CardInstance>();
            for (int i = 0; i < cards.Count; i++)
            {
                cardInstances[i] = new CardInstance
                {
                    InstanceId = cards[i].InstanceId,
                    Prototype = ToProtobufExtensions.GetCardPrototype(cards[i]),
                    Defence = cards[i].Health,
                    Attack = cards[i].Damage
                };
            }

            return cardInstances;
        }
    }

}

