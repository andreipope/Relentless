using NUnit.Framework;
using UnityEngine;

namespace Loom.ZombieBattleground.Test
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class UnitTestDetector
    {
        private static bool _isRegistered;

        static UnitTestDetector()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        private static void Register()
        {
            if (_isRegistered)
                return;

            _isRegistered = true;
            ZombieBattleground.UnitTestDetector.CheckRequested += Update;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += change =>
            {
                Update();
            };
#endif
        }

        private static void Update()
        {
            ZombieBattleground.UnitTestDetector.SetRunningUnitTest(TestContext.CurrentTestExecutionContext != null);
        }
    }
}
