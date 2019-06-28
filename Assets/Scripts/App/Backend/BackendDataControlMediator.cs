using System;
using System.IO;
using System.Threading.Tasks;
using log4net;
using Loom.Client;
using Newtonsoft.Json;
using UnityEngine;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Iap;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class BackendDataControlMediator : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(BackendDataControlMediator));

        private const string UserDataFileName = "UserLoginData.json";

        private IDataManager _dataManager;

        private IUIManager _uiManager;

        private BackendFacade _backendFacade;
        private AuthApiFacade _authApiFacade;
        private AuthFiatApiFacade _authFiatApiFacade;
        private PlasmachainBackendFacade _plasmaChainBackendFacade;

        protected string UserDataFilePath => Path.Combine(Application.persistentDataPath, UserDataFileName);

        public UserDataModel UserDataModel { get; set; }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _authApiFacade = GameClient.Get<AuthApiFacade>();
            _authFiatApiFacade = GameClient.Get<AuthFiatApiFacade>();
            _plasmaChainBackendFacade = GameClient.Get<PlasmachainBackendFacade>();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public bool LoadUserDataModel(bool force = false)
        {
            if (UserDataModel != null && !force)
            {
                return true;
            }

            if (!File.Exists(UserDataFilePath))
                return false;

            string modelJson = File.ReadAllText(UserDataFilePath);
            UserDataModel = JsonConvert.DeserializeObject<UserDataModel>(_dataManager.DecryptData(modelJson));
            return true;
        }

        public bool SetUserDataModel(UserDataModel userDataModel)
        {
            if (userDataModel == null)
                throw new ArgumentNullException(nameof(userDataModel));

            string modelJson = JsonConvert.SerializeObject(userDataModel);
            File.WriteAllText(UserDataFilePath, _dataManager.EncryptData(modelJson));
            UserDataModel = userDataModel;
            return true;
        }

        public async Task LoginAndLoadData()
        {
            LoadUserDataModel();
      
            Log.Info("User Id: " + UserDataModel.UserId);

            try
            {
                await CreateContract();
                try
                {
                    await _backendFacade.SignUp(UserDataModel.UserId);
                }
                catch (TxCommitException e) when (e.Message.Contains("user already exists"))
                {
                    // Ignore
                }
                await _backendFacade.Login(UserDataModel.UserId);
            }
            catch (RpcClientException exception)
            {
                Helpers.ExceptionReporter.SilentReportException(exception);
                Log.Warn("RpcException ==", exception);
                if (UserDataModel.IsValid) 
                {
                    GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(exception);
                }
            }

            await _dataManager.StartLoadCache();      
        }

        public async Task UpdateEndpointsFromZbVersion()
        {
            try
            {
                if (_backendFacade.BackendEndpoint == BackendEndpointsContainer.Endpoints[BackendPurpose.Production])
                {
                    BackendEndpoint backendEndpoint =
                        await _authApiFacade.GetBackendEndpointFromZbVersion(_backendFacade.BackendEndpoint.PlasmachainEndpointsConfiguration);
                    _backendFacade.BackendEndpoint = backendEndpoint;
                    _plasmaChainBackendFacade.EndpointsConfiguration = backendEndpoint.PlasmachainEndpointsConfiguration;
                    _authApiFacade.AuthApiHost = backendEndpoint.AuthHost;
                    _authApiFacade.VaultApiHost = backendEndpoint.VaultHost;
                    _authFiatApiFacade.AuthApiHost = backendEndpoint.AuthHost;
                }
            }
            catch (Exception e)
            {
                Log.Warn("Failed to update endpoints from zbversion", e);
            }
        }

        private async Task CreateContract()
        {
            DAppChainClientConfiguration clientConfiguration = new DAppChainClientConfiguration
            {
                CallTimeout = Constants.BackendCallTimeout,
                StaticCallTimeout = Constants.BackendCallTimeout
            };
            IDAppChainClientCallExecutor chainClientCallExecutor = new NotifyingDAppChainClientCallExecutor(clientConfiguration);
            await _backendFacade.CreateContract(UserDataModel.PrivateKey, clientConfiguration, chainClientCallExecutor: chainClientCallExecutor);
        }
    }
}
