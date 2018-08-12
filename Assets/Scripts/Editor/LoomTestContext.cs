using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Loom.Client;
using NUnit.Framework;

public static class LoomTestContext
{
    public static LoomManager LoomManager;

    public static void TestSetUp(string userId = "Loom")
    {
        LoomManager = new LoomManager();
        LoomManager.UserDataModel = new LoomUserDataModel(userId, CryptoUtils.GeneratePrivateKey());
    }
    
    public static void TestTearDown()
    {
        LoomManager?.Contract?.Client?.Dispose();
        LoomManager = null;
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
        if (LoomManager.Contract != null && LoomManager.IsConnected)
            return;

        await LoomManager.CreateContract(LoomManager.UserDataModel.PrivateKey);
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
