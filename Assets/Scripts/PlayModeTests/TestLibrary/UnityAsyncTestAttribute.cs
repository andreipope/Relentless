using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using NUnit.Framework.Internal.Commands;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    /// <summary>
    ///   <para>Special type of a unit test that allows to return a Task.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class UnityAsyncTestAttribute : UnityTestAttribute, ISimpleTestBuilder, ICommandWrapper
    {
        private readonly NUnitTestCaseBuilder _builder = new NUnitTestCaseBuilder();

        TestCommand ICommandWrapper.Wrap(TestCommand command)
        {
            Debug.Log("wrap!");
            WrapAsyncToEnumerableCommand asyncWrap = new WrapAsyncToEnumerableCommand(command);
            return
                new UnityTestToolsWrapper.UnityLogCheckDelegatingCommand(
                    new UnityTestToolsWrapper.EnumerableSetUpTearDownCommand(
                        new UnityTestToolsWrapper.EnumerableTestMethodCommand((TestMethod) asyncWrap.GetInnerCommand().Test).Instance
                    ).Instance
                ).Instance;
        }

        /*TestMethod ISimpleTestBuilder.BuildFrom(IMethodInfo method, NUnit.Framework.Internal.Test suite)
        {
            TestCaseParameters testCaseParameters = new TestCaseParameters();
            testCaseParameters.ExpectedResult = new object();
            testCaseParameters.HasExpectedResult = true;
            TestCaseParameters parms = testCaseParameters;
            TestMethod testMethod = _builder.BuildTestMethod(method, suite, parms);
            if (testMethod.parms != null )
            {
                testMethod.parms.HasExpectedResult = false;
            }
            return testMethod;
        }*/

        public class Wrapper
        {
            private Func<Task> _taskMethodCallFunc;

            public Wrapper(Func<Task> taskMethodCallFunc)
            {
                _taskMethodCallFunc = taskMethodCallFunc;
            }

            public IEnumerator Call()
            {
                return TestHelper.TaskAsIEnumerator(_taskMethodCallFunc);
            }
        }

        public class WrapAsyncToEnumerableCommand : DelegatingTestCommand
        {
            public WrapAsyncToEnumerableCommand(TestCommand innerCommand)
                : base(innerCommand)
            {
            }

            public override TestResult Execute(ITestExecutionContext context)
            {
                innerCommand = WrapTestCommand(innerCommand, context);
                context.CurrentResult = innerCommand.Execute(context);
                return context.CurrentResult;
            }

            private static TestCommand WrapTestCommand(TestCommand innerCommand, ITestExecutionContext context)
            {
                TestMethod testMethod = (TestMethod) innerCommand.Test;
                Task task = testMethod.Method.MethodInfo.Invoke(context.TestObject, testMethod.parms?.OriginalArguments) as Task;

                Func<IEnumerator> wrapperFunc = () => TestHelper.TaskAsIEnumerator(task);
                testMethod.Method = new MethodWrapper(typeof(WrapAsyncToEnumerableCommand), wrapperFunc.Method);

                return innerCommand;
            }
        }

        /*internal class AsyncToEnumerableConverterTestMethodCommand : TestCommand, IEnumerableTestMethodCommand
        {
            public AsyncToEnumerableConverterTestMethodCommand(TestMethod testMethod) : base(testMethod)
            {

            }

            wrap

            public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
            {
                // ISSUE: object of a compiler-generated type is created
                // ISSUE: variable of a compiler-generated type
                EnumerableTestMethodCommand.\u003CExecuteEnumerable\u003Ec__Iterator0 enumerableCIterator0 = new EnumerableTestMethodCommand.\u003CExecuteEnumerable\u003Ec__Iterator0()
                {
                    context = context,
                    \u0024this = this
                };
                // ISSUE: reference to a compiler-generated field
                enumerableCIterator0.\u0024PC = -2;
                return (IEnumerable) enumerableCIterator0;
            }
        }*/
    }
}
