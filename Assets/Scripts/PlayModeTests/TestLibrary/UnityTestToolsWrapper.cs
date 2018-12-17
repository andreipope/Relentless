using System;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public static class UnityTestToolsWrapper
    {
        public class EnumerableTestMethodCommand : SimpleReflectionWrapper<TestCommand>
        {
            protected override Type Type => typeof(UnityTestAttribute).Assembly.GetType("UnityEngine.TestTools.EnumerableTestMethodCommand", true);

            public EnumerableTestMethodCommand(TestMethod testMethod)
            {
                // ReSharper disable once PossibleNullReferenceException
                Instance = (TestCommand) Type
                    .GetConstructor(new[] { typeof(TestMethod) })
                    .Invoke(new object[] { testMethod });
            }
        }

        public class EnumerableSetUpTearDownCommand : SimpleReflectionWrapper<DelegatingTestCommand>
        {
            protected override Type Type => typeof(UnityTestAttribute).Assembly.GetType("UnityEngine.TestTools.EnumerableSetUpTearDownCommand", true);

            public EnumerableSetUpTearDownCommand(TestCommand testCommand)
            {
                // ReSharper disable once PossibleNullReferenceException
                Instance =  (DelegatingTestCommand) Type
                    .GetConstructor(new[] { typeof(TestCommand) })
                    .Invoke(new object[] { testCommand });
            }
        }

        public class UnityLogCheckDelegatingCommand : SimpleReflectionWrapper<DelegatingTestCommand>
        {
            protected override Type Type => typeof(UnityTestAttribute).Assembly.GetType("UnityEngine.TestRunner.NUnitExtensions.Runner.UnityLogCheckDelegatingCommand", true);

            public UnityLogCheckDelegatingCommand(TestCommand testCommand)
            {
                // ReSharper disable once PossibleNullReferenceException
                Instance =  (DelegatingTestCommand) Type
                    .GetConstructor(new[] { typeof(TestCommand) })
                    .Invoke(new object[] { testCommand });
            }
        }

        public abstract class SimpleReflectionWrapper<TVisibleType>
        {
            protected abstract Type Type { get; }

            public TVisibleType Instance { get; protected set; }
        }
    }
}
