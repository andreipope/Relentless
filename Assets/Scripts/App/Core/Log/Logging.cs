using System;
using System.IO;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using log4net.Util;
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
        private const string RepositoryName = "ZBLogRepository";

        private static bool _isConfigured;

        static Logging()
        {
            LogManager.CreateRepository(RepositoryName);
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
            Hierarchy hierarchy = (Hierarchy) LogManager.GetRepository(RepositoryName);

            // Unity console
            PatternLayout unityConsolePattern = new PatternLayout();
            unityConsolePattern.ConversionPattern = "[%logger] %message";
            unityConsolePattern.ActivateOptions();

            UnityConsoleAppender unityConsoleAppender = new UnityConsoleAppender
            {
                Layout = unityConsolePattern
            };

            hierarchy.Root.AddAppender(unityConsoleAppender);

            // File
            HtmlLayout htmlLayout = new CustomHtmlLayout("%utcdate{HH:mm:ss}%level%logger%message");
            htmlLayout.LogName = "Zombie Battleground";
            htmlLayout.ActivateOptions();

            RollingFileAppender fileAppender = new RollingFileAppender
            {
                File = Path.Combine(Application.persistentDataPath, "Log.html"),
                Layout = htmlLayout,
                Encoding = Encoding.UTF8,
                RollingStyle = RollingFileAppender.RollingMode.Once,
                MaxSizeRollBackups = 3,
            };

            fileAppender.ActivateOptions();
            hierarchy.Root.AddAppender(fileAppender);

            // Finish up
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

#if UNITY_EDITOR
            //EditorApplication.playModeStateChanged -= EditorApplicationOnPlayModeStateChanged;
            //EditorApplication.playModeStateChanged += EditorApplicationOnPlayModeStateChanged;
#endif
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

        public class CustomHtmlLayout : HtmlLayout
        {
            public CustomHtmlLayout(string pattern) : base(pattern) { }

            protected override bool IsFilteredPatternConverter(PatternConverter patternConverter)
            {
                switch (GetPatternConverterName(patternConverter))
                {
                    case "Logger":
                    case "Message":
                        return true;
                    default:
                        return false;
                }
            }

            protected override string GetLogItemCellClass(PatternConverter patternConverter, LoggingEvent loggingEvent)
            {
                switch (GetPatternConverterName(patternConverter))
                {
                    case "Time":
                        return "text-monospace small";
                    default:
                        return base.GetLogItemCellClass(patternConverter, loggingEvent);
                }
            }

            protected override string CreatePatternConverterName(PatternConverter patternConverter)
            {
                string typeName = patternConverter.GetType().Name;
                switch (typeName)
                {
                    case "UtcDatePatternConverter":
                        return "Time";
                    default:
                        return base.CreatePatternConverterName(patternConverter);
                }
            }

            protected override void WriteCell(LoggingEvent loggingEvent, PatternConverter patternConverter, TextWriter htmlWriter)
            {
                switch (GetPatternConverterName(patternConverter))
                {
                    case "Message":
                        base.WriteCell(loggingEvent, patternConverter, htmlWriter);
                        string exceptionString = loggingEvent.GetExceptionString();
                        if (!String.IsNullOrWhiteSpace(exceptionString))
                        {
                            htmlWriter.WriteLine("");
                            htmlWriter.WriteLine(exceptionString);
                        }
                        break;
                    default:
                        base.WriteCell(loggingEvent, patternConverter, htmlWriter);
                        break;
                }
            }

            protected override void WriteException(TextWriter writer, TextWriter htmlWriter, LoggingEvent loggingEvent, int converterCount)
            {
            }
        }
    }
}
