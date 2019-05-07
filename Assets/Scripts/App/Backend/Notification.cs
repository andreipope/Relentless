using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public abstract class Notification
    {
        public int Id { get; }

        public DateTime CreatedAt { get; }

        public bool Seen { get; }

        protected Notification(int id, DateTime createdAt, bool seen)
        {
            Id = id;
            CreatedAt = createdAt;
            Seen = seen;
        }
    }

    public class EndMatchNotification : Notification
    {
        public OverlordId OverlordId{ get; }
        public int OldLevel { get; }
        public long OldExperience { get; }
        public int NewLevel { get; }
        public long NewExperience { get; }
        public bool IsWin { get; }
        public IReadOnlyList<LevelReward> Rewards { get; }

        public EndMatchNotification(int id, DateTime createdAt, bool seen, OverlordId overlordId, int oldLevel,
            long oldExperience,
            int newLevel,
            long newExperience,
            bool isWin,
            IReadOnlyList<LevelReward> rewards) : base(id, createdAt, seen)
        {
            OverlordId = overlordId;
            OldLevel = oldLevel;
            OldExperience = oldExperience;
            NewLevel = newLevel;
            NewExperience = newExperience;
            IsWin = isWin;
            Rewards = rewards;
        }
    }
}
