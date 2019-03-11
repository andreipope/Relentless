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
            return new IFilter[]
            {
                new StringMatchFilter
                {
                    StringToMatch = "A ping was received.",
                    AcceptOnMatch = false
                },
                new StringMatchFilter
                {
                    StringToMatch = "A pong to this ping has been sent.",
                    AcceptOnMatch = false
                },
                new StringMatchFilter
                {
                    StringToMatch = "The current output action has been changed",
                    AcceptOnMatch = false
                }
            };
        }
    }
}
