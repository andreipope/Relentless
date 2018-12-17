using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Loom.ZombieBattleground.Test
{
    public class TestUtility
    {
        public static IEnumerator AsyncTest(Func<Task> testAction)
        {
            int timeout =
                TestContext.CurrentTestExecutionContext.TestCaseTimeout <= 0 ?
                    Timeout.Infinite :
                    TestContext.CurrentTestExecutionContext.TestCaseTimeout;

#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                EditorApplication.update += EditorExecuteContinuations;
            }

            return
                TestHelper.TaskAsIEnumerator(async () =>
                {
                    try
                    {
                        await testAction();
                    }
                    finally
                    {
                        if (EditorApplication.isPlaying) {
                            EditorApplication.update -= EditorExecuteContinuations;
                        }
                    }
                }, timeout);
#else
            return TestHelper.TaskAsIEnumerator(testAction, timeout);
#endif
        }

#if UNITY_EDITOR
        private static void EditorExecuteContinuations()
        {
            SynchronizationContext context = SynchronizationContext.Current;
            MethodInfo execMethod = context.GetType().GetMethod("Exec", BindingFlags.NonPublic | BindingFlags.Instance);
            execMethod.Invoke(context, null);
            Debug.Log("continuie");
        }
#endif
    }
}
