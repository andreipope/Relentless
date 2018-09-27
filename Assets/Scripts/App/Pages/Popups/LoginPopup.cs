using System;
using System.Linq;
using System.Numerics;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class LoginPopup : IUIPopup
    {
        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

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

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            OnHidePopupEvent?.Invoke();

            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            if (Self == null)
            {
                Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LoginPopup"));
            }
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _betaGroup = Self.transform.Find("Beta_Group");
            _betaButton = _betaGroup.Find("Button_Beta").GetComponent<ButtonShiftingContent>();
            _betaKeyInputField = Self.transform.Find("Beta_Group/InputField_Beta").GetComponent<InputField>();
            _betaErrorText = _betaGroup.Find("Text_Error");

            _betaButton.onClick.AddListener(PressedBetaHandler);

            _waitingGroup = Self.transform.Find("Waiting_Group");
            _versionMismatchGroup = Self.transform.Find("VersionMismatch_Group");
            _versionMismatchText = _versionMismatchGroup.Find("Text_Error").GetComponent<TextMeshProUGUI>();
            _versionMismatchExitButton = _versionMismatchGroup.Find("Button_Exit").GetComponent<Button>();
            _versionMismatchExitButton.onClick.AddListener(Application.Quit);

            _state = LoginState.BetaKeyRequest;
            SetUIState(LoginState.BetaKeyRequest);
            _betaKeyInputField.text = "";
            Self.SetActive(true);
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

        public void Update()
        {
        }

        private async void PressedBetaHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            string betaKey = _betaKeyInputField.text.Trim();
            bool isBetaKeyValid = betaKey.Length == 12;
            try
            {
                isBetaKeyValid &= CryptoUtils.HexStringToBytes(betaKey).Length == 6;
            }
            catch (Exception)
            {
                isBetaKeyValid = false;
            }

            if (isBetaKeyValid)
            {
                // check if field is empty. Can replace with exact value once we know if there's a set length for beta keys
                SetUIState(LoginState.BetaKeyValidateAndLogin);

                GenerateKeysAndUserFromBetaKey(betaKey, out byte[] privateKey, out byte[] _, out string userId);

                try
                {
                    isBetaKeyValid = await _backendFacade.CheckIfBetaKeyValid(betaKey);
                    if (!isBetaKeyValid)
                        throw new Exception("Tester key not registered");

                    UserDataModel userDataModel = new UserDataModel(userId, betaKey, privateKey)
                    {
                        IsValid = false
                    };
                    _backendDataControlMediator.SetUserDataModel(userDataModel);
                    await _backendDataControlMediator.LoginAndLoadData();

                    userDataModel.IsValid = true;
                    _backendDataControlMediator.SetUserDataModel(userDataModel);

                    SuccessfulLogin();
                }
                catch (GameVersionMismatchException e)
                {
                    SetUIState(LoginState.RemoteVersionMismatch);
                    UpdateVersionMismatchText(e);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                    SetUIState(LoginState.BetaKeyValidationFailed);
                }
            }
            else
            {
                _uiManager.DrawPopup<WarningPopup>("Input a valid Tester Key");
            }
        }

        private void SuccessfulLogin()
        {
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
            Hide();
        }

        private void SetUIState(LoginState state)
        {
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
                    throw new ArgumentOutOfRangeException(nameof(_state), _state, null);
            }
        }

        private void UpdateVersionMismatchText(GameVersionMismatchException exception)
        {
            _versionMismatchText.text =
                $"This version ({exception.LocalVersion}) is out of date.\n\nPlease download version {exception.RemoteVersion}.";
        }

        private void GenerateKeysAndUserFromBetaKey(
            string betaKey, out byte[] privateKey, out byte[] publicKey, out string userId)
        {
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
