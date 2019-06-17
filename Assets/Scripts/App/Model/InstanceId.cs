using System;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{

    /// <summary>
    /// Represents an unique object in a match.
    /// </summary>
    public struct InstanceId : IEquatable<InstanceId>
    {
        public static InstanceId Invalid = new InstanceId(-1);

        public int Id { get; }

        public InstanceId(int id)
        {
            Id = id;
        }

        public InstanceId(int id, Enumerators.ReasonForInstanceIdChange reasonForChange)
        {
            switch(reasonForChange)
            {
                case Enumerators.ReasonForInstanceIdChange.Reanimate:
                    id = id*1000+1;
                    break;
                case Enumerators.ReasonForInstanceIdChange.BackToDeck:
                    id = id*1000+2;
                    break;
                case Enumerators.ReasonForInstanceIdChange.BackToHand:
                    id = id*1000+3;
                    break;
                case Enumerators.ReasonForInstanceIdChange.BackFromGraveyard:
                    id = id*1000+4;
                    break;
                default:
                    break;
            }
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

        public override string ToString()
        {
            return $"(InstanceId: {Id})";
        }
    }
}
