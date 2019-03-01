using System;

namespace Loom.ZombieBattleground.Helpers
{
    public sealed class ExceptionReporter
    {
        public static void LogException(Exception e)
        {
            Unity.Cloud.UserReporting.Plugin.UnityUserReporting.CurrentClient.LogException(e);
        }

        public static void LogException(string e)
        {
            UnityEngine.Debug.Log(e);
            Unity.Cloud.UserReporting.Plugin.UnityUserReporting.CurrentClient.LogException(new Exception(e));
        }
    }
}
