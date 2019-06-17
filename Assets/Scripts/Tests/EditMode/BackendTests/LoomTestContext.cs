using System;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using NUnit.Framework;

namespace Loom.ZombieBattleground.Test
{
    public class LoomTestContext
    {
        public BackendFacade BackendFacade;

        private UserDataModel _userDataModel;

        public void TestSetUp()
        {
            (string userId, BigInteger userIdNumber) = TestHelper.Instance.CreateTestUser();
            _userDataModel = new UserDataModel(userId, userIdNumber, CryptoUtils.GeneratePrivateKey());
            BackendEndpoint backendEndpoint = GameClient.GetDefaultBackendEndpoint();
            TaggedLoggerWrapper taggedLoggerWrapper = new TaggedLoggerWrapper(Logging.GetLog(nameof(BackendFacade)).Logger, _userDataModel.UserId);
            ILog log = new LogImpl(taggedLoggerWrapper);
            BackendFacade = new BackendFacade(backendEndpoint, contract => new DefaultContractCallProxy<RawChainEventArgs>(contract), log, log);
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

        private async Task EnsureContract()
        {
            if (BackendFacade.IsConnected)
                return;

            await BackendFacade.CreateContract(_userDataModel.PrivateKey, new DAppChainClientConfiguration());
        }
    }
}
