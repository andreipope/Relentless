using System;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    /// <summary>
    /// Represents a unique deck ID.
    /// </summary>
    [JsonConverter(typeof(JsonIdConverter<DeckId, long>))]
    public struct DeckId : IId<long>, IEquatable<DeckId>
    {
        public long Id { get; }

        public DeckId(long id)
        {
            Id = id;
        }

        public bool Equals(DeckId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DeckId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(DeckId left, DeckId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DeckId left, DeckId right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({nameof(DeckId)}: {Id})";
        }
    }
}
