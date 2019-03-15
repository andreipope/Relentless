using log4net.Core;
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

                Logger timeMetricsContractCallProxy = Logging.GetLogger(nameof(CustomContractCallProxy));
                timeMetricsContractCallProxy.Level = Level.Warn;
            }
        }
    }
}
