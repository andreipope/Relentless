using System;

namespace Loom.ZombieBattleground.Helpers
{
    public sealed class ExceptionReporter
    {
        public static void LogException(Exception e)
        {
            Unity.Cloud.UserReporting.Plugin.UnityUserReporting.CurrentClient.LogException(e);
        }
    }
}
