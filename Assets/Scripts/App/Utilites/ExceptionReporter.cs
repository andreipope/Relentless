using System;
using log4net;
using Unity.Cloud.UserReporting.Plugin;

namespace Loom.ZombieBattleground.Helpers
{
    public static class ExceptionReporter
    {
        public static void SilentReportException(Exception e)
        {
#if !UNITY_EDITOR
            UnityUserReporting.CurrentClient.LogException(e);
#endif
        }

        public static void LogExceptionAsWarning(ILog log, Exception e)
        {
            log.Warn("", e);
            SilentReportException(e);
        }
    }
}
