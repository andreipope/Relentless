using System.Collections.Generic;
using Match = Loom.ZombieBattleground.Protobuf.Match;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class MatchMetadata
    {
        public long Id { get; }
        public IList<string> Topics { get; }
        public Match.Types.Status Status { get; }
        public bool UseBackendGameLogic { get; }

        public MatchMetadata(long id, IList<string> topics, Match.Types.Status status, bool useBackendGameLogic)
        {
            Id = id;
            Topics = topics;
            Status = status;
            UseBackendGameLogic = useBackendGameLogic;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Topics)}: {Utilites.FormatCallLogList(Topics)}, {nameof(Status)}: {Status}, {nameof(UseBackendGameLogic)}: {UseBackendGameLogic}";
        }
    }
}
