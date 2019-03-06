using System;
using log4net;

namespace Loom.ZombieBattleground.Helpers
{
    public sealed class ExceptionReporter
    {
        public static void SilentReportException(Exception e)
        {
            Unity.Cloud.UserReporting.Plugin.UnityUserReporting.CurrentClient.LogException(e);
        }

        public static void LogException(ILog log, Exception e)
        {
            log.Error("", e);
            Unity.Cloud.UserReporting.Plugin.UnityUserReporting.CurrentClient.LogException(e);
        }
    }
}
