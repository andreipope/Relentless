using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Loom.Client;
using LoomNetwork.CZB.BackendCommunication;
using NUnit.Framework;

public static class LoomTestContext
{
    public static BackendFacade BackendFacade;

    public static void TestSetUp(string userId = "Loom")
    {
        BackendFacade = new BackendFacade();
        BackendFacade.UserDataModel = new UserDataModel(userId, "", CryptoUtils.GeneratePrivateKey());
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
        } catch (Exception)
        {
            return;
        }

        throw new AssertionException("Expected an exception");
    }

    public static string CreateUniqueUserId(string userId)
    {
        return userId + new Random().NextDouble();
    }

    private static IEnumerator AsyncTest(Func<Task> action, Action preAction)
    {
        return
            TaskAsIEnumerator(Task.Run(() =>
            {
                try
                {
                    preAction?.Invoke();
                    action().Wait();
                } catch (AggregateException e)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }
            }));
    }

    private static async Task EnsureContract()
    {
        if (BackendFacade.Contract != null && BackendFacade.IsConnected)
            return;

        await BackendFacade.CreateContract(BackendFacade.UserDataModel.PrivateKey);
        /*LoomTestContext.LoomManager.UserDataModel = new LoomUserDataModel("LoomTest" + Random.value, CryptoUtils.GeneratePrivateKey());
        try
        {
            await LoomTestContext.LoomManager.SignUp(LoomTestContext.LoomManager.UserDataModel.UserId);
        } catch (TxCommitException e) when (e.Message.Contains("user already exists"))
        {
            // Ignore
        }*/
    }

    private static IEnumerator TaskAsIEnumerator(Task task)
    {
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
            throw task.Exception;
    }
}
