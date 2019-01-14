using System;
using System.IO;
using System.Threading.Tasks;
using Loom.Client;
using Newtonsoft.Json;
using UnityEngine;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class BackendDataControlMediator : IService
    {
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
            Debug.Log("User Id: " + UserDataModel.UserId);
            
            await _backendFacade.CreateContract(UserDataModel.PrivateKey);

            /*
            await _dataManager.LoadRemoteConfig();
            Version contentVersion = Version.Parse(_dataManager.CachedVersions.ContentVersion);
            if (!BuildMetaInfo.Instance.CheckBackendVersionMatch(contentVersion))
            {
                Action[] actions = new Action[2];
                actions[0] = () =>
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("Version Mismatched");
                    #elif UNITY_ANDROID
                    Application.OpenURL(Constants.GameLinkForAndroid);
                    #elif UNITY_IOS
                    Application.OpenURL(Constants.GameLinkForIOS);
                    #elif UNITY_STANDALONE_OSX
                    Application.OpenURL(Constants.GameLinkForOSX);
                    #elif UNITY_STANDALONE_WIN
                    Application.OpenURL(Constants.GameLinkForWindows);
                    #else
                    Debug.LogWarning("Version Mismatched");
                    #endif
                };
                actions[1] = () =>
                {
                    Application.Quit();
                };

                _uiManager.DrawPopup<UpdatePopup>(actions);
            }*/
            
            try
            {
                await _backendFacade.SignUp(UserDataModel.UserId);
            }
            catch (TxCommitException e) when (e.Message.Contains("user already exists"))
            {
                // Ignore
            }
            
            await _dataManager.StartLoadCache();
            
        }
    }
}
