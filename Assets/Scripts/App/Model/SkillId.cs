using System;

namespace Loom.ZombieBattleground.Data
{
    /// <summary>
    /// Represents a unique skill identifier.
    /// </summary>
    public struct SkillId : IId<long>, IEquatable<SkillId>
    {
        public long Id { get; }

        public SkillId(long id)
        {
            Id = id;
        }

        public bool Equals(SkillId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SkillId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(SkillId left, SkillId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SkillId left, SkillId right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({nameof(SkillId)}: {Id})";
        }
    }
}
