// NOTE: this class is compiled into a separate log4netUnitySupport assembly,
// so that Unity still opens the original line when double-clicking in Console

#if LOG4NET_UNITY

using System;
using log4net;
using UnityEngine;
using Object = UnityEngine.Object;

namespace log4netUnitySupport
{
    public class UnityLoggerWrapper : UnityEngine.ILogger
    {
        private readonly ILog _log;

        public UnityLoggerWrapper(ILog log4NetLogger)
        {
            _log = log4NetLogger ?? throw new ArgumentNullException(nameof(log4NetLogger));
        }

        public bool IsLogTypeAllowed(LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                    return _log.IsErrorEnabled;
                case LogType.Assert:
                    return _log.IsErrorEnabled;
                case LogType.Warning:
                    return _log.IsWarnEnabled;
                case LogType.Log:
                    return _log.IsInfoEnabled;
                case LogType.Exception:
                    return _log.IsErrorEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            Log(logType, String.Format(format, args));
        }

        public void LogException(Exception exception, Object context)
        {
            Log(LogType.Exception, null, exception, context);
        }

        public void Log(LogType logType, object message)
        {
            Log(logType, null, message, null);
        }

        public void Log(LogType logType, object message, Object context)
        {
            Log(logType, null, message, context);
        }

        public void Log(LogType logType, string tag, object message)
        {
            Log(logType, tag, message, null);
        }

        protected virtual bool FilterLog(LogType logType, string tag, object message, Object context)
        {
            return true;
        }

        public void Log(LogType logType, string tag, object message, Object context)
        {
            if (!FilterLog(logType, tag, message, context))
                return;

            string format = IsEditorProvider.IsEditor ?
                "<i>[{0}]</i> {1}" :
                "[{0}] {1}";

            switch (logType)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    if (message is Exception exception)
                    {
                        if (String.IsNullOrEmpty(tag))
                        {
                            _log.Error("", exception);
                        }
                        else
                        {

                            _log.Error(String.Format(format, tag, ""), exception);
                        }
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(tag))
                        {
                            _log.Error(message);
                        }
                        else
                        {
                            _log.ErrorFormat(format, tag, message);
                        }
                    }

                    break;
                case LogType.Warning:
                    if (String.IsNullOrEmpty(tag))
                    {
                        _log.Warn(message);
                    }
                    else
                    {
                        _log.WarnFormat(format, tag, message);
                    }
                    break;
                case LogType.Log:
                    if (String.IsNullOrEmpty(tag))
                    {
                        _log.Info(message);
                    }
                    else
                    {
                        _log.InfoFormat(format, tag, message);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        public void Log(object message)
        {
            Log(LogType.Log, null, message, null);
        }

        public void Log(string tag, object message)
        {
            Log(LogType.Log, tag, message, null);
        }

        public void Log(string tag, object message, Object context)
        {
            Log(LogType.Log, tag, message, context);
        }

        public void LogWarning(string tag, object message)
        {
            Log(LogType.Warning, tag, message, null);
        }

        public void LogWarning(string tag, object message, Object context)
        {
            Log(LogType.Warning, tag, message, context);
        }

        public void LogError(string tag, object message)
        {
            Log(LogType.Error, tag, message, null);
        }

        public void LogError(string tag, object message, Object context)
        {
            Log(LogType.Error, tag, message, context);
        }

        public void LogFormat(LogType logType, string format, params object[] args)
        {
            Log(logType, String.Format(format, args));
        }

        public void LogException(Exception exception)
        {
            LogException(exception, null);
        }

        public ILogHandler logHandler { get; set; }

        public bool logEnabled
        {
            get => true;
            set { }
        }

        public LogType filterLogType
        {
            get => LogType.Log;
            set {}
        }
    }
}

#endif
