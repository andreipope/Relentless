using System;
using System.IO;
using System.Threading.Tasks;
using Loom.Client;
using LoomNetwork.CZB.Common;
using LoomNetwork.Internal;
using Newtonsoft.Json;
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
            Debug.Log("User Id: " + UserDataModel.UserId);

            await _dataManager.LoadRemoteConfig();
            Debug.Log(
                $"Remote version {_dataManager.BetaConfig.LatestVersion}, local version {BuildMetaInfo.Instance.Version}");
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD && !USE_LOCAL_BACKEND
            if (!BuildMetaInfo.Instance.CheckBackendVersionMatch(_dataManager.BetaConfig.LatestVersion)) 
                throw new GameVersionMismatchException(BuildMetaInfo.Instance.Version.ToString(), _dataManager.BetaConfig.LatestVersion.ToString());
#endif

            try
            {
                await _backendFacade.CreateContract(UserDataModel.PrivateKey);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);

                // HACK: ignore to allow offline mode
            }

            try
            {
                await _backendFacade.SignUp(UserDataModel.UserId);
            }
            catch (TxCommitException e) when (e.Message.Contains("user already exists"))
            {
                // Ignore
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);

                // HACK: ignore to allow offline mode
            }

            await _dataManager.StartLoadCache();
        }
    }
}
