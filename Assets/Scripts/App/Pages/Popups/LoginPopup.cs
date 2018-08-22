// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;
using Loom.Client;
using LoomNetwork.CZB.BackendCommunication;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LoomNetwork.CZB
{
    public class LoginPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage;
	    private IDataManager _dataManager;
	    private BackendFacade _backendFacade;
	    private BackendDataControlMediator _backendDataControlMediator;

		private ButtonShiftingContent _betaButton;
	    private Transform _betaGroup;
	    private Transform _waitingGroup;
	    private Transform _betaErrorText;
	    private Transform _versionMismatchGroup;
	    private TextMeshProUGUI _versionMismatchText;
	    private Button _versionMismatchExitButton;

		private InputField _betaKeyInputField;

		private LoginState _state;

	    public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
	        _dataManager = GameClient.Get<IDataManager>();
	        _backendFacade = GameClient.Get<BackendFacade>();
	        _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

		public void Dispose()
		{
		}

	    private async void PressedBetaHandler () {
		    GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

		    string betaKey = _betaKeyInputField.text.Trim();
		    bool isBetaKeyValid = betaKey.Length == 12;
		    try
		    {
			    isBetaKeyValid &= CryptoUtils.HexStringToBytes(betaKey).Length == 6;
		    } catch (Exception)
		    {
			    isBetaKeyValid = false;
		    }

		    if (isBetaKeyValid) { //check if field is empty. Can replace with exact value once we know if there's a set length for beta keys
			    SetUIState(LoginState.BetaKeyValidateAndLogin);

			    byte[] privateKey;
			    byte[] publicKey;
			    string userId;
			    GenerateKeysAndUserFromBetaKey(betaKey, out privateKey, out publicKey, out userId);

			    try
			    {
				    isBetaKeyValid = await _backendFacade.CheckIfBetaKeyValid(betaKey);
				    if (!isBetaKeyValid)
					    throw new Exception("Tester key not registered");

				    UserDataModel userDataModel = new UserDataModel(userId, betaKey, privateKey)
				    {
					    // HACK
					    IsValid = true
					    //IsValid = false
				    };
				    _backendDataControlMediator.SetUserDataModel(userDataModel);
				    await _backendDataControlMediator.LoginAndLoadData();

				    // HACK
				    //userDataModel.IsValid = true;
				    _backendDataControlMediator.SetUserDataModel(userDataModel);

				    SuccessfulLogin();
			    } catch (GameVersionMismatchException e)
			    {
				    SetUIState(LoginState.RemoteVersionMismatch);
				    UpdateVersionMismatchText(e);
			    }
			    catch (Exception e)
			    {
				    Debug.LogWarning(e);
				    SetUIState(LoginState.BetaKeyValidationFailed);
			    }
		    } else {
			    _uiManager.DrawPopup<WarningPopup> ("Input a valid Tester Key");
		    }
		}

		private void SuccessfulLogin () {
			GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
			Hide();
		}

        public void Hide()
        {
            OnHidePopupEvent?.Invoke();

            if (_selfPage == null)
                return;

            _selfPage.SetActive (false);
            GameObject.Destroy (_selfPage);
            _selfPage = null;
		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LoginPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

            _betaGroup = _selfPage.transform.Find("Beta_Group");
            _betaButton = _betaGroup.Find("Button_Beta").GetComponent<ButtonShiftingContent>();
            _betaKeyInputField = _selfPage.transform.Find("Beta_Group/InputField_Beta").GetComponent<InputField>();
            _betaErrorText = _betaGroup.Find("Text_Error");

            _betaButton.onClick.AddListener(PressedBetaHandler);

            _waitingGroup = _selfPage.transform.Find("Waiting_Group");
            _versionMismatchGroup = _selfPage.transform.Find("VersionMismatch_Group");
            _versionMismatchText = _versionMismatchGroup.Find("Text_Error").GetComponent<TextMeshProUGUI>();
            _versionMismatchExitButton = _versionMismatchGroup.Find("Button_Exit").GetComponent<Button>();
            _versionMismatchExitButton.onClick.AddListener(Application.Quit);

			_state = LoginState.BetaKeyRequest;
            SetUIState(LoginState.BetaKeyRequest);
            _betaKeyInputField.text = "";
			_selfPage.SetActive(true);
        }

		public void Show(object data)
		{
			Show();

			GameVersionMismatchException gameVersionMismatchException = data as GameVersionMismatchException;
			if (gameVersionMismatchException != null)
			{
				SetUIState(LoginState.RemoteVersionMismatch);
				UpdateVersionMismatchText(gameVersionMismatchException);
			}
		}

	    public void Update() {
	    }

	    private void SetUIState(LoginState state) {
		    _state = state;
		    _waitingGroup.gameObject.SetActive(false);
		    _betaGroup.gameObject.SetActive(false);
		    _betaErrorText.gameObject.SetActive(false);
		    _versionMismatchGroup.gameObject.SetActive(false);
		    switch (_state)
		    {
			    case LoginState.BetaKeyRequest:
				    _betaGroup.gameObject.SetActive(true);
				    break;
			    case LoginState.BetaKeyValidateAndLogin:
				    _waitingGroup.gameObject.SetActive(true);
				    break;
			    case LoginState.BetaKeyValidationFailed:
				    _betaGroup.gameObject.SetActive(true);
				    _betaErrorText.gameObject.SetActive(true);
				    break;
			    case LoginState.RemoteVersionMismatch:
				    _versionMismatchGroup.gameObject.SetActive(true);
				    break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }
	    }

	    private void UpdateVersionMismatchText(GameVersionMismatchException exception)
	    {
		    _versionMismatchText.text =
			    $"This version ({exception.LocalVersion}) is out of date.\n\nPlease download version {exception.RemoteVersion}.";
	    }
	    
	    private void GenerateKeysAndUserFromBetaKey(string betaKey, out byte[] privateKey, out byte[] publicKey, out string userId) {
		    betaKey = betaKey.ToLowerInvariant();

		    byte[] betaKeySeed = CryptoUtils.HexStringToBytes(betaKey);
		    Array.Resize(ref betaKeySeed, 32);

		    BigInteger userIdNumber = new BigInteger(betaKeySeed) + betaKeySeed.Sum(b => b * 2);
		    userId = "ZombieSlayer_" + userIdNumber;

		    privateKey = CryptoUtils.GeneratePrivateKey(betaKeySeed);
  
		    publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);
	    }

	    private enum LoginState
	    {
		    BetaKeyRequest,
		    BetaKeyValidationFailed,
		    BetaKeyValidateAndLogin,
		    RemoteVersionMismatch
	    }
    }
}




