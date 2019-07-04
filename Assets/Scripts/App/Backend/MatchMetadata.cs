using System.Collections.Generic;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class MatchMetadata
    {
        public long Id { get; }
        public IList<string> Topics { get; }
        public bool UseBackendGameLogic { get; }

        public MatchMetadata(long id, IList<string> topics, bool useBackendGameLogic)
        {
            Id = id;
            Topics = topics;
            UseBackendGameLogic = useBackendGameLogic;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Topics)}: {Utilites.FormatCallLogList(Topics)}, {nameof(UseBackendGameLogic)}: {UseBackendGameLogic}";
        }
    }
}
