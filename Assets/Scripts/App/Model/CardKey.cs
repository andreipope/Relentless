using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    /// <summary>
    /// Represents a card key, which is combination of mould id and edition.
    /// </summary>
    public struct CardKey : IEquatable<CardKey>
    {
        public static IComparer<CardKey> Comparer { get; } = new MouldIdVariantRelationalComparer();

        [JsonProperty("mouldId")]
        public MouldId MouldId { get; }

        [JsonProperty("variant")]
        public Enumerators.CardVariant Variant { get; }

        [JsonConstructor]
        public CardKey(MouldId mouldId, Enumerators.CardVariant variant)
        {
            MouldId = mouldId;
            Variant = variant;
        }

        public bool Equals(CardKey other)
        {
            return MouldId.Equals(other.MouldId) && Variant == other.Variant;
        }

        public override bool Equals(object obj)
        {
            return obj is CardKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (MouldId.GetHashCode() * 397) ^ (int) Variant;
            }
        }

        public static bool operator ==(CardKey left, CardKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CardKey left, CardKey right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({nameof(MouldId)}: {MouldId.Id}, {nameof(Variant)}: {Variant})";
        }

        public static CardKey FromCardTokenId(long tokenId)
        {
            return new CardKey(
                new MouldId(tokenId / 10),
                (Enumerators.CardVariant) (tokenId % 10)
            );
        }

        private sealed class MouldIdVariantRelationalComparer : IComparer<CardKey>
        {
            public int Compare(CardKey x, CardKey y)
            {
                int mouldIdComparison = x.MouldId.CompareTo(y.MouldId);
                if (mouldIdComparison != 0)
                    return mouldIdComparison;

                return x.Variant.CompareTo(y.Variant);
            }
        }
    }
}
