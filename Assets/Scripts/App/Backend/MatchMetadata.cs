using System.Collections.Generic;
using Match = Loom.ZombieBattleground.Protobuf.Match;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class MatchMetadata
    {
        public long Id { get; }
        public IList<string> Topics { get; }
        public Match.Types.Status Status { get; }

        public MatchMetadata(long id, IList<string> topics, Match.Types.Status status)
        {
            Id = id;
            Topics = topics;
            Status = status;
        }
    }
}
