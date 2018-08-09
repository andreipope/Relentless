// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.Client;
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

		private ButtonShiftingContent _betaButton;
	    private Transform _betaGroup;
	    private Transform _waitingGroup;
	    private Transform _betaErrorText;
		private InputField _betaKeyInputField;

		private LoginState _state;

	    public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
	        _dataManager = GameClient.Get<IDataManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LoginPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

	        _betaGroup = _selfPage.transform.Find("Beta_Group");
			_betaButton = _betaGroup.Find("Button_Beta").GetComponent<ButtonShiftingContent>();
	        _betaKeyInputField = _selfPage.transform.Find("Beta_Group/InputField_Beta").GetComponent<InputField>();
	        _betaErrorText = _betaGroup.Find("Text_Error");
	        
			_betaButton.onClick.AddListener(PressedBetaHandler);

	        _waitingGroup = _selfPage.transform.Find("Waiting_Group");

            Hide();
        }


		public void Dispose()
		{
		}

	    private async void PressedBetaHandler () {
		    GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
		    
		    bool isBetaKeyValid = _betaKeyInputField.text.Length == 12;
		    try
		    {
			    isBetaKeyValid &= CryptoUtils.HexStringToBytes(_betaKeyInputField.text).Length == 6;
		    } catch (Exception)
		    {
			    isBetaKeyValid = false;
		    }
		    
		    if (isBetaKeyValid) { //check if field is empty. Can replace with exact value once we know if there's a set length for beta keys
			    SetUIState(LoginState.BetaKeyValidateAndLogin);

			    await Task.Delay(TimeSpan.FromSeconds(2));

			    byte[] privateKey;
			    byte[] publicKey;
			    string userName;
			    GenerateKeysAndUserFromBetaKey(_betaKeyInputField.text, out privateKey, out publicKey, out userName);

			    try
			    {
				    LoomUserDataModel userDataModel = new LoomUserDataModel
				    {
					    PrivateKey = privateKey,
					    UserName = userName,
					    IsValid = false
				    };
				    LoomManager.Instance.SetUserDataModel(userDataModel);

				    await LoomManager.Instance.CreateContract();
				    await LoomManager.Instance.SignUp(userDataModel.UserName);
				    await _dataManager.StartLoadCache();

				    userDataModel.IsValid = true;
				    LoomManager.Instance.SetUserDataModel(userDataModel);
				    
				    SuccessfulLogin();
			    } catch
			    {
				    SetUIState(LoginState.BetaKeyValidationFailed);
			    }
		    } else {
			    _uiManager.DrawPopup<WarningPopup> ("Input a valid Beta Key");
		    }
		}

		private void SuccessfulLogin () {
			GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
			Hide();
		}

        public void Hide()
        {
            OnHidePopupEvent?.Invoke();
            _selfPage.SetActive(false);
		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
	        _selfPage.SetActive(true);
			_state = LoginState.BetaKeyRequest;

            SetUIState(LoginState.BetaKeyRequest);
        }

		public void Show(object data)
		{
			Show();
		}

	    public void Update() {
	    }

	    private void SetUIState(LoginState state) {
		    _state = state;
		    _waitingGroup.gameObject.SetActive(false);
		    _betaGroup.gameObject.SetActive(false);
		    _betaErrorText.gameObject.SetActive(false);
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
			    default:
				    throw new ArgumentOutOfRangeException();
		    }
	    }

	    private enum LoginState
	    {
		    BetaKeyRequest,
		    BetaKeyValidationFailed,
		    BetaKeyValidateAndLogin
	    }

	    private void GenerateKeysAndUserFromBetaKey(string betaKey, out byte[] privateKey, out byte[] publicKey, out string userName) {
		    betaKey = betaKey.ToLowerInvariant();
		    userName = "ZombieSlayer_" + new System.Random().Next(1000000, 1000000 * 10);

		    byte[] betaKeySeed = CryptoUtils.HexStringToBytes(betaKey);
		    Array.Resize(ref betaKeySeed, 32);
		    privateKey = CryptoUtils.GeneratePrivateKey(betaKeySeed);
		    publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);
	    }
    }
}




