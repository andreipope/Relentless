using System;

namespace Loom.ZombieBattleground.Data
{

    /// <summary>
    /// Represents an unique object in a match.
    /// </summary>
    public struct InstanceId : IEquatable<InstanceId>, IComparable<InstanceId>
    {
        public static InstanceId Invalid = new InstanceId(-1);

        public int Id { get; }

        public InstanceId(int id)
        {
            Id = id;
        }

        public bool Equals(InstanceId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is InstanceId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(InstanceId left, InstanceId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InstanceId left, InstanceId right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(InstanceId other)
        {
            return Id.CompareTo(other.Id);
        }

        public override string ToString()
        {
            return $"(InstanceId: {Id})";
        }
    }
}
