using System;
using System.IO;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4netUnitySupport;
using UnityEngine;
using Logger = log4net.Repository.Hierarchy.Logger;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

namespace Loom.ZombieBattleground
{
    public static class Logging
    {
        private const string EnvVarLogFilePath = "ZB_LOG_FILE_PATH";
        private const string DefaultLogFileName = "Log.html";
        private const string RepositoryName = "ZBLogRepository";

        private static readonly ILog UnityLog = GetLog("Unity");

        private static bool _isRepositoryCreated;
        private static bool _isConfigured;

        public static ILoggerRepository GetRepository()
        {
            if (_isRepositoryCreated)
                return LogManager.GetRepository(RepositoryName);

            _isRepositoryCreated = true;
            return LogManager.CreateRepository(RepositoryName);
        }

        public static ILog GetLog(string name)
        {
            GetRepository();
            return LogManager.GetLogger(RepositoryName, name);
        }

        public static Logger GetLogger(string name)
        {
            GetRepository();
            return (Logger) GetLog(name).Logger;
        }

        public static string GetLogFilePath()
        {
            string path =
                GetLogFilePathFromEnvVar() ??
                Path.Combine(Application.persistentDataPath, DefaultLogFileName);

            return Path.GetFullPath(path);
        }

        public static bool FileLogEnabled
        {
            get
            {
#if FORCE_ENABLE_FILE_LOG
                return true;
#else
                if (GetLogFilePathFromEnvVar() != null)
                    return true;

                return !Application.isEditor;
#endif
            }
        }

        public static bool NonEssentialLogsDisabled =>
#if FORCE_ENABLE_ALL_LOGS
            false;
#else
            Application.isEditor && !Application.isBatchMode && !UnitTestDetector.IsRunningUnitTests;
#endif

        private static bool UsingHtmlUnityConsolePattern { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [DidReloadScripts]
#endif
        public static void Configure()
        {
            if (_isConfigured)
                return;

            _isConfigured = true;
            Hierarchy hierarchy = (Hierarchy) GetRepository();

            // Unity console
            UsingHtmlUnityConsolePattern = Application.isEditor && !Application.isBatchMode;

            PatternLayout unityConsolePattern = new PatternLayout();
            unityConsolePattern.ConversionPattern = "[%logger]";
            if (UsingHtmlUnityConsolePattern)
            {
                unityConsolePattern.ConversionPattern = "<i>[%logger]</i>";
            }

            unityConsolePattern.ConversionPattern += "\u00A0%message%exceptionpadding{\n}%exception";

            unityConsolePattern.AddConverter("exceptionpadding", typeof(ExceptionPaddingPatternLayoutConverter));
            unityConsolePattern.ActivateOptions();

            UnityConsoleAppender unityConsoleAppender = new UnityConsoleAppender
            {
                Layout = unityConsolePattern
            };

            foreach (IFilter logsFilter in LoggingPlatformConfiguration.CreateSpammyLogsFilters())
            {
                unityConsoleAppender.AddFilter(logsFilter);
            }
            unityConsoleAppender.AddFilter(new LoggerMatchFilter
            {
                LoggerToMatch = UnityLog.Logger.Name,
                AcceptOnMatch = false
            });

            hierarchy.Root.AddAppender(unityConsoleAppender);

            // File
            if (FileLogEnabled)
            {
                HtmlLayout htmlLayout = new CustomHtmlLayout("%counter%utcdate{HH:mm:ss}%level%logger%message");
                htmlLayout.LogName = Application.productName + " " + BuildMetaInfo.Instance.ShortVersionName;
                htmlLayout.ActivateOptions();

                RollingFileAppender fileAppender = new RollingFileAppender
                {
                    File = GetLogFilePath(),
                    Layout = htmlLayout,
                    Encoding = Encoding.UTF8,
                    RollingStyle = RollingFileAppender.RollingMode.Once,
                    MaxSizeRollBackups = Application.isBatchMode ? 0 : 3,
                    PreserveLogFileNameExtension = true
                };

                foreach (IFilter logsFilter in LoggingPlatformConfiguration.CreateSpammyLogsFilters())
                {
                    fileAppender.AddFilter(logsFilter);
                }

                fileAppender.ActivateOptions();
                hierarchy.Root.AddAppender(fileAppender);
            }

            Application.logMessageReceivedThreaded += ApplicationOnLogMessageReceivedThreaded;

            // Finish up
            #if UNITY_ANDROID
                hierarchy.Root.Level = Level.Off;
            #else
                hierarchy.Root.Level = Level.All;
            #endif
            
            hierarchy.Configured = true;
        }

        private static string GetLogFilePathFromEnvVar()
        {
            string path = Environment.GetEnvironmentVariable(EnvVarLogFilePath);
            if (!String.IsNullOrWhiteSpace(path))
                return Path.GetFullPath(path);

            return null;
        }

        private static void ApplicationOnLogMessageReceivedThreaded(string condition, string stacktrace, LogType type)
        {
            // Detect our own logs and only log the rest
            int patternStartIndex =
                condition.IndexOf(UsingHtmlUnityConsolePattern ? "<i>[" : "[", StringComparison.Ordinal);
            int patternEndIndex =
                condition.IndexOf(UsingHtmlUnityConsolePattern ? "]</i>\u00A0" : "]\u00A0", StringComparison.Ordinal);

            if (!(patternStartIndex == 0 && patternEndIndex > patternStartIndex))
            {
                if (!String.IsNullOrWhiteSpace(stacktrace))
                {
                    condition = condition + "\n" + stacktrace;
                }

                switch (type)
                {
                    case LogType.Log:
                        UnityLog.Info(condition);
                        break;
                    case LogType.Warning:
                        UnityLog.Warn(condition);
                        break;
                    case LogType.Error:
                    case LogType.Assert:
                    case LogType.Exception:
                        UnityLog.Error(condition);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }
    }
}
