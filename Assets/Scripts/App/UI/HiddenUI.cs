using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeStage.AdvancedFPSCounter;
using log4net;
using Opencoding.Console;
using UnityEngine;
using UnityEngine.EventSystems;
using Loom.Google.Protobuf;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class HiddenUI : MonoBehaviour
    {
        private static readonly ILog Log = Logging.GetLog(nameof(HiddenUI));

        public GameObject UIRoot;

        public BaseRaycaster[] AlwaysActiveRaycasters = new BaseRaycaster[0];
        public GameObject[] ObjectsToDisableInProduction = { };

        public Dropdown SelectBackendDropdown;

        public Button RequestFullCardCollectionSyncButton;

        private AFPSCounter _afpsCounter;
        private bool _isVisible;

        private bool ShouldBeVisible => _afpsCounter.OperationMode == OperationMode.Normal;

        void Start()
        {
            _afpsCounter = FindObjectOfType<AFPSCounter>();
            if (_afpsCounter == null)
                throw new Exception("AFPSCounter instance not found in scene");

            SelectBackendDropdown.options.Clear();
            SelectBackendDropdown.options.Add(new Dropdown.OptionData("No Override"));
            SelectBackendDropdown.options.AddRange(
                ((BackendPurpose[]) Enum.GetValues(typeof(BackendPurpose)))
                .Select(p => new Dropdown.OptionData(p.ToString()))
            );

            int backendOverrideValue = PlayerPrefs.GetInt(Constants.BackendPurposeOverrideValueKey, -1);
            SelectBackendDropdown.value = backendOverrideValue + 1;
            SelectBackendDropdown.RefreshShownValue();
            SelectBackendDropdown.onValueChanged.AddListener(SelectBackend);

            SetVisibility(ShouldBeVisible);
#if USE_PRODUCTION_BACKEND
            foreach (GameObject go in ObjectsToDisableInProduction)
            {
                go.SetActive(false);
            }
#endif
        }

        private void Update()
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (_isVisible != ShouldBeVisible)
            {
                SetVisibility(ShouldBeVisible);

                _isVisible = ShouldBeVisible;
            }
        }

        private void SetVisibility(bool visible)
        {
            BaseRaycaster[] raycasters = FindObjectsOfType<BaseRaycaster>();
            bool raycastersEnabled = !visible;
            foreach (BaseRaycaster raycaster in raycasters)
            {
                if (AlwaysActiveRaycasters.Contains(raycaster))
                    continue;

                raycaster.enabled = raycastersEnabled;
            }

            // Update UI
            if (this.UIRoot != null)
            {
                UIRoot.gameObject.SetActive(visible);
            }

            if (DebugConsole.Instance != null && visible)
            {
                DebugConsole.IsVisible = false;
            }
        }

        #region UI Handlers

        public void SelectBackend(int index)
        {
            if (index == 0)
            {
                index = -1;
            }
            else
            {
                index--;
            }

            PlayerPrefs.SetInt(Constants.BackendPurposeOverrideValueKey, index);
        }

        public async void RequestFullCardCollectionSync()
        {
            Text buttonText = RequestFullCardCollectionSyncButton.GetComponentInChildren<Text>();
            string originalButtonText = buttonText.text;
            try
            {
                RequestFullCardCollectionSyncButton.interactable = false;
                buttonText.text = "Wait...";
                BackendFacade backendFacade = GameClient.Get<BackendFacade>();
                BackendDataControlMediator backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
                await backendFacade.RequestUserFullCardCollectionSync(backendDataControlMediator.UserDataModel.UserId);
                buttonText.text = "Done! Please restart game";
            }
            catch (Exception e)
            {
                buttonText.text = "Error, try again";
                Log.Warn(nameof(RequestFullCardCollectionSync) + ":" + e);
            }
            finally
            {
                await Task.Delay(5000);
                if (RequestFullCardCollectionSyncButton != null)
                {
                    RequestFullCardCollectionSyncButton.interactable = true;
                    buttonText.text = originalButtonText;
                }
            }
        }

        public void SubmitBugReport()
        {
            UserReportingScript.Instance.CreateUserReport();
        }

        public void OpenDebugConsole()
        {
            if (DebugConsole.Instance == null)
                return;

            _afpsCounter.OperationMode = OperationMode.Disabled;
            DebugConsole.IsVisible = true;
        }

        public void SkipTutorial()
        {
            GeneralCommandsHandler.SkipTutorialFlow();
        }

        public void DumpState()
        {
            try
            {
                Protobuf.GameState currentGameState = BackendCommunication.GameStateConstructor.Create().CreateCurrentGameStateFromOnlineGame(true);

                uint variation = 0;
                string fileName = String.Empty;
                string filePath = String.Empty;

                bool fileCreated = false;
                while (!fileCreated)
                {
                    fileName = "DumpState_" + currentGameState.Id + "_" + variation + ".json";
                    filePath = GameClient.Get<IDataManager>().GetPersistentDataPath(fileName);

                    if (!File.Exists(filePath))
                    {
                        File.Create(filePath).Close();
                        fileCreated = true;
                    }
                    else
                    {
                        variation++;
                    }
                }

                File.WriteAllText(filePath, JsonFormatter.Default.Format(currentGameState));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        #endregion
    }
}
