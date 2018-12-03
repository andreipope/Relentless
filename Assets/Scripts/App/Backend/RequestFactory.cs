using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public static class RequestFactory
    {
        public static PlayerActionRequest CreateAction(long matchId, PlayerAction playerAction)
        {
            return new PlayerActionRequest
            {
                MatchId = matchId,
                PlayerAction = playerAction
            };
        }

        public static EndMatchRequest EndMatch(string userId, int matchId, string winnerId)
        {
            return new EndMatchRequest
            {
                UserId = userId,
                MatchId = matchId,
                WinnerId = winnerId
            };
        }
    }
}
