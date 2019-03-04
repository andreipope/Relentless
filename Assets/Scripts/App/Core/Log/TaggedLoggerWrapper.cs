using System;
using log4net.Core;
using log4net.Repository;

namespace Loom.ZombieBattleground
{
    public class TaggedLoggerWrapper : ILogger
    {

        public ILogger Logger { get; }

        public string Tag { get; }

        public TaggedLoggerWrapper(ILogger logger, string prefix)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Tag = prefix ?? throw new ArgumentNullException(nameof(prefix));
        }

        public void Log(Type callerStackBoundaryDeclaringType, Level level, object message, Exception exception)
        {
            if (message != null)
            {
                message = "[" + Tag + "] " + message;
            }

            Logger.Log(callerStackBoundaryDeclaringType, level, message, exception);
        }

        public void Log(LoggingEvent logEvent)
        {
            LoggingEventData loggingEventData = logEvent.GetLoggingEventData();

            if (loggingEventData.Message != null)
            {
                loggingEventData.Message = "[" + Tag + "] " + loggingEventData.Message;
            }

            loggingEventData.Properties["CustomTag"] = Tag;

            LoggingEvent loggingEvent = new LoggingEvent(loggingEventData);
            Logger.Log(loggingEvent);
        }

        public bool IsEnabledFor(Level level)
        {
            return Logger.IsEnabledFor(level);
        }

        public string Name => Logger.Name;

        public ILoggerRepository Repository => Logger.Repository;
    }
}
