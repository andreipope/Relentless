using CodeStage.AdvancedFPSCounter;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using log4net.Appender;
using Loom.ZombieBattleground.Common;
using SharpCompress.Archives.Zip;
using TMPro;
using Unity.Cloud.UserReporting;
using Unity.Cloud.UserReporting.Plugin;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CompressionLevel = SharpCompress.Compressors.Deflate.CompressionLevel;

/// <summary>
/// Represents a behavior for working with the user reporting client.
/// </summary>
/// <remarks>
/// This script is provided as a sample and isn't necessarily the most optimal solution for your project.
/// You may want to consider replacing with this script with your own script in the future.
/// </remarks>
public class UserReportingScript : MonoBehaviour
{
    private static readonly ILog Log = Logging.GetLog(nameof(UserReportingScript));

    #region Constructors

    /// <summary>
    /// Creates a new instance of the <see cref="UserReportingScript"/> class.
    /// </summary>
    public UserReportingScript()
    {
        this.UserReportSubmitting = new UnityEvent();
        this.unityUserReportingUpdater = new UnityUserReportingUpdater();
    }

    #endregion

    #region Fields

    /// <summary>
    /// Gets or sets the user report button used to create a user report.
    /// </summary>
    [Tooltip("The user report button used to create a user report.")]
    public Button UserReportButton;

    public Button BugReportFormCancelButton;

    public Button BugReportFormExitButton;

    public Text BugReportFormCrashText;

    public GameObject CrashBackupObjectsRoot;

    /// <summary>
    /// Gets or sets the UI for the user report form. Shown after a user report is created.
    /// </summary>
    [Tooltip("The UI for the user report form. Shown after a user report is created.")]
    public Canvas UserReportForm;

    /// <summary>
    /// Gets or sets the UI for the event raised when a user report is submitting.
    /// </summary>
    [Tooltip("The event raised when a user report is submitting.")]
    public UnityEvent UserReportSubmitting;

    /// <summary>
    /// Gets or sets the category dropdown.
    /// </summary>
    [Tooltip("The category dropdown.")] public Dropdown CategoryDropdown;

    /// <summary>
    /// Gets or sets the description input on the user report form.
    /// </summary>
    [Tooltip("The description input on the user report form.")]
    public InputField DescriptionInput;

    /// <summary>
    /// Gets or sets the UI shown when there's an error.
    /// </summary>
    [Tooltip("The UI shown when there's an error.")]
    public Canvas ErrorPopup;

    private bool isCreatingUserReport;

    /// <summary>
    /// Gets or sets a value indicating whether the hotkey is enabled (Left Alt + Left Shift + B).
    /// </summary>
    [Tooltip("A value indicating whether the hotkey is enabled (Left Alt + Left Shift + B).")]
    public bool IsHotkeyEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether the user report client reports metrics about itself.
    /// </summary>
    [Tooltip("A value indicating whether the user report client reports metrics about itself.")]
    public bool IsSelfReporting;

    private bool isShowingError;

    private bool isSubmitting;

    /// <summary>
    /// Gets or sets the display text for the progress text.
    /// </summary>
    [Tooltip("The display text for the progress text.")]
    public TextMeshProUGUI ProgressText;

    /// <summary>
    /// Gets or sets a value indicating whether the user report client send events to analytics.
    /// </summary>
    [Tooltip("A value indicating whether the user report client send events to analytics.")]
    public bool SendEventsToAnalytics;

    /// <summary>
    /// Gets or sets the UI shown while submitting.
    /// </summary>
    [Tooltip("The UI shown while submitting.")]
    public Canvas SubmittingPopup;

    /// <summary>
    /// Gets or sets the summary input on the user report form.
    /// </summary>
    [Tooltip("The summary input on the user report form.")]
    public InputField SummaryInput;

    /// <summary>
    /// Gets or sets the thumbnail viewer on the user report form.
    /// </summary>
    [Tooltip("The thumbnail viewer on the user report form.")]
    public Image ThumbnailViewer;

    private UnityUserReportingUpdater unityUserReportingUpdater;

    private AFPSCounter _afpsCounter;

    private bool _isCrashing;

    private string _exceptionStacktrace = "";
    private string _exceptionCondition = "";

    #endregion

    #region Properties

    public static UserReportingScript Instance { get; private set; }

