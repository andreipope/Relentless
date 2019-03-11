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
            string logFilePath = GetLogFilePathFromEnvVar();
            if (logFilePath != null)
                return logFilePath;

            return Path.Combine(Application.persistentDataPath, DefaultLogFileName);
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

        public static bool NonEssentialLogsDisabled => Application.isEditor && !Application.isBatchMode && !UnitTestDetector.IsRunningUnitTests;

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

            IFilter[] spammyLogsFilters = LoggingPlatformConfiguration.CreateSpammyLogsFilters();

            // Unity console
            PatternLayout unityConsolePattern = new PatternLayout();
            unityConsolePattern.ConversionPattern = "[%logger] %message";
            if (Application.isEditor && !Application.isBatchMode)
            {
                unityConsolePattern.ConversionPattern = "<i>[%logger]</i> %message";
            }
            unityConsolePattern.ActivateOptions();

            UnityConsoleAppender unityConsoleAppender = new UnityConsoleAppender
            {
                Layout = unityConsolePattern
            };

            foreach (IFilter logsFilter in spammyLogsFilters)
            {
                unityConsoleAppender.AddFilter(logsFilter);
            }

            hierarchy.Root.AddAppender(unityConsoleAppender);

            // File
            if (FileLogEnabled)
            {
                HtmlLayout htmlLayout = new CustomHtmlLayout("%counter%utcdate{HH:mm:ss}%level%logger%message");
                htmlLayout.LogName = "Zombie Battleground " + BuildMetaInfo.Instance.ShortVersionName;
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

                foreach (IFilter logsFilter in spammyLogsFilters)
                {
                    fileAppender.AddFilter(logsFilter);
                }

                fileAppender.ActivateOptions();
                hierarchy.Root.AddAppender(fileAppender);
            }

            // Finish up
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
        }

        private static string GetLogFilePathFromEnvVar()
        {
            string path = Environment.GetEnvironmentVariable(EnvVarLogFilePath);
            if (!String.IsNullOrWhiteSpace(path))
                return Path.GetFullPath(path);

            return null;
        }
    }
}
