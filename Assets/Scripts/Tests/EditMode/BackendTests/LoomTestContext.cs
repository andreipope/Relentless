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

        private UserDataModel _userDataModel;

        public void TestSetUp()
        {
            BackendEndpoint backendEndpoint = GameClient.GetDefaultBackendEndpoint();
            BackendFacade = new BackendFacade(backendEndpoint, contract => new DefaultContractCallProxy(contract));
            BackendFacade.EnableRpcLogging = true;
            _userDataModel = new UserDataModel(TestHelper.Instance.CreateTestUserName(), CryptoUtils.GeneratePrivateKey());
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
            return userId + "_" + Guid.NewGuid();
        }

        private async Task EnsureContract()
        {
            if (BackendFacade.IsConnected)
                return;

            await BackendFacade.CreateContract(_userDataModel.PrivateKey);
        }
    }
}
