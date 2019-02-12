using System;
using System.Linq;
using CodeStage.AdvancedFPSCounter;
using Opencoding.Console;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

            if (visible)
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
            _afpsCounter.OperationMode = OperationMode.Disabled;
            DebugConsole.IsVisible = true;
        }

        public void SkipTutorial()
        {
            GeneralCommandsHandler.SkipTutorialFlow();
        }

#endregion
    }
}
