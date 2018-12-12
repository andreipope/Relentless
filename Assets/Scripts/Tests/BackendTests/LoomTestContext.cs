using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using NUnit.Framework;

public static class LoomTestContext
{
    public static BackendFacade BackendFacade;

    public static UserDataModel UserDataModel;

    public static void TestSetUp(string userId = "Loom")
    {
        BackendEndpoint backendEndpoint = new BackendEndpoint(BackendEndpointsContainer.Endpoints[BackendPurpose.Local]);
        BackendFacade = new BackendFacade(backendEndpoint);
        UserDataModel = new UserDataModel(userId, CryptoUtils.GeneratePrivateKey());
    }

    public static void TestTearDown()
    {
        BackendFacade?.Contract?.Client?.Dispose();
        BackendFacade = null;
    }

    public static IEnumerator AsyncTest(Func<Task> action)
    {
        return AsyncTest(action, null);
    }

    public static IEnumerator ContractAsyncTest(Func<Task> action)
    {
        return AsyncTest(action, () => EnsureContract().Wait());
    }

    public static async Task AssertThrowsAsync(Func<Task> func)
    {
        try
        {
            await func();
            throw new AssertionException("Expected an exception");
        }
        catch
        {
        }
    }

    public static string CreateUniqueUserId(string userId)
    {
        return userId + "_" + new Random().NextDouble();
    }

    private static IEnumerator AsyncTest(Func<Task> action, Action preAction)
    {
        return TaskAsIEnumerator(
            Task.Run(
                () =>
                {
                    try
                    {
                        preAction?.Invoke();
                        action().Wait();
                    }
                    catch (AggregateException e)
                    {
                        ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                    }
                }));
    }

    private static async Task EnsureContract()
    {
        if (BackendFacade.Contract != null && BackendFacade.IsConnected)
            return;

        await BackendFacade.CreateContract(UserDataModel.PrivateKey);
    }

    private static IEnumerator TaskAsIEnumerator(Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        task.Wait();
    }
}
