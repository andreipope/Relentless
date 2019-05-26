using System;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    /// <summary>
    /// Represents a unique overlord ID.
    /// </summary>
    [JsonConverter(typeof(JsonIdConverter<OverlordId, long>))]
    public struct OverlordId : IId<long>, IEquatable<OverlordId>
    {
        public long Id { get; }

        public OverlordId(long id)
        {
            Id = id;
        }

        public bool Equals(OverlordId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is OverlordId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(OverlordId left, OverlordId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OverlordId left, OverlordId right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({nameof(OverlordId)}: {Id})";
        }
    }
}
