using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
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
        public Toggle ForceUseAuthToggle;

        public Button RequestFullCardCollectionSyncButton;
        public Button DebugCheatSetFullCardCollectionButton;
        private bool _isVisible;

        private readonly List<Vector2> gesturePoints = new List<Vector2>();
		private int gestureCount;

        private bool ShouldBeVisible;

        void Start()
        {
            ShouldBeVisible = false;

            SelectBackendDropdown.options.Clear();
            SelectBackendDropdown.options.Add(new Dropdown.OptionData("No Override"));
            SelectBackendDropdown.options.AddRange(
                ((BackendPurpose[]) Enum.GetValues(typeof(BackendPurpose)))
                .Select(p => new Dropdown.OptionData(p.ToString()))
            );

            int backendOverrideValue = PlayerPrefs.GetInt(Constants.BackendPurposeOverrideValuePlayerPrefsKey, -1);
            SelectBackendDropdown.value = backendOverrideValue + 1;
            SelectBackendDropdown.RefreshShownValue();
            SelectBackendDropdown.onValueChanged.AddListener(OnSelectBackendDropdownValueChanged);

            bool forceUseAuth = PlayerPrefs.GetInt(Constants.ForceUseAuthPlayerPrefsKey, 0) != 0;
            ForceUseAuthToggle.isOn = forceUseAuth;
            ForceUseAuthToggle.onValueChanged.AddListener(OnForceUseAuthToggleValueChanged);

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
            if (PerformedCircleOnScreen())
			{
				ShouldBeVisible = ShouldBeVisible ? false : true;
			}

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
        }

        #region UI Handlers

        public async void RequestFullCardCollectionSync()
        {
            Text buttonText = RequestFullCardCollectionSyncButton.GetComponentInChildren<Text>();
            string originalButtonText = buttonText.text;
            try
            {
                RequestFullCardCollectionSyncButton.interactable = false;
                buttonText.text = "Wait...";
                BackendFacade backendFacade = GameClient.Get<BackendFacade>();
                BackendDataSyncService backendDataSyncService = GameClient.Get<BackendDataSyncService>();
                BackendDataControlMediator backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
                await backendFacade.RequestUserFullCardCollectionSync(backendDataControlMediator.UserDataModel.UserId);
                backendDataSyncService.SetCollectionDataDirtyFlag();
                await Task.Delay(5000);
                buttonText.text = "Done!";
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
            ShouldBeVisible = false;
        }

        public async void DebugCheatSetFullCardCollection()
        {
            Text buttonText = DebugCheatSetFullCardCollectionButton.GetComponentInChildren<Text>();
            string originalButtonText = buttonText.text;
            try
            {
                DebugCheatSetFullCardCollectionButton.interactable = false;
                buttonText.text = "Wait...";
                await CollectionCommandHandler.DebugCheatSetFullCardCollection();
                buttonText.text = "Done!";
            }
            catch (Exception e)
            {
                buttonText.text = "Error, try again";
                Log.Warn(nameof(DebugCheatSetFullCardCollection) + ":" + e);
            }
            finally
            {
                await Task.Delay(5000);
                if (DebugCheatSetFullCardCollectionButton != null)
                {
                    DebugCheatSetFullCardCollectionButton.interactable = true;
                    buttonText.text = originalButtonText;
                }
            }
        }

        private bool PerformedCircleOnScreen()
		{
			int pointsCount = gesturePoints.Count;

			if (Input.GetMouseButton(0))
			{
				Vector2 mousePosition = Input.mousePosition;
				if (pointsCount == 0 || (mousePosition - gesturePoints[pointsCount - 1]).magnitude > 10)
				{
					gesturePoints.Add(mousePosition);
					pointsCount++;
				}
			}
			else if (Input.GetMouseButtonUp(0))
			{
				pointsCount = 0;
				gestureCount = 0;
				gesturePoints.Clear();
			}

			if (pointsCount < 10)
				return false;

			float finalDeltaLength = 0;

			Vector2 finalDelta = Vector2.zero;
			Vector2 previousPointsDelta = Vector2.zero;

			for (int i = 0; i < pointsCount - 2; i++)
			{
				Vector2 pointsDelta = gesturePoints[i + 1] - gesturePoints[i];
				finalDelta += pointsDelta;

				float pointsDeltaLength = pointsDelta.magnitude;
				finalDeltaLength += pointsDeltaLength;

				float dotProduct = Vector2.Dot(pointsDelta, previousPointsDelta);
				if (dotProduct < 0f)
				{
					gesturePoints.Clear();
					gestureCount = 0;
					return false;
				}

				previousPointsDelta = pointsDelta;
			}

			bool result = false;
			int gestureBase = (Screen.width + Screen.height) / 4;

			if (finalDeltaLength > gestureBase && finalDelta.magnitude < gestureBase / 2f)
			{
				gesturePoints.Clear();
				gestureCount++;

				if (gestureCount >= 4)
				{
					gestureCount = 0;
					result = true;
				}
			}

			return result;
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

        private void OnSelectBackendDropdownValueChanged(int index)
        {
            if (index == 0)
            {
                index = -1;
            }
            else
            {
                index--;
            }

            PlayerPrefs.SetInt(Constants.BackendPurposeOverrideValuePlayerPrefsKey, index);
        }

        private void OnForceUseAuthToggleValueChanged(bool forceUseAuth)
        {
            PlayerPrefs.SetInt(Constants.ForceUseAuthPlayerPrefsKey, forceUseAuth ? 1 : 0);
        }

        #endregion
    }
}
