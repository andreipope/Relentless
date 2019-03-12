using System.Linq;
using log4net.Core;
using log4net.Filter;
using Loom.ZombieBattleground.BackendCommunication;
using UnityEngine;
using Logger = log4net.Repository.Hierarchy.Logger;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

namespace Loom.ZombieBattleground
{
    public static class LoggingPlatformConfiguration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [DidReloadScripts]
#endif
        public static void Setup()
        {
            Logging.Configure();

            if (Logging.NonEssentialLogsDisabled)
            {
                Logger backendFacadeRpc = Logging.GetLogger(nameof(BackendFacade) + "Rpc");
                backendFacadeRpc.Level = Level.Warn;

                Logger timeMetricsContractCallProxy = Logging.GetLogger(nameof(TimeMetricsContractCallProxy));
                timeMetricsContractCallProxy.Level = Level.Warn;
            }
        }

        public static IFilter[] CreateSpammyLogsFilters()
        {
            string[] strings =
            {
                "A ping was received.",
                "A pong to this ping has been sent.",
                "The current output action has been changed",
                "Not a WebSocket handshake response"
            };

            return
                strings
                    .Select(s => new StringMatchFilter
                    {
                        StringToMatch = s,
                        AcceptOnMatch = false
                    })
                    .Cast<IFilter>()
                    .ToArray();
        }
    }
}
