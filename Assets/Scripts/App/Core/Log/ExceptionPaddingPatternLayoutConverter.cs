using System;
using System.IO;
using log4net.Core;
using log4net.Layout.Pattern;

namespace Loom.ZombieBattleground
{
    public class ExceptionPaddingPatternLayoutConverter : PatternLayoutConverter
    {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (loggingEvent.ExceptionObject == null || String.IsNullOrEmpty(loggingEvent.RenderedMessage))
                return;

            string text = Option;
            writer.Write(text);
        }
    }
}
