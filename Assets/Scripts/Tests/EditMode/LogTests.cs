using System;
using System.IO;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;
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

            Debug.Log(stringWriter.ToString());

#if UNITY_EDITOR
            UnityEditor.EditorGUIUtility.systemCopyBuffer = stringWriter.ToString();
#endif
        }
    }
}
