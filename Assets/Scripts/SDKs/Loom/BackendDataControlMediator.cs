using System;
using System.IO;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Newtonsoft.Json;
using UnityEngine;

namespace LoomNetwork.CZB.BackendCommunication
{
    public class BackendDataControlMediator : IService
    {
        private const string UserDataFileName = "UserData.json";
        
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

            UserDataModel = JsonConvert.DeserializeObject<UserDataModel>(File.ReadAllText(UserDataFilePath));
            return true;
        }

        public bool SetUserDataModel(UserDataModel userDataModel)
        {
            if (userDataModel == null)
                throw new ArgumentNullException(nameof(userDataModel));

            File.WriteAllText(UserDataFilePath, JsonConvert.SerializeObject(userDataModel));
            UserDataModel = userDataModel;
            return true;
        }

        public async Task LoginAndLoadData()
        {
            LoadUserDataModel();
            Debug.Log("User Id: " + UserDataModel.UserId);

            try
            {
                await _backendFacade.CreateContract(UserDataModel.PrivateKey);
            } catch (Exception)
            {
                // HACK: ignore to allow offline mode
            }
            
            try
            {
                await _backendFacade.SignUp(UserDataModel.UserId);
            } catch (TxCommitException e) when (e.Message.Contains("user already exists"))
            {
                // Ignore
            } catch (Exception)
            {
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
