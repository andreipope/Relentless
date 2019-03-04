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
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Loom.ZombieBattleground
{
    public static class Logging
    {
        public const string LogFileName = "Log.html";
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
                PreserveLogFileNameExtension = true,
                ImmediateFlush = false
            };

            fileAppender.ActivateOptions();
            hierarchy.Root.AddAppender(fileAppender);
#endif

            // Finish up
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

#if UNITY_EDITOR
            //EditorApplication.playModeStateChanged -= EditorApplicationOnPlayModeStateChanged;
            //EditorApplication.playModeStateChanged += EditorApplicationOnPlayModeStateChanged;
#endif
        }

        public static string GetLogFilePath()
        {
            return Path.Combine(Application.persistentDataPath, LogFileName);
        }

#if UNITY_EDITOR
        private static void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange state)
        {
            Hierarchy hierarchy = (Hierarchy) LogManager.GetRepository();
            hierarchy.Shutdown();
            hierarchy.Clear();
            _isConfigured = false;
        }
#endif

    }

}
