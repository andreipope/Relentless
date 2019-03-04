using System;
using System.IO;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4netUnitySupport;
using UnityEditor.Callbacks;
using UnityEngine;
using Logger = log4net.Repository.Hierarchy.Logger;

namespace Loom.ZombieBattleground
{
    public static class Logging
    {
        private const string LogFilePathEnvVariable = "ZB_LOG_FILE_PATH";
        private const string DefaultLogFileName = "Log.html";
        private const string RepositoryName = "ZBLogRepository";

        private static bool _isConfigured;

        static Logging()
        {
            LogManager.CreateRepository(RepositoryName);
        }

        public static ILoggerRepository GetRepository()
        {
            return LogManager.GetRepository(RepositoryName);
        }

        public static ILog GetLog(string name)
        {
            return LogManager.GetLogger(RepositoryName, name);
        }

        public static Logger GetLogger(string name)
        {
            return (Logger) LogManager.GetLogger(RepositoryName, name).Logger;
        }

        public static string GetLogFilePath()
        {
            string logFilePath = Environment.GetEnvironmentVariable(LogFilePathEnvVariable);
            if (!String.IsNullOrWhiteSpace(logFilePath))
                return logFilePath;

            return Path.Combine(Application.persistentDataPath, DefaultLogFileName);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [DidReloadScripts]
        public static void Setup()
        {
            if (_isConfigured)
                return;

            _isConfigured = true;
            Hierarchy hierarchy = (Hierarchy) GetRepository();

            // Unity console
            PatternLayout unityConsolePattern = new PatternLayout();
            unityConsolePattern.ConversionPattern = "[%logger] %message";
            if (Application.isEditor)
            {
                unityConsolePattern.ConversionPattern = "<i>[%logger]</i> %message";
            }
            unityConsolePattern.ActivateOptions();

            UnityConsoleAppender unityConsoleAppender = new UnityConsoleAppender
            {
                Layout = unityConsolePattern
            };

            hierarchy.Root.AddAppender(unityConsoleAppender);

#if !UNITY_EDITOR || FORCE_ENABLE_ALL_LOGS
            // File
            HtmlLayout htmlLayout = new CustomHtmlLayout("%utcdate{HH:mm:ss}%level%logger%message");
            htmlLayout.LogName = "Zombie Battleground " + BuildMetaInfo.Instance.ShortVersionName;
            htmlLayout.ActivateOptions();

            RollingFileAppender fileAppender = new RollingFileAppender
            {
                File = GetLogFilePath(),
                Layout = htmlLayout,
                Encoding = Encoding.UTF8,
                RollingStyle = RollingFileAppender.RollingMode.Once,
                MaxSizeRollBackups = 3,
                PreserveLogFileNameExtension = true
            };

            fileAppender.ActivateOptions();
            hierarchy.Root.AddAppender(fileAppender);
#endif

            // Finish up
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
        }
    }
}
