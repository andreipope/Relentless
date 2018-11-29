using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class Card : ICard, IReadOnlyCard
    {
        [JsonProperty("id")]
        public long MouldId { get; set; }

        public string Name { get; protected set; }

        public int Cost { get; set; }

        public string Description { get; protected set; }

        [JsonProperty("flavor_text")]
        public string FlavorText { get; protected set; }

        public string Picture { get; protected set; }

        public int Damage { get; protected set; }

        public int Health { get; protected set; }

        [JsonProperty("Set")]
        public Enumerators.SetType CardSetType { get; set; }

        public string Frame { get; protected set; }

        [JsonProperty("Kind")]
        public Enumerators.CardKind CardKind { get; protected set; }

        [JsonProperty("Rank")]
        public Enumerators.CardRank CardRank { get; protected set; }

        [JsonProperty("Type")]
        public Enumerators.CardType CardType { get; protected set; }

<<<<<<< HEAD
        public List<AbilityData> Abilities { get; }
=======
        [JsonIgnore]
        public List<AbilityData> InitialAbilities { get; }

        public List<AbilityData> Abilities { get; private set; }
>>>>>>> content-development

        [JsonProperty("card_view_info")]
        public CardViewInfo CardViewInfo { get; protected set; }

        [JsonProperty("unique_animation_type")]
        public Enumerators.UniqueAnimationType UniqueAnimationType { get; protected set; }

        [JsonConstructor]
        public Card(
            long mouldId,
            string name,
            int cost,
            string description,
            string flavorText,
            string picture,
            int damage,
            int health,
            Enumerators.SetType cardSetType,
            string frame,
            Enumerators.CardKind cardKind,
            Enumerators.CardRank cardRank,
            Enumerators.CardType cardType,
            List<AbilityData> abilities,
            CardViewInfo cardViewInfo,
            Enumerators.UniqueAnimationType uniqueAnimationType
            )
        {
            MouldId = mouldId;
            Name = name;
            Cost = cost;
            Description = description;
            FlavorText = flavorText;
            Picture = picture;
            Damage = damage;
            Health = health;
            CardSetType = cardSetType;
            Frame = frame;
            CardKind = cardKind;
            CardRank = cardRank;
            CardType = cardType;
            Abilities = abilities ?? new List<AbilityData>();
            CardViewInfo = cardViewInfo;
            UniqueAnimationType = uniqueAnimationType;
<<<<<<< HEAD
=======
            InitialAbilities = Abilities;
>>>>>>> content-development
        }

        public Card(IReadOnlyCard sourceCard)
        {
            MouldId = sourceCard.MouldId;
            Name = sourceCard.Name;
            Cost = sourceCard.Cost;
            Description = sourceCard.Description;
            FlavorText = sourceCard.FlavorText;
            Picture = sourceCard.Picture;
            Damage = sourceCard.Damage;
            Health = sourceCard.Health;
            CardSetType = sourceCard.CardSetType;
            Frame = sourceCard.Frame;
            CardKind = sourceCard.CardKind;
            CardRank = sourceCard.CardRank;
            CardType = sourceCard.CardType;
            Abilities =
                sourceCard.Abilities
                    .Select(a => new AbilityData(a))
                    .ToList();
            CardViewInfo = new CardViewInfo(sourceCard.CardViewInfo);
            UniqueAnimationType = sourceCard.UniqueAnimationType;
<<<<<<< HEAD
=======
            InitialAbilities = Abilities;
>>>>>>> content-development
        }

        public override string ToString()
        {
            return $"({nameof(Name)}: {Name}, {nameof(CardSetType)}: {CardSetType})";
<<<<<<< HEAD
=======
        }

        public void ForceUpdateAbilities(List<AbilityData> abilities)
        {
            if (abilities != null)
            {
                Abilities = abilities;
            }
>>>>>>> content-development
        }
    }

    public class CardViewInfo
    {
        public FloatVector3 Position { get; protected set; } = FloatVector3.Zero;
        public FloatVector3 Scale { get; protected set; } = new FloatVector3(0.38f);

        public CardViewInfo()
        {
        }
<<<<<<< HEAD

        public CardViewInfo(FloatVector3 position, FloatVector3 scale)
        {
            Position = position;
            Scale = scale;
        }

=======

        public CardViewInfo(FloatVector3 position, FloatVector3 scale)
        {
            Position = position;
            Scale = scale;
        }

>>>>>>> content-development
        public CardViewInfo(CardViewInfo source)
        {
            Position = source.Position;
            Scale = source.Scale;
        }
    }
}
