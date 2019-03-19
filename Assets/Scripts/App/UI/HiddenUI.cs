using System;
using System.IO;
using System.Linq;
using CodeStage.AdvancedFPSCounter;
using KellermanSoftware.CompareNetObjects;
using Opencoding.Console;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground
{
    public class HiddenUI : MonoBehaviour
    {
        public GameObject UIRoot;

        public BaseRaycaster[] AlwaysActiveRaycasters = new BaseRaycaster[0];
        public GameObject[] ObjectsToDisableInProduction = { };

        private AFPSCounter _afpsCounter;
        private bool _isVisible;

        private bool ShouldBeVisible => _afpsCounter.OperationMode == OperationMode.Normal;

        void Start()
        {
            _afpsCounter = FindObjectOfType<AFPSCounter>();
            if (_afpsCounter == null)
                throw new Exception("AFPSCounter instance not found in scene");

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

        public void JumpToTutorial(int number)
        {
            GeneralCommandsHandler.JumpToTutorialNumber(number);
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
