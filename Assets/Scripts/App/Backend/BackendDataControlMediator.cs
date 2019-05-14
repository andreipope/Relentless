using System;
using System.IO;
using System.Threading.Tasks;
using log4net;
using Loom.Client;
using Newtonsoft.Json;
using UnityEngine;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class BackendDataControlMediator : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(BackendDataControlMediator));

        private const string UserDataFileName = "UserLoginData.json";

        private IDataManager _dataManager;

        private IUIManager _uiManager;

        private BackendFacade _backendFacade;

        protected string UserDataFilePath => Path.Combine(Application.persistentDataPath, UserDataFileName);

        public UserDataModel UserDataModel { get; set; }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
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
            {
                return false;
            }

            string modelJson = File.ReadAllText(UserDataFilePath);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_dataManager.ConfigData.EncryptData)
            {
                UserDataModel = JsonConvert.DeserializeObject<UserDataModel>(_dataManager.DecryptData(modelJson));
            } else {
                UserDataModel = JsonConvert.DeserializeObject<UserDataModel>(modelJson);
            }
            return true;
        }

        public bool SetUserDataModel(UserDataModel userDataModel)
        {
            if (userDataModel == null)
                throw new ArgumentNullException(nameof(userDataModel));

            string modelJson = JsonConvert.SerializeObject(userDataModel);

            if (_dataManager.ConfigData.EncryptData)
            {
                File.WriteAllText(UserDataFilePath, _dataManager.EncryptData(modelJson));
            }
            else
            {
                File.WriteAllText(UserDataFilePath, modelJson);
            }
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
                await _backendFacade.SignUp(UserDataModel.UserId);
            }
            catch (TxCommitException e) when (e.Message.Contains("user already exists"))
            {
                // Ignore
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
