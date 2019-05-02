using System;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    /// <summary>
    /// Represents a unique card mould id.
    /// </summary>
    [JsonConverter(typeof(MouldIdConverter))]
    public struct MouldId : IEquatable<MouldId>
    {
        public long Id { get; }

        public MouldId(long id)
        {
            Id = id;
        }

        public bool Equals(MouldId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MouldId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(MouldId left, MouldId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MouldId left, MouldId right)
        {
            return !left.Equals(right);
        }

        /*public static bool operator ==(MouldId left, int right)
        {
            return left.Id == right;
        }

        public static bool operator !=(MouldId left, int right)
        {
            return left.Id != right;
        }*/

        public override string ToString()
        {
            return $"({nameof(MouldId)}: {Id})";
        }
    }
}