    /// <summary>
    /// Gets the current user report.
    /// </summary>
    public UserReport CurrentUserReport { get; private set; }

    /// <summary>
    /// Gets the current state.
    /// </summary>
    public UserReportingState State
    {
        get
        {
            if (this.CurrentUserReport != null)
            {
                if (this.IsSilent)
                {
                    return UserReportingState.Idle;
                }
                else if (this.isSubmitting)
                {
                    return UserReportingState.SubmittingForm;
                }
                else
                {
                    return UserReportingState.ShowingForm;
                }
            }
            else
            {
                if (this.isCreatingUserReport)
                {
                    return UserReportingState.CreatingUserReport;
                }
                else
                {
                    return UserReportingState.Idle;
                }
            }
        }
    }

    // Silent mode does not show the user report form.
    public bool IsSilent { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Cancels the user report.
    /// </summary>
    public void CancelUserReport()
    {
        this.CurrentUserReport = null;
        this.ClearForm();
    }

    private IEnumerator ClearError()
    {
        yield return new WaitForSeconds(10);
        this.isShowingError = false;
    }

    private void ClearForm()
    {
        this.SummaryInput.text = null;
        this.DescriptionInput.text = null;
    }

    public void ExitApplication()
    {
#if UNITY_EDITOR
        Log.Info("Application.Quit();");
#endif
        Application.Quit();
    }

    /// <summary>
    /// Gets a value indicating whether the user report is submitting.
    /// </summary>
    /// <returns>A value indicating whether the user report is submitting.</returns>
    public bool IsSubmitting()
    {
        return this.isSubmitting;
    }

    private void SetThumbnail(UserReport userReport)
    {
        if (userReport != null && this.ThumbnailViewer != null)
        {
            byte[] data = Convert.FromBase64String(userReport.Thumbnail.DataBase64);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(data);
            this.ThumbnailViewer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5F, 0.5F));
            this.ThumbnailViewer.preserveAspect = true;
        }
    }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);

#if !UNITY_EDITOR || FORCE_ENABLE_CRASH_REPORTER
        Application.logMessageReceived += OnLogMessageReceived;
