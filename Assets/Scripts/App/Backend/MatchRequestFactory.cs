using Loom.Google.Protobuf.Collections;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class MatchRequestFactory
    {
        private readonly long _matchId;

        public MatchRequestFactory(long matchId)
        {
            _matchId = matchId;
        }

        public PlayerActionRequest CreateAction(PlayerAction playerAction)
        {
            return new PlayerActionRequest
            {
                MatchId = _matchId,
                PlayerAction = playerAction
            };
        }

        public EndMatchRequest EndMatch(string userId, string winnerId, long[] matchExperiences)
        {
            return new EndMatchRequest
            {
                UserId = userId,
                MatchId = _matchId,
                WinnerId = winnerId,
                MatchExperiences = { matchExperiences }
            };
        }
    }
}
