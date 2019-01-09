using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Test;
using NUnit.Framework;

namespace Loom.ZombieBattleground.Test
{
    public class LoomTestContext
    {
        public BackendFacade BackendFacade;

        public UserDataModel UserDataModel;

        public void TestSetUp(string userId = "Loom")
        {
            BackendEndpoint backendEndpoint = new BackendEndpoint(BackendEndpointsContainer.Endpoints[BackendPurpose.Local]);
            BackendFacade = new BackendFacade(backendEndpoint);
            UserDataModel = new UserDataModel(userId, CryptoUtils.GeneratePrivateKey());
        }

        public void TestTearDown()
        {
            BackendFacade?.Dispose();
            BackendFacade = null;
        }

        public IEnumerator ContractAsyncTest(Func<Task> action)
        {
            return TestUtility.AsyncTest(async () =>
            {
                await EnsureContract();
                await action();
            });
        }

        public async Task AssertThrowsAsync(Func<Task> func)
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

        public string CreateUniqueUserId(string userId)
        {
            return userId + "_" + new Random().NextDouble();
        }

        private async Task EnsureContract()
        {
            if (BackendFacade.IsConnected)
                return;

            await BackendFacade.CreateContract(UserDataModel.PrivateKey);
        }
    }
}
