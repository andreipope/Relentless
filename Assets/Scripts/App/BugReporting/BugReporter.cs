using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeStage.AdvancedFPSCounter;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using TMPro;
using Unity.Cloud.BugReporting;
using Unity.Cloud.BugReporting.Plugin;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class BugReporter : MonoBehaviour
    {

        #region Constructors

        public BugReporter()
        {
            BugReportSubmitting = new UnityEvent();
            _unityBugReportingUpdater = new UnityBugReportingUpdater();
        }

        #endregion

        #region Fields

        public Button BugReportButton;

        public Button BugReportFormCancelButton;

        public Button BugReportFormExitButton;

        public Text BugReportFormCrashText;

        public Canvas BugReportForm;

        public UnityEvent BugReportSubmitting;

        public InputField DescriptionInput;

        public TextMeshProUGUI ReportUploadProgressText;

        public GameObject CrashBackupObjectsRoot;

        public bool IsInSilentMode;

        public bool IsSelfReporting;

        public bool SendEventsToAnalytics;

        public Canvas SubmittingPopup;

        public InputField SummaryInput;

        public Image ThumbnailViewer;

        private bool _isSubmitting;

        private bool _isCreatingBugReport;

        private UnityBugReportingUpdater _unityBugReportingUpdater;

        private AFPSCounter _afpsCounter;

        private bool _isCrashing;

        private string _exceptionStacktrace;

        #endregion

        #region Properties

        public BugReport CurrentBugReport { get; private set; }

        public BugReportingState State
        {
            get
            {
                if (CurrentBugReport != null)
                {
                    if (IsInSilentMode)
                    {
                        return BugReportingState.Idle;
                    }

                    if (_isSubmitting)
                    {
                        return BugReportingState.SubmittingForm;
                    }

                    return BugReportingState.ShowingForm;
                }

                if (_isCreatingBugReport)
                {
                    return BugReportingState.CreatingBugReport;
                }

                return BugReportingState.Idle;
            }
        }

        #endregion

        #region Methods

        public void CancelBugReport()
        {
            CurrentBugReport = null;
            ClearForm();
        }

        private void ClearForm()
        {
            SummaryInput.text = null;
            DescriptionInput.text = null;
        }

        public void ExitApplication()
        {
#if UNITY_EDITOR
            Debug.Log("Application.Quit();");
#endif
            Application.Quit();
        }

        public void CreateBugReport(bool isCrashReport)
        {
            // Check Creating Flag
            if (_isCreatingBugReport)
            {
                return;
            }

            // Hide FPS counter
            if (_afpsCounter != null)
            {
                _afpsCounter.OperationMode = OperationMode.Background;
            }

            BugReportFormCancelButton.gameObject.SetActive(!isCrashReport);
            BugReportFormExitButton.gameObject.SetActive(isCrashReport);
            BugReportFormCrashText.gameObject.SetActive(isCrashReport);
            CrashBackupObjectsRoot.SetActive(isCrashReport);

            // Set Creating Flag
            _isCreatingBugReport = true;

            if (!String.IsNullOrEmpty(_exceptionStacktrace))
            {
                Debug.LogError(_exceptionStacktrace);
            }

            // Take Main Screenshot
            UnityBugReporting.CurrentClient.TakeScreenshot(1280,
                1280,
                s =>
                {
                });

            // Kill everything else to make sure no more exceptions are being thrown
            if (isCrashReport)
            {
                Scene[] scenes = new Scene[SceneManager.sceneCount];
                for (int i = 0; i < SceneManager.sceneCount; ++i)
                {
                    scenes[i] = SceneManager.GetSceneAt(i);
                }

                IEnumerable<GameObject> rootGameObjects =
                    scenes
                        .Concat(new[]
                        {
                            gameObject.scene
                        })
                        .Distinct()
                        .SelectMany(scene => scene.GetRootGameObjects())
                        .Where((go, i) => go != transform.root.gameObject);

                foreach (GameObject rootGameObject in rootGameObjects)
                {
                    Destroy(rootGameObject);
                }
            }

            // Create Report
            UnityBugReporting.CurrentClient.CreateBugReport(br =>
            {
                // Ensure Project Identifier
                if (string.IsNullOrEmpty(br.ProjectIdentifier))
                {
                    Debug.LogError(
                        "The bug report's project identifier is not set. Please setup cloud services using the Services tab or manually specify a project identifier when calling UnityBugReporting.Configure().");
                }

                // Attachments
                if (!String.IsNullOrEmpty(_exceptionStacktrace))
                {
                    br.Attachments.Add(
                        new BugReportAttachment(
                            "Exception",
                            "Exception.txt",
                            "text/plain",
                            global::System.Text.Encoding.UTF8.GetBytes(_exceptionStacktrace)
                            ));
                }

                try
                {
                    BackendFacade backendFacade = GameClient.Get<BackendFacade>();
                    IDataManager dataManager = GameClient.Get<IDataManager>();
                    if (backendFacade.ContractCallProxy is TimeMetricsContractCallProxy callProxy)
                    {
                        string callMetricsJson = dataManager.SerializeToJson(callProxy.MethodToCallRoundabouts, true);
                        br.Attachments.Add(
                            new BugReportAttachment(
                                TimeMetricsContractCallProxy.CallMetricsFileName,
                                TimeMetricsContractCallProxy.CallMetricsFileName,
                                "application/json",
                                global::System.Text.Encoding.UTF8.GetBytes(callMetricsJson)
                            ));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Error while getting call metrics:" + e);
                }

                br.DeviceMetadata.Add(new BugReportNamedValue("Full Version", BuildMetaInfo.Instance.FullVersionName));
                br.DeviceMetadata.Add(new BugReportNamedValue("Min FPS", _afpsCounter.fpsCounter.LastMinimumValue.ToString()));
                br.DeviceMetadata.Add(new BugReportNamedValue("Max FPS", _afpsCounter.fpsCounter.LastMaximumValue.ToString()));
                br.DeviceMetadata.Add(new BugReportNamedValue("Average FPS", _afpsCounter.fpsCounter.LastAverageValue.ToString()));

                // Dimensions
                string platform = "Unknown";
                string version = BuildMetaInfo.Instance.DisplayVersionName;
                foreach (BugReportNamedValue deviceMetadata in br.DeviceMetadata)
                {
                    if (deviceMetadata.Name == "Platform")
                    {
                        platform = deviceMetadata.Value;
                    }
                }

                br.Dimensions.Add(new BugReportNamedValue("Platform.Version", string.Format("{0}.{1}", platform, version)));

                br.Dimensions.Add(new BugReportNamedValue("IsCrashReport", isCrashReport.ToString()));

                br.Dimensions.Add(new BugReportNamedValue("GitBranch", BuildMetaInfo.Instance.GitBranchName));

                // Set Current Report
                CurrentBugReport = br;

                // Set Creating Flag
                _isCreatingBugReport = false;

                // Set Thumbnail
                SetThumbnail(br);

                // Submit Immediately in Silent Mode
                if (IsInSilentMode)
                {
                    SubmitBugReport();
                }
            });
        }

        public bool IsSubmitting()
        {
            return _isSubmitting;
        }

        private void SetThumbnail(BugReport bugReport)
        {
            if (bugReport != null && ThumbnailViewer != null)
            {
                byte[] data = Convert.FromBase64String(bugReport.Thumbnail.DataBase64);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(data);
                ThumbnailViewer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5F, 0.5F));
                ThumbnailViewer.preserveAspect = true;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(transform.root.gameObject);

#if !UNITY_EDITOR || FORCE_ENABLE_CRASH_REPORTER
            Application.logMessageReceived += OnLogMessageReceived;
#endif
        }

        private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            if (type != LogType.Exception || _isCrashing)
                return;

            _isCrashing = true;
            _exceptionStacktrace = stacktrace;
            StartCoroutine(DelayedCreateBugReport(true));
        }

        public IEnumerator DelayedCreateBugReport(bool isCrashReport)
        {
            yield return new WaitForEndOfFrame();
            CreateBugReport(isCrashReport);
        }

        private void Start()
        {
            // Set Up Event System
            if (Application.isPlaying)
            {
                EventSystem sceneEventSystem = FindObjectOfType<EventSystem>();
                if (sceneEventSystem == null)
                {
                    GameObject eventSystem = new GameObject("EventSystem");
                    eventSystem.AddComponent<EventSystem>();
                    eventSystem.AddComponent<StandaloneInputModule>();
                }
            }

            // Configure Client
            // This where you would want to change endpoints, override your project identifier, or provide configuration for events, metrics, and screenshots.
            if (UnityBugReporting.CurrentClient == null)
            {
                UnityBugReporting.Configure();
            }

            _afpsCounter = FindObjectOfType<AFPSCounter>();
            if (_afpsCounter == null)
                throw new Exception("AFPSCounter instance not found in scene");
        }

        public void SubmitBugReport()
        {
            // Preconditions
            if (_isSubmitting || CurrentBugReport == null)
            {
                return;
            }

            // Set Submitting Flag
            _isSubmitting = true;

            // Set Summary
            if (SummaryInput != null)
            {
                CurrentBugReport.Summary = SummaryInput.text;
            }

            // Set Description
            // This is how you would add additional fields.
            if (DescriptionInput != null)
            {
                BugReportNamedValue bugReportField = new BugReportNamedValue();
                bugReportField.Name = "Description";
                bugReportField.Value = DescriptionInput.text;
                CurrentBugReport.Fields.Add(bugReportField);
            }

            // Clear Form
            ClearForm();

            // Raise Event
            RaiseBugReportSubmitting();

            // Send Report
            UnityBugReporting.CurrentClient.SendBugReport(
                CurrentBugReport,
                (uploadProgress, downloadProgress) =>
                {
                    ReportUploadProgressText.text = $"{uploadProgress:P0}";
                },
                (success, br2) =>
                {
                    Debug.Log("Successfully sent bug report: " + success);
                    CurrentBugReport = null;
                    _isSubmitting = false;

                    if (_isCrashing)
                    {
                        ExitApplication();
                    }
                }
            );
        }

        private void Update()
        {
            // Update Client
            UnityBugReporting.CurrentClient.IsSelfReporting = IsSelfReporting;
            UnityBugReporting.CurrentClient.SendEventsToAnalytics = SendEventsToAnalytics;

            // Update UI
            if (BugReportButton != null)
            {
                BugReportButton.gameObject.SetActive(_afpsCounter.OperationMode == OperationMode.Normal);
                BugReportButton.interactable = State == BugReportingState.Idle;
            }

            if (BugReportForm != null)
            {
                BugReportForm.enabled = State == BugReportingState.ShowingForm;
            }

            if (SubmittingPopup != null)
            {
                SubmittingPopup.enabled = State == BugReportingState.SubmittingForm;
            }

            // Update Client
            // The UnityBugReportingUpdater updates the client at multiple points during the current frame.
            _unityBugReportingUpdater.Reset();
            StartCoroutine(_unityBugReportingUpdater);
        }

        #endregion

        #region Virtual Methods

        protected virtual void RaiseBugReportSubmitting()
        {
            if (BugReportSubmitting != null)
            {
                BugReportSubmitting.Invoke();
            }
        }

        #endregion

    }
}
