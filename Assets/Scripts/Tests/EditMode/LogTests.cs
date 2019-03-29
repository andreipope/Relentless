using System;
using System.IO;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4netUnitySupport;
using NUnit.Framework;
using UnityEngine;

namespace Loom.ZombieBattleground.Test
{
    public class LogTests
    {
        [Test]
        public void HtmlLayout()
        {
            ILoggerRepository repository = LogManager.CreateRepository(Guid.NewGuid().ToString());
            Hierarchy hierarchy = (Hierarchy) LogManager.GetRepository(repository.Name);
            HtmlLayout htmlLayout = new CustomHtmlLayout("%counter%utcdate{HH:mm:ss}%level%logger%message");
            htmlLayout.LogName = "Test";
            htmlLayout.MaxTextLengthBeforeCollapse = 50;
            htmlLayout.ActivateOptions();

            StringWriter stringWriter = new StringWriter();
            TextWriterAppender writerAppender = new TextWriterAppender
            {
                Layout = htmlLayout,
                Threshold = Level.All,
                Writer = stringWriter
            };

            hierarchy.Root.AddAppender(writerAppender);

            // Finish up
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

            ILog fooLog = LogManager.GetLogger(repository.Name, "FooLogger");
            ILog barLog = LogManager.GetLogger(repository.Name,"BarLogger");

            fooLog.Info("Some info 1");
            fooLog.Info("Some info 2");
            fooLog.Warn("achtung 1");
            fooLog.Error("something awful!!1");

            barLog.Info("Some info 1");
            barLog.Info("Some info 2");
            barLog.Error(new ArgumentNullException(nameof(hierarchy)));
            barLog.Error("exception message!", new ArgumentNullException(nameof(hierarchy)));
            try
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }
            catch (Exception e)
            {
                barLog.Error("exception message!", e);
            }

            barLog.Warn("achtung 1");
            barLog.Error("something awful!!1");

            //Debug.Log(stringWriter.ToString());

#if UNITY_EDITOR
            UnityEditor.EditorGUIUtility.systemCopyBuffer = stringWriter.ToString();
#endif
        }

        [Test]
        [Ignore("only for manual run")]
        public void UnityAppender()
        {
            ILoggerRepository repository = LogManager.CreateRepository(Guid.NewGuid().ToString());
            Hierarchy hierarchy = (Hierarchy) LogManager.GetRepository(repository.Name);
            HtmlLayout htmlLayout = new CustomHtmlLayout("%counter%utcdate{HH:mm:ss}%level%logger%message");
            htmlLayout.LogName = "Test";
            htmlLayout.MaxTextLengthBeforeCollapse = 50;
            htmlLayout.ActivateOptions();

            PatternLayout unityConsolePattern = new PatternLayout();
            unityConsolePattern.ConversionPattern = "[%logger] %message";
            if (Application.isEditor && !Application.isBatchMode)
            {
                unityConsolePattern.ConversionPattern = "<i>[%logger]</i> %message%exceptionpadding{\n}%exception";
            }
            unityConsolePattern.AddConverter("exceptionpadding", typeof(ExceptionPaddingPatternLayoutConverter));
            unityConsolePattern.ActivateOptions();

            UnityConsoleAppender unityConsoleAppender = new UnityConsoleAppender
            {
                Layout = unityConsolePattern
            };

            hierarchy.Root.AddAppender(unityConsoleAppender);

            // Finish up
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

            ILog fooLog = LogManager.GetLogger(repository.Name, "FooLogger");
            ILog barLog = LogManager.GetLogger(repository.Name,"BarLogger");

            fooLog.Info("Some info 1");
            fooLog.Info("Some info 2");
            fooLog.Warn("achtung 1");
            fooLog.Error("something awful!!1");

            barLog.Info("Some info 1");
            barLog.Info("Some info 2");
            barLog.Error(new ArgumentNullException(nameof(hierarchy)));
            barLog.Error("exception message!", new ArgumentNullException(nameof(hierarchy)));
            try
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }
            catch (Exception e)
            {
                barLog.Error("exception message!", e);
            }

            barLog.Warn("achtung 1");
            barLog.Error("something awful!!1");

            barLog.Warn(new Exception("warning exception with no message"));
            barLog.Warn("some message", new Exception("warning exception with message"));

            try
            {
                ThrowException("warning exception with empty message!!");
            }
            catch (Exception e)
            {
                barLog.Warn("", e);
                barLog.Warn("some message", e);
            }

            barLog.Error(new Exception("error exception with no message"));
            barLog.Error("some message", new Exception("error exception with message"));
            barLog.Error("", new Exception("error exception with empty message"));
        }

        private Exception ThrowException(string message)
        {
            throw new Exception(message);
        }
    }
}
