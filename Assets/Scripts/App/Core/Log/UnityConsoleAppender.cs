// NOTE: this class is compiled into a separate log4netUnitySupport assembly,
// so that Unity still opens the original line when double-clicking in Console

/*
using System;
using System.Diagnostics;
using System.IO;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using Debug = UnityEngine.Debug;

namespace Loom.ZombieBattleground
{
    public class UnityConsoleAppender : AppenderSkeleton
    {
        public virtual ILayout Layout { get; set; }

        [DebuggerStepThrough]
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (String.IsNullOrEmpty(loggingEvent.RenderedMessage) && loggingEvent.ExceptionObject != null)
            {
                Debug.LogException(loggingEvent.ExceptionObject);
                return;
            }

            string message = FormatMessage(loggingEvent);
            LogMessage(loggingEvent, message);
        }

        private string FormatMessage(LoggingEvent loggingEvent)
        {
            string message = loggingEvent.RenderedMessage;
            if (Layout != null)
            {
                using (StringWriter writer = new StringWriter())
                {
                    Layout.Format(writer, loggingEvent);
                    message = writer.ToString();
                }
            }

            return message;
        }

        protected virtual void LogMessage(LoggingEvent loggingEvent, string message)
        {
            if (loggingEvent.Level >= Level.Error)
            {
                Debug.LogError(message);
            }
            else if (loggingEvent.Level >= Level.Warn)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }
    }
}
*/
