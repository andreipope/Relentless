using System.Collections.Generic;
using Match = Loom.ZombieBattleground.Protobuf.Match;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class MatchMetadata
    {
        public long Id { get; set; }
        public IList<string> Topics { get; set; }
        public Match.Types.Status Status { get; set; }
    }
}