#endif
    }

    private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
    {
        if (type != LogType.Exception || _isCrashing)
            return;

        CreateUserReport(false, true, condition, stacktrace);
    }

    public void CreateUserReport(bool isSilent, bool isCrashing, string exceptionCondition, string exceptionStacktrace)
    {
        IsSilent = isSilent;
        _isCrashing = isCrashing;
        _exceptionStacktrace = exceptionStacktrace;
        _exceptionCondition = exceptionCondition;
        Log.Info($"Starting submitting report, isSilent: {isSilent}, isCrashing: {isCrashing}");
        StartCoroutine(DelayedCreateBugReport());
    }

    public void CreateUserReport()
    {
        CreateUserReport(false, false, "", "");
    }

    /// <summary>
    /// Submits the user report.
    /// </summary>
    public void SubmitUserReport()
    {
        // Preconditions
        if (this.isSubmitting || this.CurrentUserReport == null)
        {
            return;
        }

        // Set Submitting Flag
        this.isSubmitting = true;

        // Set Summary
        if (this.SummaryInput != null)
        {
            this.CurrentUserReport.Summary = this.SummaryInput.text;
        }

        // Set Category
        if (this.CategoryDropdown != null)
        {
            Dropdown.OptionData optionData = this.CategoryDropdown.options[this.CategoryDropdown.value];
            string category = optionData.text;
            this.CurrentUserReport.Dimensions.Add(new UserReportNamedValue("Category", category));
            this.CurrentUserReport.Fields.Add(new UserReportNamedValue("Category", category));
        }

        // Set Description
        // This is how you would add additional fields.
        if (this.DescriptionInput != null)
        {
            UserReportNamedValue userReportField = new UserReportNamedValue();
            userReportField.Name = "Description";
            userReportField.Value = this.DescriptionInput.text;
            this.CurrentUserReport.Fields.Add(userReportField);
        }

        // Clear Form
        this.ClearForm();

        // Raise Event
        this.RaiseUserReportSubmitting();

        // Send Report
        UnityUserReporting.CurrentClient.SendUserReport(this.CurrentUserReport, (uploadProgress, downloadProgress) =>
        {
            if (this.ProgressText != null)
            {
                string progressText = uploadProgress.ToString("0%");
                this.ProgressText.text = progressText;
            }
        }, (success, br2) =>
        {
            Log.Info("Successfully sent bug report: " + success);

            if (!success)
            {
                this.isShowingError = true;
                this.StartCoroutine(this.ClearError());
            }

            this.CurrentUserReport = null;
            this.isSubmitting = false;

            if (_isCrashing)
            {
                ExitApplication();
            }
        });
    }

    private IEnumerator DelayedCreateBugReport()
    {
        yield return new WaitForEndOfFrame();
        CreateUserReportInternal();
    }

    private void Start()
    {
        // Set Up Event System
        if (Application.isPlaying)
        {
            EventSystem sceneEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (sceneEventSystem == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }
        }

        // Configure Client
        // This where you would want to change endpoints, override your project identifier, or provide configuration for events, metrics, and screenshots.
        if (UnityUserReporting.CurrentClient == null)
        {
            UnityUserReporting.Configure();
        }

        _afpsCounter = FindObjectOfType<AFPSCounter>();
        if (_afpsCounter == null)
            throw new Exception("AFPSCounter instance not found in scene");
    }

        /// <summary>
    /// Creates a user report.
    /// </summary>
    private void CreateUserReportInternal()
    {
        // Check Creating Flag
        if (this.isCreatingUserReport)
        {
            return;
        }

        // Hide FPS counter
        if (_afpsCounter != null)
        {
            _afpsCounter.OperationMode = OperationMode.Background;
        }

        BugReportFormCancelButton.gameObject.SetActive(!_isCrashing);
        BugReportFormExitButton.gameObject.SetActive(_isCrashing);
        BugReportFormCrashText.gameObject.SetActive(_isCrashing);
        CrashBackupObjectsRoot.SetActive(_isCrashing);

        // Set Creating Flag
        this.isCreatingUserReport = true;

        // Take Main Screenshot
        UnityUserReporting.CurrentClient.TakeScreenshot(1280, 1280, s => { });

        // Take Thumbnail Screenshot
        UnityUserReporting.CurrentClient.TakeScreenshot(256, 256, s => { });

        // Attempt to get match id
        long? matchId = null;
        if (GameClient.Get<IMatchManager>()?.MatchType == Enumerators.MatchType.PVP)
        {
            matchId = GameClient.Get<IPvPManager>()?.MatchMetadata?.Id;
        }

        // Kill everything else to make sure no more exceptions are being thrown
        if (_isCrashing)
        {
            IEnumerable<GameObject> rootGameObjects =
                Utilites.CollectAllSceneRootGameObjects(gameObject)
                    .Where((go, i) => go != transform.root.gameObject);

            foreach (GameObject rootGameObject in rootGameObjects)
            {
                Destroy(rootGameObject);
            }
        }

        // Create Report
        UnityUserReporting.CurrentClient.CreateUserReport((br) =>
        {
            // Ensure Project Identifier
            if (string.IsNullOrEmpty(br.ProjectIdentifier))
            {
                Log.Warn("The user report's project identifier is not set. Please setup cloud services using the Services tab or manually specify a project identifier when calling UnityUserReporting.Configure().");
            }

            // Fields
            if (matchId != null)
            {
                br.Fields.Add(new UserReportNamedValue("Online Match Id", matchId.Value.ToString()));
            }

            // Exception
            if (!String.IsNullOrEmpty(_exceptionStacktrace))
            {
                string exception = _exceptionCondition + Environment.NewLine + _exceptionStacktrace;
                AddTextAttachment(br, "Exception.txt", exception);
                Log.Error(exception);
            }

            // Call metrics
            try
            {
                BackendFacade backendFacade = GameClient.Get<BackendFacade>();
                IDataManager dataManager = GameClient.Get<IDataManager>();
                if (backendFacade.ContractCallProxy is ThreadedContractCallProxyWrapper threadedCallProxy &&
                    threadedCallProxy.WrappedProxy is TimeMetricsContractCallProxy timeMetricsCallProxy)
                {
                    string callMetricsJson = dataManager.SerializeToJson(timeMetricsCallProxy.MethodToCallRoundabouts, true);
                    AddTextAttachment(br, TimeMetricsContractCallProxy.CallMetricsFileName, callMetricsJson, "application/json");
                }
            }
            catch (Exception e)
            {
                UnityUserReporting.CurrentClient.LogException(e);
                Log.Warn("Error while getting call metrics:" + e);
            }

            // HTML log
            try
            {
                if (Logging.GetRepository() is IFlushable flushable)
                {
                    flushable.Flush(5000);
                }

                byte[] htmlLog;
                using(FileStream fileStream = new FileStream(
                    Logging.GetLogFilePath(),
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    using(BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        htmlLog = binaryReader.ReadBytes(int.MaxValue);
                    }
                }

                using (MemoryStream zipStream = new MemoryStream())
                {
                    using (ZipArchive zipArchive = ZipArchive.Create())
                    {
                        zipArchive.DeflateCompressionLevel = CompressionLevel.BestCompression;
                        zipArchive.AddEntry(Logging.LogFileName, new MemoryStream(htmlLog), true);
                        zipArchive.SaveTo(zipStream);
                    }

                    br.Attachments.Add(
                        new UserReportAttachment(
                            "Log.html.zip",
                            "Log.html.zip",
                            "application/zip",
                            zipStream.ToArray()
                        ));
                }
            }
            catch (Exception e)
            {
                UnityUserReporting.CurrentClient.LogException(e);
                Log.Warn("Error while getting HTML log:" + e);
            }

            br.DeviceMetadata.Add(new UserReportNamedValue("Full Version", BuildMetaInfo.Instance.FullVersionName));
            br.DeviceMetadata.Add(new UserReportNamedValue("Min FPS", _afpsCounter.fpsCounter.LastMinimumValue.ToString()));
            br.DeviceMetadata.Add(new UserReportNamedValue("Max FPS", _afpsCounter.fpsCounter.LastMaximumValue.ToString()));
            br.DeviceMetadata.Add(new UserReportNamedValue("Average FPS", _afpsCounter.fpsCounter.LastAverageValue.ToString()));

            // Dimensions
            string platform = "Unknown";
            string version = BuildMetaInfo.Instance.DisplayVersionName;
            foreach (UserReportNamedValue deviceMetadata in br.DeviceMetadata)
            {
                if (deviceMetadata.Name == "Platform")
                {
                    platform = deviceMetadata.Value;
                }
            }

            br.Dimensions.Add(new UserReportNamedValue("Platform.Version", string.Format("{0}.{1}", platform, version)));

            br.Dimensions.Add(new UserReportNamedValue("IsCrashReport", _isCrashing.ToString()));

            br.Dimensions.Add(new UserReportNamedValue("GitBranch", BuildMetaInfo.Instance.GitBranchName));

            // Set Current Report
            this.CurrentUserReport = br;

            // Set Creating Flag
            this.isCreatingUserReport = false;

            // Set Thumbnail
            this.SetThumbnail(br);

            // Submit Immediately in Silent Mode
            if (this.IsSilent)
            {
                this.SubmitUserReport();
            }
        });
    }

    private void Update()
    {
        // Update Client
        UnityUserReporting.CurrentClient.IsSelfReporting = this.IsSelfReporting;
        UnityUserReporting.CurrentClient.SendEventsToAnalytics = this.SendEventsToAnalytics;

        if (this.UserReportForm != null)
        {
            this.UserReportForm.enabled = this.State == UserReportingState.ShowingForm;
        }

        if (this.SubmittingPopup != null)
        {
            this.SubmittingPopup.enabled = this.State == UserReportingState.SubmittingForm;
        }

        if (this.ErrorPopup != null)
        {
            this.ErrorPopup.enabled = this.isShowingError;
        }

        // Update Client
        // The UnityUserReportingUpdater updates the client at multiple points during the current frame.
        this.unityUserReportingUpdater.Reset();
        this.StartCoroutine(this.unityUserReportingUpdater);
    }

    #endregion

    #region Virtual Methods

    /// <summary>
    /// Occurs when a user report is submitting.
    /// </summary>
    protected virtual void RaiseUserReportSubmitting()
    {
        if (this.UserReportSubmitting != null)
        {
            this.UserReportSubmitting.Invoke();
        }
    }

    #endregion

    private void AddTextAttachment(UserReport report, string name, string text,  string contentType = "text/plain")
    {
        // Convert to Windows encoding for easy viewing
        text = text.Replace("\r\n", "\n").Replace("\n", "\r\n");
        report.Attachments.Add(
            new UserReportAttachment(
                name,
                name,
                contentType,
                Encoding.UTF8.GetBytes(text)
            ));
    }
}
