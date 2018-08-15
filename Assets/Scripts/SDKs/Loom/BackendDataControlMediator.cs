using System;
using System.IO;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Newtonsoft.Json;
using LoomNetwork.CZB.Common;
using LoomNetwork.Internal;
using UnityEngine;

namespace LoomNetwork.CZB.BackendCommunication
{
    public class BackendDataControlMediator : IService
    {
        private const string UserDataFileName = "UserLoginData.json";
        
        private IDataManager _dataManager;
        private BackendFacade _backendFacade;
        
        protected string UserDataFilePath => Path.Combine(Application.persistentDataPath, UserDataFileName);
  
        public UserDataModel UserDataModel { get; set; }

        public bool LoadUserDataModel(bool force = false)
        {
            if (UserDataModel != null && !force)
                return true;

            if (!File.Exists(UserDataFilePath))
                return false;

            string modelJson = File.ReadAllText(UserDataFilePath);
            if (Constants.DATA_ENCRYPTION_ENABLED)
            {
                modelJson = Utilites.Decrypt(modelJson, Constants.PRIVATE_ENCRYPTION_KEY_FOR_APP);
            }
            
            UserDataModel = JsonConvert.DeserializeObject<UserDataModel>(modelJson);
            return true;
        }

        public bool SetUserDataModel(UserDataModel userDataModel)
        {
            if (userDataModel == null)
                throw new ArgumentNullException(nameof(userDataModel));

            string modelJson = JsonConvert.SerializeObject(userDataModel);
            if (Constants.DATA_ENCRYPTION_ENABLED)
            {
                modelJson = Utilites.Encrypt(modelJson, Constants.PRIVATE_ENCRYPTION_KEY_FOR_APP);
            }
                
            File.WriteAllText(UserDataFilePath, modelJson);
            UserDataModel = userDataModel;
            return true;
        }

        public async Task LoginAndLoadData()
        {
            LoadUserDataModel();
            Debug.Log("User Id: " + UserDataModel.UserId);

            await _dataManager.LoadRemoteConfig();
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD && !FORCE_LOCAL_ENDPOINT
            if (_dataManager.BetaConfig.LatestVersion != Constants.CURRENT_VERSION_FULL) 
                throw new GameVersionMismatchException(Constants.CURRENT_VERSION_FULL, _dataManager.BetaConfig.LatestVersion);
#endif

            try
            {
                await _backendFacade.CreateContract(UserDataModel.PrivateKey);
            } catch (Exception e)
            {
                Debug.LogWarning(e);
                // HACK: ignore to allow offline mode
            }
            
            try
            {
                await _backendFacade.SignUp(UserDataModel.UserId);
            } catch (TxCommitException e) when (e.Message.Contains("user already exists"))
            {
                // Ignore
            } catch (Exception e)
            {
                Debug.LogWarning(e);
                // HACK: ignore to allow offline mode
            }

            await _dataManager.StartLoadCache();
        }
        
        /*public async Task LoadUserDataModelAndCreateContract()
        {
            LoadUserDataModel();
            Debug.Log("User Id: " + UserDataModel.UserId);
            await _backendFacade.CreateContract(UserDataModel.PrivateKey);
        }*/
        
        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
        }

        public void Update()
        {

        }

        public void Dispose()
        {

        }
    }
}
